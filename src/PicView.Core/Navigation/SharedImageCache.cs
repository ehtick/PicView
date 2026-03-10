using System.Collections.Concurrent;
using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

/// <summary>
/// Acts as the central station for acquiring and managing cached <see cref="ImageModel"/> resources.
/// <para>
/// This class coordinates between the storage container (multiple <see cref="EvictingDictionary{TValue}"/>) 
/// and the background worker (<see cref="Preloader2"/>) to ensure images are loaded, retrieved, 
/// and evicted efficiently across multiple tab owners.
/// </para>
/// </summary>
public class SharedImageCache : IImageCache
{
    private readonly ConcurrentDictionary<string, EvictingDictionary<PreLoadValue>> _ownerDictionaries = new();
    
    // Fast lookup by file path (using OS-specific string comparer)
    private readonly ConcurrentDictionary<string, PreLoadValue> _pathLookup;
    // Lazy disposal list: FilePath -> (PreLoadValue, ExpirationTime)
    private readonly ConcurrentDictionary<string, (PreLoadValue Item, DateTime Expiration)> _disposalList;

    
    // Keep track of which directories and file lists each owner has for transfer logic
    private readonly ConcurrentDictionary<string, (string Directory, IReadOnlyList<FileInfo> Files, int CurrentIndex)> _ownerContexts = new();
    
    // The worker
    private readonly Preloader2 _preLoader;
    
    private readonly Lock _disposeLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SharedImageCache"/> class.
    /// </summary>
    /// <param name="imageLoader">The function used to load an ImageModel from a FileInfo.</param>
    public SharedImageCache(Func<FileInfo, ValueTask<ImageModel>> imageLoader)
    {
        var pathComparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
            
        _pathLookup = new ConcurrentDictionary<string, PreLoadValue>(pathComparer);
        _disposalList = new ConcurrentDictionary<string, (PreLoadValue, DateTime)>(pathComparer);

        _preLoader = new Preloader2(imageLoader, this);
    }
    
    public async Task<ImageModel?> LoadAsync(string ownerId, int index, IReadOnlyList<FileInfo> list, CancellationToken ct = default)
    {
        return await _preLoader.AddAsync(ownerId, index, list, false, ct).ConfigureAwait(false);
    }

    public void RegisterOwner(string ownerId)
    {
        _ownerDictionaries.TryAdd(ownerId, new EvictingDictionary<PreLoadValue>(PreLoaderConfig.MaxCount));
    }

    public void RemoveOwner(string ownerId)
    {
        _ownerDictionaries.TryRemove(ownerId, out _);
        _ownerContexts.TryRemove(ownerId, out _);
    }

    public bool TryGet(FileInfo f, out PreLoadValue? value)
    {
        if (_pathLookup.TryGetValue(f.FullName, out value))
        {
            return true;
        }
        return TryResurrect(f.FullName, out value);
    }

    public bool TryGet(string ownerId, int index, out PreLoadValue? value)
    {
        if (_ownerDictionaries.TryGetValue(ownerId, out var dict))
        {
            return dict.TryGetValue(index, out value);
        }
        value = null;
        return false;
    }

    public bool TryGet(ReadOnlySpan<char> f, out PreLoadValue? value)
    {
        var lookup = _pathLookup.GetAlternateLookup<ReadOnlySpan<char>>();
        if (lookup.TryGetValue(f, out value))
        {
            return true;
        }
        return TryResurrect(f.ToString(), out value);
    }

    private bool TryResurrect(string path, out PreLoadValue? value)
    {
        // Check if it's in the disposal list
        if (_disposalList.TryRemove(path, out var pendingDisposal))
        {
            value = pendingDisposal.Item;
            // Add it back to path lookup
            _pathLookup.TryAdd(path, value);
            return true;
        }
        
        value = null;
        return false;
    }

    public void Clear()
    {
        foreach (var dict in _ownerDictionaries.Values)
        {
            dict.Clear();
        }
        _pathLookup.Clear();
        
        foreach (var kvp in _disposalList)
        {
            DisposeHelper(kvp.Value.Item);
        }
        _disposalList.Clear();
    }

    public void Clear(string ownerId)
    {
        if (!_ownerDictionaries.TryGetValue(ownerId, out var dict))
        {
            return;
        }

        var values = dict.Values.ToList();
        dict.Clear();
            
        foreach (var value in values)
        {
            CheckAndDisposeIfNotReferenced(value);
        }
    }

    public bool Contains(ReadOnlySpan<char> span, out PreLoadValue? value) =>
        TryGet(span, out value);

    public bool Contains(PreLoadValue value) =>
        TryGet(value.ImageModel.FileInfo.FullName, out var preLoadValue) && preLoadValue == value;

    public bool Add(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse)
    {
        return TryAdd(ownerId, index, preLoadValue, listCount, isReverse, out _);
    }

    public bool TryAdd(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse, out PreLoadValue? value)
    {
        value = null;
        if (!_ownerDictionaries.TryGetValue(ownerId, out var dict))
        {
            return false;
        }

        var path = preLoadValue.ImageModel.FileInfo.FullName;
        
        // Ensure path lookup has it
        _pathLookup.TryAdd(path, preLoadValue);
        
        // Add to owner dictionary
        var evicted = dict.TryAdd(index, preLoadValue, listCount, isReverse, out var evictedValue);

        if (!evicted || evictedValue == null)
        {
            return evicted;
        }

        CheckAndDisposeIfNotReferenced(evictedValue);
        value = evictedValue;

        return evicted;
    }

    internal void CheckAndDisposeIfNotReferenced(PreLoadValue item)
    {
        var isReferenced = false;
        foreach (var dict in _ownerDictionaries.Values)
        {
            foreach (var value in dict.Values)
            {
                if (value != item)
                {
                    continue;
                }

                isReferenced = true;
                break;
            }
            if (isReferenced) break;
        }

        if (isReferenced)
        {
            return;
        }

        var path = item.ImageModel.FileInfo.FullName;
        _pathLookup.TryRemove(path, out _);
        
        // Add to disposal queue instead of immediately disposing
        _disposalList.AddOrUpdate(path, 
            (item, DateTime.UtcNow.AddMinutes(1)), 
            (_, existing) => existing.Item == item ? (existing.Item, DateTime.UtcNow.AddMinutes(1)) : existing);
            
        // Process any expired items lazily
        ProcessDisposalQueue();
    }

    private void ProcessDisposalQueue()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _disposalList)
        {
            if (now >= kvp.Value.Expiration)
            {
                if (_disposalList.TryRemove(kvp.Key, out var removed))
                {
                    DisposeHelper(removed.Item);
                }
            }
        }
    }

    public void Preload(string ownerId, int currentIndex, bool reversed, IReadOnlyList<FileInfo> files, CancellationToken token) 
    {
        // Update context for transfer logic
        if (files.Count > 0)
        {
            _ownerContexts[ownerId] = (files[0].DirectoryName ?? string.Empty, files, currentIndex);
        }
        _preLoader.Preload(ownerId, currentIndex, reversed, files, token);
    }

    public async ValueTask<bool> WaitForLoadingCompleteAsync(string ownerId, int index)
    {
        if (!TryGet(ownerId, index, out var value) || value is null)
        {
            return false;
        }

        await value.WaitForLoadingCompleteAsync();
        return true;
    }
    
    public void Clear(TabViewModel tab, int currentIndex, string directory, IReadOnlyList<FileInfo> files)
    {
        var id = tab.Id;
        
        if (_ownerDictionaries.TryGetValue(id, out var dict))
        {
            var closingItems = dict.Values.ToList();
            dict.Clear();
            
            // Find another eligible tab
            string? targetOwnerId = null;
            IReadOnlyList<FileInfo>? targetFiles = null;
            var targetCurrentIndex = 0;
            
            foreach (var kvp in _ownerContexts)
            {
                if (kvp.Key == id || kvp.Value.Directory != directory)
                {
                    continue;
                }

                targetOwnerId = kvp.Key;
                targetFiles = kvp.Value.Files;
                targetCurrentIndex = kvp.Value.CurrentIndex;
                break;
            }
            
            if (targetOwnerId != null && targetFiles != null && _ownerDictionaries.TryGetValue(targetOwnerId, out var targetDict))
            {
                var fileIndexMap = new Dictionary<string, int>(_pathLookup.Comparer);
                for (var i = 0; i < targetFiles.Count; i++)
                {
                    fileIndexMap[targetFiles[i].FullName] = i;
                }
                
                var count = targetFiles.Count;
                
                foreach (var item in closingItems)
                {
                    if (fileIndexMap.TryGetValue(item.ImageModel.FileInfo.FullName, out var targetIndex))
                    {
                        // Calculate distance
                        var distForward = (targetIndex - targetCurrentIndex + count) % count;
                        var distBackward = (targetCurrentIndex - targetIndex + count) % count;
                        
                        if (distForward <= PreLoaderConfig.PositiveIterations || distBackward <= PreLoaderConfig.NegativeIterations)
                        {
                            // Transfer
                            if (targetDict.TryAdd(targetIndex, item, count, false, out var evictedTargetItem))
                            {
                                if (evictedTargetItem != null)
                                {
                                    CheckAndDisposeIfNotReferenced(evictedTargetItem);
                                }
                            }
                            continue;
                        }
                    }
                    
                    // Not transferred
                    CheckAndDisposeIfNotReferenced(item);
                }
            }
            else
            {
                // No eligible tab found
                foreach (var item in closingItems)
                {
                    CheckAndDisposeIfNotReferenced(item);
                }
            }
        }
        
        RemoveOwner(id);
    }

    public void TryRemove(string ownerId, int index)
    {
        if (!_ownerDictionaries.TryGetValue(ownerId, out var dict))
        {
            return;
        }

        if (!dict.Remove(index, out var removedValue))
        {
            return;
        }

        if (removedValue != null)
        {
            CheckAndDisposeIfNotReferenced(removedValue);
        }
    }

    public void Resynchronize(string ownerId, IReadOnlyList<FileInfo> files)
    {
        if (!_ownerDictionaries.TryGetValue(ownerId, out var dict))
        {
            return;
        }

        var oldItems = dict.GetEnumerator();
        using IDisposable oldItems1 = oldItems;
        var currentItems = new List<KeyValuePair<int, PreLoadValue>>();
        while (oldItems.MoveNext())
        {
            currentItems.Add(oldItems.Current);
        }
        dict.Clear();

        var newFileMap = new Dictionary<string, int>(_pathLookup.Comparer);
        for (var i = 0; i < files.Count; i++)
        {
            newFileMap[files[i].FullName] = i;
        }

        foreach (var item in currentItems)
        {
            if (newFileMap.TryGetValue(item.Value.ImageModel.FileInfo.FullName, out var newIndex))
            {
                // Put it back at new index
                dict.TryAdd(newIndex, item.Value, files.Count, false, out var evicted);
                if (evicted != null)
                {
                    CheckAndDisposeIfNotReferenced(evicted);
                }
            }
            else
            {
                // File no longer exists in the list
                CheckAndDisposeIfNotReferenced(item.Value);
            }
        }
        
        // Update context
        if (files.Count <= 0)
        {
            return;
        }

        if (_ownerContexts.TryGetValue(ownerId, out var ctx))
        {
            _ownerContexts[ownerId] = (files[0].DirectoryName ?? string.Empty, files, ctx.CurrentIndex);
        }
    }

    internal void DisposeHelper(PreLoadValue? item)
    {
        if (item?.ImageModel?.Image is not IDisposable disposable)
        {
            return;
        }

        _disposeLock.Enter();
        try
        {
            disposable.Dispose();
        }
        finally
        {
            _disposeLock.Exit();
        }
    }
}
