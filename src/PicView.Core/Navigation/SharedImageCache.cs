using System.Collections.Concurrent;
using PicView.Core.DebugTools;
using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;
using ZLinq;

namespace PicView.Core.Navigation;

public class SharedImageCache : IImageCache
{
    private readonly Lock _evictionLock = new();
    private readonly Func<FileInfo, ValueTask<ImageModel>> _imageLoader;

    // Map: FilePath -> CachedItem
    private readonly ConcurrentDictionary<string, CachedItem> _items = new();

    // Config
    private readonly int _maxItems;

    public SharedImageCache(Func<FileInfo, ValueTask<ImageModel>> imageLoader, int maxItems = 100)
    {
        _imageLoader = imageLoader ?? throw new ArgumentNullException(nameof(imageLoader));
        _maxItems = maxItems > 0 ? maxItems : 100;
    }
    
    public PreLoadValue GetOrScheduleLoad(FileInfo file, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        var key = file.FullName;

        // 1. Get or Create the container
        var newItem = new CachedItem(new PreLoadValue(new ImageModel { FileInfo = file }));
        var cachedItem = _items.GetOrAdd(key, newItem);
        var preLoadValue = cachedItem.Value;

        // 2. Check if we need to trigger a background load
        // We lock or check state to ensure we only spin up one loader task
        if (preLoadValue.ImageModel.Image != null || preLoadValue.IsLoading)
        {
            return preLoadValue;
        }

        preLoadValue.IsLoading = true;

        // Run the load in the background (Fire-and-forget or tracked by PreLoadValue)
        _ = Task.Run(async () =>
        {
            try
            {
                var loadedModel = await _imageLoader(file).ConfigureAwait(false);
        
                // CRITICAL: Set the model BEFORE setting IsLoading to false
                preLoadValue.ImageModel = loadedModel; 
            }
            finally
            {
                // This triggers the TaskCompletionSource in PreLoadValue
                // The Iterator wakes up, sees the new ImageModel, and updates the UI
                preLoadValue.IsLoading = false; 
            }
        }, ct);

        // 3. Return the handle immediately so UI can bind to what we have NOW
        return preLoadValue;
    }

    public async ValueTask<ImageModel> GetOrLoadAsync(FileInfo file, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        var key = file.FullName;

        // 1. Try get existing
        if (_items.TryGetValue(key, out var cachedItem))
        {
            if (cachedItem.Value.ImageModel?.Image is not null)
            {
                return cachedItem.Value.ImageModel;
            }
            // If it exists but image is null/disposed, we might need to reload. 
            // But PreLoadValue handles loading state.
        }

        // 2. If not found, add new
        var newItem = new CachedItem(new PreLoadValue(new ImageModel { FileInfo = file }));

        // Use GetOrAdd to ensure single instance
        cachedItem = _items.GetOrAdd(key, newItem);

        // 3. Trigger load if needed
        // The PreLoadValue logic handles concurrent loads for the same instance.
        var preLoadValue = cachedItem.Value;

        if (preLoadValue.ImageModel.Image == null && !preLoadValue.IsLoading)
        {
            // It's a fresh item or empty. Load it.
            preLoadValue.IsLoading = true;
            try
            {
                var loadedModel = await _imageLoader(file).ConfigureAwait(false);
                preLoadValue.ImageModel = loadedModel;
            }
            catch (Exception ex)
            {
                DebugHelper.LogDebug(nameof(SharedImageCache), nameof(GetOrLoadAsync), ex);
                // On failure, remove from cache so we can try again later? 
                // Or keep it as failed? For now, remove.
                _items.TryRemove(key, out _);
                throw;
            }
            finally
            {
                preLoadValue.IsLoading = false;
            }
        }
        else if (preLoadValue.IsLoading)
        {
            // Wait for existing load
            await preLoadValue.WaitForLoadingCompleteAsync().WaitAsync(ct).ConfigureAwait(false);
        }

        // 4. Check Eviction (if we added something new and size is too big)
        // We do this asynchronously or periodically?
        // Let's do it inline but effectively.
        if (_items.Count > _maxItems)
        {
            EvictExcessItems();
        }

        return preLoadValue.ImageModel;
    }

    public void UpdatePriorities(object owner, IEnumerable<string> prioritizedFiles)
    {
        if (owner is null || prioritizedFiles is null)
        {
            return;
        }

        // 1. Update priorities for this owner
        // We need to efficiently update the "Owner Priorities" map in each CachedItem.
        // But iterating all items in cache is O(CacheSize) which is small (100).
        // Iterating prioritizedFiles (e.g. 10 items) is also small.

        var priorityList = prioritizedFiles.ToList();

        // First, clear old priorities for this owner in all items?
        // Or just update. 
        // Strategy: 
        //   For each file in priorityList:
        //      If in cache, set priority = index.
        //      (Optionally: If not in cache, maybe pre-load? - The requirement says cache is passive-ish, but typically "UpdatePriorities" implies "I want these").
        //      Let's assume this method is ONLY for cache maintenance of *existing* items or triggering loads?
        //      "Files not in the list for an owner are considered lowest priority".
        //      So we MUST visit all cached items to unset priority if they are no longer in the list.

        // Efficient approach:
        // Set of keys in the new priority list.
        var newKeys = new HashSet<string>(priorityList);

        foreach (var (key, item) in _items)
        {
            // Check if this key is in the new priority list
            var index = priorityList.IndexOf(key);

            lock (item.Priorities)
            {
                if (index >= 0)
                {
                    item.Priorities[owner] = index;
                }
                else
                {
                    // Remove priority for this owner (distance = infinity)
                    item.Priorities.Remove(owner);
                }
            }
        }

        // Trigger loading for high priority items that are NOT in cache?
        // The Interface says: "The cache uses this to determine which images to keep."
        // It doesn't explicitly say "Load missing ones". 
        // BUT, `PreLoader` usually does aggressive preloading.
        // Let's stick to: If it's in `prioritizedFiles`, we *ensure* it's in the cache.
        foreach (var file in priorityList.Where(file => !_items.ContainsKey(file)).AsValueEnumerable())
        {
            // Trigger background load
            _ = Task.Run(() => GetOrLoadAsync(new FileInfo(file)));
        }

        if (_items.Count > _maxItems)
        {
            EvictExcessItems();
        }
    }

    public void RemoveOwner(object owner)
    {
        foreach (var item in _items.Values)
        {
            lock (item.Priorities)
            {
                item.Priorities.Remove(owner);
            }
        }
        // Maybe trigger eviction check
    }

    public bool TryGet(FileInfo f, out PreLoadValue? value)
    {
        if (_items.TryGetValue(f.FullName, out var item))
        {
            value = item.Value;
            return true;
        }

        value = null;
        return false;
    }

    public void Remove(FileInfo f)
    {
        if (_items.TryRemove(f.FullName, out var item))
        {
            DisposeItem(item);
        }
    }

    public void Clear()
    {
        foreach (var key in _items.Keys)
        {
            if (_items.TryRemove(key, out var item))
            {
                DisposeItem(item);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        Clear();
    }

    public void Clear(TabViewModel tab)
    {
        RemoveOwner(tab);
    }

    private void EvictExcessItems()
    {
        // Don't run multiple evictions at once
        if (!_evictionLock.TryEnter())
        {
            return;
        }

        try
        {
            while (_items.Count > _maxItems)
            {
                // Find victim
                // Score = Min(Priority for each owner). 
                // We want to remove the item with the HIGHEST Score (Furthest away).
                // If an item has NO owners interested, Score = Infinity (MaxValue).

                string? victimKey = null;
                var maxScore = -1;

                foreach (var kvp in _items)
                {
                    var item = kvp.Value;
                    int itemScore;

                    lock (item.Priorities)
                    {
                        if (item.Priorities.Count == 0)
                        {
                            itemScore = int.MaxValue;
                        }
                        else
                        {
                            itemScore = item.Priorities.Values.Min();
                        }
                    }

                    // We want to maximize itemScore
                    if (itemScore > maxScore)
                    {
                        maxScore = itemScore;
                        victimKey = kvp.Key;
                    }
                    else if (itemScore == maxScore && itemScore == int.MaxValue)
                    {
                        // Tie breaker for infinite distance? Oldest? 
                        // For now just pick first found.
                        victimKey = kvp.Key;
                    }
                }

                if (victimKey != null)
                {
                    if (_items.TryRemove(victimKey, out var removedItem))
                    {
                        DisposeItem(removedItem);
                    }
                    // Race condition, item already gone
                }
                else
                {
                    // Should not happen if count > maxItems
                    break;
                }
            }
        }
        finally
        {
            _evictionLock.Exit();
        }
    }

    private void DisposeItem(CachedItem item)
    {
        if (item.Value.ImageModel?.Image is IDisposable d)
        {
            d.Dispose();
        }
    }

    // Internal helper class
    private class CachedItem
    {
        public CachedItem(PreLoadValue value)
        {
            Value = value;
        }

        public PreLoadValue Value { get; }

        // Owner -> Priority (Index)
        public Dictionary<object, int> Priorities { get; } = new();
    }
}