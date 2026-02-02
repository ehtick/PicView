using System.Collections.Concurrent;
using PicView.Core.DebugTools;
using PicView.Core.Models;
using PicView.Core.Navigation;
using PicView.Core.Navigation.Interfaces;

namespace PicView.Core.Preloading;

public class Preloader2 : IPreloader
{
    private readonly IImageCache _cache;
    private readonly Func<FileInfo, ValueTask<ImageModel>> _imageModelLoader;
    private int _isRunningFlag; // 0 = idle, 1 = running
    private Dictionary<string , string> Owners = new();
    private string _currentOwner;
    private readonly Lock _lock = new();
    
    public Preloader2(Func<FileInfo, ValueTask<ImageModel>> imageModelLoader, IImageCache cache)
    {
        _imageModelLoader = imageModelLoader ?? throw new ArgumentNullException(nameof(imageModelLoader));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public void Preload(string ownerId, int currentIndex, bool reversed, IReadOnlyList<FileInfo> files, CancellationToken token)
    {
        if (files is null || files.Count == 0)
        {
            DebugHelper.LogDebug(nameof(Preloader2), nameof(Preload), "No files to preload");
            return;
        }
        if (Interlocked.CompareExchange(ref _isRunningFlag, 1, 0) != 0)
        {
            if (ownerId != _currentOwner)
            {
                return; // Already running
            }
            // Allow other tab s to preload
        }
        
        Task.Run(() => PreLoadInternalAsync(ownerId, currentIndex, files, reversed, token), token);
        lock (_lock)
        {
            _currentOwner = ownerId;
        }
    }

    public void RegisterOwner(string ownerId)
    {
        lock (_lock)
        {
            Owners.Add(ownerId, ownerId);
        }

    }

    public void RemoveOwner(string ownerId)
    {
        lock (_lock)
        {
            var foundKey = Owners.FirstOrDefault(x => x.Value.Equals(ownerId)).Key;
            Owners.Remove(foundKey);
        }
    }

    // --- Core Loading Logic (AddAsync) ---

    public async ValueTask<ImageModel?> AddAsync(string ownerId, int index, IReadOnlyList<FileInfo> list,
        bool isReverse = false, CancellationToken ct = default)
    {
        if (list == null || index < 0 || index >= list.Count)
        {
            return null;
        }

        var fileInfo = list[index];

        // 1. Fast Path: Already cached?
        if (_cache.TryGet(fileInfo, out var cachedValue) && cachedValue is not null)
        {
            // Refresh LRU position
            _cache.Add(ownerId, index, cachedValue, list.Count, isReverse);

            if (cachedValue.IsLoading)
            {
                // Piggyback on the existing load
                await cachedValue.WaitForLoadingCompleteAsync().ConfigureAwait(false);
            }

            return cachedValue.ImageModel;
        }

        if (ct.IsCancellationRequested)
        {
            return null;
        }

        // 2. Reserve the slot (Optimistic locking)
        var placeholder = new PreLoadValue(new ImageModel { FileInfo = fileInfo }, true);

        // 'evicted' tells us if we bumped someone out. 
        // Note: The logic for SharedImageCache disposal is preserved here.
        var evicted = _cache.TryAdd(ownerId, index, placeholder, list.Count, isReverse, out var evictedValue);
        if (evicted && _cache is SharedImageCache shared)
        {
            shared.DisposeHelper(evictedValue);
        }

        // 3. Re-check: Did someone beat us to it?
        _cache.TryGet(ownerId, index, out var slotValue);
        slotValue ??= placeholder;

        if (!ReferenceEquals(slotValue, placeholder))
        {
            // Lost the race, wait for winner
            if (slotValue.IsLoading)
            {
                await slotValue.WaitForLoadingCompleteAsync().ConfigureAwait(false);
            }

            return slotValue.ImageModel;
        }

        // 4. Heavy Lift: Load from disk
        try
        {
            // Check cancel before IO
            if (ct.IsCancellationRequested)
            {
                _cache.TryRemove(ownerId, index);
                return null;
            }

            var imageModel = await _imageModelLoader(fileInfo).ConfigureAwait(false);
            slotValue.ImageModel = imageModel;
            return imageModel;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(Preloader2), nameof(AddAsync), ex);
            _cache.TryRemove(ownerId, index); // Clean up failed load
            return null;
        }
        finally
        {
            slotValue.IsLoading = false;
        }
    }
    
    public void Add(string ownerId, int index, ImageModel model, IReadOnlyList<FileInfo> list)
    {
        var evicted = _cache.TryAdd(ownerId, index, new PreLoadValue(model), list.Count, false, out var evictedValue);
        if (evicted && _cache is SharedImageCache shared)
        {
            shared.DisposeHelper(evictedValue);
        }
    }

    public void Resynchronize(string ownerId, IReadOnlyList<FileInfo> files)
    {
        _cache.Resynchronize(ownerId, files);
    }
    
    private async Task PreLoadInternalAsync(string ownerId, int currentIndex, IReadOnlyList<FileInfo> list, bool reversed, CancellationToken token)
    {
        var count = list.Count;
        var nextStartingIndex = (currentIndex + 1) % count;
        var prevStartingIndex = (currentIndex - 1 + count) % count;

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = PreLoaderConfig.MaxParallelism,
            CancellationToken = token
        };

        try
        {
            if (reversed)
            {
                await LoopAsync(options, false);
                await LoopAsync(options, true);
            }
            else
            {
                await LoopAsync(options, true);
                await LoopAsync(options, false);
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(Preloader2), nameof(PreLoadInternalAsync), ex);
        }
        finally
        {
            Interlocked.Exchange(ref _isRunningFlag, 0);
        }
        
        return;


        async Task LoopAsync(ParallelOptions parallelOptions, bool positive)
        {
            if (positive)
            {
                await Parallel.ForAsync(0, PreLoaderConfig.PositiveIterations, parallelOptions,
                    async (i, _) => { await AddAddition((nextStartingIndex + i) % count); });
            }
            else
            {
                await Parallel.ForAsync(0, PreLoaderConfig.NegativeIterations, parallelOptions,
                    async (i, _) => { await AddAddition((prevStartingIndex - i + count) % count); });
            }
        }

        async Task AddAddition(int index)
        {
            token.ThrowIfCancellationRequested();
            // Double check cancellation after waiting
            if (token.IsCancellationRequested)
            {
                return;
            }
        
            if (_cache.TryGet(ownerId, index, out _))
            {
                // Return early if cached
                return;
            }
            await AddAsync(ownerId, index, list, reversed, token);
        }
    }
}