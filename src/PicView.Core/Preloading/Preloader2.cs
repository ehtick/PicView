using System.Collections.Concurrent;
using System.Diagnostics;
using PicView.Core.DebugTools;
using PicView.Core.Models;
using PicView.Core.Navigation;
using PicView.Core.Navigation.Interfaces;

namespace PicView.Core.Preloading;

/// <summary>
/// The background worker responsible for calculating navigation indices and orchestrating image loading.
/// <para>
/// This class calculates the "look-ahead" indices (forward and backward) based on the current 
/// navigation direction and delegates the physical loading of the <see cref="ImageModel"/> 
/// to the loader. It ensures images are pre-fetched into the cache before the UI requests them.
/// </para>
/// </summary>
public class Preloader2(Func<FileInfo, ValueTask<ImageModel>> imageModelLoader, IImageCache cache) : IPreloader
{
    private readonly IImageCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    
    private CancellationTokenSource? _cancellationTokenSource;

    // Map Owner IDs to their specific Cancellation Tokens
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _loadingTasks = new();
    
    public void Add(string ownerId, int index, FileInfo file, ImageModel model, IReadOnlyList<FileInfo> list)
    {
        var evicted = _cache.TryAdd(ownerId,  index, new PreLoadValue(model), list.Count, false, out var evictedValue);

        if (!evicted)
        {
            return;
        }

        if (_cache is SharedImageCache cache)
        {
            cache.DisposeHelper(evictedValue);
        }
    }

    // Update AddAsync to reuse this logic and avoid circular logic
    public async ValueTask<ImageModel?> AddAsync(string ownerId, int index, IReadOnlyList<FileInfo> list, bool isReverse = false, CancellationToken ct = default)
    {
        if (list == null || index < 0 || index >= list.Count)
        {
            DebugHelper.LogDebug(nameof(Preloader2), nameof(AddAsync), "invalid parameters");
            return null;
        }

        if (_cache.Contains(list[index].FullName, out var cached))
        {
            if (!cached.IsLoading && cached.ImageModel != null)
            {
                return cached.ImageModel;
            }
        }

        // Pre-insert a placeholder marked as loading to avoid duplicate concurrent loads for same index
        var fileInfo = list[index];
        var preLoadValue = new PreLoadValue(new ImageModel { FileInfo = fileInfo }, isLoading: true);
        ct.ThrowIfCancellationRequested();
        var evicted = _cache.TryAdd(ownerId,  index, preLoadValue, list.Count, isReverse, out var evictedValue);

        try
        {
            if (evicted)
            {
                if (_cache is SharedImageCache cache)
                {
                    cache.DisposeHelper(evictedValue);
                }
            }

            var imageModel = await imageModelLoader(fileInfo).ConfigureAwait(false);
            
            ct.ThrowIfCancellationRequested();

            preLoadValue.ImageModel = imageModel;
#if DEBUG
            if (DebugHelper.ShowCacheAdditionsAndRemovals)
            {
                Trace.WriteLine($"{imageModel?.FileInfo?.Name} added at {index}");
            }
#endif
            return imageModel;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(AddAsync), ex);
            return null;
        }
        finally
        {
            preLoadValue?.IsLoading = false; // This will trigger the TaskCompletionSource
        }
    }

    public async ValueTask<ImageModel?> GetOrLoadAsync(string ownerId, int index, IReadOnlyList<FileInfo> list, CancellationToken ct = default)
    {
        if (list == null || index < 0 || index >= list.Count)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(GetOrLoadAsync), "invalid parameters");
            return null;
        }
        
        // Don't re-add at same index
        if (_cache.Contains(list[index].FullName.AsSpan(), out var cached))
        {
            if (!cached.IsLoading)
            {
                return cached.ImageModel;
            }
        }

        await AddAsync(ownerId, index, list, false, ct).ConfigureAwait(false);
        return _cache.Contains(list[index].FullName.AsSpan(), out var newlyAdded) ? newlyAdded.ImageModel : null;
    }
    
    public PreLoadValue? Get(string ownerId, int index, IReadOnlyList<FileInfo> list)
    {
        throw new NotImplementedException();
    }

    public PreLoadValue? Get(FileInfo file, IReadOnlyList<FileInfo> list)
    {
        throw new NotImplementedException();
    }

    public void Resynchronize(IReadOnlyList<FileInfo> files)
    {
        throw new NotImplementedException();
    }

    public async ValueTask CancelOwnerInstanceAsync(string ownerId)
    {
        // 1. Try to remove the token for this specific ID
        if (_loadingTasks.TryRemove(ownerId, out var cts))
        {
            // 2. Cancel it
            await cts.CancelAsync().ConfigureAwait(false);
            
            // 3. Dispose resources
            cts.Dispose();
        }
    }

    public void RegisterOwner(string ownerId)
    {
        _loadingTasks.TryAdd(ownerId, new CancellationTokenSource());
    }

    public async Task PreloadAsync(string ownerId, int currentIndex, bool reversed, IReadOnlyList<FileInfo> files, CancellationToken ct)
    {
        if (files == null)
        {
            return;
        }
        
        if (_cancellationTokenSource is not null)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }
        }
        else
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        try
        {
            await PreLoadInternalAsync(ownerId, currentIndex, reversed, files, _cancellationTokenSource.Token)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            DebugHelper.LogDebug(nameof(Preloader2), nameof(PreloadAsync), exception);
        }
    }
    
    private async Task PreLoadInternalAsync(string ownerId, int currentIndex, bool reverse, IReadOnlyList<FileInfo> list,
        CancellationToken token)
    {
        var count = list.Count;
        var nextStartingIndex = (currentIndex + 1) % count;
        var prevStartingIndex = (currentIndex - 1 + count) % count;
        
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = PreLoaderConfig.MaxParallelism,
            CancellationToken = token
        };


        if (reverse)
        {
            await LoopAsync(false);
            await LoopAsync( true);
        }
        else
        {
            await LoopAsync(true);
            await LoopAsync(false);
        }

        return;


        async Task LoopAsync(bool positive)
        {
            if (positive)
            {
                await Parallel.ForAsync(0, PreLoaderConfig.PositiveIterations, options,
                    async (i, _) => { await AddAddition((nextStartingIndex + i) % count); });
            }
            else
            {
                await Parallel.ForAsync(0, PreLoaderConfig.NegativeIterations, options,
                    async (i, _) => { await AddAddition((prevStartingIndex - i + count) % count); });
            }
        }

        async Task AddAddition(int index)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                await AddAsync(ownerId, index, list, reverse, token);
            }
            catch (Exception e)
            {
                DebugHelper.LogDebug(nameof(PreLoader), nameof(PreLoadInternalAsync), e);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
        }
        // Cancel all running tasks for all tabs
        foreach (var key in _loadingTasks.Keys)
        {
            await CancelOwnerInstanceAsync(key);
        }
        _loadingTasks.Clear();
        _cancellationTokenSource?.Dispose();
    }
    
    // Non-Async Dispose
    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        foreach (var cts in _loadingTasks.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _loadingTasks.Clear();
        _cancellationTokenSource?.Dispose();
    }
}
