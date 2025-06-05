using System.Runtime.InteropServices;
using Avalonia.Threading;
using PicView.Avalonia.Crop;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Preloading;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileHistory;
using PicView.Core.Gallery;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;
using PicView.Core.Navigation;

namespace PicView.Avalonia.Navigation;

/// <summary>
///     Manages image navigation within the application.
/// </summary>
public static class NavigationManager
{
    public static TiffManager.TiffNavigationInfo? TiffNavigationInfo { get; private set; }

    // Should be updated to handle multiple iterators in the future when adding tab support
    public static ImageIterator? ImageIterator { get; private set; }

    /// <summary>
    ///     Gets the list of files in the next or previous folder.
    /// </summary>
    /// <param name="next">True to get the next folder, false for the previous folder.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation that returns a list of file paths.</returns>
    private static async Task<List<string>?> GetNextFolderFileList(bool next, MainViewModel vm)
    {
        return await Task.Run(() =>
        {
            var indexChange = next ? 1 : -1;
            var currentFolder = Path.GetDirectoryName(ImageIterator?.ImagePaths[ImageIterator.CurrentIndex]);
            var parentFolder = Path.GetDirectoryName(currentFolder);
            var directories = Directory.GetDirectories(parentFolder, "*", SearchOption.TopDirectoryOnly);

            if (directories.Length == 0 || string.IsNullOrEmpty(currentFolder))
            {
                return null;
            }

            var directoryIndex = Array.IndexOf(directories, currentFolder);

            if (directoryIndex == -1)
            {
                return null; // Current folder not found
            }

            var dirCount = directories.Length;
            var currentIndex = directoryIndex;

            // Loop until we've come back to the starting directory
            do
            {
                // Move to next/previous directory
                currentIndex = (currentIndex + indexChange + dirCount) % dirCount;

                // If we've come full circle without finding anything, return null
                if (currentIndex == directoryIndex)
                {
                    return null;
                }

                var fileList = vm.PlatformService.GetFiles(new FileInfo(directories[currentIndex]));
                if (fileList is { Count: > 0 })
                {
                    return fileList;
                }
            } while (true);
        }).ConfigureAwait(false);
    }


    /// <summary>
    /// Loads a picture from a given file, reloads the ImageIterator and loads the corresponding gallery from the file's
    /// directory.
    /// </summary>
    /// <param name="fileInfo">The FileInfo object representing the file to load.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <param name="files">
    /// Optional: The list of file paths to load. If null, the list is loaded from the given file's
    /// directory.
    /// </param>
    /// <param name="index">Optional: The index at which to start the navigation. Defaults to 0.</param>
    public static async Task LoadWithoutImageIterator(FileInfo fileInfo, MainViewModel vm, List<string>? files = null,
        int index = 0)
    {
        var imageModel = await GetImageModel.GetImageModelAsync(fileInfo).ConfigureAwait(false);
        ImageModel? nextImageModel = null;
        vm.PicViewer.ImageSource = imageModel.Image;
        vm.PicViewer.ImageType = imageModel.ImageType;
        if (!Settings.ImageScaling.ShowImageSideBySide)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                vm.ImageViewer.SetTransform(imageModel.EXIFOrientation, imageModel.Format);
                WindowResizing.SetSize(imageModel.PixelWidth, imageModel.PixelHeight, 0, 0, vm.RotationAngle,
                    vm);
            });
        }

        await DisposeImageIteratorAsync();

        if (files is null)
        {
            ImageIterator = new ImageIterator(fileInfo, vm);
            index = ImageIterator.CurrentIndex;
        }
        else
        {
            ImageIterator = new ImageIterator(fileInfo, files, index, vm);
        }

        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            nextImageModel = (await ImageIterator.GetNextPreLoadValueAsync()).ImageModel;
            vm.PicViewer.SecondaryImageSource = nextImageModel.Image;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                WindowResizing.SetSize(imageModel.PixelWidth, imageModel.PixelHeight, nextImageModel.PixelWidth,
                    nextImageModel.PixelHeight, imageModel.Rotation, vm);
            });

            TitleManager.SetSideBySideTitle(vm, imageModel, nextImageModel);
            UpdateImage.SetStats(vm, index, imageModel);

            // Fixes incorrect rendering in the side by side view
            // TODO: Improve and fix side by side and remove this hack 
            Dispatcher.UIThread.Post(() => { vm.ImageViewer?.MainImage?.InvalidateVisual(); });
        }
        else
        {
            vm.IsSingleImage = false;
            var isTiffUpdated = await CheckIfTiffAndUpdate(vm, fileInfo, index);
            if (!isTiffUpdated)
            {
                if (Settings.ImageScaling.ShowImageSideBySide)
                {
                    TitleManager.SetSideBySideTitle(vm, imageModel, nextImageModel);
                }
                else
                {
                    TitleManager.SetTitle(vm, imageModel);
                }

                UpdateImage.SetStats(vm, index, imageModel);
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            await WindowFunctions.ResizeAndFixRenderingError(vm);
        }

        vm.IsLoading = false;
        FileHistoryManager.Add(ImageIterator.ImagePaths[index]);
        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            FileHistoryManager.Add(ImageIterator.ImagePaths[ImageIterator.GetIteration(index, NavigateTo.Next)]);
        }

        await GalleryLoad.CheckAndReloadGallery(fileInfo, vm);
    }

    #region Navigation

    /// <summary>
    /// Determines whether navigation is possible based on the current state of the <see cref="MainViewModel" />.
    /// </summary>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>True if navigation is possible, otherwise false.</returns>
    public static bool CanNavigate(MainViewModel vm) =>
        ImageIterator?.ImagePaths is not null &&
        ImageIterator.ImagePaths.Count > 0 && !CropFunctions.IsCropping &&
        !DialogManager.IsDialogOpen && vm is { IsEditableTitlebarOpen: false, PicViewer.FileInfo: not null };

    /// <summary>
    ///     Navigates to the next or previous image based on the <paramref name="next" /> parameter.
    /// </summary>
    /// <param name="next">True to navigate to the next image, false for the previous image.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Navigate(bool next, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            if (vm.PicViewer.FileInfo is null && ImageIterator is not null)
            {
                // Fixes issue that shouldn't happen. Should investigate.
                vm.PicViewer.FileInfo = new FileInfo(ImageIterator.ImagePaths[0]);
            }
            else
            {
                return;
            }
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            await GalleryNavigation.ScrollGallery(next);
            return;
        }

        if (ImageIterator.CurrentIndex < 0 || ImageIterator.CurrentIndex >= ImageIterator.ImagePaths.Count)
        {
            ErrorHandling.ShowStartUpMenu(vm);
            return;
        }

        var navigateTo = next ? NavigateTo.Next : NavigateTo.Previous;
        var nextIteration = ImageIterator.GetIteration(ImageIterator.CurrentIndex, navigateTo);
        var currentFileName = ImageIterator.ImagePaths[ImageIterator.CurrentIndex];
        if (TiffManager.IsTiff(currentFileName))
        {
            await TiffNavigation(vm, currentFileName, nextIteration).ConfigureAwait(false);
        }
        else
        {
            await ImageLoader.CheckCancellationAndStartIterateToIndex(nextIteration, ImageIterator)
                .ConfigureAwait(false);
        }
    }

    private static async Task TiffNavigation(MainViewModel vm, string currentFileName, int nextIteration)
    {
        if (TiffNavigationInfo is null && !ImageIterator.IsReversed)
        {
            var tiffPages = await Task.FromResult(TiffManager.LoadTiffPages(currentFileName)).ConfigureAwait(false);
            if (tiffPages.Count < 1)
            {
                await ImageLoader.CheckCancellationAndStartIterateToIndex(nextIteration, ImageIterator)
                    .ConfigureAwait(false);
                return;
            }

            TiffNavigationInfo = new TiffManager.TiffNavigationInfo
            {
                CurrentPage = 0,
                PageCount = tiffPages.Count,
                Pages = tiffPages
            };
        }

        if (TiffNavigationInfo is null)
        {
            await ImageLoader.CheckCancellationAndStartIterateToIndex(nextIteration, ImageIterator)
                .ConfigureAwait(false);
        }
        else
        {
            if (ImageIterator.IsReversed)
            {
                if (TiffNavigationInfo.CurrentPage - 1 < 0)
                {
                    await ExitTiffNavigationAndNavigate().ConfigureAwait(false);
                    return;
                }

                TiffNavigationInfo.CurrentPage -= 1;
            }
            else
            {
                TiffNavigationInfo.CurrentPage += 1;
            }

            if (TiffNavigationInfo.CurrentPage >= TiffNavigationInfo.PageCount || TiffNavigationInfo.CurrentPage < 0)
            {
                await ExitTiffNavigationAndNavigate().ConfigureAwait(false);
            }
            else
            {
                await UpdateImage.SetTiffImageAsync(TiffNavigationInfo, ImageIterator.CurrentIndex,
                    vm.PicViewer.FileInfo, vm);
            }
        }

        return;

        async Task ExitTiffNavigationAndNavigate()
        {
            await ImageLoader.CheckCancellationAndStartIterateToIndex(nextIteration, ImageIterator)
                .ConfigureAwait(false);
            TiffNavigationInfo?.Dispose();
            TiffNavigationInfo = null;
        }
    }

    public static async Task<bool> CheckIfTiffAndUpdate(MainViewModel vm, FileInfo fileInfo, int index)
    {
        if (!TiffManager.IsTiff(fileInfo))
        {
            return false;
        }

        var tiffPages = await Task.FromResult(TiffManager.LoadTiffPages(fileInfo.FullName)).ConfigureAwait(false);
        if (tiffPages.Count < 1)
        {
            return false;
        }

        TiffNavigationInfo = new TiffManager.TiffNavigationInfo
        {
            CurrentPage = 0,
            PageCount = tiffPages.Count,
            Pages = tiffPages
        };
        await UpdateImage.SetTiffImageAsync(TiffNavigationInfo, index, fileInfo, vm);
        return true;
    }

    public static async Task Navigate(int index, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            return;
        }

        await ImageLoader.CheckCancellationAndStartIterateToIndex(index, ImageIterator).ConfigureAwait(false);
    }

    public static async Task Navigate(string fileName, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            return;
        }

        var index = ImageIterator.ImagePaths.IndexOf(fileName);

        await ImageLoader.CheckCancellationAndStartIterateToIndex(index, ImageIterator).ConfigureAwait(false);
    }

    private static async Task NavigateIncrements(MainViewModel vm, bool next, bool is10, bool is100)
    {
        if (!CanNavigate(vm))
        {
            return;
        }

        var currentIndex = ImageIterator.CurrentIndex;
        var direction = next ? NavigateTo.Next : NavigateTo.Previous;
        var index = ImageIterator.GetIteration(currentIndex, direction, false, is10, is100);

        await ImageLoader.CheckCancellationAndStartIterateToIndex(index, ImageIterator).ConfigureAwait(false);
    }

    public static Task Next10(MainViewModel vm) => NavigateIncrements(vm, true, true, false);
    public static Task Next100(MainViewModel vm) => NavigateIncrements(vm, true, false, true);
    public static Task Prev10(MainViewModel vm) => NavigateIncrements(vm, false, true, false);
    public static Task Prev100(MainViewModel vm) => NavigateIncrements(vm, false, false, true);

    /// <summary>
    ///     Navigates to the first or last image in the collection.
    /// </summary>
    /// <param name="last">True to navigate to the last image, false to navigate to the first image.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task NavigateFirstOrLast(bool last, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            return;
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            GalleryNavigation.NavigateGallery(last, vm);
        }
        else
        {
            if (last)
            {
                await ImageLoader.LastIterationAsync(ImageIterator).ConfigureAwait(false);
            }
            else
            {
                await ImageLoader.FirstIterationAsync(ImageIterator).ConfigureAwait(false);
            }

            await UIHelper.ScrollToEndIfNecessary(last);
        }
    }

    /// <summary>
    ///     Iterates to the next or previous image based on the <paramref name="next" /> parameter.
    /// </summary>
    /// <param name="next">True to iterate to the next image, false for the previous image.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Iterate(bool next, MainViewModel vm)
    {
        if (GalleryFunctions.IsFullGalleryOpen)
        {
            GalleryNavigation.NavigateGallery(next ? Direction.Right : Direction.Left, vm);
        }
        else
        {
            await Navigate(next, vm);
        }
    }

    /// <summary>
    ///     Navigates and moves the cursor to the corresponding button.
    /// </summary>
    /// <param name="next">True to navigate to the next image, false for the previous image.</param>
    /// <param name="arrow">True to move cursor to the arrow, false for the button.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task NavigateAndPositionCursor(bool next, bool arrow, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            return;
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            await GalleryNavigation.ScrollGallery(next);
        }
        else
        {
            await Navigate(next, vm);
            await UIHelper.MoveCursorOnButtonClick(next, arrow, vm);
        }
    }

    /// <summary>
    ///     Navigates to the next or previous folder and loads the first image in that folder.
    /// </summary>
    /// <param name="next">True to navigate to the next folder, false for the previous folder.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task GoToNextFolder(bool next, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            return;
        }

        TitleManager.SetLoadingTitle(vm);
        await ImageLoader.CancelAsync().ConfigureAwait(false);

        var fileList = await GetNextFolderFileList(next, vm).ConfigureAwait(false);

        if (fileList is null)
        {
            TitleManager.SetTitle(vm);
        }
        else
        {
            vm.PlatformService.StopTaskbarProgress();
            await LoadWithoutImageIterator(new FileInfo(fileList[0]), vm, fileList);
            if (vm.PicViewer.Title == TranslationManager.Translation.Loading)
            {
                TitleManager.SetTitle(vm);
            }
        }
    }

    #endregion

    #region Load pictures from string, file or url

    /// <inheritdoc cref="ImageLoader.LoadPicFromStringAsync(string, MainViewModel, Navigation.ImageIterator)" />
    public static async Task LoadPicFromStringAsync(string source, MainViewModel vm) =>
        await ImageLoader.LoadPicFromStringAsync(source, vm, ImageIterator).ConfigureAwait(false);

    /// <inheritdoc cref="ImageLoader.LoadPicFromFile(string, MainViewModel, Navigation.ImageIterator, FileInfo)" />
    public static async Task LoadPicFromFile(string fileName, MainViewModel vm, FileInfo? fileInfo = null) =>
        await ImageLoader.LoadPicFromFile(fileName, vm, ImageIterator, fileInfo).ConfigureAwait(false);

    /// <inheritdoc cref="ImageLoader.LoadPicFromArchiveAsync(string, MainViewModel, Navigation.ImageIterator)" />
    public static async Task LoadPicFromArchiveAsync(string path, MainViewModel vm) =>
        await ImageLoader.LoadPicFromArchiveAsync(path, vm, ImageIterator).ConfigureAwait(false);

    /// <inheritdoc cref="ImageLoader.LoadPicFromUrlAsync(string, MainViewModel, Navigation.ImageIterator)" />
    public static async Task LoadPicFromUrlAsync(string url, MainViewModel vm) =>
        await ImageLoader.LoadPicFromUrlAsync(url, vm, ImageIterator).ConfigureAwait(false);

    /// <inheritdoc cref="ImageLoader.LoadPicFromBase64Async(string, MainViewModel, Navigation.ImageIterator)" />
    public static async Task LoadPicFromBase64Async(string base64, MainViewModel vm) =>
        await ImageLoader.LoadPicFromBase64Async(base64, vm, ImageIterator).ConfigureAwait(false);

    /// <inheritdoc cref="ImageLoader.LoadPicFromDirectoryAsync(string, MainViewModel, FileInfo)"/>
    public static async Task LoadPicFromDirectoryAsync(string file, MainViewModel vm, FileInfo? fileInfo = null) =>
        await ImageLoader.LoadPicFromDirectoryAsync(file, vm, fileInfo).ConfigureAwait(false);

    #endregion

    #region ImageIterator

    public static void InitializeImageIterator(MainViewModel vm)
    {
        ImageIterator ??= new ImageIterator(vm.PicViewer.FileInfo, vm);
    }

    public static async Task DisposeImageIteratorAsync()
    {
        if (ImageIterator is null)
        {
            return;
        }

        await ImageIterator.ClearAsync();
        ImageIterator.ImagePaths.Clear();
        await ImageIterator.DisposeAsync();
    }

    public static bool IsCollectionEmpty => ImageIterator?.ImagePaths is null || ImageIterator?.ImagePaths?.Count < 0;
    public static List<string>? GetCollection => ImageIterator?.ImagePaths;

    public static void UpdateFileListAndIndex(List<string> fileList, int index) =>
        ImageIterator?.UpdateFileListAndIndex(fileList, index);

    public static int GetFileNameIndex(string fileName)
    {
        if (IsCollectionEmpty)
        {
            return -1;
        }

        fileName = fileName.Replace("/", "\\");
        return ImageIterator.ImagePaths.IndexOf(fileName);
    }

    /// <summary>
    ///     Returns the file name at a given index in the image collection.
    /// </summary>
    /// <param name="index">The index of the file to retrieve.</param>
    /// <returns>The file name at the given index.</returns>
    public static string? GetFileNameAt(int index)
    {
        if (IsCollectionEmpty)
        {
            return null;
        }

        if (index < 0 || index >= ImageIterator.ImagePaths.Count)
        {
            return null;
        }

        return ImageIterator.ImagePaths[index];
    }

    /// <summary>
    ///     Gets the current file name.
    /// </summary>
    public static string? GetCurrentFileName => GetFileNameAt(ImageIterator?.CurrentIndex ?? -1);

    /// <summary>
    ///     Gets the next file name.
    /// </summary>
    public static string? GetNextFileName => GetFileNameAt(ImageIterator?.NextIndex ?? -1);

    public static int GetCurrentIndex => ImageIterator?.CurrentIndex ?? -1;

    public static int GetNextIndex => ImageIterator?.NextIndex ?? -1;

    public static int GetNonZeroIndex => ImageIterator?.GetNonZeroIndex ?? -1;

    public static int GetCount => ImageIterator?.GetCount ?? -1;

    public static FileInfo? GetInitialFileInfo => ImageIterator?.InitialFileInfo;

    public static PreLoadValue? TryGetPreLoadValue(int index) =>
        ImageIterator?.GetPreLoadValue(index) ?? null;

    public static PreLoadValue? TryGetPreLoadValue(string fileName) =>
        ImageIterator?.GetPreLoadValue(fileName) ?? null;

    public static async Task<PreLoadValue?> GetPreLoadValueAsync(int index) =>
        await ImageIterator?.GetOrLoadPreLoadValueAsync(index) ?? null;

    public static async Task<PreLoadValue?> GetPreLoadValueAsync(string fileName) =>
        await ImageIterator?.GetOrLoadPreLoadValueAsync(GetFileNameIndex(fileName)) ?? null;

    public static PreLoadValue? GetCurrentPreLoadValue() =>
        ImageIterator?.GetCurrentPreLoadValue() ?? null;

    public static async Task<PreLoadValue?> GetCurrentPreLoadValueAsync() =>
        await ImageIterator?.GetCurrentPreLoadValueAsync() ?? null;

    public static PreLoadValue? GetNextPreLoadValue() =>
        ImageIterator?.GetNextPreLoadValue() ?? null;

    public static async Task<PreLoadValue?> GetNextPreLoadValueAsync() =>
        await ImageIterator?.GetNextPreLoadValueAsync() ?? null;

    public static async Task ReloadFileListAsync() =>
        await ImageIterator?.ReloadFileListAsync();

    public static void AddToPreloader(int index, ImageModel imageModel) =>
        ImageIterator?.Add(index, imageModel);

    public static bool AddToPreloader(string file, ImageModel imageModel) =>
        ImageIterator?.Add(file, imageModel) ?? false;

    public static async Task PreloadAsync() =>
        await ImageIterator?.PreloadAsync();

    public static async Task QuickReload() =>
        await ImageIterator?.QuickReload();

    #endregion
}