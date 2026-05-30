using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using PicView.Avalonia.Clipboard;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.Crop;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.ImageTransformations;
using PicView.Avalonia.SettingsManagement;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.Input;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileHistory;
using PicView.Core.FileSorting;
using PicView.Core.IPlatform;
using PicView.Core.Keybindings;
using PicView.Core.Navigation;
using PicView.Core.ProcessHandling;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Functions;

public class FunctionsMapper(MainWindowViewModel vm, Window window) : IFunctionsMapper
{
    public Func<ValueTask>? GetFunctionByName(string functionName)
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
            "Rotate0" => Rotate0,
            "Rotate90" => Rotate90,
            "Rotate180" => Rotate180,
            "Rotate270" => Rotate270,

            // Scroll
            "ScrollUp" => ScrollUp,
            "ScrollDown" => ScrollDown,
            "ScrollToTop" => ScrollToTop,
            "ScrollToBottom" => ScrollToBottom,

            // Zoom
            "ZoomIn" => ZoomIn,
            "ZoomOut" => ZoomOut,
            "ResetZoom" => ResetZoom,
            "ResetZoomAndRotations" => ResetZoomAndRotations,
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
            "CheckForUpdates" => CheckForUpdates,
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
            "SaveAsPDF" => SaveAsPDF,
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
            "ShowRecentHistoryFile" => ShowRecentHistoryFile,
            
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
            "Stretch" => ZoomToFit,

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
            
            // Tabs
            "NewTab" => NewTab,
            "CloseTab" => CloseTab,

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

    public ValueTask ToggleDropDownMenu()
    {
        vm.TopTitlebarViewModel.ToggleDropDownMenu(default);
        return ValueTask.CompletedTask;
    }

    #endregion Menus

    #region Navigation, zoom and rotation

    /// <inheritdoc cref="Core.ViewModels.TabOverviewViewModel.NextFile()" />
    public async ValueTask Next() =>
        await vm.WindowTabs.NavigateDirectionalAsync(MainKeyboardShortcuts2.IsKeyHeldDown,
            NavigateTo.Next).ConfigureAwait(false);

    /// <inheritdoc cref="NavigationManager.NavigateBetweenDirectories(bool, MainViewModel)" />
    public async ValueTask NextFolder() =>
        await vm.WindowTabs.NextFolder().ConfigureAwait(false);

    public async ValueTask NextArchive()
    {
        
    }
    
    /// <inheritdoc cref="NavigationManager.NavigateFirstOrLast(bool, MainViewModel)" />
    public async ValueTask Last() =>
        await vm.WindowTabs.LastFile().ConfigureAwait(false);

    /// <inheritdoc cref="Core.ViewModels.TabOverviewViewModel.PrevFile()" />
    public async ValueTask Prev() =>
        await vm.WindowTabs.NavigateDirectionalAsync(MainKeyboardShortcuts2.IsKeyHeldDown,
            NavigateTo.Previous).ConfigureAwait(false);

    /// <inheritdoc cref="NavigationManager.NavigateBetweenDirectories(bool, MainViewModel)" />
    public async ValueTask PrevFolder() =>
        await vm.WindowTabs.PrevFolder().ConfigureAwait(false);

    public async ValueTask PrevArchive()
    {
        
    }

    /// <inheritdoc cref="NavigationManager.NavigateFirstOrLast(bool, MainViewModel)" />
    public async ValueTask First() =>
        await vm.WindowTabs.FirstFile().ConfigureAwait(false);
    
    /// <inheritdoc cref="Core.ViewModels.TabOverviewViewModel.Next10()" />
    public async ValueTask Next10() =>
        await vm.WindowTabs.Next10().ConfigureAwait(false);

    /// <inheritdoc cref="Core.ViewModels.TabOverviewViewModel.Next100()" />
    public async ValueTask Next100() =>
        await vm.WindowTabs.Next100().ConfigureAwait(false);
    
    /// <inheritdoc cref="NavigationManager.Prev10(MainViewModel)" />
    public async ValueTask Prev10() =>
        await vm.WindowTabs.Prev10().ConfigureAwait(false);
    
    /// <inheritdoc cref="NavigationManager.Prev100(MainViewModel)" />
    public async ValueTask Prev100()
    {
        await vm.WindowTabs.Prev100().ConfigureAwait(false);
        // await NavigationManager.Prev100(vm).ConfigureAwait(false);
    }

    public async ValueTask Search() =>
        await Dispatcher.UIThread.InvokeAsync(DialogManager.AddFileSearchDialog);
    

    /// <inheritdoc cref="RotationNaRotationNavigationp(MainViewModel)" />
    public async ValueTask Up()
    {
        if (vm.WindowTabs.ActiveTab.Value.Gallery.IsGalleryExpanded.Value)
        {
            await vm.WindowTabs.NavigateDirectionalAsync(MainKeyboardShortcuts2.IsKeyHeldDown, NavigateTo.Up).ConfigureAwait(false);
            return;
        }

        await RotateRight();
    }

    /// <inheritdoc cref="RotationNavigation.RotateRight(MainViewModel)" />
    public ValueTask RotateRight()
    {
        RotationManager.RotateRight(vm);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="RotationNavigation.RotateLeft(MainViewModel)" />
    public ValueTask RotateLeft()
    {
        RotationManager.RotateLeft(vm);
        return ValueTask.CompletedTask;
    }

    public ValueTask Rotate0()
    {
        RotationManager.Rotate(vm, 0);
        return ValueTask.CompletedTask;
    }

    public ValueTask Rotate90()
    {
        RotationManager.Rotate(vm, 90);
        return ValueTask.CompletedTask;
    }

    public ValueTask Rotate180()
    {
        RotationManager.Rotate(vm, 180);
        return ValueTask.CompletedTask;
    }
    
    public ValueTask Rotate270()
    {
        RotationManager.Rotate(vm, 270);
        return ValueTask.CompletedTask;
    }
    
    public ValueTask Flip()
    {
        RotationManager.Flip(vm);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="RotationNavigation.NavigateDown(MainViewModel)" />
    public async ValueTask Down()
    {
        if (vm.WindowTabs.ActiveTab.Value.Gallery.IsGalleryExpanded.Value)
        {
            await vm.WindowTabs.NavigateDirectionalAsync(MainKeyboardShortcuts2.IsKeyHeldDown, NavigateTo.Down).ConfigureAwait(false);
            return;
        }

        await RotateLeft();
    }
    
    public async ValueTask ScrollDown()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // vm.ImageViewer.ImageScrollViewer.LineDown();
        });
    }
    
    public async ValueTask ScrollUp()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // vm.ImageViewer.ImageScrollViewer.LineUp();
        });
    }

    public async ValueTask ScrollToTop()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // vm.ImageViewer.ImageScrollViewer.ScrollToHome();
        });
    }

    public async ValueTask ScrollToBottom()
    {
        // TODO: ImageViewer Needs refactor
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // vm.ImageViewer.ImageScrollViewer.ScrollToEnd();
        });
    }

    public ValueTask ZoomIn()
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer imageViewer)
        {
            imageViewer.ZoomIn();
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask ZoomOut()
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer imageViewer)
        {
            imageViewer.ZoomOut();
        }
        return ValueTask.CompletedTask;
    }
    
    public ValueTask ResetZoomAndRotations()
    {
        RotationManager.ResetZoomAndRotations(vm);
        return ValueTask.CompletedTask;
    }

    public ValueTask ResetZoom()
    {
        RotationManager.ResetZoom(vm);
        return ValueTask.CompletedTask;
    }

    #endregion

        #region Toggle UI functions

    /// <inheritdoc cref="SettingsUpdater.ToggleScroll(MainViewModel)" />
    public async ValueTask ToggleScroll()
    {
        await SettingsUpdater.ToggleScroll(vm).ConfigureAwait(false);
    }

    /// <inheritdoc cref="SettingsUpdater.ToggleCtrlZoom(MainViewModel)" />
    public async ValueTask ChangeCtrlZoom()
    {
        // await SettingsUpdater.ToggleCtrlZoom(vm).ConfigureAwait(false);
        return;
    }

    /// <inheritdoc cref="SettingsUpdater.ToggleLooping(MainViewModel)" />
    public async ValueTask ToggleLooping()
    {
        await SettingsUpdater.ToggleLooping(vm).ConfigureAwait(false);
    }
    
    /// <inheritdoc cref="HideInterfaceLogic.ToggleUI(MainViewModel)" />
    public async ValueTask ToggleInterface()
    {
        // await HideInterfaceLogic.ToggleUI(vm).ConfigureAwait(false);
        return;
    }
    
    /// <inheritdoc cref="SettingsUpdater.ToggleSubdirectories(MainViewModel)" />
    public async ValueTask ToggleSubdirectories()
    {
        await SettingsUpdater.ToggleSubdirectories(vm).ConfigureAwait(false);
    }
    
    /// <inheritdoc cref="HideInterfaceLogic.ToggleBottomToolbar(MainViewModel)" />
    public async ValueTask ToggleBottomToolbar()
    {
        await ToggleUIVisibility.ToggleBottomBar(vm);
    }
    
    /// <inheritdoc cref="SettingsUpdater2.ToggleTaskbarProgress(MainWindowViewModel)" />
    public async ValueTask ToggleTaskbarProgress()
    {
        await SettingsUpdater.ToggleTaskbarProgress(vm).ConfigureAwait(false);
    }
    
    /// <inheritdoc cref="SettingsUpdater.ToggleConstrainBackgroundColor()" />
    public async ValueTask ToggleConstrainBackgroundColor()
    {
        await SettingsUpdater.ToggleConstrainBackgroundColor().ConfigureAwait(false);
    }
    
    #endregion

    #region Gallery functions

    /// <inheritdoc cref="GalleryFunctions.ToggleGallery(MainViewModel)" />
    public async ValueTask ToggleGallery()
    {
        vm.WindowTabs.ActiveTab.CurrentValue.Gallery.ToggleGalleryCommand.Execute(Unit.Default);
    }

    /// <inheritdoc cref="GalleryFunctions.OpenCloseBottomGallery(MainViewModel)" />
    public async ValueTask OpenCloseBottomGallery()
    {
        // await Task.Run(() => GalleryFunctions.OpenCloseBottomGallery(vm));
        return;
    }
    
    /// <inheritdoc cref="GalleryFunctions.CloseGallery(MainViewModel)" />
    public ValueTask CloseGallery()
    {
        // GalleryFunctions.CloseGallery(vm);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="GalleryNavigation.GalleryClick(MainViewModel)" />
    public ValueTask GalleryClick()
    {
        var gallery = vm.WindowTabs.ActiveTab.CurrentValue.Gallery;
        var index = gallery.SelectedGalleryItemIndex.Value;
        if (index > -1)
        {
            gallery.OpenSelectedItemCommand.Execute(index);
        }
        
        return ValueTask.CompletedTask;
    }

    #endregion
    
    #region Windows and window functions

    public async ValueTask ShowStartUpMenu()
    {
        //TODO: Needs refactor, add async overload for ShowStartUpMenu
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // ErrorHandling.ShowStartUpMenu(vm);
        });
    }
    
    /// <inheritdoc cref="DialogManager.HandleShouldClosing" />
    public async ValueTask Close()
    {
        await DialogManager.HandleShouldClosing(vm);
    }
    
    public ValueTask Exit()
    {
        DialogManager.CloseMainWindow();
        return ValueTask.CompletedTask;
    }

    public ValueTask Center()
    {
        UIHelper.Center(vm);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="Interfaces.IPlatformWindowService.MaximizeRestore" />
    public async ValueTask Maximize()
    {
        await vm.PlatformWindowService.MaximizeRestore();
    }
    
    /// <inheritdoc cref="Interfaces.IPlatformWindowService.Restore" />
    public async ValueTask Restore()
    {
        await vm.PlatformWindowService.Restore();
    }

    public ValueTask Minimize()
    {
        vm.PlatformWindowService.Minimize();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="ProcessHelper.StartNewProcess()" />
    public async ValueTask NewWindow() =>
        await Task.Run(ProcessHelper.StartNewProcess).ConfigureAwait(false);

    public ValueTask AboutWindow()
    {
        vm?.PlatformWindowService?.ShowAboutWindow();
        return ValueTask.CompletedTask;
    }

    public async ValueTask CheckForUpdates()
    {
        await Dispatcher.UIThread.InvokeAsync(() => vm.PlatformWindowService?.ShowAboutWindow());
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        await core.AboutView.UpdateCurrentVersion();
    }

    public ValueTask ConvertWindow()
    {
        vm?.PlatformWindowService?.ShowConvertWindow();
        return ValueTask.CompletedTask;
    }

    public ValueTask KeybindingsWindow()
    {
        vm?.PlatformWindowService?.ShowKeybindingsWindow();
        return ValueTask.CompletedTask;
    }

    public ValueTask EffectsWindow()
    {
        vm?.PlatformWindowService?.ShowEffectsWindow();
        return ValueTask.CompletedTask;
    }

    public ValueTask ImageInfoWindow()
    {
        vm?.PlatformWindowService?.ShowImageInfoWindow();
        return ValueTask.CompletedTask;
    }

    public ValueTask ResizeWindow()
    {
        vm?.PlatformWindowService?.ShowSingleImageResizeWindow();
        return ValueTask.CompletedTask;
    }

    public ValueTask BatchResizeWindow()
    {
        vm?.PlatformWindowService?.ShowBatchResizeWindow();
        return ValueTask.CompletedTask;
    }

    public ValueTask SettingsWindow() =>
        vm.PlatformWindowService.ShowSettingsWindow();

    #endregion Windows

    #region Image Scaling and Window Behavior
    
    /// <inheritdoc cref="SettingsUpdater2.ToggleZoomToFit" />
    public async ValueTask ZoomToFit()
    {
        await SettingsUpdater.ToggleZoomToFit(vm);
    }
    
    /// <inheritdoc cref="WindowFunctions.ToggleAutoFit(MainWindowViewModel, Window)" />
    public async ValueTask AutoFitWindow()
    {
        await WindowFunctions.ToggleAutoFit(vm, window);
    }

    /// <inheritdoc cref="WindowFunctions.SetManualWindow(MainWindowViewModel)" />
    public ValueTask NormalWindow()
    {
        WindowFunctions.SetManualWindow(vm);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="Interfaces.IPlatformWindowService.ToggleFullscreen" />
    public async ValueTask ToggleFullscreen() =>
        await vm.PlatformWindowService.ToggleFullscreen().ConfigureAwait(false);
    
    // This shouldn't be here, but keep as alias and backwards compatibility.
    public ValueTask Fullscreen() => ToggleFullscreen();

    /// <inheritdoc cref="WindowFunctions.ToggleTopMost(MainWindowViewModel)" />
    public async ValueTask SetTopMost()
    {
        await WindowFunctions.ToggleTopMost(vm).ConfigureAwait(false);
    }

    #endregion

    #region File funnctions

    /// <inheritdoc cref=" UIHelper.OpenLastFile(MainWindowViewModel)" />
    public async ValueTask OpenLastFile()
    {
        await UIHelper.OpenLastFile(vm);
    }

    public async ValueTask OpenPreviousFileHistoryEntry()
    {
        await UIHelper.OpenPreviousFileHistoryEntry(vm).ConfigureAwait(false);
    }
   
    public async ValueTask OpenNextFileHistoryEntry()
    {
        await UIHelper.OpenNextFileHistoryEntry(vm).ConfigureAwait(false);
    }
    
    public async ValueTask Print()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        await Task.Run(() => core.PlatformService.Print(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo?.FullName));
    }
    
    public async ValueTask SaveAsPDF()
    {
        await PdfExport.SavePdfWithFilePicker(vm);
    }

    /// <inheritdoc cref="FilePicker.SelectAndLoadFile(MainWindowViewModel)" />
    public async ValueTask Open()
    {
        await FilePicker.SelectAndLoadFile(vm).ConfigureAwait(false);
    }

    /// <inheritdoc cref="FileManager.OpenWith(string)" />
    public ValueTask OpenWith()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return ValueTask.CompletedTask;
        }

        core.PlatformService.OpenWith(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo?.FullName);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="FileManager.LocateOnDisk(string)" />
    public ValueTask OpenInExplorer()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return ValueTask.CompletedTask;
        }

        core.PlatformService.LocateOnDisk(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo?.FullName);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="FileSaverHelper.SaveCurrentFile(MainWindowViewModel)" />
    public async ValueTask Save()
    {
        await FileSaverHelper.SaveCurrentFile(vm).ConfigureAwait(false);
    }
    
    /// <inheritdoc cref="FileSaverHelper.SaveFileAs(MainWindowViewModel)" />
    public async ValueTask SaveAs()
    {
        await FileSaverHelper.SaveFileAs(vm).ConfigureAwait(false);
    }
    
    /// <inheritdoc cref="FileManager.DeleteFileWithOptionalDialog" />
    public async ValueTask DeleteFile()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        const bool recycle = true;
        await FileManager
            .DeleteFileWithOptionalDialog(recycle, vm.WindowTabs.ActiveTab.CurrentValue.Model
                .FileInfo?.FullName, core.PlatformService)
            .ConfigureAwait(false);
    }
    
    /// <inheritdoc cref="FileManager.DeleteFileWithOptionalDialog" />
    public async ValueTask DeleteFilePermanently()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        const bool recycle = false;
        await FileManager
            .DeleteFileWithOptionalDialog(recycle, vm.WindowTabs.ActiveTab.CurrentValue.Model
                .FileInfo?.FullName, core.PlatformService)
            .ConfigureAwait(false);
    }

    public async ValueTask Rename()
    {
        // TODO: Needs refactor for selecting file name
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            UIHelper.GetEditableTitlebar.SelectFileName();
        });
    }

    /// <inheritdoc cref="FileManager.ShowFileProperties(string, MainViewModel)" />
    public async ValueTask ShowFileProperties()
    {
        await Task.Run(() =>
            FileManager.ShowFileProperties(vm.WindowTabs.ActiveTab.CurrentValue.Model
                .FileInfo?.FullName)).ConfigureAwait(false);
    }

    #endregion

    #region Copy and Paste functions

    /// <inheritdoc cref="ClipboardFileOperations.CopyFileToClipboard(string, MainViewModel)" />
    public async ValueTask CopyFile()
    {
        await ClipboardFileOperations.CopyFileToClipboard(vm.WindowTabs.ActiveTab.CurrentValue.Model
            .FileInfo?.FullName).ConfigureAwait(false);
    }
    
    /// <inheritdoc cref="ClipboardTextOperations.CopyTextToClipboard(string)" />
    public async ValueTask CopyFilePath()
    {
        await ClipboardTextOperations.CopyTextToClipboard(vm.WindowTabs.ActiveTab.CurrentValue.Model
            .FileInfo?.FullName).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ClipboardImageOperations.CopyImageToClipboard(MainViewModel)" />
    public async ValueTask CopyImage()
    {
        await ClipboardImageOperations.CopyImageToClipboard(vm).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ClipboardImageOperations.CopyBase64ToClipboard(string, MainViewModel)" />
    public async ValueTask CopyBase64()
    {
        await ClipboardImageOperations.CopyBase64ToClipboard(vm.WindowTabs.ActiveTab.CurrentValue?.FileInfo?.CurrentValue?.FullName, vm).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ClipboardFileOperations.Duplicate(string, MainViewModel)" />
    public async ValueTask DuplicateFile()
    {
        await ClipboardFileOperations.Duplicate(vm.WindowTabs.ActiveTab.CurrentValue.Model
            .FileInfo?.FullName, vm).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ClipboardFileOperations.CutFile(string, MainViewModel)" />
    public async ValueTask CutFile()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        await ClipboardFileOperations.CutFile(vm.WindowTabs.ActiveTab.CurrentValue.Model
            .FileInfo?.FullName, core.PlatformService).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ClipboardPasteOperations.Paste(MainViewModel)" />
    public async ValueTask Paste()
    {
        await ClipboardPasteOperations.Paste(vm).ConfigureAwait(false);
    }
    
    #endregion

    #region Image Functions
    
    /// <inheritdoc cref="BackgroundManager.ChangeBackground(MainWindowViewModel)" />
    public async ValueTask ChangeBackground()
    {
        await BackgroundManager.ChangeBackgroundAsync(vm).ConfigureAwait(false);
    }
    
    /// <inheritdoc cref="SettingsUpdater.ToggleSideBySide(MainWindowViewModel)" />
    public async ValueTask SideBySide()
    {
        await SettingsUpdater.ToggleSideBySide().ConfigureAwait(false);
    }
    
    /// <inheritdoc cref="ErrorHandling.ReloadAsync(MainViewModel)" />
    public async ValueTask Reload()
    {
        await vm.WindowTabs.ActiveTab.CurrentValue.ImageIterator.ReloadAsync(vm.WindowTabs.ActiveTab.CurrentValue.GetTabCancellation()).ConfigureAwait(false);
    }

    public async ValueTask ResizeImage() =>
        await ResizeWindow();

    /// <inheritdoc cref="CropManager.StartCropControlAsync(MainWindowViewModel)" />
    public async ValueTask Crop()
    {
        await CropManager.StartCropControlAsync(vm).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ImageOptimizer.OptimizeImageAsync(MainWindowViewModel)" />
    public async ValueTask OptimizeImage()
    {
        await ImageOptimizer.OptimizeImageAsync(vm).ConfigureAwait(false);
    }

    /// <inheritdoc cref="Navigation.Slideshow.StartSlideshow(MainWindowViewModel)" />
    public async ValueTask Slideshow()
    {
        await Navigation.Slideshow.StartSlideshow(vm).ConfigureAwait(false);
    }

    public ValueTask ColorPicker()
    {
        throw new NotImplementedException();
    }
    
    #endregion

    #region Sorting

    /// <inheritdoc cref="Core.Navigation.Interfaces.INavigationService.SortAsync(TabViewModel, bool, CancellationTokenSource)" />
    public async ValueTask SortFilesByName() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.Name).ConfigureAwait(false);

    /// <inheritdoc cref="Core.Navigation.Interfaces.INavigationService.SortAsync(TabViewModel, bool, CancellationTokenSource)" />
    public async ValueTask SortFilesByCreationTime() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.CreationTime).ConfigureAwait(false);

    /// <inheritdoc cref="Core.Navigation.Interfaces.INavigationService.SortAsync(TabViewModel, bool, CancellationTokenSource)" />
    public async ValueTask SortFilesByLastAccessTime() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.LastAccessTime).ConfigureAwait(false);

    /// <inheritdoc cref="Core.Navigation.Interfaces.INavigationService.SortAsync(TabViewModel, bool, CancellationTokenSource)" />
    public async ValueTask SortFilesByLastWriteTime() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.LastWriteTime).ConfigureAwait(false);

    /// <inheritdoc cref="Core.Navigation.Interfaces.INavigationService.SortAsync(TabViewModel, bool, CancellationTokenSource)" />
    public async ValueTask SortFilesBySize() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.FileSize).ConfigureAwait(false);

    /// <inheritdoc cref="Core.Navigation.Interfaces.INavigationService.SortAsync(TabViewModel, bool, CancellationTokenSource)" />
    public async ValueTask SortFilesByExtension() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.Extension).ConfigureAwait(false);

    /// <inheritdoc cref="Core.Navigation.Interfaces.INavigationService.SortAsync(TabViewModel, bool, CancellationTokenSource)" />
    public async ValueTask SortFilesRandomly() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.Random).ConfigureAwait(false);

    /// <inheritdoc cref="Core.Navigation.Interfaces.INavigationService.SortAsync(TabViewModel, bool, CancellationTokenSource)" />
    public async ValueTask SortFilesAscending() =>
        await vm.WindowTabs.SortAsync(ascending: true).ConfigureAwait(false);
    
    /// <inheritdoc cref="Core.Navigation.Interfaces.INavigationService.SortAsync(TabViewModel, bool, CancellationTokenSource)" />
    public async ValueTask SortFilesDescending() =>
        await vm.WindowTabs.SortAsync(ascending: false).ConfigureAwait(false);

    #endregion Sorting

    #region Rating

    public async ValueTask Set0Star()
    {
        await SetExifRatingHelper.Set0Star(vm);
    }

    public async ValueTask Set1Star()
    {
        await SetExifRatingHelper.Set1Star(vm);
    }

    public async ValueTask Set2Star()
    {
        await SetExifRatingHelper.Set2Star(vm);
    }

    public async ValueTask Set3Star()
    {
        await SetExifRatingHelper.Set3Star(vm);
    }

    public async ValueTask Set4Star()
    {
        await SetExifRatingHelper.Set4Star(vm);
    }

    public async ValueTask Set5Star()
    {
        await SetExifRatingHelper.Set5Star(vm);
    }

    #endregion

    #region Open GPS link

    public async ValueTask OpenGoogleMaps()
    {
        // TODO: Needs refactoring into its own method
        if (vm is null)
        {
            return;
        }
        // if (string.IsNullOrEmpty(vm.Exif.GoogleLink.CurrentValue))
        // {
        //     return;
        // }
        //
        // await Task.Run(() => ProcessHelper.OpenLink(vm.Exif.GoogleLink.CurrentValue));
    }
    
    public async ValueTask OpenBingMaps()
    {
        // TODO: Needs refactoring into its own method
        if (vm is null)
        {
            return;
        }
        // if (string.IsNullOrEmpty(vm.Exif.BingLink.CurrentValue))
        // {
        //     return;
        // }
        //
        // await Task.Run(() => ProcessHelper.OpenLink(vm.Exif.BingLink.CurrentValue));
    }

    #endregion

    #region Wallpaper and lockscreen image

    public async ValueTask SetAsWallpaper() =>
        await SetAsWallpaperFilled();

    public async ValueTask SetAsWallpaperTiled()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        await Task.Run(() => core.PlatformService.SetAsWallpaper(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo.FullName, 0)).ConfigureAwait(false);
    }
    
    public async ValueTask SetAsWallpaperCentered()     
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        await Task.Run(() => core.PlatformService.SetAsWallpaper(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo.FullName, 1)).ConfigureAwait(false);
    }
    
    public async ValueTask SetAsWallpaperStretched()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        await Task.Run(() => core.PlatformService.SetAsWallpaper(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo.FullName, 2)).ConfigureAwait(false);
    }
    
    public async ValueTask SetAsWallpaperFitted()     
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        await Task.Run(() => core.PlatformService.SetAsWallpaper(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo.FullName, 3)).ConfigureAwait(false);
    }
    
    public async ValueTask SetAsWallpaperFilled()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        await Task.Run(() => core.PlatformService.SetAsWallpaper(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo.FullName, 4)).ConfigureAwait(false);
    }
    
    public async ValueTask SetAsLockscreenCentered()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        await Task.Run(() => core.PlatformService.SetAsLockScreen(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo.FullName)).ConfigureAwait(false);
    }

    public async ValueTask SetAsLockScreen()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        await Task.Run(() =>
            core.PlatformService.SetAsLockScreen(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo
                .FullName)).ConfigureAwait(false);
    }

    #endregion

    #region Tabs

    public ValueTask NewTab()
    {
        var tab = vm.WindowTabs.CreateTab();
        TabNavigationInitializer.InitializeNewTab(tab, vm);
        return ValueTask.CompletedTask;
    }
    
    public ValueTask CloseTab()
    {
        vm.WindowTabs.CloseTab();
        return ValueTask.CompletedTask;
    }
    
    public ValueTask StopRepeatedNavigation()
    {
        vm?.WindowTabs?.StopRepeatedNavigation();
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Other settings

    /// <inheritdoc cref="SettingsUpdater.ResetSettings(MainViewModel)" />
    public async ValueTask ResetSettings()
    {
        // await SettingsUpdater.ResetSettings(vm).ConfigureAwait(false);
        return;
    }
    
    public async ValueTask Restart()
    {
        // // TODO: Needs refactoring into its own method
        // var openFile = string.Empty;
        // var getFromArgs = false;
        // if (vm?.PicViewer.FileInfo is not null)
        // {
        //     if (vm.PicViewer.FileInfo.CurrentValue.Exists)
        //     {
        //         openFile = vm.PicViewer.FileInfo.CurrentValue.FullName;
        //     }
        //     else
        //     {
        //         getFromArgs = true;
        //     }
        // }
        // else
        // {
        //     getFromArgs = true;
        // }
        // if (getFromArgs)
        // {
        //     var args = Environment.GetCommandLineArgs();
        //     if (args is not null && args.Length > 0)
        //     {
        //         openFile = args[1];
        //     }
        // }
        // ProcessHelper.RestartApp(openFile);
        //
        // if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        // {
        //     Environment.Exit(0);
        //     return;
        // }
        // await Dispatcher.UIThread.InvokeAsync(() =>
        // {
        //     desktop.MainWindow?.Close();
        // });
        return;
    }
    
    public ValueTask ShowSettingsFile()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return ValueTask.CompletedTask;
        }
        core.PlatformService.OpenWith(CurrentSettingsPath);
        return ValueTask.CompletedTask;
    }
    
    public ValueTask ShowKeybindingsFile()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return ValueTask.CompletedTask;
        }
        core.PlatformService.OpenWith(KeybindingFunctions.CurrentKeybindingsPath);
        return ValueTask.CompletedTask;
    }
    
    public ValueTask ShowRecentHistoryFile()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return ValueTask.CompletedTask;
        }
        core.PlatformService.OpenWith(FileHistoryManager.CurrentFileHistoryFile);
        return ValueTask.CompletedTask;
    }
    
    public async ValueTask ToggleOpeningInSameWindow()
    {
        // await SettingsUpdater.ToggleOpeningInSameWindow(vm).ConfigureAwait(false);
        return;
    }
    
    public async ValueTask ToggleFileHistory()
    {
        // await SettingsUpdater.ToggleFileHistory(vm).ConfigureAwait(false);
        return;
    }

    #endregion
    
    #endregion
}
