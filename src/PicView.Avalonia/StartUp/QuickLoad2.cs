using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.Exif;
using PicView.Core.Extensions;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.Gallery;
using PicView.Core.ImageDecoding;
using PicView.Core.Models;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.StartUp;

/// <summary>
/// Provides methods for loading images during application startup.
/// </summary>
public static class QuickLoad2
{
    /// <summary>
    /// Asynchronously loads an image, archive, URL, base64 string, or directory into the application view,
    /// updating the UI state and loading indicative properties as necessary.
    /// </summary>
    /// <param name="vm">The main view model.</param>
    /// <param name="file">The file, URL, or directory path to be loaded.</param>
    /// <param name="window">The main window used to optimize when it is shown, to avoid flickering from quick resizing.</param>
    /// <param name="continueFromLeftOff">A boolean indicating whether to continue loading from the last session folder structure.</param>
    public static async ValueTask QuickLoadAsync(MainViewModel vm, string file, Window window, bool continueFromLeftOff)
    {
        var fileInfo = new FileInfo(file);
        if (!fileInfo.Exists) // If not file, try to load if URL, base64 or directory
        {
            vm.MainWindow.IsLoadingIndicatorShown.Value = true;
            Dispatcher.UIThread.Invoke(window.Show, DispatcherPriority.Send);
            await NavigationManager.LoadPicFromStringAsync(file, vm).ConfigureAwait(false);
            return;
        }

        if (file.IsArchive()) // Handle if file exist and is an archive
        {
            vm.MainWindow.IsLoadingIndicatorShown.Value = true;
            Dispatcher.UIThread.Invoke(window.Show, DispatcherPriority.Send);
            await NavigationManager.LoadPicFromArchiveAsync(file, vm).ConfigureAwait(false);
            return;
        }

        var magickImage = new MagickImage();
        try
        {
            magickImage.Ping(fileInfo);
        }
        catch (Exception e)
        {
            // Pinging can lead to crashes when the file cannot be read. 
            // Just catching the exception here means it will still load correctly regardless
            DebugHelper.LogDebug(nameof(QuickLoad), nameof(QuickLoadAsync), e);
        }
        
        var imageModel = await GetImageModel.GetImageModelAsync(fileInfo, magickImage).ConfigureAwait(false);

        SetPicViewerValues(vm, imageModel, fileInfo);

    }
    private static void SetPicViewerValues(MainViewModel vm, ImageModel imageModel, FileInfo fileInfo)
    {
        if (imageModel.ImageType is ImageType.AnimatedGif or ImageType.AnimatedWebp)
        {
            vm.ImageViewer.MainImage.InitialAnimatedSource = fileInfo.FullName;
        }
        
        vm.PicViewer.FileInfo.Value = fileInfo;
        
        vm.NavigationViewModel.ActiveTab.Value.IModel.FileInfo = fileInfo;
        vm.NavigationViewModel.ActiveTab.Value.Initialize();
        vm.NavigationViewModel.ActiveTab.Value.IModel.Image = imageModel.Image;
        vm.NavigationViewModel.ActiveTab.Value.IModel.ImageType = imageModel.ImageType;
        vm.NavigationViewModel.ActiveTab.Value.IModel.PixelWidth = imageModel.PixelWidth;
        vm.NavigationViewModel.ActiveTab.Value.IModel.PixelHeight = imageModel.PixelHeight;
        vm.NavigationViewModel.ActiveTab.Value.IModel.Format = imageModel.Format;
        vm.NavigationViewModel.ActiveTab.Value.IModel.Orientation = imageModel.Orientation;
        
        vm.PicViewer.GetIndex.Value = NavigationManager.GetNonZeroIndex;
        vm.PicViewer.Index.Value = NavigationManager.GetCurrentIndex;
        
        vm.PicViewer.ImageSource.Value = imageModel.Image;
        vm.PicViewer.ImageType.Value = imageModel.ImageType;
        vm.PicViewer.RotationAngle.Value = 0;
        vm.PicViewer.PixelWidth.Value = imageModel.PixelWidth;
        vm.PicViewer.PixelHeight.Value = imageModel.PixelHeight;
        vm.PicViewer.Format.Value = imageModel.Format;
        vm.PicViewer.ExifOrientation.Value = imageModel.Orientation;
        
        Settings.StartUp.LastFile = fileInfo.FullName;
        
        // Temporary Dummy title
        var title = $"{fileInfo.Name} 1234/5678 files ({imageModel.PixelWidth} x {imageModel.PixelHeight}) {fileInfo.Length.GetReadableFileSize()}";
        vm.PicViewer.WindowTitle.Value = 
        vm.PicViewer.Title.Value = 
        vm.PicViewer.TitleTooltip.Value = title;
    }
}