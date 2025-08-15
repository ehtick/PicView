using System.Collections.Concurrent;
using System.Diagnostics;
using PicView.Core.DebugTools;
using PicView.Core.Models;
using ZLinq;
using static System.GC;

namespace PicView.Core.Preloading;

/// <summary>
/// The <see cref="PreLoader"/> class is responsible for asynchronously preloading images
/// and caching them for efficient retrieval. It provides methods to add, remove, refresh,
/// and resynchronize preloaded images, manage cache size, and handle asynchronous disposal.
/// </summary>
public class PreLoader(Func<FileInfo, Task<ImageModel>> imageModelLoader) : IAsyncDisposable
{
#if DEBUG
    // ReSharper disable once ConvertToConstant.Local
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static bool _showAddRemove = false;
#endif

    private readonly Lock _disposeLock = new();

    private readonly ConcurrentDictionary<int, PreLoadValue> _preLoadList = new();

    private CancellationTokenSource? _cancellationTokenSource;

    private int _isRunningFlag; // 0 = idle, 1 = running

    #region Add

    /// <summary>
    ///     Adds an image to the preload list asynchronously.
    ///     If the image already exists and is loaded, the operation is skipped.
    ///     On success, the image is loaded and cached for future retrieval.
    /// </summary>
    /// <param name="index">The index of the image in the list.</param>
    /// <param name="list">The list of image file paths.</param>
    /// <returns>
    ///     A task representing the asynchronous operation. 
    ///     Returns <c>true</c> if the image was added and loaded successfully; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> AddAsync(int index, List<FileInfo> list, CancellationToken ct = default)
    {
        if (list == null || index < 0 || index >= list.Count)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(AddAsync), "invalid parameters");
            return false;
        }

        if (_preLoadList.TryGetValue(index, out var existing))
        {
            if (existing.ImageModel?.Image is not null)
            {
                return false;
            }
        }

        // Pre-insert a placeholder marked as loading to avoid duplicate concurrent loads for same index
        var fileInfo = list[index];
        var preLoadValue = new PreLoadValue(new ImageModel { FileInfo = fileInfo }, isLoading: true);

        if (!_preLoadList.TryAdd(index, preLoadValue))
        {
            return false;
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            var imageModel = await imageModelLoader(fileInfo).ConfigureAwait(false);
            
            ct.ThrowIfCancellationRequested();

            preLoadValue.ImageModel = imageModel;
#if DEBUG
            if (_showAddRemove)
            {
                Trace.WriteLine($"{imageModel?.FileInfo?.Name} added at {index}");
            }
#endif
            return true;
        }
        catch (Exception ex)
        {
            _preLoadList.TryRemove(index, out _); // Remove failed entry
            DebugHelper.LogDebug(nameof(PreLoader), nameof(AddAsync), ex);
            return false;
        }
        finally
        {
            preLoadValue.IsLoading = false; // This will trigger the TaskCompletionSource
        }
    }

    /// <summary>
    /// Adds a preloaded image model to the preload list at the specified index.
    /// Does not perform loading, only inserts an existing model.
    /// </summary>
    /// <param name="index">The index at which to add the image model.</param>
    /// <param name="list">The list of image file paths corresponding to the preload list.</param>
    /// <param name="imageModel">The image model to preload.</param>
    /// <returns>
    ///     <c>true</c> if the image model was successfully added to the preload list; otherwise, <c>false</c>.
    /// </returns>
    public bool Add(int index, List<FileInfo> list, ImageModel imageModel)
    {
        if (list == null || index < 0 || index >= list.Count)
        {
#if DEBUG
            Trace.WriteLine($"{nameof(PreLoader)}.{nameof(Add)} invalid parameters: \n{index}");
#endif
            return false;
        }

        if (imageModel is null)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(AddAsync), "ImageModel is null");
            return false;
        }

        var preLoadValue = new PreLoadValue(imageModel);
        return _preLoadList.TryAdd(index, preLoadValue);
    }

    #endregion

    #region Refresh and resynchronize

    /// <summary>
    /// Updates the <see cref="FileInfo"/> associated with a specific index in the preload list.
    /// Useful if the file information has changed due to file operations.
    /// </summary>
    /// <param name="index">The index of the item to update.</param>
    /// <param name="fileInfo">The new file information to assign.</param>
    /// <param name="list">The list of file paths.</param>
    public void RefreshFileInfo(int index, FileInfo fileInfo, List<FileInfo> list)
    {
        if (list == null)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(RefreshFileInfo), "list null, index: " + index);
            return;
        }

        if (index < 0 || index >= list.Count)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(RefreshFileInfo), "invalid index: " + index);
            return;
        }

        var isExisting = _preLoadList.TryGetValue(index, out var value);
        if (!isExisting)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(RefreshFileInfo), "index not found: " + index);
            return;
        }

        value.ImageModel.FileInfo = fileInfo;
    }

    /// <summary>
    ///     Resynchronizes the preload list with the given list of image file paths.
    ///     Moves or removes entries as needed to match the new ordering or contents.
    /// </summary>
    /// <remarks>
    ///     Call this method after the file watcher detects changes, or the list is resorted.
    /// </remarks>
    public void Resynchronize(List<FileInfo> list)
    {
        if (list == null)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(Resynchronize), "list is null");
            return;
        }

        if (list.Count == 0 || _preLoadList.IsEmpty)
        {
            return;
        }

        _cancellationTokenSource?.Cancel();

        // Create a reverse lookup from file path to current index
        var reverseLookup = new Dictionary<string, int>(list.Count);
        
        for (var i = 0; i < list.Count; i++)
        {
            reverseLookup[list[i].FullName] = i;
        }

        // Snapshot of current keys to avoid modification during iteration
        var keys = _preLoadList.Keys.ToArray();

        foreach (var oldIndex in keys)
        {
            if (!_preLoadList.TryGetValue(oldIndex, out var preLoadValue))
            {
                continue;
            }

            var file = preLoadValue.ImageModel?.FileInfo;
            if (file is null)
            {
                Remove(oldIndex, list);
                continue;
            }

            if (!reverseLookup.TryGetValue(file.FullName, out var newIndex))
            {
                // File no longer exists in the list
                Remove(oldIndex, list);
                continue;
            }

            if (newIndex == oldIndex)
            {
                // Index is unchanged, no action needed
                continue;
            }

            if (newIndex < 0 || newIndex >= list.Count)
            {
                // Invalid new index, remove the entry
                Remove(oldIndex, list);
                continue;
            }

            // Attempt to move the entry to the new index
            if (!_preLoadList.TryRemove(oldIndex, out var removedValue))
            {
                continue;
            }

            if (!_preLoadList.TryAdd(newIndex, removedValue))
            {
#if DEBUG
                if (_showAddRemove)
                {
                    Trace.WriteLine($"Failed to resynchronize {file} to index {newIndex}");
                }
#endif
            }
#if DEBUG
            else if (_showAddRemove)
            {
                Trace.WriteLine($"Resynchronized {file} from index {oldIndex} to {newIndex}");
            }
#endif
        }
    }

    #endregion

    #region Get and Contains

    /// <summary>
    ///     Checks if a specific key exists in the preload list.
    /// </summary>
    /// <param name="key">The key (index) to check.</param>
    /// <param name="list">The list of image file paths.</param>
    /// <returns>
    ///     <c>true</c> if the key exists in the preload list and is a valid index in <paramref name="list"/>; otherwise, <c>false</c>.
    /// </returns>
    public bool Contains(int key, List<FileInfo> list) =>
        list != null && key >= 0 && key < list.Count && _preLoadList.ContainsKey(key);

    /// <summary>
    ///     Gets the preloaded value for a specific key (index).
    /// </summary>
    /// <param name="key">The key (index) of the preloaded value.</param>
    /// <param name="list">The list of image file paths.</param>
    /// <returns>
    ///     The <see cref="PreLoadValue"/> if it exists; otherwise, <c>null</c>.
    /// </returns>
    public PreLoadValue? Get(int key, List<FileInfo> list)
    {
        if (list != null && key >= 0 && key < list.Count
            && _preLoadList.TryGetValue(key, out var value))
        {
            return value;
        }

        DebugHelper.LogDebug(nameof(PreLoader), nameof(Get), "invalid parameters:" + key);
        return null;
    }


    /// <summary>
    ///     Gets the preloaded value for a specific file. Should only be used when resynchronizing.
    /// </summary>
    /// <param name="file">The <see cref="FileInfo"/> of the image file to retrieve the preloaded value for.</param>
    /// <param name="list">The list of image file paths.</param>
    /// <returns>
    ///     The <see cref="PreLoadValue"/> if it exists; otherwise, <c>null</c>.
    /// </returns>
    public PreLoadValue? Get(FileInfo file, List<FileInfo> list)
    {
        if (list == null || file is null)
        {
            return null;
        }

        var index = list.FindIndex(x => x.FullName == file.FullName);
        if (index > -1 && index < list.Count)
        {
            return _preLoadList.GetValueOrDefault(index);
        }
        return null;
    }


    /// <summary>
    /// Retrieves a preloaded image value for the specified index, or loads it asynchronously if not already loaded.
    /// </summary>
    /// <param name="key">The index of the image in the list.</param>
    /// <param name="list">The list of image file paths.</param>
    /// <returns>
    ///     The <see cref="PreLoadValue"/> if found or successfully loaded; otherwise, <c>null</c>.
    /// </returns>
    public async Task<PreLoadValue?> GetOrLoadAsync(int key, List<FileInfo> list)
    {
        if (list == null || key < 0 || key >= list.Count)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(GetOrLoadAsync), "invalid parameters: " + key);
            return null;
        }

        if (Contains(key, list))
        {
            return _preLoadList[key];
        }

        var isAdded = await AddAsync(key, list);
        if (!isAdded)
        {
            return null;
        }

        return key >= list.Count ? null : _preLoadList[key];
    }

    /// <summary>
    ///     Gets the preloaded value for a specific file asynchronously. Should only be used when resynchronizing.
    /// </summary>
    /// <param name="fileInfo">The <see cref="FileInfo"/> of the image file to retrieve the preloaded value for.</param>
    /// <param name="list">The list of image file paths.</param>
    /// <returns>
    ///     The <see cref="PreLoadValue"/> if it exists; otherwise, <c>null</c>.
    /// </returns>
    public async Task<PreLoadValue?> GetOrLoadAsync(FileInfo fileInfo, List<FileInfo> list)
    {
        if (list == null || fileInfo == null) return null;

        // Find the entry without creating a new list
        var entry = _preLoadList.
            AsValueEnumerable()
            .FirstOrDefault(kvp => kvp.Value.ImageModel?.FileInfo?.FullName == fileInfo.FullName);

        if (entry.Value != null)
        {
            return await GetOrLoadAsync(entry.Key, list);
        }

        // If not found in cache, find its index in the master list and load
        var index = list.FindIndex(f => f.FullName == fileInfo.FullName);
        if (index != -1)
        {
            return await GetOrLoadAsync(index, list);
        }

        return null;
    }
    
    public async Task<PreLoadValue?> GetOrLoadAsync(int key, List<FileInfo> list, CancellationToken ct)
    {
        if (list == null || key < 0 || key >= list.Count)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(GetOrLoadAsync), "invalid parameters: " + key);
            return null;
        }

        if (_preLoadList.TryGetValue(key, out var existing))
        {
            return existing;
        }

        var isAdded = await AddAsync(key, list, ct).ConfigureAwait(false);
        return !isAdded ? null : _preLoadList.GetValueOrDefault(key);
    }


    #endregion

    #region Remove and clear

    /// <summary>
    ///     Removes a specific key (index) from the preload list.
    ///     Disposes the associated image if necessary.
    /// </summary>
    /// <param name="key">The key (index) to remove.</param>
    /// <param name="list">The list of image file paths.</param>
    /// <returns>
    ///     <c>true</c> if the key was removed; otherwise, <c>false</c>.
    /// </returns>
    public bool Remove(int key, List<FileInfo> list)
    {
        if (list == null || key < 0 || key >= list.Count)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(Remove), "invalid parameters: " + key);
            return false;
        }

        if (!Contains(key, list))
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(Remove), "key does not exist: " + key);
            return false;
        }

        try
        {
            if (_preLoadList.TryGetValue(key, out var item))
            {
                var removed = _preLoadList.TryRemove(key, out _);
                if (item.ImageModel.Image is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                item.ImageModel.Image = null;
#if DEBUG
                if (!removed || !_showAddRemove)
                {
                    return removed;
                }

                var name = item.ImageModel?.FileInfo?.Name ?? "Unknown";
                Trace.WriteLine($"{name} removed at {key}");
#endif
                return removed;
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(Remove), e);
        }

        return false;
    }


    /// <summary>
    /// Removes an image from the preload list by file name.
    /// </summary>
    /// <param name="fileName">The full file name of the image to remove.</param>
    /// <param name="list">The list of image file paths.</param>
    /// <returns>
    ///     <c>true</c> if the image was successfully removed; otherwise, <c>false</c>.
    /// </returns>
    public bool Remove(string fileName, List<FileInfo> list)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        // Iterate the dictionary directly to find the matching entry
        return (from kvp in _preLoadList where kvp.Value.ImageModel?.FileInfo?.FullName == fileName 
            select Remove(kvp.Key, list))
            .AsValueEnumerable()
            .FirstOrDefault();
    }

    /// <summary>
    /// Clears all preloaded images and associated resources.
    /// Cancels any ongoing operations, disposes resources such as image bitmaps,
    /// and clears the internal preload list. It logs a debug message when running in DEBUG mode.
    /// </summary>
    public void Clear()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        foreach (var item in _preLoadList.Values)
        {
            lock (_disposeLock)
            {
                if (item.ImageModel?.Image is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        _preLoadList.Clear();

#if DEBUG
        Trace.WriteLine("Preloader cleared");
#endif
    }

    /// <summary>
    /// Clears all preloaded images asynchronously, canceling and disposing any active operations.
    /// </summary>      
    public async Task ClearAsync()
    {
        try
        {
            if (_cancellationTokenSource is not null)
            {
                await _cancellationTokenSource?.CancelAsync();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(ClearAsync), e);
        }

        Clear();
    }

    #endregion

    #region Preload
    
    /// <summary>
    ///     Preloads images asynchronously.
    /// </summary>
    /// <param name="currentIndex">The current index of the image.</param>
    /// <param name="reverse">Indicates whether to preload in reverse order.</param>
    /// <param name="list">The list of image paths.</param>
    public async Task PreLoadAsync(int currentIndex, bool reverse, List<FileInfo> list)
    {
        if (list == null)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(PreLoadAsync), $"list null \n{currentIndex}");
            return;
        }

        if (Interlocked.CompareExchange(ref _isRunningFlag, 1, 0) != 0)
        {
            return; // Already running
        }

#if DEBUG
        if (_showAddRemove)
        {
            var direction = reverse ? "backwards" : "forwards";
            Trace.WriteLine($"\nPreLoading started {direction} at {currentIndex}\n");
        }
#endif

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
            await PreLoadInternalAsync(currentIndex, reverse, list, _cancellationTokenSource.Token)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(PreLoadAsync), exception);
        }
        finally
        {
            Interlocked.Exchange(ref _isRunningFlag, 0);
        }
    }

    
    /// <summary>
    /// Performs the internal logic for preloading images asynchronously.
    /// Loads images ahead and/or behind the current index, manages the cache size,
    /// and removes excess entries outside the configured range.
    /// </summary>
    /// <param name="currentIndex">The current index for preloading.</param>
    /// <param name="reverse">Whether to preload in reverse order.</param>
    /// <param name="list">The list of image file paths.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to observe while waiting for tasks to complete.</param>
    private async Task PreLoadInternalAsync(int currentIndex, bool reverse, List<FileInfo> list,
        CancellationToken token)
    {
        var count = list.Count;
        var nextStartingIndex = (currentIndex + 1) % count;
        var prevStartingIndex = (currentIndex - 1 + count) % count;
        var isPreloadListUnderMax = _preLoadList.Count < PreLoaderConfig.MaxCount;
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = PreLoaderConfig.MaxParallelism,
            CancellationToken = token
        };

        var additions = new List<int>();

        if (reverse)
        {
            await LoopAsync(options, false);
            await LoopAsync(options, true);
        }
        else
        {
            await LoopAsync(options, true);
            await LoopAsync(options, false);
        }

        if (!isPreloadListUnderMax)
        {
            RemoveLoop();
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
            if (await AddAsync(index, list, token))
            {
                additions.Add(index);
            }
        }

        void RemoveLoop()
        {
            if (_preLoadList.Count < PreLoaderConfig.MaxCount)
            {
                return;
            }

            // Remove keys that are too far from the current index
            var keysToRemove = 
                (from key in _preLoadList.Keys let distance = Math.Min(Math.Abs(key - currentIndex), list.Count - Math.Abs(key - currentIndex)) 
                    where distance > PreLoaderConfig.PositiveIterations && distance > PreLoaderConfig.NegativeIterations
                    where !additions.Contains(key) select key)
                .AsValueEnumerable();

            foreach (var key in keysToRemove)
            {
                Remove(key, list);
            }
        }
    }

    #endregion

    #region IDisposable

    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Clear();
        }

        _disposed = true;

#if DEBUG
        if (_showAddRemove)
        {
            Trace.WriteLine("Preloader disposed");
        }
#endif
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await _cancellationTokenSource?.CancelAsync();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        await ClearAsync().ConfigureAwait(false);
        Dispose(false);
        SuppressFinalize(this);
    }

    ~PreLoader()
    {
        Dispose(false);
    }

    #endregion
}