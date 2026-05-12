using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.Clipboard;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.Crop;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.SettingsManagement;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileHistory;
using PicView.Core.FileSorting;
using PicView.Core.Keybindings;
using PicView.Core.ProcessHandling;

namespace PicView.Avalonia.Functions;

/// <summary>
/// Used to map functions to their names, used for keyboard shortcuts
/// </summary>
/// // TODO replace with FunctionsMapper2
public static class FunctionsMapper
{
    public static MainViewModel? Vm;

    public static Func<ValueTask>? GetFunctionByName(string functionName)
    {
        // Remember to have exact matching names, or it will be null
        return functionName switch
        {
            // Navigation values
            "Next" => Next,
            "Prev" => Prev,
            
            "NextFolder" => NextFolder,
            "PrevFolder" => PrevFolder,
            
            "Up" => Up,
            "Down" => Down,
            
            "Last" => Last,
            "First" => First,
            
            "Next10" => Next10,
            "Prev10" => Prev10,
            
            "Next100" => Next100,
            "Prev100" => Prev100,

            "Search" => Search,

            "NextArchive" => NextArchive,
            "PrevArchive" => PrevArchive,
            
            // Rotate
            "RotateLeft" => RotateLeft,
            "RotateRight" => RotateRight,

            // Scroll
            "ScrollUp" => ScrollUp,
            "ScrollDown" => ScrollDown,
            "ScrollToTop" => ScrollToTop,
            "ScrollToBottom" => ScrollToBottom,

            // Zoom
            "ZoomIn" => ZoomIn,
            "ZoomOut" => ZoomOut,
            "ResetZoom" => ResetZoom,
            "ChangeCtrlZoom" => ChangeCtrlZoom,

            // Toggles
            "ToggleScroll" => ToggleScroll,
            "ToggleLooping" => ToggleLooping,
            "ToggleGallery" => ToggleGallery,

            // Scale Window
            "AutoFitWindow" => AutoFitWindow,
            "NormalWindow" => NormalWindow,

            // Window functions
            "Fullscreen" => Fullscreen,
            "ToggleFullscreen" => ToggleFullscreen,
            "SetTopMost" => SetTopMost,
            "Close" => Close,
            "ToggleInterface" => ToggleInterface,
            "NewWindow" => NewWindow,
            "Center" => Center,
            "Maximize" => Maximize,
            "Restore" => Restore,

            // Windows
            "AboutWindow" => AboutWindow,
            "EffectsWindow" => EffectsWindow,
            "ImageInfoWindow" => ImageInfoWindow,
            "ResizeWindow" => ResizeWindow,
            "SettingsWindow" => SettingsWindow,
            "KeybindingsWindow" => KeybindingsWindow,
            "BatchResizeWindow" => BatchResizeWindow,
            "ConvertWindow" => ConvertWindow,

            // Open functions
            "Open" => Open,
            "OpenWith" => OpenWith,
            "OpenInExplorer" => OpenInExplorer,
            "Save" => Save,
            "SaveAs" => SaveAs,
            "Print" => Print,
            "Reload" => Reload,

            // Copy functions
            "CopyFile" => CopyFile,
            "CopyFilePath" => CopyFilePath,
            "CopyImage" => CopyImage,
            "CopyBase64" => CopyBase64,
            "DuplicateFile" => DuplicateFile,
            "CutFile" => CutFile,
            "Paste" => Paste,

            // File functions
            "DeleteFile" => DeleteFile,
            "DeleteFilePermanently" => DeleteFilePermanently,
            "Rename" => Rename,
            "ShowFileProperties" => ShowFileProperties,
            "ShowSettingsFile" => ShowSettingsFile,
            "ShowKeybindingsFile" => ShowKeybindingsFile,
            
            // Sorting functions
            "SortFilesByName" => SortFilesByName,
            "SortFilesByCreationTime" => SortFilesByCreationTime,
            "SortFilesByLastAccessTime" => SortFilesByLastAccessTime,
            "SortFilesByLastWriteTime" => SortFilesByLastWriteTime,
            "SortFilesBySize" => SortFilesBySize,
            "SortFilesByExtension" => SortFilesByExtension,
            "SortFilesRandomly" => SortFilesRandomly,
            
            "SortFilesAscending" => SortFilesAscending,
            "SortFilesDescending" => SortFilesDescending,
            
            // Image functions
            "ResizeImage" => ResizeImage,
            "Crop" => Crop,
            "Flip" => Flip,
            "OptimizeImage" => OptimizeImage,
            "Stretch" => Stretch,

            // Set stars
            "Set0Star" => Set0Star,
            "Set1Star" => Set1Star,
            "Set2Star" => Set2Star,
            "Set3Star" => Set3Star,
            "Set4Star" => Set4Star,
            "Set5Star" => Set5Star,
            
            // Background and lock screen image
            "SetAsLockScreen" => SetAsLockScreen,
            "SetAsLockscreenCentered" => SetAsLockscreenCentered,
            "SetAsWallpaper" => SetAsWallpaper,
            "SetAsWallpaperFitted" => SetAsWallpaperFitted,
            "SetAsWallpaperStretched" => SetAsWallpaperStretched,
            "SetAsWallpaperFilled" => SetAsWallpaperFilled,
            "SetAsWallpaperCentered" => SetAsWallpaperCentered,
            "SetAsWallpaperTiled" => SetAsWallpaperTiled,

            // Misc
            "ChangeBackground" => ChangeBackground,
            "SideBySide" => SideBySide,
            "GalleryClick" => GalleryClick,
            "Slideshow" => Slideshow,
            "ColorPicker" => ColorPicker,
            "Restart" => Restart,

            _ => null
        };
    }

    #region Functions

    #region Menus

    public static Task CloseMenus()
    {
        return Task.CompletedTask;
    }

    public static Task ToggleFileMenu()
    {
        return Task.CompletedTask;
    }

    public static Task ToggleImageMenu()
    {
        return Task.CompletedTask;
    }

    public static Task ToggleSettingsMenu()
    {
        return Task.CompletedTask;
    }

    public static Task ToggleToolsMenu()
    {
        return Task.CompletedTask;
    }

    #endregion Menus

    #region Navigation, zoom and rotation

    /// <inheritdoc cref="NavigationManager.Iterate(bool, MainViewModel)" />
    public static async ValueTask Next() =>
        await ValueTask.CompletedTask;
    
    /// <inheritdoc cref="NavigationManager.NavigateBetweenDirectories(bool, MainViewModel)" />
    public static async ValueTask NextFolder() =>
        await ValueTask.CompletedTask;

    public static async ValueTask NextArchive() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="NavigationManager.NavigateFirstOrLast(bool, MainViewModel)" />
    public static async ValueTask Last() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="NavigationManager.Iterate(bool, MainViewModel)" />
    public static async ValueTask Prev() =>
        await ValueTask.CompletedTask;
    
    /// <inheritdoc cref="NavigationManager.NavigateBetweenDirectories(bool, MainViewModel)" />
    public static async ValueTask PrevFolder() =>
        await ValueTask.CompletedTask;

    public static async ValueTask PrevArchive() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="NavigationManager.NavigateFirstOrLast(bool, MainViewModel)" />
    public static async ValueTask First() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="NavigationManager.Next10(MainViewModel)" />
    public static async ValueTask Next10() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="NavigationManager.Next100(MainViewModel)" />
    public static async ValueTask Next100() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="NavigationManager.Prev10(MainViewModel)" />
    public static async ValueTask Prev10() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="NavigationManager.Prev100(MainViewModel)" />
    public static async ValueTask Prev100() =>
        await ValueTask.CompletedTask;

    public static async ValueTask Search() =>
        await Dispatcher.UIThread.InvokeAsync(DialogManager.AddFileSearchDialog);


    /// <inheritdoc cref="RotationNaRotationNavigationp(MainViewModel)" />
    public static ValueTask Up() => ValueTask.CompletedTask;

    /// <inheritdoc cref="RotationNavigation.RotateRight(MainViewModel)" />
    public static ValueTask RotateRight() =>ValueTask.CompletedTask;

    /// <inheritdoc cref="RotationNavigation.RotateLeft(MainViewModel)" />
    public static ValueTask RotateLeft() => ValueTask.CompletedTask;

    /// <inheritdoc cref="RotationNavigation.NavigateDown(MainViewModel)" />
    public static  ValueTask Down() =>ValueTask.CompletedTask;
    
    public static async ValueTask ScrollDown()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Vm.ImageViewer.ImageScrollViewer.LineDown();
        });
    }
    
    public static async ValueTask ScrollUp()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Vm.ImageViewer.ImageScrollViewer.LineUp();
        });
    }

    public static async ValueTask ScrollToTop()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Vm.ImageViewer.ImageScrollViewer.ScrollToHome();
        });
    }

    public static async ValueTask ScrollToBottom()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Vm.ImageViewer.ImageScrollViewer.ScrollToEnd();
        });
    }

    public static async ValueTask ZoomIn()
    {
        // TODO: ImageViewer Needs refactor
        if (Vm is null)
        {
            return;
        }
        // await Dispatcher.UIThread.InvokeAsync(Vm.ImageViewer.ZoomIn);
    }

    public static async ValueTask ZoomOut()
    {
        // TODO: ImageViewer Needs refactor
        if (Vm is null)
        {
            return;
        }
        // await Dispatcher.UIThread.InvokeAsync(Vm.ImageViewer.ZoomOut);
    }

    public static async ValueTask ResetZoom()
    {
        // TODO: ImageViewer Needs refactor
        if (Vm is null)
        {
            return;
        }

        // await Dispatcher.UIThread.InvokeAsync(() => Vm.ImageViewer.ResetZoom(Settings.Zoom.IsZoomAnimated));
    }
    
    #endregion

    #region Toggle UI functions

    /// <inheritdoc cref="SettingsUpdater.ToggleScroll(MainViewModel)" />
    public static async ValueTask ToggleScroll() =>
        await SettingsUpdater.ToggleScroll(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="SettingsUpdater.ToggleCtrlZoom(MainViewModel)" />
    public static async ValueTask ChangeCtrlZoom() =>
        await SettingsUpdater.ToggleCtrlZoom(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="SettingsUpdater.ToggleLooping(MainViewModel)" />
    public static async ValueTask ToggleLooping() =>
        await SettingsUpdater.ToggleLooping(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="HideInterfaceLogic.ToggleUI(MainViewModel)" />
    public static async ValueTask ToggleInterface() =>
        await HideInterfaceLogic.ToggleUI(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="SettingsUpdater.ToggleSubdirectories(MainViewModel)" />
    public static async ValueTask ToggleSubdirectories() =>
        await SettingsUpdater.ToggleSubdirectories(vm: Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="HideInterfaceLogic.ToggleBottomToolbar(MainViewModel)" />
    public static async ValueTask ToggleBottomToolbar() =>
        await HideInterfaceLogic.ToggleBottomToolbar(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="SettingsUpdater.ToggleValueTaskbarProgress(MainViewModel)" />
    public static async ValueTask ToggleTaskbarProgress() =>
        await SettingsUpdater.ToggleTaskbarProgress(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="SettingsUpdater.ToggleConstrainBackgroundColor(MainViewModel)" />
    public static async ValueTask ToggleConstrainBackgroundColor() =>
        await SettingsUpdater.ToggleConstrainBackgroundColor(Vm).ConfigureAwait(false);
    
    #endregion

    #region Gallery functions

    /// <inheritdoc cref="GalleryFunctions.ToggleGallery(MainViewModel)" />
    public static  ValueTask ToggleGallery() =>ValueTask.CompletedTask;

    /// <inheritdoc cref="GalleryFunctions.OpenCloseBottomGallery(MainViewModel)" />
    public static ValueTask OpenCloseBottomGallery()
    {
        try
        {
            return ValueTask.CompletedTask;
        }
        catch (Exception exception)
        {
            return ValueTask.FromException(exception);
        }
    }

    /// <inheritdoc cref="GalleryFunctions.CloseGallery(MainViewModel)" />
    public static ValueTask CloseGallery()
    {
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="GalleryNavigation.GalleryClick(MainViewModel)" />
    public static async ValueTask GalleryClick() =>
        await ValueTask.CompletedTask;

    #endregion
    
    #region Windows and window functions

    public static async Task ShowStartUpMenu()
    {
    }
    
    /// <inheritdoc cref="DialogManager.HandleShouldClosing" />
    public static async ValueTask Close() =>
        await DialogManager.HandleShouldClosing(Vm).ConfigureAwait(false);

    public static async ValueTask Center()
    {
    }

    /// <inheritdoc cref="Interfaces.IPlatformWindowService.MaximizeRestore" />
    public static async ValueTask Maximize()
    {
        await Vm.PlatformWindowService.MaximizeRestore();
    }
    
    /// <inheritdoc cref="Interfaces.IPlatformWindowService.Restore" />
    public static async ValueTask Restore()
    {
        await Vm.PlatformWindowService.Restore();
    }

    /// <inheritdoc cref="ProcessHelper.StartNewProcess()" />
    public static async ValueTask NewWindow() =>
        await Task.Run(ProcessHelper.StartNewProcess).ConfigureAwait(false);

    public static async ValueTask AboutWindow() =>
        await Dispatcher.UIThread.InvokeAsync(() => Vm?.PlatformWindowService?.ShowAboutWindow());

    public static async ValueTask ConvertWindow() =>
        await Dispatcher.UIThread.InvokeAsync(() => Vm?.PlatformWindowService?.ShowConvertWindow());

    public static async ValueTask KeybindingsWindow() =>
        await Dispatcher.UIThread.InvokeAsync(() => Vm?.PlatformWindowService?.ShowKeybindingsWindow());

    public static async ValueTask EffectsWindow() =>
        await Dispatcher.UIThread.InvokeAsync(() =>
            Vm?.PlatformWindowService?.ShowEffectsWindow());

    public static async ValueTask ImageInfoWindow() =>
        await Vm?.PlatformWindowService?.ShowImageInfoWindow();

    public static async ValueTask ResizeWindow() =>
        await Dispatcher.UIThread.InvokeAsync(() => Vm?.PlatformWindowService?.ShowSingleImageResizeWindow());

    public static async ValueTask BatchResizeWindow() =>
        await Vm?.PlatformWindowService?.ShowBatchResizeWindow();

    public static async ValueTask SettingsWindow() =>
        await Vm?.PlatformWindowService?.ShowSettingsWindow();

    #endregion Windows

    #region Image Scaling and Window Behavior

    /// <inheritdoc cref="WindowFunctions.Stretch(MainViewModel)" />
    public static async ValueTask Stretch() =>
        await ValueTask.CompletedTask;
    
    /// <inheritdoc cref="WindowFunctions.ToggleAutoFit(MainViewModel)" />
    public static async ValueTask AutoFitWindow() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="WindowFunctions.NormalWindow(MainViewModel)" />
    public static async ValueTask NormalWindow() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="Interfaces.IPlatformWindowService.ToggleFullscreen" />
    public static async ValueTask ToggleFullscreen() =>
        await Vm.PlatformWindowService.ToggleFullscreen().ConfigureAwait(false);
    
    // This shouldn't be here, but keep as alias and backwards compatibility.
    public static ValueTask Fullscreen() => ToggleFullscreen();

    /// <inheritdoc cref="WindowFunctions.ToggleTopMost(MainViewModel)" />
    public static async ValueTask SetTopMost() =>
        await ValueTask.CompletedTask;

    #endregion

    #region File funnctions

    /// <inheritdoc cref="NavigationManager.LoadPicFromStringAsync(string, MainViewModel)" />
    public static async Task OpenPreviousFileHistoryEntry() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="NavigationManager.LoadPicFromStringAsync(string, MainViewModel)" />
    public static async Task OpenNextFileHistoryEntry() =>
        await ValueTask.CompletedTask;
    
    /// <inheritdoc cref="FileManager.Print(string, MainViewModel)" />
    public static async ValueTask Print() =>
        await FileManager.Print(Vm.PicViewer.FileInfo?.CurrentValue?.FullName, Vm).ConfigureAwait(false);

    /// <inheritdoc cref="FilePicker.SelectAndLoadFile(MainViewModel)" />
    public static async ValueTask Open() =>
        await FilePicker.SelectAndLoadFile(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="FileManager.OpenWith(string, MainViewModel)" />
    public static async ValueTask OpenWith() =>
        await Task.Run(() => Vm?.PlatformService?.OpenWith(Vm.PicViewer.FileInfo?.CurrentValue?.FullName))
            .ConfigureAwait(false);
    
    /// <inheritdoc cref="FileManager.LocateOnDisk(string, MainViewModel)" />
    public static async ValueTask OpenInExplorer()=>
        await Task.Run(() => Vm?.PlatformService?.LocateOnDisk(Vm.PicViewer.FileInfo?.CurrentValue?.FullName))
            .ConfigureAwait(false);

    /// <inheritdoc cref="FileSaverHelper.SaveCurrentFile(MainViewModel)" />
    public static async ValueTask Save() =>
        await FileSaverHelper.SaveCurrentFile(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="FileSaverHelper.SaveFileAs(MainViewModel)" />
    public static async ValueTask SaveAs() =>
        await FileSaverHelper.SaveFileAs(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="FileManager.DeleteFileWithOptionalDialog" />
    public static async ValueTask DeleteFile() =>
        await FileManager
            .DeleteFileWithOptionalDialog(true, Vm.PicViewer?.FileInfo?.CurrentValue?.FullName, Vm.PlatformService)
            .ConfigureAwait(false);
    
    /// <inheritdoc cref="FileManager.DeleteFileWithOptionalDialog" />
    public static async ValueTask DeleteFilePermanently() =>
        await FileManager
            .DeleteFileWithOptionalDialog(false, Vm.PicViewer?.FileInfo?.CurrentValue?.FullName, Vm.PlatformService)
            .ConfigureAwait(false);

    public static async ValueTask Rename()
    {
        // TODO: Needs refactor for selecting file name
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            UIHelper.GetEditableTitlebar.SelectFileName();
        });
    }
    
    /// <inheritdoc cref="FileManager.ShowFileProperties(string, MainViewModel)" />
    public static async ValueTask ShowFileProperties() =>
        await Task.Run(() => Vm?.PlatformService?.ShowFileProperties(Vm.PicViewer.FileInfo?.CurrentValue.FullName)).ConfigureAwait(false);
    
    #endregion

    #region Copy and Paste functions

    /// <inheritdoc cref="ClipboardFileOperations.CopyFileToClipboard(string, MainViewModel)" />
    public static async ValueTask CopyFile() =>
        await ClipboardFileOperations.CopyFileToClipboard(Vm?.PicViewer.FileInfo?.CurrentValue.FullName).ConfigureAwait(false);
    
    /// <inheritdoc cref="ClipboardTextOperations.CopyTextToClipboard(string)" />
    public static async ValueTask CopyFilePath() => 
        await ClipboardTextOperations.CopyTextToClipboard(Vm?.PicViewer.FileInfo?.CurrentValue.FullName).ConfigureAwait(false);

    /// <inheritdoc cref="ClipboardImageOperations.CopyImageToClipboard(MainViewModel)" />
    public static async ValueTask CopyImage() => 
        await ClipboardImageOperations.CopyImageToClipboard(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="ClipboardImageOperations.CopyBase64ToClipboard(string, MainViewModel)" />
    public static async ValueTask CopyBase64() =>
        await ClipboardImageOperations.CopyBase64ToClipboard(Vm.PicViewer.FileInfo?.CurrentValue.FullName, vm: Vm).ConfigureAwait(false);

    /// <inheritdoc cref="ClipboardFileOperations.Duplicate(string, MainViewModel)" />
    public static async ValueTask DuplicateFile() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="ClipboardFileOperations.CutFile(string, MainViewModel)" />
    public static async ValueTask CutFile() =>
        await ValueTask.CompletedTask;

    /// <inheritdoc cref="ClipboardPasteOperations.Paste(MainViewModel)" />
    public static async ValueTask Paste() =>
        await ValueTask.CompletedTask;
    
    #endregion

    #region Image Functions
    
    /// <inheritdoc cref="BackgroundManager.ChangeBackground(MainViewModel)" />
    public static async ValueTask ChangeBackground() =>
        await BackgroundManager.ChangeBackgroundAsync(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="SettingsUpdater.ToggleSideBySide(MainViewModel)" />
    public static async ValueTask SideBySide() =>
        await SettingsUpdater.ToggleSideBySide(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="ErrorHandling.ReloadAsync(MainViewModel)" />
    public static async ValueTask Reload() =>
        await ValueTask.CompletedTask;

    public static async ValueTask ResizeImage() =>
        await ResizeWindow();

    /// <inheritdoc cref="CropFunctions.StartCropControl(MainViewModel)" />
    public static async ValueTask Crop() =>
        await CropFunctions.StartCropControlAsync(Vm).ConfigureAwait(false);

    public static ValueTask Flip() => ValueTask.CompletedTask;

    /// <inheritdoc cref="ImageOptimizer.OptimizeImageAsync(MainViewModel)" />
    public static async ValueTask OptimizeImage() =>
        await ImageOptimizer.OptimizeImageAsync(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="Navigation.Slideshow.StartSlideshow(MainViewModel)" />
    public static async ValueTask Slideshow() =>
        await Navigation.Slideshow.StartSlideshow(Vm).ConfigureAwait(false);

    public static ValueTask ColorPicker()
    {
        throw new NotImplementedException();
    }
    
    #endregion

    #region Sorting

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public static async ValueTask SortFilesByName() =>
        await FileListManager.UpdateFileList(Vm.PlatformService, Vm, SortFilesBy.Name).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public static async ValueTask SortFilesByCreationTime() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.CreationTime).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public static async ValueTask SortFilesByLastAccessTime() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.LastAccessTime).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public static async ValueTask SortFilesByLastWriteTime() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.LastWriteTime).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public static async ValueTask SortFilesBySize() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.FileSize).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public static async ValueTask SortFilesByExtension() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.Extension).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public static async ValueTask SortFilesRandomly() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.Random).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, bool)" />
    public static async ValueTask SortFilesAscending() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, ascending: true).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, bool)" />
    public static async ValueTask SortFilesDescending() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, ascending: false).ConfigureAwait(false);

    #endregion Sorting

    #region Rating

    public static async ValueTask Set0Star()
        => await ValueTask.CompletedTask;

    public static async ValueTask Set1Star()
        => await ValueTask.CompletedTask;

    public static async ValueTask Set2Star()
        => await ValueTask.CompletedTask;

    public static async ValueTask Set3Star()
        => await ValueTask.CompletedTask;

    public static async ValueTask Set4Star()
        => await ValueTask.CompletedTask;

    public static async ValueTask Set5Star()
        => await ValueTask.CompletedTask;

    #endregion

    #region Wallpaper and lockscreen image

    public static async ValueTask SetAsWallpaper() =>
        await SetAsWallpaperFilled();

    public static async ValueTask SetAsWallpaperTiled() =>
        await Task.Run(() => Vm.PlatformService.SetAsWallpaper(Vm.PicViewer.FileInfo.CurrentValue.FullName, 0)).ConfigureAwait(false);
    
    public static async ValueTask SetAsWallpaperCentered() =>
        await Task.Run(() => Vm.PlatformService.SetAsWallpaper(Vm.PicViewer.FileInfo.CurrentValue.FullName, 1)).ConfigureAwait(false);
    
    public static async ValueTask SetAsWallpaperStretched() =>
        await Task.Run(() => Vm.PlatformService.SetAsWallpaper(Vm.PicViewer.FileInfo.CurrentValue.FullName, 2)).ConfigureAwait(false);
    
    public static async ValueTask SetAsWallpaperFitted() =>
        await Task.Run(() => Vm.PlatformService.SetAsWallpaper(Vm.PicViewer.FileInfo.CurrentValue.FullName, 3)).ConfigureAwait(false);
    
    public static async ValueTask SetAsWallpaperFilled() =>
        await Task.Run(() => Vm.PlatformService.SetAsWallpaper(Vm.PicViewer.FileInfo.CurrentValue.FullName, 4)).ConfigureAwait(false);
    
    public static async ValueTask SetAsLockscreenCentered() =>
        await Task.Run(() => Vm.PlatformService.SetAsLockScreen(Vm.PicViewer.FileInfo.CurrentValue.FullName)).ConfigureAwait(false);
    
    public static async ValueTask SetAsLockScreen() =>
        await Task.Run(() => Vm.PlatformService.SetAsLockScreen(Vm.PicViewer.FileInfo.CurrentValue.FullName)).ConfigureAwait(false);

    #endregion

    #region Other settings

    /// <inheritdoc cref="SettingsUpdater.ResetSettings(MainViewModel)" />
    public static async ValueTask ResetSettings() =>
        await SettingsUpdater.ResetSettings(Vm).ConfigureAwait(false);
    
    public static async ValueTask Restart()
    {
        // TODO: Needs refactoring into its own method
        var openFile = string.Empty;
        var getFromArgs = false;
        if (Vm?.PicViewer.FileInfo is not null)
        {
            if (Vm.PicViewer.FileInfo.CurrentValue.Exists)
            {
                openFile = Vm.PicViewer.FileInfo.CurrentValue.FullName;
            }
            else
            {
                getFromArgs = true;
            }
        }
        else
        {
            getFromArgs = true;
        }
        if (getFromArgs)
        {
            var args = Environment.GetCommandLineArgs();
            if (args is not null && args.Length > 0)
            {
                openFile = args[1];
            }
        }
        ProcessHelper.RestartApp(openFile);

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            Environment.Exit(0);
            return;
        }
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            desktop.MainWindow?.Close();
        });
    }
    
    public static async ValueTask ShowSettingsFile() =>
        await Task.Run(() => Vm?.PlatformService?.OpenWith(CurrentSettingsPath)).ConfigureAwait(false);
    
    public static async ValueTask ShowKeybindingsFile() =>
        await Task.Run(() => Vm?.PlatformService?.OpenWith(KeybindingFunctions.CurrentKeybindingsPath)).ConfigureAwait(false);
    
    public static async ValueTask ShowRecentHistoryFile() =>
        await Task.Run(() => Vm?.PlatformService?.OpenWith(FileHistoryManager.CurrentFileHistoryFile)).ConfigureAwait(false);
    
    public static async ValueTask ToggleOpeningInSameWindow() =>
        await SettingsUpdater.ToggleOpeningInSameWindow(Vm).ConfigureAwait(false);
    
    public static async ValueTask ToggleFileHistory() =>
        await SettingsUpdater.ToggleFileHistory(Vm).ConfigureAwait(false);

    #endregion
    
    #endregion
    
#if DEBUG
    public static void Invalidate()
    {
        // Vm?.ImageViewer?.MainImage?.InvalidateVisual();
        // Vm?.ImageViewer?.InvalidateVisual();
        // Vm?.ImageViewer?.MainImage?.InvalidateMeasure();
        // Vm?.ImageViewer?.InvalidateMeasure();
    }
#endif
}