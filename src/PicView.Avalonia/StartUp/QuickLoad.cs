using System.Diagnostics;
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
using PicView.Core.FileSorting;
using PicView.Core.Gallery;
using PicView.Core.Http;
using PicView.Core.Localization;
using PicView.Core.Models;
using PicView.Core.Navigation;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.StartUp;

/// <summary>
/// Provides methods for quickly loading the image first, and then initializing the rest of the navigation.
/// </summary>
public static class QuickLoad
{
    /// <summary>
    /// Asynchronously loads an image, archive, URL, base64 string, or directory into the application view,
    /// updating the UI state and loading indicative properties as necessary.
    /// </summary>
    /// <param name="core">The main view model.</param>
    /// <param name="source">The file, URL, or directory path to be loaded.</param>
    /// <param name="continueFromLeftOff">A boolean indicating whether to continue loading from the last session folder structure.</param>
    public static async ValueTask QuickLoadAsync(CoreViewModel core, string source, bool continueFromLeftOff)
    {        
        var fileInfo = new FileInfo(source);
        if (!fileInfo.Exists) // If not file, try to load if URL, base64 or directory
        {
            core.MainWindows.ActiveWindow.Value.IsLoadingIndicatorShown.Value = true;
            var check = FileTypeResolver.CheckIfLoadableString(source);
            if (check is null)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.CurrentView.Value = new StartUpMenu();
                    core.MainWindows.ActiveWindow.Value.IsLoadingIndicatorShown.Value = false;
                }, DispatcherPriority.Send);
                return;
            }

            switch (check.Value.Type)
            {
                case FileTypeResolver.LoadAbleFileType.File:
                    await LoadSingleFileAsync(core, fileInfo).ConfigureAwait(false);
                    break;
                case FileTypeResolver.LoadAbleFileType.Directory:
                {
                    var files = FileListRetriever.RetrieveFiles(new FileInfo(check.Value.Data),core.PlatformService.CompareStrings);
                    if (files.Count == 0)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.CurrentView.Value = new StartUpMenu();
                        }, DispatcherPriority.Send);
                        return;
                    }
                    await LoadSingleFileAsync(core, files[0], files).ConfigureAwait(false);
                    break;
                }
                case FileTypeResolver.LoadAbleFileType.Web:
                {
                    await LoadUrlImageAsync(core, check.Value.Data).ConfigureAwait(false);
                    break;
                }
                case FileTypeResolver.LoadAbleFileType.Base64:
                case FileTypeResolver.LoadAbleFileType.Zip:
                    throw new NotImplementedException();
                default:
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.CurrentView.Value = new StartUpMenu();
                        core.MainWindows.ActiveWindow.Value.IsLoadingIndicatorShown.Value = false;
                    }, DispatcherPriority.Send);
                    break;
            }
            core.MainWindows.ActiveWindow.Value.IsLoadingIndicatorShown.Value = false;
            return;
        }

        if (source.IsArchive()) // Handle if file exist and is an archive
        {
     //       vm.MainWindow.IsLoadingIndicatorShown.Value = true;);
          //  await NavigationManager.LoadPicFromArchiveAsync(file, vm).ConfigureAwait(false);
            return;
        }
        else
        {
            await LoadSingleFileAsync(core, fileInfo).ConfigureAwait(false);
        }
    }

    private static async ValueTask LoadUrlImageAsync(CoreViewModel core, string url)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.CurrentView.Value = new ImageViewer();
        }, DispatcherPriority.Send);
        var safeFileName = HttpManager.GetSafeFileName(url);
        var destPath = TempFileManager.GetNewTempFilePath(safeFileName);
        using var client = new HttpClientDownloadWithProgress(url, destPath);
        Debug.Assert(core.MainWindows.ActiveWindow.CurrentValue != null);
        var tab = core.MainWindows.ActiveWindow.CurrentValue.WindowTabs.ActiveTab.CurrentValue;
        TabNavigationInitializer.Initialize(core);
        client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
        {
            var displayProgress = HttpManager.GetProgressDisplay(totalFileSize, totalBytesDownloaded, progressPercentage);
            var title = $"{safeFileName} {TranslationManager.Translation?.Downloading} {displayProgress}";

            // Update UI properties
            if (tab.TabTitle.Value != title) tab.TabTitle.Value = title;
            if (tab.Title.Value != title) tab.Title.Value = title;
            if (tab.WindowTitle.Value != title) tab.WindowTitle.Value = title;
            if (tab.TitleTooltip.Value != title) tab.TitleTooltip.Value = title;

            // if (totalBytesDownloaded.HasValue && totalFileSize.HasValue)
            // {
            //     platformService.SetTaskbarProgress((ulong)totalBytesDownloaded.Value, (ulong)totalFileSize.Value);
            // }
        };
        await client.StartDownloadAsync(CancellationToken.None).ConfigureAwait(false);
        var model = await GetImageModel.GetImageModelAsync(new FileInfo(destPath)).ConfigureAwait(false);
        tab.Model = model;
        tab.SourceURL = url;
        tab.SingleImageType = SingleImageType.Url;
        
        tab.TabTitle.Value = safeFileName;
        tab.Title.Value = safeFileName;
        tab.WindowTitle.Value = safeFileName;
        tab.TitleTooltip.Value = destPath;

        FileHistoryManager.Add(url);
    }

    private static async ValueTask LoadSingleFileAsync(CoreViewModel core, FileInfo fileInfo, List<FileInfo>? files = null)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
           core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.CurrentView.Value = new ImageViewer();
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
            files ??= core.PlatformService.GetFiles(fileInfo);
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
            WindowFunctions.CenterWindowOnScreen();
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
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (tab.CurrentView.CurrentValue is ImageViewer imageViewer)
                {
                    var gallery = imageViewer.GalleryView.GalleryItemsControl;
                    gallery.CurrentItemIndex = tab.NavigationIndex.Value;
                    gallery.UpdatePreviousAndNextSelection(tab.NavigationIndex.Value, -1);
                    gallery.ScrollToCenterOfCurrentItem();
                }
            });

        }
        else
        {
            Settings.Gallery.DockPosition = GalleryDockPosition.Closed;
        }
    }
}
