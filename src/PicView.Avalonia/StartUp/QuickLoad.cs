using Avalonia.Threading;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileHandling;
using PicView.Core.Gallery;
using PicView.Core.ImageDecoding;
using PicView.Core.Navigation;

namespace PicView.Avalonia.StartUp;

public static class QuickLoad
{
    public static async Task QuickLoadAsync(MainViewModel vm, string file)
    {
        var fileInfo = new FileInfo(file);
        if (!fileInfo.Exists) // If not file, try to load if URL, base64 or directory
        {
            await NavigationManager.LoadPicFromStringAsync(file, vm).ConfigureAwait(false);
            return;
        }

        if (file.IsArchive()) // Handle if file exist and is an archive
        {
            await NavigationManager.LoadPicFromArchiveAsync(file, vm).ConfigureAwait(false);
            return;
        }
        
        vm.PicViewer.FileInfo = fileInfo;

        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            await SideBySideLoadingAsync(vm, fileInfo).ConfigureAwait(false);
        }
        else
        {
            await SingeImageLoadingAsync(vm, fileInfo).ConfigureAwait(false);
        }

        vm.IsLoading = false;
    }

    private static async Task SingeImageLoadingAsync(MainViewModel vm, FileInfo fileInfo)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        ImageModel? imageModel = null;
        await Task.WhenAll(
            Task.Run(() => { NavigationManager.InitializeImageIterator(vm); }, cancellationTokenSource.Token),
            Task.Run(async () => imageModel = await SetSingleImageAsync(vm, fileInfo), cancellationTokenSource.Token))
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

    private static async Task<ImageModel> SetSingleImageAsync(MainViewModel vm, FileInfo fileInfo)
    {
        var imageModel = await GetImageModel.GetImageModelAsync(fileInfo).ConfigureAwait(false);
        SetPicViewerValues(vm, imageModel, fileInfo);
        vm.IsLoading = false;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            WindowResizing.SetSize(imageModel.PixelWidth, imageModel.PixelHeight, vm);
        }, DispatcherPriority.Send);
        await RenderingFixes(vm, imageModel, null);
        return imageModel;
    }

    private static async Task SideBySideLoadingAsync(MainViewModel vm, FileInfo fileInfo)
    {
        NavigationManager.InitializeImageIterator(vm);
        var imageModel = await GetImageModel.GetImageModelAsync(fileInfo);
        var secondaryPreloadValue = await NavigationManager.GetNextPreLoadValueAsync();
        vm.PicViewer.SecondaryImageSource = secondaryPreloadValue?.ImageModel?.Image;
        SetPicViewerValues(vm, imageModel, fileInfo);
        await RenderingFixes(vm, imageModel, secondaryPreloadValue.ImageModel);
        TitleManager.SetSideBySideTitle(vm, imageModel, secondaryPreloadValue?.ImageModel);
            
        // Sometimes the images are not rendered in side by side, this fixes it
        // TODO: Improve and fix side by side and remove this hack 
        Dispatcher.UIThread.Post(() =>
        {
            vm.ImageViewer?.MainImage?.InvalidateVisual();
        });
        await StartPreloaderAndGalleryAsync(vm, imageModel, fileInfo);
    }

    private static void SetPicViewerValues(MainViewModel vm, ImageModel imageModel, FileInfo fileInfo)
    {
        if (imageModel.ImageType is ImageType.AnimatedGif or ImageType.AnimatedWebp)
        {
            vm.ImageViewer.MainImage.InitialAnimatedSource = fileInfo.FullName;
        }
        
        vm.PicViewer.ImageSource = imageModel.Image;
        vm.PicViewer.ImageType = imageModel.ImageType;
        vm.ZoomValue = 1;
        vm.PicViewer.PixelWidth = imageModel.PixelWidth;
        vm.PicViewer.PixelHeight = imageModel.PixelHeight;
        
        vm.PicViewer.ExifOrientation = imageModel.EXIFOrientation;
        vm.GetIndex = NavigationManager.GetNonZeroIndex;
    }

    private static async Task RenderingFixes(MainViewModel vm, ImageModel imageModel, ImageModel? secondaryModel)
    {
        // When width and height are the same, it renders image incorrectly at startup,
        // so need to handle it specially
        var is1To1 = imageModel.PixelWidth == imageModel.PixelHeight;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            vm.ImageViewer.SetTransform(imageModel.EXIFOrientation, false);
            if (Settings.WindowProperties.AutoFit && !Settings.Zoom.ScrollEnabled)
            {
                SetSize(vm, imageModel, secondaryModel);
                WindowFunctions.CenterWindowOnScreen();
            }
            else if (is1To1)
            {
                var size = WindowResizing.GetSize(imageModel.PixelWidth, imageModel.PixelHeight,
                    secondaryModel?.PixelWidth ?? 0, secondaryModel?.PixelHeight ?? 0, vm.RotationAngle, vm);
                if (!size.HasValue)
                {
#if DEBUG
                    Console.WriteLine($"{nameof(QuickLoadAsync)} {nameof(size)} is null");
#endif
                    ErrorHandling.ShowStartUpMenu(vm);
                    return;
                }

                WindowResizing.SetSize(size.Value, vm);
                vm.ImageViewer.MainBorder.Height = size.Value.Width;
                vm.ImageViewer.MainBorder.Width = size.Value.Height;
            }
            else if (imageModel.PixelWidth <= UIHelper.GetMainView.Bounds.Width &&
                     imageModel.PixelHeight <= UIHelper.GetMainView.Bounds.Height)
            {
                SetSize(vm, imageModel, secondaryModel);
            }
        }, DispatcherPriority.Send);
        
        if (Settings.Zoom.ScrollEnabled)
        {
            // Bad fix for scrolling
            // TODO: Implement proper startup scrolling fix
            Settings.Zoom.ScrollEnabled = false;
            await Dispatcher.UIThread.InvokeAsync(() => SetSize(vm, imageModel, secondaryModel), DispatcherPriority.Render);
            Settings.Zoom.ScrollEnabled = true;
            await Dispatcher.UIThread.InvokeAsync(() => SetSize(vm, imageModel, secondaryModel), DispatcherPriority.Send);
            if (Settings.WindowProperties.AutoFit)
            {
                await Dispatcher.UIThread.InvokeAsync(() => WindowFunctions.CenterWindowOnScreen());
            }
        }
    }
    
    private static async Task StartPreloaderAndGalleryAsync(MainViewModel vm, ImageModel imageModel, FileInfo fileInfo)
    {
        // Add recent files, except when browsing archive
        if (string.IsNullOrWhiteSpace(TempFileHelper.TempFilePath))
        {
            FileHistory.Add(fileInfo.FullName);
        }

        NavigationManager.AddToPreloader(NavigationManager.GetCurrentIndex, imageModel);
        
        var tasks = new List<Task>();
        
        if (NavigationManager.GetCount > 1)
        {
            if (Settings.UIProperties.IsTaskbarProgressEnabled)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    vm.PlatformService.SetTaskbarProgress((ulong)NavigationManager.GetCurrentIndex, (ulong)NavigationManager.GetCount);
                });
            }

            tasks.Add(NavigationManager.PreloadAsync());
        }

        if (Settings.Gallery.IsBottomGalleryShown)
        {
            if (vm.IsUIShown)
            {
                vm.GalleryMode = GalleryMode.BottomNoAnimation;
                tasks.Add(GalleryLoad.LoadGallery(vm, fileInfo.DirectoryName));
            }
            else if (Settings.Gallery.ShowBottomGalleryInHiddenUI)
            {
                vm.GalleryMode = GalleryMode.BottomNoAnimation;
                tasks.Add(GalleryLoad.LoadGallery(vm, fileInfo.DirectoryName));
            }
            else if (Settings.WindowProperties.Fullscreen)
            {
                vm.GalleryMode = GalleryMode.BottomNoAnimation;
                tasks.Add(GalleryLoad.LoadGallery(vm, fileInfo.DirectoryName));
            }
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static void SetSize(MainViewModel vm, ImageModel imageModel, ImageModel? secondaryModel)
    {
        var size = WindowResizing.GetSize(imageModel.PixelWidth, imageModel.PixelHeight, secondaryModel?.PixelWidth ?? 0, secondaryModel?.PixelHeight ?? 0, vm.RotationAngle, vm);
        if (!size.HasValue)
        {
#if DEBUG
            Console.WriteLine($"{nameof(QuickLoadAsync)} {nameof(size)} is null");           
#endif
            ErrorHandling.ShowStartUpMenu(vm);
            return;
        }
        WindowResizing.SetSize(size.Value, vm);
    }
}
