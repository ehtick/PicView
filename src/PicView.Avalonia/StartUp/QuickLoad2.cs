using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.Navigation.Services;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.Gallery;
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
    /// <param name="core">The main view model.</param>
    /// <param name="file">The file, URL, or directory path to be loaded.</param>
    /// <param name="continueFromLeftOff">A boolean indicating whether to continue loading from the last session folder structure.</param>
    public static async ValueTask QuickLoadAsync(CoreViewModel core, string file, bool continueFromLeftOff)
    {        
        var fileInfo = new FileInfo(file);
        if (!fileInfo.Exists) // If not file, try to load if URL, base64 or directory
        {
  //          vm.MainWindow.IsLoadingIndicatorShown.Value = true;
                //          await NavigationManager.LoadPicFromStringAsync(file, vm).ConfigureAwait(false);
            return;
        }

        if (file.IsArchive()) // Handle if file exist and is an archive
        {
     //       vm.MainWindow.IsLoadingIndicatorShown.Value = true;);
          //  await NavigationManager.LoadPicFromArchiveAsync(file, vm).ConfigureAwait(false);
            return;
        }
        Dispatcher.UIThread.Invoke(() =>
        {
           core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.CurrentView.Value = new ImageViewer2();
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
        var tab = core.MainWindows.ActiveWindow.CurrentValue.WindowTabs.ActiveTab.CurrentValue;
        tab.Image.Value = imageModel.Image;
        tab.Model = imageModel;
        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            var files = core.PlatformService.GetFiles(fileInfo);
            var index = files.FindIndex(x =>
                x.FullName.AsSpan().Equals(fileInfo.FullName.AsSpan(), StringComparison.OrdinalIgnoreCase));
            var (nextIndex, _) = IterationHelper.GetIteration(index, files.Count, NavigateTo.Next, SkipAmount.One);
            var nextFileInfo = files[nextIndex];
            var secondImageModel = await GetImageModel.GetImageModelAsync(nextFileInfo, magickImage).ConfigureAwait(false);
            tab.SecondaryModel = secondImageModel;
            UpdateImage.ChangeImage(tab, core.MainWindows.ActiveWindow.CurrentValue);
            UpdateImage.UpdateTabSideBySideTitles(core.MainWindows.ActiveWindow.CurrentValue.WindowTabs.ActiveTab.CurrentValue, index, nextIndex, fileInfo, nextFileInfo, files);
            TabNavigationInitializer.Initialize(core, files);
        }
        else
        {
            TabNavigationInitializer.Initialize(core, fileInfo);
        }

        if (Settings.WindowProperties.AutoFit)
        {
            WindowFunctions2.CenterWindowOnScreen();
        }

        core.MainWindows.ActiveWindow.Value.IsLoadingIndicatorShown.Value = false;
        FileHistoryManager.Add(fileInfo.FullName);

        if (Settings.Gallery.IsGalleryDocked)
        {
            if (Settings.Gallery.DockPosition is GalleryDockPosition.Closed)
            {
                Settings.Gallery.DockPosition = GalleryDockPosition.Bottom;
            }

            await GalleryLoader.LoadGalleryAsync(core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value,
                    core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.ImageIterator.Files,
                    new AvaloniaThumbnailLoader(),
                    core.SharedThumbnailCache,
                    core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.GetTabCancellation().Token)
                .ConfigureAwait(false);
        }
        else
        {
            Settings.Gallery.DockPosition = GalleryDockPosition.Closed;
        }

    }
}
