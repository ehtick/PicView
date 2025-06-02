using System.Diagnostics;
using Avalonia.Threading;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Input;
using PicView.Avalonia.Preloading;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.Gallery;
using PicView.Core.Navigation;
using Timer = System.Timers.Timer;

namespace PicView.Avalonia.Navigation;

public class ImageIterator : IAsyncDisposable
{
    #region Properties

    private bool _disposed;

    public List<string> ImagePaths { get; private set; }
    public int CurrentIndex { get; private set; }

    public int GetNonZeroIndex => CurrentIndex + 1 > GetCount ? 1 : CurrentIndex + 1;

    public int NextIndex => GetIteration(CurrentIndex, NavigateTo.Next);

    public int GetCount => ImagePaths.Count;

    public FileInfo InitialFileInfo { get; private set; } = null!;
    public bool IsReversed { get; private set; }
    private PreLoader PreLoader { get; } = new();

    private static FileSystemWatcher? _watcher;

    private bool _isRunning;

    private readonly MainViewModel? _vm;

    #endregion

    #region Constructors

    public ImageIterator(FileInfo fileInfo, MainViewModel vm)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(fileInfo);
#endif
        _vm = vm;
        ImagePaths = vm.PlatformService.GetFiles(fileInfo);
        CurrentIndex = Directory.Exists(fileInfo.FullName) ? 0 : ImagePaths.IndexOf(fileInfo.FullName);
        InitiateFileSystemWatcher(fileInfo);
    }

    public ImageIterator(FileInfo fileInfo, List<string> imagePaths, int currentIndex, MainViewModel vm)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(fileInfo);
#endif
        _vm = vm;
        ImagePaths = imagePaths;
        CurrentIndex = currentIndex;
        InitiateFileSystemWatcher(fileInfo);
    }

    #endregion

    #region File Watcher

    private void InitiateFileSystemWatcher(FileInfo fileInfo)
    {
        InitialFileInfo = fileInfo;
        if (_watcher is not null)
        {
            _watcher.Dispose();
            _watcher = null;
        }

        _watcher?.Dispose();
        
        if (!Settings.Navigation.IsFileWatcherEnabled)
        {
            return;
        }
        _watcher = new FileSystemWatcher(fileInfo.DirectoryName!)
        {
            EnableRaisingEvents = true,
            Filter = "*.*",
            IncludeSubdirectories = Settings.Sorting.IncludeSubDirectories,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        _watcher.Created += (_, e) =>
        {
            if (!e.FullPath.IsSupported() || !Settings.Navigation.IsFileWatcherEnabled)
            {
                return; // Early exit
            }

            if (_vm.IsEditableTitlebarOpen)
            {
                // Don't react to changes when renaming
                return;
            }

            Task.Run(() => OnFileAdded(e)).ContinueWith(t =>
            {
                if (t.Exception == null)
                {
                    return;
                }

                DebugHelper.LogDebug(nameof(ImageIterator), nameof(OnFileAdded), t.Exception);
            });
        };
        _watcher.Deleted += (_, e) =>
        {
            if (!e.FullPath.IsSupported() || !Settings.Navigation.IsFileWatcherEnabled)
            {
                return; // Early exit
            }

            if (_vm.IsEditableTitlebarOpen)
            {
                // Don't react to changes when renaming
                return;
            }

            Task.Run(() => OnFileDeleted(e)).ContinueWith(t =>
            {
                if (t.Exception == null)
                {
                    return;
                }

                DebugHelper.LogDebug(nameof(ImageIterator), nameof(OnFileDeleted), t.Exception);
            });
        };
        _watcher.Renamed += (_, e) =>
        {
            if (!e.FullPath.IsSupported() || !Settings.Navigation.IsFileWatcherEnabled)
            {
                return; // Early exit
            }

            if (_vm.IsEditableTitlebarOpen)
            {
                // Don't react to changes when renaming
                return;
            }

            Task.Run(() => OnFileRenamed(e)).ContinueWith(t =>
            {
                if (t.Exception == null)
                {
                    return;
                }

                DebugHelper.LogDebug(nameof(ImageIterator), nameof(OnFileRenamed), t.Exception);
            });
        };
    }

    private async Task OnFileAdded(FileSystemEventArgs e)
    {
        try
        {
            var fileInfo = new FileInfo(e.FullPath);
            if (fileInfo.Exists == false)
            {
                return;
            }

            var sourceFileInfo = Settings.Sorting.IncludeSubDirectories
                ? new FileInfo(_watcher.Path)
                : fileInfo;

            var newList = await Task.FromResult(_vm.PlatformService.GetFiles(sourceFileInfo));
            if (newList.Count == 0)
            {
                return;
            }

            ImagePaths = newList;
            _isRunning = true;

            TitleManager.SetTitle(_vm);

            var index = ImagePaths.IndexOf(e.FullPath);
            if (index < 0)
            {
                PreLoader.Resynchronize(ImagePaths);
                _isRunning = false;
                return;
            }

            var isGalleryItemAdded = await GalleryFunctions.AddGalleryItem(index, fileInfo, _vm);
            if (isGalleryItemAdded)
            {
                if (Settings.Gallery.IsBottomGalleryShown && ImagePaths.Count > 1)
                {
                    if (_vm.GalleryMode is GalleryMode.BottomToClosed or GalleryMode.FullToClosed)
                    {
                        _vm.GalleryMode = GalleryMode.ClosedToBottom;
                    }
                }

                GalleryNavigation.CenterScrollToSelectedItem(_vm);
            }

            PreLoader.Resynchronize(ImagePaths);
        }
        catch (Exception exception)
        {
#if DEBUG
            Console.WriteLine(
                $"{nameof(ImageIterator)}.{nameof(OnFileAdded)} {exception.Message} \n{exception.StackTrace}");
#endif
        }
        finally
        {
            _isRunning = false;
        }
    }

    private async Task OnFileDeleted(FileSystemEventArgs e)
    {
        try
        {
            if (ImagePaths.Contains(e.FullPath) == false)
            {
                return;
            }

            _isRunning = true;

            var index = ImagePaths.IndexOf(e.FullPath);
            if (index < 0)
            {
                return;
            }

            var currentIndex = CurrentIndex;
            var isSameFile = currentIndex == index;

            if (!ImagePaths.Remove(e.FullPath))
            {
#if DEBUG
                Console.WriteLine($"Failed to remove {e.FullPath}");
#endif
                return;
            }

            if (isSameFile)
            {
                if (ImagePaths.Count <= 0)
                {
                    ErrorHandling.ShowStartUpMenu(_vm);
                    return;
                }

                RemoveCurrentItemFromPreLoader();
                PreLoader.Resynchronize(ImagePaths);
                var newIndex = GetIteration(index, NavigateTo.Previous);
                CurrentIndex = newIndex;
                _vm.PicViewer.FileInfo = new FileInfo(ImagePaths[CurrentIndex]);
                await IterateToIndex(CurrentIndex, new CancellationTokenSource());
            }
            else
            {
                RemoveItemFromPreLoader(index);
                TitleManager.SetTitle(_vm);
            }

            var removed = GalleryFunctions.RemoveGalleryItem(index, _vm);
            if (removed)
            {
                if (Settings.Gallery.IsBottomGalleryShown)
                {
                    if (ImagePaths.Count == 1)
                    {
                        _vm.GalleryMode = GalleryMode.BottomToClosed;
                    }
                }

                var indexOf = ImagePaths.IndexOf(_vm.PicViewer.FileInfo.FullName);
                _vm.SelectedGalleryItemIndex = indexOf; // Fixes deselection bug 
                CurrentIndex = indexOf;
                if (isSameFile)
                {
                    GalleryNavigation.CenterScrollToSelectedItem(_vm);
                }
            }

            if (!isSameFile)
            {
                PreLoader.Resynchronize(ImagePaths);
            }

            FileHistoryManager.Remove(e.FullPath);
        }
        catch (Exception exception)
        {
#if DEBUG
            Console.WriteLine(
                $"{nameof(ImageIterator)}.{nameof(OnFileDeleted)} {exception.Message} \n{exception.StackTrace}");
#endif
        }
        finally
        {
            _isRunning = false;
        }
    }

    private async Task OnFileRenamed(RenamedEventArgs e)
    {
        try
        {
            if (e.FullPath.IsSupported() == false)
            {
                if (ImagePaths.Contains(e.OldFullPath))
                {
                    ImagePaths.Remove(e.OldFullPath);
                }

                return;
            }

            _isRunning = true;

            var oldIndex = ImagePaths.IndexOf(e.OldFullPath);
            var currentIndex = CurrentIndex;
            var sameFile = currentIndex == oldIndex;
            var fileInfo = new FileInfo(e.FullPath);
            if (fileInfo.Exists == false)
            {
                return;
            }

            var sourceFileInfo = Settings.Sorting.IncludeSubDirectories
                ? new FileInfo(_watcher.Path)
                : fileInfo;
            var newList = FileListHelper.RetrieveFiles(sourceFileInfo).ToList();
            if (newList.Count == 0)
            {
                return;
            }

            if (fileInfo.Exists == false)
            {
                return;
            }

            ImagePaths = newList;

            var index = ImagePaths.IndexOf(e.FullPath);
            if (index < 0)
            {
                return;
            }

            if (fileInfo.Exists == false)
            {
                return;
            }

            if (sameFile)
            {
                _vm.PicViewer.FileInfo = fileInfo;
            }

            TitleManager.SetTitle(_vm);
            PreLoader.RefreshFileInfo(oldIndex, fileInfo, ImagePaths);
            Resynchronize();

            _isRunning = false;
            FileHistoryManager.Rename(e.OldFullPath, e.FullPath);
            await Dispatcher.UIThread.InvokeAsync(() =>
                GalleryFunctions.RenameGalleryItem(oldIndex, index, Path.GetFileNameWithoutExtension(e.Name),
                    e.FullPath,
                    _vm));
            if (sameFile)
            {
                _vm.SelectedGalleryItemIndex = index;
                GalleryFunctions.CenterGallery(_vm);
            }
        }
        catch (Exception exception)
        {
#if DEBUG
            Console.WriteLine(
                $"{nameof(ImageIterator)}.{nameof(OnFileRenamed)} {exception.Message} \n{exception.StackTrace}");
#endif
        }
        finally
        {
            _isRunning = false;
        }
    }

    #endregion

    #region Preloader

    public async Task ClearAsync() =>
        await PreLoader.ClearAsync().ConfigureAwait(false);

    public async Task PreloadAsync() =>
        await PreLoader.PreLoadAsync(CurrentIndex, IsReversed, ImagePaths).ConfigureAwait(false);

    public async Task AddAsync(int index) =>
        await PreLoader.AddAsync(index, ImagePaths).ConfigureAwait(false);

    public void Add(int index, ImageModel imageModel) =>
        PreLoader.Add(index, ImagePaths, imageModel);

    public bool Add(string file, ImageModel imageModel)
    {
        file = file.Replace('/', '\\');
        return PreLoader.Add(ImagePaths.IndexOf(file), ImagePaths, imageModel);
    }

    public PreLoadValue? GetPreLoadValue(int index)
    {
        if (index < 0 || index >= ImagePaths.Count)
        {
            return null;
        }

        return _isRunning
            ? PreLoader.Get(ImagePaths[index], ImagePaths)
            : PreLoader.Get(index, ImagePaths);
    }

    public PreLoadValue? GetPreLoadValue(string file)
    {
        var index = ImagePaths.IndexOf(file);
        if (index < 0 || index >= ImagePaths.Count)
        {
            return null;
        }

        return _isRunning
            ? PreLoader.Get(ImagePaths[index], ImagePaths)
            : PreLoader.Get(index, ImagePaths);
    }


    public async Task<PreLoadValue?> GetOrLoadPreLoadValueAsync(int index) =>
        await PreLoader.GetOrLoadAsync(index, ImagePaths);

    public PreLoadValue? GetCurrentPreLoadValue() =>
        _isRunning
            ? PreLoader.Get(_vm.PicViewer.FileInfo.FullName, ImagePaths)
            : PreLoader.Get(CurrentIndex, ImagePaths);

    public async Task<PreLoadValue?> GetCurrentPreLoadValueAsync() =>
        _isRunning
            ? await PreLoader.GetOrLoadAsync(_vm.PicViewer.FileInfo.FullName, ImagePaths)
            : await PreLoader.GetOrLoadAsync(CurrentIndex, ImagePaths);

    public PreLoadValue? GetNextPreLoadValue()
    {
        var nextIndex = GetIteration(CurrentIndex, IsReversed ? NavigateTo.Previous : NavigateTo.Next);
        return _isRunning ? PreLoader.Get(ImagePaths[nextIndex], ImagePaths) : PreLoader.Get(nextIndex, ImagePaths);
    }

    public async Task<PreLoadValue?>? GetNextPreLoadValueAsync()
    {
        var nextIndex = GetIteration(CurrentIndex, NavigateTo.Next);
        return _isRunning
            ? await PreLoader.GetOrLoadAsync(ImagePaths[nextIndex], ImagePaths)
            : await PreLoader.GetOrLoadAsync(nextIndex, ImagePaths);
    }

    public void RemoveItemFromPreLoader(int index) => PreLoader.Remove(index, ImagePaths);
    public void RemoveItemFromPreLoader(string fileName) => PreLoader.Remove(fileName, ImagePaths);

    public void RemoveCurrentItemFromPreLoader() => PreLoader.Remove(CurrentIndex, ImagePaths);

    public void Resynchronize() => PreLoader.Resynchronize(ImagePaths);

    #endregion

    #region Navigation

    public async Task ReloadFileListAsync()
    {
        try
        {
            _isRunning = true;
            var fileList = await Task.FromResult(_vm.PlatformService.GetFiles(_vm.PicViewer.FileInfo))
                .ConfigureAwait(false);
            var oldList = ImagePaths;
            ImagePaths = fileList;
            CurrentIndex = ImagePaths.IndexOf(_vm.PicViewer.FileInfo.FullName);
            TitleManager.SetTitle(_vm);
            await ClearAsync().ConfigureAwait(false);
            await PreloadAsync().ConfigureAwait(false);
            Resynchronize();
            _isRunning = false;
            if (fileList.Count > oldList.Count)
            {
                for (var i = 0; i < oldList.Count; i++)
                {
                    if (i < fileList.Count && !oldList[i].Contains(fileList[i]))
                    {
                        await GalleryFunctions.AddGalleryItem(fileList.IndexOf(fileList[i]), new FileInfo(fileList[i]),
                            _vm, DispatcherPriority.Background);
                    }
                }
            }
            else if (fileList.Count < oldList.Count)
            {
                for (var i = 0; i < fileList.Count; i++)
                {
                    if (i < oldList.Count && fileList[i].Contains(oldList[i]))
                    {
                        GalleryFunctions.RemoveGalleryItem(i, _vm);
                    }
                }
            }
        }
        finally
        {
            _isRunning = false;
        }
    }

    public async Task QuickReload()
    {
        RemoveCurrentItemFromPreLoader();
        await IterateToIndex(CurrentIndex, new CancellationTokenSource()).ConfigureAwait(false);
    }

    public int GetIteration(int index, NavigateTo navigateTo, bool skip1 = false, bool skip10 = false,
        bool skip100 = false)
    {
        int next;

        if (skip100)
        {
            if (ImagePaths.Count > PreLoader.MaxCount)
            {
                PreLoader.Clear();
            }
        }

        // Determine skipAmount based on input flags
        var skipAmount = skip100 ? 100 : skip10 ? 10 : skip1 ? 2 : 1;

        switch (navigateTo)
        {
            case NavigateTo.Next:
            case NavigateTo.Previous:
                var indexChange = navigateTo == NavigateTo.Next ? skipAmount : -skipAmount;
                IsReversed = navigateTo == NavigateTo.Previous;

                if (Settings.UIProperties.Looping)
                {
                    // Calculate new index with looping
                    next = (index + indexChange + ImagePaths.Count) % ImagePaths.Count;
                }
                else
                {
                    // Calculate new index without looping and ensure bounds
                    var newIndex = index + indexChange;
                    if (newIndex < 0)
                    {
                        return 0;
                    }

                    if (newIndex >= ImagePaths.Count)
                    {
                        return ImagePaths.Count - 1;
                    }

                    next = newIndex;
                }

                break;

            case NavigateTo.First:
            case NavigateTo.Last:
                if (ImagePaths.Count > PreLoader.MaxCount)
                {
                    PreLoader.Clear();
                }

                next = navigateTo == NavigateTo.First ? 0 : ImagePaths.Count - 1;
                break;

            default:
#if DEBUG
                Console.WriteLine($"{nameof(ImageIterator)}: {navigateTo} is not a valid NavigateTo value.");
#endif
                return -1;
        }

        return next;
    }

    public async Task NextIteration(NavigateTo navigateTo, CancellationTokenSource cts)
    {
        var index = GetIteration(CurrentIndex, navigateTo,
            Settings.ImageScaling.ShowImageSideBySide);
        if (index < 0)
        {
            return;
        }

        await NextIteration(index, cts).ConfigureAwait(false);
    }

    public async Task NextIteration(int iteration, CancellationTokenSource cts)
    {
        // Handle side-by-side navigation
        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            // Handle properly navigating first or last image
            if (iteration == GetCount - 1)
            {
                if (!Settings.UIProperties.Looping)
                {
                    return;
                }

                var targetIndex = IsReversed ? GetCount - 2 < 0 ? 0 : GetCount - 2 : 0;
                await IterateToIndex(targetIndex, cts).ConfigureAwait(false);
                return;
            }

            // Determine the next index based on navigation direction
            var nextIndex = GetIteration(iteration, IsReversed ? NavigateTo.Previous : NavigateTo.Next);
            await IterateToIndex(nextIndex, cts).ConfigureAwait(false);
            return;
        }

        // When not showing side-by-side, decide based on keyboard state
        if (!MainKeyboardShortcuts.IsKeyHeldDown)
        {
            await IterateToIndex(iteration, cts).ConfigureAwait(false);
        }
        else
        {
            await TimerIteration(iteration, cts).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Iterates to the given index in the image list, shows the corresponding image and preloads the next/previous images.
    /// </summary>
    /// <param name="index">The index to iterate to.</param>
    /// <param name="cts">The cancellation token source.</param>
    public async Task IterateToIndex(int index, CancellationTokenSource cts)
    {
        if (index < 0 || index >= ImagePaths.Count)
        {
            // Invalid index. Probably a race condition? Do nothing and report
#if DEBUG
            Trace.WriteLine($"Invalid index {index} in {nameof(ImageIterator)}:{nameof(IterateToIndex)}");
#endif
            return;
        }

        try
        {
            CurrentIndex = index;

            // Get cached preload value first, if available
            // ReSharper disable once MethodHasAsyncOverload
            var preloadValue = GetPreLoadValue(index);
            if (preloadValue is not null)
            {
                // Wait for image to load if it's still loading
                if (preloadValue is { IsLoading: true, ImageModel.Image: null })
                {
                    LoadingPreview();

                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
                    linkedCts.CancelAfter(TimeSpan.FromMinutes(1));

                    try
                    {
                        // Wait for the loading to complete or timeout
                        await preloadValue.WaitForLoadingCompleteAsync().WaitAsync(linkedCts.Token);
                    }
                    catch (OperationCanceledException) when (!cts.IsCancellationRequested)
                    {
                        // This is a timeout, not cancellation from navigation
                        preloadValue =
                            new PreLoadValue(
                                await GetImageModel.GetImageModelAsync(new FileInfo(ImagePaths[CurrentIndex])))
                            {
                                IsLoading = false
                            };
                    }

                    // Check if user navigated away during loading
                    if (CurrentIndex != index)
                    {
                        await cts.CancelAsync();
                        return;
                    }
                }
            }
            else
            {
                var imageModel = await GetImageModel.GetImageModelAsync(new FileInfo(ImagePaths[index]))
                    .ConfigureAwait(false);
                preloadValue = new PreLoadValue(imageModel);
            }

            if (CurrentIndex != index)
            {
                // Skip loading if user went to next value
                await cts.CancelAsync();
                return;
            }

            if (Settings.ImageScaling.ShowImageSideBySide)
            {
                var nextIndex = GetIteration(index, IsReversed ? NavigateTo.Previous : NavigateTo.Next);
                var nextPreloadValue = await GetOrLoadPreLoadValueAsync(nextIndex).ConfigureAwait(false);
                if (CurrentIndex != index)
                {
                    // Skip loading if user went to next value
                    await cts.CancelAsync();
                    return;
                }

                if (!cts.IsCancellationRequested && index == CurrentIndex)
                {
                    await UpdateImage.UpdateSource(_vm, index, ImagePaths, preloadValue,
                            nextPreloadValue)
                        .ConfigureAwait(false);
                }
            }
            else
            {
                if (!cts.IsCancellationRequested && index == CurrentIndex)
                {
                    await UpdateImage.UpdateSource(_vm, index, ImagePaths, preloadValue)
                        .ConfigureAwait(false);
                }
            }

            if (ImagePaths.Count > 1)
            {
                if (Settings.UIProperties.IsTaskbarProgressEnabled)
                {
                    Dispatcher.UIThread.Invoke(
                        () => { _vm.PlatformService.SetTaskbarProgress((ulong)CurrentIndex, (ulong)ImagePaths.Count); },
                        DispatcherPriority.Render);
                }

                await PreLoader.PreLoadAsync(index, IsReversed, ImagePaths)
                    .ConfigureAwait(false);
            }

            PreLoader.Add(index, ImagePaths, preloadValue?.ImageModel);

            // Add recent files
            if (string.IsNullOrWhiteSpace(TempFileHelper.TempFilePath) && ImagePaths.Count > CurrentIndex)
            {
                FileHistoryManager.Add(ImagePaths[CurrentIndex]);
                if (Settings.ImageScaling.ShowImageSideBySide)
                {
                    FileHistoryManager.Add(
                        ImagePaths[GetIteration(CurrentIndex, IsReversed ? NavigateTo.Previous : NavigateTo.Next)]);
                }
            }
        }
        catch (OperationCanceledException)
        {
#if DEBUG
            Trace.WriteLine($"\n{nameof(IterateToIndex)} canceled\n");
#endif
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(ImageIterator), nameof(IterateToIndex), e);
#if DEBUG
            await TooltipHelper.ShowTooltipMessageAsync(e.Message);
#endif
        }
        finally
        {
            if (index == CurrentIndex)
            {
                _vm.IsLoading = false;
            }
        }

        return;

        void LoadingPreview()
        {
            TitleManager.SetLoadingTitle(_vm);

            _vm.SelectedGalleryItemIndex = index;
            if (Settings.Gallery.IsBottomGalleryShown)
            {
                GalleryNavigation.CenterScrollToSelectedItem(_vm);
            }

            var thumb = GetThumbnails.GetExifThumb(NavigationManager.GetFileNameAt(index));

            if (index != CurrentIndex)
            {
                return;
            }

            if (!Settings.ImageScaling.ShowImageSideBySide)
            {
                _vm.PicViewer.ImageSource = thumb;
                _vm.IsLoading = thumb is null;
            }
            else
            {
                var secondaryThumb = GetThumbnails.GetExifThumb(NavigationManager.GetNextFileName);
                if (index != CurrentIndex)
                {
                    return;
                }

                _vm.PicViewer.ImageSource = thumb;
                _vm.PicViewer.SecondaryImageSource = secondaryThumb;
                _vm.IsLoading = thumb is null || secondaryThumb is null;
            }
        }
    }

    private static Timer? _timer;


    private async Task TimerIteration(int index, CancellationTokenSource cts)
    {
        if (_timer is null)
        {
            _timer = new Timer
            {
                AutoReset = false,
                Enabled = true
            };
        }
        else if (_timer.Enabled)
        {
            if (!MainKeyboardShortcuts.IsKeyHeldDown)
            {
                _timer = null;
            }

            return;
        }

        _timer.Interval = TimeSpan.FromSeconds(Settings.UIProperties.NavSpeed).TotalMilliseconds;
        _timer.Start();
        await IterateToIndex(index, cts).ConfigureAwait(false);
    }

    public void UpdateFileListAndIndex(List<string> fileList, int index)
    {
        ImagePaths = fileList;
        CurrentIndex = index;
    }

    #endregion

    #region IDisposable

    public async ValueTask DisposeAsync()
    {
        await ClearAsync().ConfigureAwait(false);
        Dispose(true, true);
    }

    private void Dispose(bool disposing, bool cleared = false)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _watcher?.Dispose();
            if (!cleared)
            {
                PreLoader.Clear();
            }

            _timer?.Dispose();
            PreLoader.Dispose();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion
}