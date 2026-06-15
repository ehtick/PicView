using System.Diagnostics;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.Navigation.Services;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ArchiveHandling;
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
        if (!fileInfo.Exists) // If not file, try to load if URL or directory
        {
            var check = FileTypeResolver.CheckIfLoadableString(source);
            if (check is null)
            {
                RevertToStartUpMenuOnFail(core);
                return;
            }

            switch (check.Value.Type)
            {
                case FileTypeResolver.LoadAbleFileType.Directory:
                {
                    var files = FileListRetriever.RetrieveFiles(new FileInfo(check.Value.Data),core.PlatformService.CompareStrings);
                    if (files.Count == 0)
                    {
                        RevertToStartUpMenuOnFail(core);
                        return;
                    }
                    await LoadSingleFileAsync(core, files[0], continueFromLeftOff, files).ConfigureAwait(false);
                    return;
                }
                case FileTypeResolver.LoadAbleFileType.Web:
                {
                    await LoadUrlImageAsync(core, check.Value.Data).ConfigureAwait(false);
                    return;
                }
                default:
                    RevertToStartUpMenuOnFail(core);
                    return;
            }
        }
        
        if (source.IsArchive())
        {
            await LoadArchiveFileAsync(core, fileInfo).ConfigureAwait(false);
        }
        else
        {
            await LoadSingleFileAsync(core, fileInfo, continueFromLeftOff).ConfigureAwait(false);
        }
    }

    private static void RevertToStartUpMenuOnFail(CoreViewModel core)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.CurrentView.Value = new StartUpMenu();
            core.MainWindows.ActiveWindow.Value.IsLoadingIndicatorShown.Value = false;
        }, DispatcherPriority.Send);
        TabNavigationInitializer.Initialize(core);
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

            tab.TabTitle.Value = 
            tab.Title.Value = 
            tab.WindowTitle.Value = 
            tab.TitleTooltip.Value = title;

            if (!Settings.UIProperties.IsTaskbarProgressEnabled || !totalBytesDownloaded.HasValue || !totalFileSize.HasValue)
            {
                return;
            }

            var downloadedBytes = (ulong)totalBytesDownloaded.Value;
            var totalSize = (ulong)totalFileSize.Value;
            core.PlatformService.SetTaskbarProgress(downloadedBytes, totalSize);

        };
        await client.StartDownloadAsync(CancellationToken.None).ConfigureAwait(false);
        var model = await GetImageModel.GetImageModelAsync(new FileInfo(destPath)).ConfigureAwait(false);
        tab.Model = model;
        tab.SourceURL = url;
        tab.SingleImageType = SingleImageType.Url;
        tab.UpdateTabTitle();

        FileHistoryManager.Add(url);

        if (Settings.UIProperties.IsTaskbarProgressEnabled)
        {
            core.PlatformService.StopTaskbarProgress();
        }
    }

    private static async ValueTask LoadSingleFileAsync(CoreViewModel core,
        FileInfo fileInfo,
        bool continueFromLeftOff,
        List<FileInfo>? files = null)
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
        var tab = core.MainWindows.ActiveWindow.CurrentValue.WindowTabs.ActiveTab.CurrentValue;
        tab.SetLoading();
        var imageModel = await GetImageModel.GetImageModelAsync(fileInfo, magickImage).ConfigureAwait(false);
        tab.Image.Value = imageModel.Image;
        tab.Model = imageModel;
        var initialDirectory = GetInitialDirectory(!Settings.ImageScaling.ShowImageSideBySide, fileInfo);

        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            files ??= core.PlatformService.GetFiles(initialDirectory);
            var index = files.FindIndex(x =>
                x.FullName.AsSpan().Equals(fileInfo.FullName.AsSpan(), StringComparison.OrdinalIgnoreCase));
            var (nextIndex, _) = IterationHelper.GetIteration(index, files.Count, NavigateTo.Next, SkipAmount.One);
            var nextFileInfo = files[nextIndex];
            var secondImageModel = await GetImageModel.GetImageModelAsync(nextFileInfo).ConfigureAwait(false);
            tab.SecondaryModel = secondImageModel;
            UpdateImage.ChangeImage(tab, core.MainWindows.ActiveWindow.CurrentValue);
            UpdateImage.UpdateTabSideBySideTitles(core.MainWindows.ActiveWindow.CurrentValue.WindowTabs.ActiveTab.CurrentValue, index, nextIndex, fileInfo, nextFileInfo, files);
            TabNavigationInitializer.Initialize(core, files);
        }
        else
        {
            UpdateImage.ChangeImage(tab, core.MainWindows.ActiveWindow.CurrentValue);
            if (files is null)
            {
                TabNavigationInitializer.Initialize(core, initialDirectory);
            }
            else
            {
                TabNavigationInitializer.Initialize(core, files);
            }
        }

        if (Settings.WindowProperties.AutoFit)
        {
            WindowFunctions.CenterWindowOnScreen();
        }

        core.MainWindows.ActiveWindow.Value.IsLoadingIndicatorShown.Value = false;
        FileHistoryManager.Add(fileInfo.FullName);

        await LoadGalleryIfNeeded(core, tab).ConfigureAwait(false);
        if (continueFromLeftOff)
        {
            Settings.StartUp.StartUpDirectory = initialDirectory.FullName;
        }
        magickImage.Dispose();
    }
    
    private static async ValueTask LoadArchiveFileAsync(CoreViewModel core, FileInfo source)
    {
        var tab = core.MainWindows.ActiveWindow.CurrentValue.WindowTabs.ActiveTab.CurrentValue;
        Dispatcher.UIThread.Invoke(() =>
        {
            core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.CurrentView.Value = new ImageViewer();
        }, DispatcherPriority.Send);
        TabNavigationInitializer.Initialize(core, source);
        core.MainWindows.ActiveWindow.Value.IsLoadingIndicatorShown.Value = true;
        tab.SetLoading();

        var isArchiveLoaded = await core.MainWindows.ActiveWindow.CurrentValue.WindowTabs.LoadFromArchiveAsync(source.FullName).ConfigureAwait(false);
        if (!isArchiveLoaded)
        {
            RevertToStartUpMenuOnFail(core);
            return;
        }
        core.MainWindows.ActiveWindow.Value.IsLoadingIndicatorShown.Value = false;
        await LoadGalleryIfNeeded(core, tab).ConfigureAwait(false);
    }

    private static async ValueTask LoadGalleryIfNeeded(CoreViewModel core, TabViewModel tab)
    {
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

    private static FileInfo GetInitialDirectory(bool continueFromLeftOff, FileInfo fileInfo)
    {
        if (!continueFromLeftOff)
        {
            return fileInfo;
        }

        if (!string.IsNullOrWhiteSpace(Settings.StartUp.StartUpDirectory) && !ArchiveExtraction.IsArchived)
        {
            return fileInfo.FullName.Contains(Settings.StartUp.StartUpDirectory) ?
                new FileInfo(Settings.StartUp.StartUpDirectory) : new FileInfo(fileInfo.DirectoryName);
        }
        return fileInfo;
    }
}
