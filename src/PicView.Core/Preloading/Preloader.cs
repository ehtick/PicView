using System.Collections.Concurrent;
using System.Diagnostics;
using ImageMagick;
using PicView.Core.DebugTools;
using PicView.Core.Models;
using static System.GC;

namespace PicView.Core.Preloading;

/// <summary>
/// The PreLoader class is responsible for preloading images asynchronously and caching them.
/// </summary>
public class PreLoader(Func<FileInfo, MagickImage, Task<ImageModel>> imageModelLoader) : IAsyncDisposable
{
#if DEBUG
    // ReSharper disable once ConvertToConstant.Local
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static bool _showAddRemove = true;
#endif

    private readonly PreLoaderConfig _config = new();

    private readonly Lock _disposeLock = new();

    private readonly ConcurrentDictionary<int, PreLoadValue> _preLoadList = new();

    private CancellationTokenSource? _cancellationTokenSource;

    private int _isRunningFlag; // 0 = idle, 1 = running

    /// <summary>
    ///     Gets the maximum count of preloaded images.
    /// </summary>
    public static int MaxCount => PreLoaderConfig.MaxCount;

    /// <summary>
    ///     Checks if a specific key exists in the preload list.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="list">The list of image paths.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    public bool Contains(int key, List<string> list) =>
        list != null && key >= 0 && key < list.Count && _preLoadList.ContainsKey(key);

    #region Add

    /// <summary>
    ///     Adds an image to the preload list asynchronously.
    /// </summary>
    /// <param name="index">The index of the image in the list.</param>
    /// <param name="list">The list of image paths.</param>
    /// <returns>True if the image was added successfully; otherwise, false.</returns>
    public async Task<bool> AddAsync(int index, List<string> list)
    {
        if (list == null || index < 0 || index >= list.Count)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(AddAsync), "invalid parameters");
            return false;
        }

        if (_preLoadList.ContainsKey(index))
        {
            return false;
        }

        var imageModel = new ImageModel();
        var preLoadValue = new PreLoadValue(imageModel, true); // Set isLoading to true

        if (!_preLoadList.TryAdd(index, preLoadValue))
        {
            return false;
        }

        try
        {
            var fileInfo = imageModel.FileInfo = new FileInfo(list[index]);
            imageModel = await imageModelLoader(fileInfo, null!).ConfigureAwait(false);
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

    public bool Add(int index, List<string> list, ImageModel imageModel)
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

    public void RefreshFileInfo(int index, FileInfo fileInfo, List<string> list)
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
    ///     Resynchronizes the preload list with the given list of image paths.
    /// </summary>
    /// <param name="list">The list of image paths.</param>
    /// <remarks>
    ///     Call it after the file watcher detects changes, or the list is resorted
    /// </remarks>
    public void Resynchronize(List<string> list)
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
            reverseLookup[list[i]] = i;
        }

        // Snapshot of current keys to avoid modification during iteration
        var keys = _preLoadList.Keys.ToArray();

        foreach (var oldIndex in keys)
        {
            if (!_preLoadList.TryGetValue(oldIndex, out var preLoadValue))
            {
                continue;
            }

            var filePath = preLoadValue.ImageModel?.FileInfo?.FullName;
            if (string.IsNullOrEmpty(filePath))
            {
                Remove(oldIndex, list);
                continue;
            }

            if (!reverseLookup.TryGetValue(filePath, out var newIndex))
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
                    Trace.WriteLine($"Failed to resynchronize {filePath} to index {newIndex}");
                }
#endif
            }
#if DEBUG
            else if (_showAddRemove)
            {
                Trace.WriteLine($"Resynchronized {filePath} from index {oldIndex} to {newIndex}");
            }
#endif
        }
    }

    #endregion

    #region Get

    /// <summary>
    ///     Gets the preloaded value for a specific key.
    /// </summary>
    /// <param name="key">The key of the preloaded value.</param>
    /// <param name="list">The list of image paths.</param>
    /// <returns>The preloaded value if it exists; otherwise, null.</returns>
    public PreLoadValue? Get(int key, List<string> list)
    {
        if (list != null && key >= 0 && key < list.Count)
        {
            return Contains(key, list) ? _preLoadList[key] : null;
        }

        DebugHelper.LogDebug(nameof(PreLoader), nameof(Get), "invalid parameters:" + key);
        return null;
    }

    /// <summary>
    ///     Gets the preloaded value for a specific file name. Should only be used when resynchronizing.
    /// </summary>
    /// <param name="fileName">The full path of the image file to retrieve the preloaded value for.</param>
    /// <param name="list">The list of image paths.</param>
    /// <returns>The preloaded value if it exists; otherwise, null.</returns>
    public PreLoadValue? Get(string fileName, List<string> list)
    {
        if (list == null || string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var index = list.IndexOf(fileName);
        return index >= 0 ? _preLoadList[index] : null;
    }


    /// <summary>
    /// Retrieves a preloaded image value or loads it asynchronously if not already loaded.
    /// </summary>
    /// <param name="key">The index of the image in the list.</param>
    /// <param name="list">The list of image paths.</param>
    /// <returns>The preloaded image value if found or successfully loaded; otherwise, null.</returns>
    public async Task<PreLoadValue?> GetOrLoadAsync(int key, List<string> list)
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

        await AddAsync(key, list);
        return _preLoadList[key];
    }

    /// <summary>
    ///     Gets the preloaded value for a specific file name asynchronously. Should only be used when resynchronizing.
    /// </summary>
    /// <param name="fileName">The full path of the image file to retrieve the preloaded value for.</param>
    /// <param name="list">The list of image paths.</param>
    /// <returns>The preloaded value if it exists; otherwise, null.</returns>
    public async Task<PreLoadValue?> GetOrLoadAsync(string fileName, List<string> list) =>
        await GetOrLoadAsync(_preLoadList.Values.ToList().FindIndex(x => x.ImageModel?.FileInfo?.FullName == fileName),
            list);

    #endregion

    #region Remove and clear

    /// <summary>
    ///     Removes a specific key from the preload list.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <param name="list">The list of image paths.</param>
    /// <returns>True if the key was removed; otherwise, false.</returns>
    public bool Remove(int key, List<string> list)
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
    /// Removes an image from the preload list.
    /// </summary>
    /// <param name="fileName">The full path of the image to remove.</param>
    /// <param name="list">The list of image paths.</param>
    /// <returns>True if the image was successfully removed; otherwise, false.</returns>
    public bool Remove(string fileName, List<string> list)
    {
        var index = _preLoadList.Values.ToList().FindIndex(x => x.ImageModel.FileInfo.FullName == fileName);
        return Remove(index, list);
    }

    /// <summary>
    /// Clears all preloaded images and associated resources.
    /// </summary>
    /// <remarks>
    /// This method cancels any ongoing operations, disposes resources such as image bitmaps,
    /// and clears the internal preload list. It logs a debug message when running in DEBUG mode.
    /// </remarks>
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
        if (_showAddRemove)
        {
            Trace.WriteLine("Preloader cleared");
        }
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
            DebugHelper.LogDebug(nameof(PreLoader), nameof(ClearAsync),  e);
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
    public async Task PreLoadAsync(int currentIndex, bool reverse, List<string> list)
    {
        if (list == null)
        {
            DebugHelper.LogDebug(nameof(PreLoader), nameof(PreLoadAsync),  $"list null \n{currentIndex}");
            return;
        }

        if (Interlocked.CompareExchange(ref _isRunningFlag, 1, 0) != 0)
        {
            return; // Already running
        }

#if DEBUG
        if (_showAddRemove)
        {
            Trace.WriteLine($"\nPreLoading started at {currentIndex}\n");
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

    private async Task PreLoadInternalAsync(int currentIndex, bool reverse, List<string> list,
        CancellationToken token)
    {
        var count = list.Count;

        int nextStartingIndex, prevStartingIndex;
        if (reverse)
        {
            nextStartingIndex = (currentIndex - 1 + count) % count;
            prevStartingIndex = currentIndex + 1;
        }
        else
        {
            nextStartingIndex = (currentIndex + 1) % count;
            prevStartingIndex = currentIndex - 1;
        }

        var isPreloadListUnderMax = _preLoadList.Count < PreLoaderConfig.MaxCount;
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = _config.MaxParallelism,
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
            await Parallel.ForAsync(0, PreLoaderConfig.PositiveIterations, parallelOptions, async (i, _) =>
            {
                token.ThrowIfCancellationRequested();
                var index = positive ? (nextStartingIndex + i) % count : (prevStartingIndex - i + count) % count;
                if (await AddAsync(index, list))
                {
                    additions.Add(index);
                }
            });
        }

        void RemoveLoop()
        {
            // Remove items outside the preload range
            if (list.Count <= PreLoaderConfig.MaxCount + PreLoaderConfig.NegativeIterations ||
                _preLoadList.Count <= PreLoaderConfig.MaxCount)
            {
                return;
            }

            var keysToRemove = _preLoadList.Keys
                .OrderByDescending(k => Math.Abs(k - currentIndex))
                .Take(_preLoadList.Count - PreLoaderConfig.MaxCount);

            foreach (var key in keysToRemove)
            {
                if (!additions.Contains(key))
                {
                    Remove(key, list);
                }
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