using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.Clipboard;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.Crop;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.ImageTransformations.Rotation;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.SettingsManagement;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Exif;
using PicView.Core.FileHistory;
using PicView.Core.FileSorting;
using PicView.Core.ImageDecoding;
using PicView.Core.Keybindings;
using PicView.Core.ProcessHandling;

namespace PicView.Avalonia.Functions;

/// <summary>
/// Used to map functions to their names, used for keyboard shortcuts
/// </summary>
public static class FunctionsMapper
{
    public static MainViewModel? Vm;

    public static Task<Func<Task>> GetFunctionByName(string functionName)
    {
        // Remember to have exact matching names, or it will be null
        return Task.FromResult<Func<Task>>(functionName switch
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
            "AutoFitWindowAndStretch" => AutoFitWindowAndStretch,
            "NormalWindow" => NormalWindow,
            "NormalWindowAndStretch" => NormalWindowAndStretch,

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
        });
    }

    #region Functions

    #region Menus

    public static Task CloseMenus()
    {
        if (Vm is null)
        {
            return Task.CompletedTask;
        }
        MenuManager.CloseMenus(Vm);
        return Task.CompletedTask;
    }

    public static Task ToggleFileMenu()
    {
        if (Vm is null)
        {
            return Task.CompletedTask;
        }
        MenuManager.ToggleFileMenu(Vm);
        return Task.CompletedTask;
    }

    public static Task ToggleImageMenu()
    {
        if (Vm is null)
        {
            return Task.CompletedTask;
        }
        MenuManager.ToggleImageMenu(Vm);
        return Task.CompletedTask;
    }

    public static Task ToggleSettingsMenu()
    {
        if (Vm is null)
        {
            return Task.CompletedTask;
        }
        MenuManager.ToggleSettingsMenu(Vm);
        return Task.CompletedTask;
    }

    public static Task ToggleToolsMenu()
    {
        if (Vm is null)
        {
            return Task.CompletedTask;
        }
        MenuManager.ToggleToolsMenu(Vm);
        return Task.CompletedTask;
    }

    #endregion Menus

    #region Navigation, zoom and rotation

    /// <inheritdoc cref="NavigationManager.Iterate(bool, MainViewModel)" />
    public static async Task Next() =>
        await NavigationManager.Iterate(next: true, Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="NavigationManager.NavigateBetweenDirectories(bool, MainViewModel)" />
    public static async Task NextFolder() =>
        await NavigationManager.NavigateBetweenDirectories(true, Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="NavigationManager.NavigateFirstOrLast(bool, MainViewModel)" />
    public static async Task Last() =>
        await NavigationManager.NavigateFirstOrLast(last: true, Vm).ConfigureAwait(false);

    /// <inheritdoc cref="NavigationManager.Iterate(bool, MainViewModel)" />
    public static async Task Prev() =>
        await NavigationManager.Iterate(next: false, Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="NavigationManager.NavigateBetweenDirectories(bool, MainViewModel)" />
    public static async Task PrevFolder() =>
        await NavigationManager.NavigateBetweenDirectories(false, Vm).ConfigureAwait(false);

    /// <inheritdoc cref="NavigationManager.NavigateFirstOrLast(bool, MainViewModel)" />
    public static async Task First() =>
        await NavigationManager.NavigateFirstOrLast(last: false, Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="NavigationManager.Next10(MainViewModel)" />
    public static async Task Next10() =>
        await NavigationManager.Next10(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="NavigationManager.Next100(MainViewModel)" />
    public static async Task Next100() =>
        await NavigationManager.Next100(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="NavigationManager.Prev10(MainViewModel)" />
    public static async Task Prev10() =>
        await NavigationManager.Prev10(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="NavigationManager.Prev100(MainViewModel)" />
    public static async Task Prev100() =>
        await NavigationManager.Prev100(Vm).ConfigureAwait(false);
    

    /// <inheritdoc cref="RotationNaRotationNavigationp(MainViewModel)" />
    public static async Task Up() =>
        await RotationNavigation.NavigateUp(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="RotationNavigation.RotateRight(MainViewModel)" />
    public static async Task RotateRight() =>
        await RotationNavigation.RotateRight(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="RotationNavigation.RotateLeft(MainViewModel)" />
    public static async Task RotateLeft() =>
        await RotationNavigation.RotateLeft(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="RotationNavigation.NavigateDown(MainViewModel)" />
    public static async Task Down() =>
        await RotationNavigation.NavigateDown(Vm).ConfigureAwait(false);
    
    public static async Task ScrollDown()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Vm.ImageViewer.ImageScrollViewer.LineDown();
        });
    }
    
    public static async Task ScrollUp()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Vm.ImageViewer.ImageScrollViewer.LineUp();
        });
    }

    public static async Task ScrollToTop()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Vm.ImageViewer.ImageScrollViewer.ScrollToHome();
        });
    }

    public static async Task ScrollToBottom()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Vm.ImageViewer.ImageScrollViewer.ScrollToEnd();
        });
    }

    public static async Task ZoomIn()
    {
        // TODO: ImageViewer Needs refactor
        if (Vm is null)
        {
            return;
        }
        await Dispatcher.UIThread.InvokeAsync(Vm.ImageViewer.ZoomIn);
    }

    public static async Task ZoomOut()
    {
        // TODO: ImageViewer Needs refactor
        if (Vm is null)
        {
            return;
        }
        await Dispatcher.UIThread.InvokeAsync(Vm.ImageViewer.ZoomOut);
    }

    public static async Task ResetZoom()
    {
        // TODO: ImageViewer Needs refactor
        if (Vm is null)
        {
            return;
        }
        await Dispatcher.UIThread.InvokeAsync(() => Vm.ImageViewer.ResetZoom(true));
    }
    
    #endregion

    #region Toggle UI functions

    /// <inheritdoc cref="SettingsUpdater.ToggleScroll(MainViewModel)" />
    public static async Task ToggleScroll() =>
        await SettingsUpdater.ToggleScroll(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="SettingsUpdater.ToggleCtrlZoom(MainViewModel)" />
    public static async Task ChangeCtrlZoom() =>
        await SettingsUpdater.ToggleCtrlZoom(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="SettingsUpdater.ToggleLooping(MainViewModel)" />
    public static async Task ToggleLooping() =>
        await SettingsUpdater.ToggleLooping(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="HideInterfaceLogic.ToggleUI(MainViewModel)" />
    public static async Task ToggleInterface() =>
        await HideInterfaceLogic.ToggleUI(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="SettingsUpdater.ToggleSubdirectories(MainViewModel)" />
    public static async Task ToggleSubdirectories() =>
        await SettingsUpdater.ToggleSubdirectories(vm: Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="HideInterfaceLogic.ToggleBottomToolbar(MainViewModel)" />
    public static async Task ToggleBottomToolbar() =>
        await HideInterfaceLogic.ToggleBottomToolbar(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="SettingsUpdater.ToggleTaskbarProgress(MainViewModel)" />
    public static async Task ToggleTaskbarProgress() =>
        await SettingsUpdater.ToggleTaskbarProgress(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="SettingsUpdater.ToggleConstrainBackgroundColor(MainViewModel)" />
    public static async Task ToggleConstrainBackgroundColor() =>
        await SettingsUpdater.ToggleConstrainBackgroundColor(Vm).ConfigureAwait(false);
    
    #endregion

    #region Gallery functions

    /// <inheritdoc cref="GalleryFunctions.ToggleGallery(MainViewModel)" />
    public static Task ToggleGallery() =>
        Task.Run(() => GalleryFunctions.ToggleGallery(Vm));

    /// <inheritdoc cref="GalleryFunctions.OpenCloseBottomGallery(MainViewModel)" />
    public static Task OpenCloseBottomGallery() =>
        Task.Run(() => GalleryFunctions.OpenCloseBottomGallery(Vm));
    
    /// <inheritdoc cref="GalleryFunctions.CloseGallery(MainViewModel)" />
    public static Task CloseGallery() =>
        Task.Run(() => GalleryFunctions.CloseGallery(Vm));
    
    /// <inheritdoc cref="GalleryNavigation.GalleryClick(MainViewModel)" />
    public static async Task GalleryClick() =>
        await GalleryNavigation.GalleryClick(Vm).ConfigureAwait(false);

    #endregion
    
    #region Windows and window functions

    public static async Task ShowStartUpMenu()
    {
        //TODO: Needs refactor, add async overload for ShowStartUpMenu
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ErrorHandling.ShowStartUpMenu(Vm);
        });
    }
    
    /// <inheritdoc cref="DialogManager.HandleShouldClosing" />
    public static async Task Close() =>
        await DialogManager.HandleShouldClosing(Vm).ConfigureAwait(false);

    public static async Task Center() =>
        await UIHelper.CenterAsync(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="Interfaces.IPlatformWindowService.MaximizeRestore" />
    public static Task Maximize()
    {
        Vm.PlatformWindowService.MaximizeRestore();
        return Task.CompletedTask; 
    }
    
    /// <inheritdoc cref="Interfaces.IPlatformWindowService.Restore" />
    public static Task Restore()
    {
        Vm.PlatformWindowService.Restore();
        return Task.CompletedTask; 
    }

    /// <inheritdoc cref="ProcessHelper.StartNewProcess()" />
    public static async Task NewWindow() =>
        await Task.Run(ProcessHelper.StartNewProcess).ConfigureAwait(false);

    public static Task AboutWindow()
    {
        Dispatcher.UIThread.Invoke(() => Vm?.PlatformWindowService?.ShowAboutWindow());
        return Task.CompletedTask;
    }

    public static Task KeybindingsWindow()
    {
        Dispatcher.UIThread.Invoke(() => Vm?.PlatformWindowService?.ShowKeybindingsWindow());
        return Task.CompletedTask;
    }

    public static Task EffectsWindow()
    {
        Dispatcher.UIThread.Invoke(() => Vm?.PlatformWindowService?.ShowEffectsWindow());
        return Task.CompletedTask;
    }

    public static async Task ImageInfoWindow() => await Vm?.PlatformWindowService?.ShowImageInfoWindow();

    public static Task ResizeWindow()
    {
        Dispatcher.UIThread.Invoke(() => Vm?.PlatformWindowService?.ShowSingleImageResizeWindow());
        return Task.CompletedTask;
    }
    
    public static Task BatchResizeWindow()
    {
        Dispatcher.UIThread.Invoke(() => Vm?.PlatformWindowService?.ShowBatchResizeWindow());
        return Task.CompletedTask;
    }

    public static Task SettingsWindow()
    {
        Dispatcher.UIThread.Invoke(() => Vm?.PlatformWindowService?.ShowSettingsWindow());
        return Task.CompletedTask;
    }
    
    #endregion Windows

    #region Image Scaling and Window Behavior
    
    /// <inheritdoc cref="WindowFunctions.Stretch(MainViewModel)" />
    public static async Task Stretch() =>
        await WindowFunctions.Stretch(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="WindowFunctions.ToggleAutoFit(MainViewModel)" />
    public static async Task AutoFitWindow() =>
        await WindowFunctions.ToggleAutoFit(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="WindowFunctions.AutoFitAndStretch(MainViewModel)" />
    public static async Task AutoFitWindowAndStretch() =>
        await WindowFunctions.AutoFitAndStretch(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="WindowFunctions.NormalWindow(MainViewModel)" />
    public static async Task NormalWindow() =>
        await WindowFunctions.NormalWindow(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="WindowFunctions.NormalWindowStretch(MainViewModel)" />
    public static async Task NormalWindowAndStretch() =>
        await WindowFunctions.NormalWindowStretch(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="Interfaces.IPlatformWindowService.ToggleFullscreen" />
    public static async Task ToggleFullscreen() =>
        await Vm.PlatformWindowService.ToggleFullscreen().ConfigureAwait(false);
    
    // This shouldn't be here, but keep as alias and backwards compatibility.
    public static Task Fullscreen() => ToggleFullscreen();

    /// <inheritdoc cref="WindowFunctions.ToggleTopMost(MainViewModel)" />
    public static async Task SetTopMost() =>

        await WindowFunctions.ToggleTopMost(Vm).ConfigureAwait(false);

    #endregion

    #region File funnctions

    /// <inheritdoc cref="NavigationManager.LoadPicFromStringAsync(string, MainViewModel)" />
    public static async Task OpenLastFile() =>
        await NavigationManager.LoadLastFileAsync(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="NavigationManager.LoadPicFromStringAsync(string, MainViewModel)" />
    public static async Task OpenPreviousFileHistoryEntry() =>
        await NavigationManager.LoadPicFromStringAsync(FileHistoryManager.GetPreviousEntry(), Vm).ConfigureAwait(false);
   
    /// <inheritdoc cref="NavigationManager.LoadPicFromStringAsync(string, MainViewModel)" />
    public static async Task OpenNextFileHistoryEntry() =>
        await NavigationManager.LoadPicFromStringAsync(FileHistoryManager.GetNextEntry(), Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="FileManager.Print(string, MainViewModel)" />
    public static async Task Print() =>
        await FileManager.Print(null, Vm).ConfigureAwait(false);

    /// <inheritdoc cref="FilePicker.SelectAndLoadFile(MainViewModel)" />
    public static async Task Open() =>
        await FilePicker.SelectAndLoadFile(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="FileManager.OpenWith(string, MainViewModel)" />
    public static async Task OpenWith() =>
        await Task.Run(() => Vm?.PlatformService?.OpenWith(Vm.PicViewer.FileInfo?.CurrentValue.FullName)).ConfigureAwait(false);
    
    /// <inheritdoc cref="FileManager.LocateOnDisk(string, MainViewModel)" />
    public static async Task OpenInExplorer()=>
        await Task.Run(() => Vm?.PlatformService?.LocateOnDisk(Vm.PicViewer.FileInfo?.CurrentValue.FullName)).ConfigureAwait(false);

    /// <inheritdoc cref="FileSaverHelper.SaveCurrentFile(MainViewModel)" />
    public static async Task Save() =>
        await FileSaverHelper.SaveCurrentFile(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="FileSaverHelper.SaveFileAs(MainViewModel)" />
    public static async Task SaveAs() =>
        await FileSaverHelper.SaveFileAs(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="FileManager.DeleteFileWithOptionalDialog" />
    public static async Task DeleteFile() =>
        await FileManager.DeleteFileWithOptionalDialog(true, Vm.PicViewer?.FileInfo?.CurrentValue.FullName, Vm.PlatformService).ConfigureAwait(false);
    
    /// <inheritdoc cref="FileManager.DeleteFileWithOptionalDialog" />
    public static async Task DeleteFilePermanently() =>
        await FileManager.DeleteFileWithOptionalDialog(false, Vm.PicViewer?.FileInfo?.CurrentValue.FullName, Vm.PlatformService).ConfigureAwait(false);

    public static async Task Rename()
    {
        // TODO: Needs refactor for selecting file name
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            UIHelper.GetEditableTitlebar.SelectFileName();
        });
    }
    
    /// <inheritdoc cref="FileManager.ShowFileProperties(string, MainViewModel)" />
    public static async Task ShowFileProperties() =>
        await Task.Run(() => Vm?.PlatformService?.ShowFileProperties(Vm.PicViewer.FileInfo?.CurrentValue.FullName)).ConfigureAwait(false);
    
    #endregion

    #region Copy and Paste functions

    /// <inheritdoc cref="ClipboardFileOperations.CopyFileToClipboard(string, MainViewModel)" />
    public static async Task CopyFile() =>
        await ClipboardFileOperations.CopyFileToClipboard(Vm?.PicViewer.FileInfo?.CurrentValue.FullName, Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="ClipboardTextOperations.CopyTextToClipboard(string)" />
    public static async Task CopyFilePath() => 
        await ClipboardTextOperations.CopyTextToClipboard(Vm?.PicViewer.FileInfo?.CurrentValue.FullName).ConfigureAwait(false);

    /// <inheritdoc cref="ClipboardImageOperations.CopyImageToClipboard(MainViewModel)" />
    public static async Task CopyImage() => 
        await ClipboardImageOperations.CopyImageToClipboard(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="ClipboardImageOperations.CopyBase64ToClipboard(string, MainViewModel)" />
    public static async Task CopyBase64() =>
        await ClipboardImageOperations.CopyBase64ToClipboard(Vm.PicViewer.FileInfo?.CurrentValue.FullName, vm: Vm).ConfigureAwait(false);

    /// <inheritdoc cref="ClipboardFileOperations.Duplicate(string, MainViewModel)" />
    public static async Task DuplicateFile() => 
        await ClipboardFileOperations.Duplicate(Vm.PicViewer.FileInfo?.CurrentValue.FullName, Vm).ConfigureAwait(false);

    /// <inheritdoc cref="ClipboardFileOperations.CutFile(string, MainViewModel)" />
    public static async Task CutFile() =>
        await ClipboardFileOperations.CutFile(Vm.PicViewer.FileInfo.CurrentValue.FullName, Vm).ConfigureAwait(false);

    /// <inheritdoc cref="ClipboardPasteOperations.Paste(MainViewModel)" />
    public static async Task Paste() =>
        await ClipboardPasteOperations.Paste(Vm).ConfigureAwait(false);
    
    #endregion

    #region Image Functions
    
    /// <inheritdoc cref="BackgroundManager.ChangeBackground(MainViewModel)" />
    public static async Task ChangeBackground() =>
        await BackgroundManager.ChangeBackgroundAsync(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="SettingsUpdater.ToggleSideBySide(MainViewModel)" />
    public static async Task SideBySide() =>
        await SettingsUpdater.ToggleSideBySide(Vm).ConfigureAwait(false);
    
    /// <inheritdoc cref="ErrorHandling.ReloadAsync(MainViewModel)" />
    public static async Task Reload() =>
        await ErrorHandling.ReloadAsync(Vm).ConfigureAwait(false);

    public static Task ResizeImage() =>
        ResizeWindow();

    /// <inheritdoc cref="CropFunctions.StartCropControl(MainViewModel)" />
    public static async Task Crop() =>
        await CropFunctions.StartCropControlAsync(Vm).ConfigureAwait(false);

    public static Task Flip()
    {
        RotationNavigation.Flip(Vm);
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ImageOptimizer.OptimizeImageAsync(MainViewModel)" />
    public static async Task OptimizeImage() =>
        await ImageOptimizer.OptimizeImageAsync(Vm).ConfigureAwait(false);

    /// <inheritdoc cref="Navigation.Slideshow.StartSlideshow(MainViewModel)" />
    public static async Task Slideshow() =>
        await Navigation.Slideshow.StartSlideshow(Vm).ConfigureAwait(false);

    public static Task ColorPicker()
    {
        return Task.CompletedTask;
    }
    
    #endregion

    #region Sorting

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, FileSortOrder.SortFilesBy)" />
    public static async Task SortFilesByName() =>
        await FileListManager.UpdateFileList(Vm.PlatformService, Vm, SortFilesBy.Name).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, FileSortOrder.SortFilesBy)" />
    public static async Task SortFilesByCreationTime() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.CreationTime).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, FileSortOrder.SortFilesBy)" />
    public static async Task SortFilesByLastAccessTime() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.LastAccessTime).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, FileSortOrder.SortFilesBy)" />
    public static async Task SortFilesByLastWriteTime() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.LastWriteTime).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, FileSortOrder.SortFilesBy)" />
    public static async Task SortFilesBySize() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.FileSize).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, FileSortOrder.SortFilesBy)" />
    public static async Task SortFilesByExtension() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.Extension).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, FileSortOrder.SortFilesBy)" />
    public static async Task SortFilesRandomly() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, SortFilesBy.Random).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, bool)" />
    public static async Task SortFilesAscending() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, ascending: true).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, bool)" />
    public static async Task SortFilesDescending() =>
        await FileListManager.UpdateFileList(Vm?.PlatformService, Vm, ascending: false).ConfigureAwait(false);

    #endregion Sorting

    #region Rating

    public static async Task Set0Star()
        => await SetExifRatingHelper.Set0Star(Vm);

    public static async Task Set1Star()
        => await SetExifRatingHelper.Set1Star(Vm);

    public static async Task Set2Star()
        => await SetExifRatingHelper.Set2Star(Vm);

    public static async Task Set3Star()
        => await SetExifRatingHelper.Set3Star(Vm);

    public static async Task Set4Star()
        => await SetExifRatingHelper.Set4Star(Vm);

    public static async Task Set5Star()
        => await SetExifRatingHelper.Set5Star(Vm);

    #endregion

    #region Open GPS link

    public static async Task OpenGoogleMaps()
    {
        // TODO: Needs refactoring into its own method
        if (Vm is null)
        {
            return;
        }
        if (string.IsNullOrEmpty(Vm.Exif.GoogleLink.CurrentValue))
        {
            return;
        }

        await Task.Run(() => ProcessHelper.OpenLink(Vm.Exif.GoogleLink.CurrentValue));
    }
    
    public static async Task OpenBingMaps()
    {
        // TODO: Needs refactoring into its own method
        if (Vm is null)
        {
            return;
        }
        if (string.IsNullOrEmpty(Vm.Exif.BingLink.CurrentValue))
        {
            return;
        }

        await Task.Run(() => ProcessHelper.OpenLink(Vm.Exif.BingLink.CurrentValue));
    }

    #endregion

    #region Wallpaper and lockscreen image

    public static async Task SetAsWallpaper() =>
        await SetAsWallpaperFilled();

    public static async Task SetAsWallpaperTiled() =>
        await Task.Run(() => Vm.PlatformService.SetAsWallpaper(Vm.PicViewer.FileInfo.CurrentValue.FullName, 0)).ConfigureAwait(false);
    
    public static async Task SetAsWallpaperCentered() =>
        await Task.Run(() => Vm.PlatformService.SetAsWallpaper(Vm.PicViewer.FileInfo.CurrentValue.FullName, 1)).ConfigureAwait(false);
    
    public static async Task SetAsWallpaperStretched() =>
        await Task.Run(() => Vm.PlatformService.SetAsWallpaper(Vm.PicViewer.FileInfo.CurrentValue.FullName, 2)).ConfigureAwait(false);
    
    public static async Task SetAsWallpaperFitted() =>
        await Task.Run(() => Vm.PlatformService.SetAsWallpaper(Vm.PicViewer.FileInfo.CurrentValue.FullName, 3)).ConfigureAwait(false);
    
    public static async Task SetAsWallpaperFilled() =>
        await Task.Run(() => Vm.PlatformService.SetAsWallpaper(Vm.PicViewer.FileInfo.CurrentValue.FullName, 4)).ConfigureAwait(false);
    
    public static async Task SetAsLockscreenCentered() =>
        await Task.Run(() => Vm.PlatformService.SetAsLockScreen(Vm.PicViewer.FileInfo.CurrentValue.FullName)).ConfigureAwait(false);
    
    public static async Task SetAsLockScreen() =>
        await Task.Run(() => Vm.PlatformService.SetAsLockScreen(Vm.PicViewer.FileInfo.CurrentValue.FullName)).ConfigureAwait(false);

    #endregion

    #region Other settings

    /// <inheritdoc cref="SettingsUpdater.ResetSettings(MainViewModel)" />
    public static async Task ResetSettings() =>
        await SettingsUpdater.ResetSettings(Vm).ConfigureAwait(false);
    
    public static async Task Restart()
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
    
    public static async Task ShowSettingsFile() =>
        await Task.Run(() => Vm?.PlatformService?.OpenWith(CurrentSettingsPath)).ConfigureAwait(false);
    
    public static async Task ShowKeybindingsFile() =>
        await Task.Run(() => Vm?.PlatformService?.OpenWith(KeybindingFunctions.CurrentKeybindingsPath)).ConfigureAwait(false);
    
    public static async Task ShowRecentHistoryFile() =>
        await Task.Run(() => Vm?.PlatformService?.OpenWith(FileHistoryManager.CurrentFileHistoryFile)).ConfigureAwait(false);
    
    public static async Task ToggleOpeningInSameWindow() =>
        await SettingsUpdater.ToggleOpeningInSameWindow(Vm).ConfigureAwait(false);
    
    public static async Task ToggleFileHistory() =>
        await SettingsUpdater.ToggleFileHistory(Vm).ConfigureAwait(false);

    #endregion
    
    #endregion
    
#if DEBUG
    public static void Invalidate()
    {
        Vm?.ImageViewer?.MainImage?.InvalidateVisual();
        Vm?.ImageViewer?.InvalidateVisual();
        Vm?.ImageViewer?.MainImage?.InvalidateMeasure();
        Vm?.ImageViewer?.InvalidateMeasure();
    }
#endif
}