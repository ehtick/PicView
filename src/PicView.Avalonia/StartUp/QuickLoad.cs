using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.Gallery;
using PicView.Core.ImageDecoding;
using PicView.Core.Models;

namespace PicView.Avalonia.StartUp;

/// <summary>
/// Provides methods for loading images during application startup.
/// </summary>
public static class QuickLoad
{
    /// <summary>
    /// Start up procedure to load an image.
    /// </summary>
    /// <param name="vm">
    /// The main view model.
    /// </param>
    /// <param name="file">
    /// The path of the file to be loaded, which can be a file, archive, directory,
    /// URL, or base64 string.
    /// </param>
    public static async Task QuickLoadAsync(MainViewModel vm, string file)
    {
        var fileInfo = new FileInfo(file);
        if (!fileInfo.Exists) // If not file, try to load if URL, base64 or directory
        {
            vm.MainWindow.IsLoadingIndicatorShown.Value = true;
            await NavigationManager.LoadPicFromStringAsync(file, vm).ConfigureAwait(false);
            return;
        }

        if (file.IsArchive()) // Handle if file exist and is an archive
        {
            vm.MainWindow.IsLoadingIndicatorShown.Value = true;
            await NavigationManager.LoadPicFromArchiveAsync(file, vm).ConfigureAwait(false);
            return;
        }

        var magickImage = new MagickImage();
        magickImage.Ping(fileInfo);
        vm.PicViewer.FileInfo.Value = fileInfo;
        var isLargeImage = magickImage.Width * magickImage.Height > 5000000; // ~5 megapixels threshold
        if (isLargeImage || Settings.ImageScaling.ShowImageSideBySide)
        {
            // Don't show loading indicator if image is too small
            vm.MainWindow.IsLoadingIndicatorShown.Value = true;
        }

        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            await SideBySideLoadingAsync(vm, fileInfo, magickImage).ConfigureAwait(false);
        }
        else
        {
            await SingeImageLoadingAsync(vm, fileInfo, magickImage).ConfigureAwait(false);
        }
        
        vm.PicViewer.GetIndex.Value = NavigationManager.GetNonZeroIndex;
    }

    /// <summary>
    /// Asynchronously handles the loading of a single image into the application state and updates the relevant UI
    /// properties accordingly.
    /// </summary>
    /// <param name="vm">The main view model.</param>
    /// <param name="fileInfo">The file information object representing the image to be loaded.</param>
    /// <param name="magickImage">The MagickImage to not consecutively ping it.</param>
    private static async Task SingeImageLoadingAsync(MainViewModel vm, FileInfo fileInfo, MagickImage magickImage)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        ImageModel? imageModel = null;
        await Task.WhenAll(
                Task.Run(() => { NavigationManager.InitializeImageIterator(vm); }, cancellationTokenSource.Token),
                Task.Run(async () => imageModel = await SetSingleImageAsync(vm, fileInfo, magickImage),
                    cancellationTokenSource.Token))
            .ConfigureAwait(false);
        if (TiffManager.IsTiff(imageModel.FileInfo.FullName))
        {
            TitleManager.TrySetTiffTitle(imageModel, vm);
        }
        else
        {
            TitleManager.SetTitle(vm, imageModel);
        }

        await StartPreloaderAndGalleryAsync(vm, imageModel, fileInfo);
        cancellationTokenSource.Dispose();
    }

    /// <summary>
    /// Sets a single image in the viewer by updating the view model and rendering the necessary UI changes.
    /// </summary>
    /// <param name="vm">The main view model.</param>
    /// <param name="fileInfo">The file information of the image to be loaded.</param>
    /// <param name="magickImage">The MagickImage to not consecutively ping it.</param>
    /// <returns>The <see cref="ImageModel" /> instance representing the loaded image and its associated properties.</returns>
    private static async Task<ImageModel> SetSingleImageAsync(MainViewModel vm, FileInfo fileInfo,
        MagickImage magickImage)
    {
        if (Settings.WindowProperties.AutoFit)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                vm.ImageViewer.SetTransform(EXIFHelper.GetImageOrientation(magickImage), magickImage.Format);
                WindowResizing.SetSize(magickImage.Width, magickImage.Height, vm);
                WindowFunctions.CenterWindowOnScreen();
            }, DispatcherPriority.Send);
        }

        var imageModel = await GetImageModel.GetImageModelAsync(fileInfo, magickImage).ConfigureAwait(false);
        SetPicViewerValues(vm, imageModel, fileInfo);
        vm.MainWindow.IsLoadingIndicatorShown.Value = false;
        if (!Settings.WindowProperties.AutoFit)
        {
            await Dispatcher.UIThread.InvokeAsync(
                () => { WindowResizing.SetSize(imageModel.PixelWidth, imageModel.PixelHeight, vm); },
                DispatcherPriority.Send);
        }
        return imageModel;
    }

    /// <summary>
    /// Loads and sets up images in a side-by-side configuration for the main application view.
    /// </summary>
    /// <param name="vm">The main view model managing the application's state and UI properties.</param>
    /// <param name="fileInfo">Information about the file to be loaded.</param>
    /// <param name="magickImage">The MagickImage to not consecutively ping it.</param>
    private static async Task SideBySideLoadingAsync(MainViewModel vm, FileInfo fileInfo, MagickImage magickImage)
    {
        NavigationManager.InitializeImageIterator(vm);
        var imageModel = await GetImageModel.GetImageModelAsync(fileInfo, magickImage);
        var secondaryPreloadValue = await NavigationManager.GetNextPreLoadValueAsync();

        vm.PicViewer.SecondaryImageSource.Value = secondaryPreloadValue?.ImageModel?.Image;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            vm.ImageViewer.SetTransform(EXIFHelper.GetImageOrientation(magickImage), magickImage.Format);
            WindowResizing.SetSize(magickImage.Width, magickImage.Height, secondaryPreloadValue.ImageModel.PixelWidth, secondaryPreloadValue.ImageModel.PixelHeight, vm.GlobalSettings.RotationAngle.CurrentValue, vm);
        }, DispatcherPriority.Send);
        SetPicViewerValues(vm, imageModel, fileInfo);
        
        TitleManager.SetSideBySideTitle(vm, imageModel, secondaryPreloadValue?.ImageModel);
        await StartPreloaderAndGalleryAsync(vm, imageModel, fileInfo);
    }

    /// <summary>
    /// Updates the PicViewerModel with values based on the provided image model and file information.
    /// </summary>
    /// <param name="vm">The main view model.</param>
    /// <param name="imageModel">The ImageModel to populate PicViewerModel.</param>
    /// <param name="fileInfo">Used for setting specific properties like animated sources.
    /// </param>
    private static void SetPicViewerValues(MainViewModel vm, ImageModel imageModel, FileInfo fileInfo)
    {
        if (imageModel.ImageType is ImageType.AnimatedGif or ImageType.AnimatedWebp)
        {
            vm.ImageViewer.MainImage.InitialAnimatedSource = fileInfo.FullName;
        }
        
        vm.PicViewer.ImageSource.Value = imageModel.Image;
        vm.PicViewer.ImageType.Value = imageModel.ImageType;
        vm.GlobalSettings.RotationAngle.Value = 0;
        vm.PicViewer.PixelWidth.Value = imageModel.PixelWidth;
        vm.PicViewer.PixelHeight.Value = imageModel.PixelHeight;

        vm.PicViewer.ExifOrientation.Value = imageModel.EXIFOrientation;
    }

    /// <summary>
    /// Initiates the preloader and bottom gallery loading process for the application.
    /// </summary>
    /// <param name="vm">The main view model.</param>
    /// <param name="imageModel">The current image model containing the image data to be managed by the preloader and gallery.</param>
    /// <param name="fileInfo">The file information of the image to be processed and loaded.</param>
    private static async Task StartPreloaderAndGalleryAsync(MainViewModel vm, ImageModel imageModel,
        FileInfo fileInfo)
    {
        vm.MainWindow.IsLoadingIndicatorShown.Value = false;
        
        // Add recent files, except when browsing archive
        if (string.IsNullOrWhiteSpace(TempFileHelper.TempFilePath))
        {
            FileHistoryManager.Add(fileInfo.FullName);
        }

        NavigationManager.AddToPreloader(NavigationManager.GetCurrentIndex, imageModel);

        var tasks = new List<Task>();

        if (NavigationManager.GetCount > 1)
        {
            if (Settings.UIProperties.IsTaskbarProgressEnabled)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    vm.PlatformService.SetTaskbarProgress((ulong)NavigationManager.GetCurrentIndex,
                        (ulong)NavigationManager.GetCount);
                });
            }

            tasks.Add(NavigationManager.PreloadAsync());
        }

        if (Settings.Gallery.IsBottomGalleryShown)
        {
            bool loadGallery;
            if (!vm.MainWindow.IsUIShown.CurrentValue)
            {
                loadGallery = Settings.Gallery.ShowBottomGalleryInHiddenUI;
            }
            else
            {
                loadGallery = true;
            }

            if (loadGallery)
            {
                vm.Gallery.GalleryMode.Value = GalleryMode.BottomNoAnimation;
                tasks.Add(GalleryLoad.LoadGallery(vm, fileInfo.DirectoryName));
            }
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}