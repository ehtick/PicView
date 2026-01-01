using Avalonia.Controls;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.Navigation.Services;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.Localization;
using PicView.Core.Models;
using PicView.Core.Navigation;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.StartUp;

/// <summary>
/// Provides methods for quickly loading the image first, and then initializing the rest of the navigation.
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
    public static async ValueTask QuickLoadAsync(CoreViewModel vm, string file, Window window, bool continueFromLeftOff)
    {
        vm.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.CurrentValue.WindowTitle.Value = vm.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.CurrentValue.Title.Value =
            TranslationManager.Translation.Loading ?? "Loading...";
        
        var fileInfo = new FileInfo(file);
        if (!fileInfo.Exists) // If not file, try to load if URL, base64 or directory
        {
  //          vm.MainWindow.IsLoadingIndicatorShown.Value = true;
            Dispatcher.UIThread.Invoke(window.Show, DispatcherPriority.Send);
                //          await NavigationManager.LoadPicFromStringAsync(file, vm).ConfigureAwait(false);
            return;
        }

        if (file.IsArchive()) // Handle if file exist and is an archive
        {
     //       vm.MainWindow.IsLoadingIndicatorShown.Value = true;
            Dispatcher.UIThread.Invoke(window.Show, DispatcherPriority.Send);
          //  await NavigationManager.LoadPicFromArchiveAsync(file, vm).ConfigureAwait(false);
            return;
        }
        Dispatcher.UIThread.Invoke(() =>
        {
           vm.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.CurrentView.Value = new ImageViewer2();
        }, DispatcherPriority.Send);
    
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

        TabNavigationInitializer.Initialize(vm, fileInfo);
    }
    
    private static void SetPicViewerValues(CoreViewModel vm, ImageModel imageModel, FileInfo fileInfo)
    {
       vm.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.Model.Value = imageModel;
       vm.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.Initialize();
        
        Settings.StartUp.LastFile = fileInfo.FullName;
        vm.MainWindows.ActiveWindow.Value.IsLoadingIndicatorShown.Value = false;
    }
}
