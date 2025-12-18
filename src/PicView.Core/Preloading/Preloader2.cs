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
    
    // One worker per Tab (OwnerId)
    private readonly ConcurrentDictionary<string, PreloadWorker> _workers = new();

    public Preloader2(Func<FileInfo, ValueTask<ImageModel>> imageModelLoader, IImageCache cache)
    {
        _imageModelLoader = imageModelLoader ?? throw new ArgumentNullException(nameof(imageModelLoader));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public void Preload(string ownerId, int currentIndex, bool reversed, IReadOnlyList<FileInfo> files)
    {
        if (files is null || files.Count == 0)
        {
            DebugHelper.LogDebug(nameof(Preloader2), nameof(Preload), "No files to preload");
            return;
        }

        var worker = _workers.GetOrAdd(ownerId, CreateWorker);
        var job = new PreloadJob(currentIndex, reversed, files);
        
        // Non-blocking write
        worker.Writer.TryWrite(job);
    }

    private PreloadWorker CreateWorker(string ownerId)
    {
        // Pass the method that performs the actual heavy lifting
        return new PreloadWorker((job, token) => ExecuteBatchLoadAsync(ownerId, job, token));
    }

    /// <summary>
    /// The core logic for calculating indices and loading them concurrently.
    /// </summary>
    private async Task ExecuteBatchLoadAsync(string ownerId, PreloadJob job, CancellationToken token)
    {
        // 1. Calculate the indices we want to load
        var indicesToLoad = GetLookaheadIndices(job);
        
        // 2. Setup concurrency limits
        using var semaphore = new SemaphoreSlim(PreLoaderConfig.MaxParallelism);
        var tasks = new List<Task>(indicesToLoad.Count);

        foreach (var index in indicesToLoad)
        {
            if (token.IsCancellationRequested) break;

            // 3. Queue the tasks. 
            // Note: We do NOT await here. We add the task to the list.
            // The semaphore inside LoadItemInternal ensures we don't flood the system.
            tasks.Add(LoadItemInternal(ownerId, index, job.Files, job.Reversed, semaphore, token));
        }

        // 4. Wait for this batch to finish (or be cancelled)
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Wraps the AddAsync call with Semaphore throttling.
    /// </summary>
    private async Task LoadItemInternal(string ownerId, int index, IReadOnlyList<FileInfo> list, bool reversed, SemaphoreSlim semaphore, CancellationToken token)
    {
        // Wait for a slot to open up
        await semaphore.WaitAsync(token).ConfigureAwait(false);
        try
        {
            // Double check cancellation after waiting
            if (token.IsCancellationRequested) return;

            await AddAsync(ownerId, index, list, reversed, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(Preloader2), "LoadItemInternal", ex);
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Generates the list of indices to preload based on direction.
    /// </summary>
    private static List<int> GetLookaheadIndices(PreloadJob job)
    {
        var results = new List<int>();
        var count = job.Files.Count;
        var start = job.Index;

        // Helper for wrap-around math
        int Wrap(int i) => (i % count + count) % count;

        void AddForward(int iterations)
        {
            for (int i = 1; i <= iterations; i++) 
                results.Add(Wrap(start + i));
        }

        void AddBackward(int iterations)
        {
            for (int i = 1; i <= iterations; i++) 
                results.Add(Wrap(start - i));
        }

        // Prioritize based on direction (reversed = user going left/up)
        if (job.Reversed)
        {
            AddBackward(PreLoaderConfig.NegativeIterations); // e.g. Look behind 5
            AddForward(PreLoaderConfig.PositiveIterations);  // e.g. Look ahead 2
        }
        else
        {
            AddForward(PreLoaderConfig.PositiveIterations);  // e.g. Look ahead 5
            AddBackward(PreLoaderConfig.NegativeIterations); // e.g. Look behind 2
        }

        return results;
    }

    public void RegisterOwner(string ownerId) => _workers.GetOrAdd(ownerId, CreateWorker);

    public async ValueTask CancelOwnerInstanceAsync(string ownerId)
    {
        if (_workers.TryRemove(ownerId, out var worker))
        {
            await worker.DisposeAsync().ConfigureAwait(false);
        }
    }

    // --- Core Loading Logic (AddAsync) ---

    public async ValueTask<ImageModel?> AddAsync(string ownerId, int index, IReadOnlyList<FileInfo> list,
        bool isReverse = false, CancellationToken ct = default)
    {
        if (list == null || index < 0 || index >= list.Count) return null;

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

        if (ct.IsCancellationRequested) return null;

        // 2. Reserve the slot (Optimistic locking)
        var placeholder = new PreLoadValue(new ImageModel { FileInfo = fileInfo }, isLoading: true);
        
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
            if (slotValue.IsLoading) await slotValue.WaitForLoadingCompleteAsync().ConfigureAwait(false);
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
    
    // --- Unused Interface Implementations ---
    public PreLoadValue? Get(FileInfo file, IReadOnlyList<FileInfo> list) => throw new NotImplementedException();
    public PreLoadValue? Get(string ownerId, int index, IReadOnlyList<FileInfo> list) => throw new NotImplementedException();
    public void Resynchronize(IReadOnlyList<FileInfo> files) => throw new NotImplementedException();
    public void Add(string ownerId, int index, FileInfo file, ImageModel model, IReadOnlyList<FileInfo> list) => throw new NotImplementedException();
}