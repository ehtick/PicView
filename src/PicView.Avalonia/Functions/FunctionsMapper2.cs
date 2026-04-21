using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.Clipboard;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.Crop;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.ImageTransformations;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.SettingsManagement;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.Input;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ColorHandling;
using PicView.Core.FileHistory;
using PicView.Core.FileSorting;
using PicView.Core.IPlatform;
using PicView.Core.Navigation;
using PicView.Core.ProcessHandling;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Functions;

public class FunctionsMapper2(Core.ViewModels.MainWindowViewModel vm, Window window) : IFunctionsMapper
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
    public async ValueTask NextFolder()
    {
        // await NavigationManager.NavigateBetweenDirectories(true, vm).ConfigureAwait(false);
        return;
    }

    public async ValueTask NextArchive()
    {
        // await NavigationManager.NavigateBetweenArchives(true, vm).ConfigureAwait(false);
        return;
    }
    
    /// <inheritdoc cref="NavigationManager.NavigateFirstOrLast(bool, MainViewModel)" />
    public async ValueTask Last() =>
        await vm.WindowTabs.LastFile().ConfigureAwait(false);

    /// <inheritdoc cref="Core.ViewModels.TabOverviewViewModel.PrevFile()" />
    public async ValueTask Prev() =>
        await vm.WindowTabs.NavigateDirectionalAsync(MainKeyboardShortcuts2.IsKeyHeldDown,
            NavigateTo.Previous).ConfigureAwait(false);

    /// <inheritdoc cref="NavigationManager.NavigateBetweenDirectories(bool, MainViewModel)" />
    public async ValueTask PrevFolder()
    {
        // await NavigationManager.NavigateBetweenDirectories(false, vm).ConfigureAwait(false);
        return;
    }

    public async ValueTask PrevArchive()
    {
        // await NavigationManager.NavigateBetweenArchives(false, vm).ConfigureAwait(false);
        return;
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
        // await SettingsUpdater.ToggleScroll(vm).ConfigureAwait(false);
        return;
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
        // await SettingsUpdater.ToggleLooping(vm).ConfigureAwait(false);
        return;
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
        // await SettingsUpdater.ToggleSubdirectories(vm: vm).ConfigureAwait(false);
        return;
    }
    
    /// <inheritdoc cref="HideInterfaceLogic.ToggleBottomToolbar(MainViewModel)" />
    public async ValueTask ToggleBottomToolbar()
    {
        await ToggleUIVisibility.ToggleBottomBar(vm);
    }
    
    /// <inheritdoc cref="SettingsUpdater.ToggleValueTaskbarProgress(MainViewModel)" />
    public async ValueTask ToggleTaskbarProgress()
    {
        // await SettingsUpdater.ToggleTaskbarProgress(vm).ConfigureAwait(false);
        return;
    }
    
    /// <inheritdoc cref="SettingsUpdater.ToggleConstrainBackgroundColor(MainViewModel)" />
    public ValueTask ToggleConstrainBackgroundColor()
    {
        Settings.UIProperties.IsConstrainBackgroundColorEnabled =
            !Settings.UIProperties.IsConstrainBackgroundColorEnabled;
        if (Application.Current.DataContext is not CoreViewModel core || core?.MainWindows.ActiveWindow.Value is not { } activeWindow)
        {
            return ValueTask.CompletedTask;
        }

        var brush = BackgroundManager.GetBackgroundBrush((BackgroundType)Settings.UIProperties.BgColorChoice);
        var globalSettings = core.GlobalSettings;
                 
        if (Settings.UIProperties.IsConstrainBackgroundColorEnabled)
        {
            globalSettings.ImageBackground.Value = new SolidColorBrush(Colors.Transparent);
            globalSettings.ConstrainedImageBackground.Value = brush;
        }
        else
        {
            globalSettings.ImageBackground.Value = brush;
            globalSettings.ConstrainedImageBackground.Value = new SolidColorBrush(Colors.Transparent);
        }
                 
        globalSettings.BackgroundChoice.Value = Settings.UIProperties.BgColorChoice;
        
        return ValueTask.CompletedTask;
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
    public ValueTask Close()
    {
        DialogManager2.CloseWithOptionalDialog();
        return ValueTask.CompletedTask;
    }
    
    public ValueTask Exit()
    {
        DialogManager2.Close();
        return ValueTask.CompletedTask;
    }

    public async ValueTask Center()
    {
        // await UIHelper.CenterAsync(vm).ConfigureAwait(false);
        return;
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

    public async ValueTask AboutWindow() =>
        await Dispatcher.UIThread.InvokeAsync(() => vm?.PlatformWindowService?.ShowAboutWindow());
    public async ValueTask CheckForUpdates()
    {
        await Dispatcher.UIThread.InvokeAsync(() => vm.PlatformWindowService?.ShowAboutWindow());
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        await core.AboutView.UpdateCurrentVersion();
    }

    public async ValueTask ConvertWindow() =>
        await Dispatcher.UIThread.InvokeAsync(() => vm?.PlatformWindowService?.ShowConvertWindow());

    public async ValueTask KeybindingsWindow() =>
        await vm?.PlatformWindowService?.ShowKeybindingsWindow();

    public async ValueTask EffectsWindow() =>
        await Dispatcher.UIThread.InvokeAsync(() =>
            vm?.PlatformWindowService?.ShowEffectsWindow());

    public async ValueTask ImageInfoWindow() =>
        await vm?.PlatformWindowService?.ShowImageInfoWindow();

    public async ValueTask ResizeWindow() =>
        await Dispatcher.UIThread.InvokeAsync(() => vm?.PlatformWindowService?.ShowSingleImageResizeWindow());

    public async ValueTask BatchResizeWindow() =>
        await vm?.PlatformWindowService?.ShowBatchResizeWindow();

    public async ValueTask SettingsWindow() =>
        await vm.PlatformWindowService.ShowSettingsWindow();

    #endregion Windows

    #region Image Scaling and Window Behavior
    
    /// <inheritdoc cref="WindowFunctions.Stretch(MainViewModel)" />
    public async ValueTask Stretch()
    {
        // await WindowFunctions.Stretch(vm).ConfigureAwait(false);
        return;
    }
    
    /// <inheritdoc cref="WindowFunctions.ToggleAutoFit(MainViewModel)" />
    public async ValueTask AutoFitWindow()
    {
        await WindowFunctions2.ToggleAutoFit(vm, window);
    }

    /// <inheritdoc cref="WindowFunctions.NormalWindow(MainViewModel)" />
    public async ValueTask NormalWindow()
    {
        // await WindowFunctions.NormalWindow(vm).ConfigureAwait(false);
        return;
    }

    /// <inheritdoc cref="Interfaces.IPlatformWindowService.ToggleFullscreen" />
    public async ValueTask ToggleFullscreen() =>
        await vm.PlatformWindowService.ToggleFullscreen().ConfigureAwait(false);
    
    // This shouldn't be here, but keep as alias and backwards compatibility.
    public ValueTask Fullscreen() => ToggleFullscreen();

    /// <inheritdoc cref="WindowFunctions.ToggleTopMost(MainViewModel)" />
    public async ValueTask SetTopMost()
    {
        // await WindowFunctions.ToggleTopMost(vm).ConfigureAwait(false);
        return;
    }

    #endregion

    #region File funnctions

    /// <inheritdoc cref="NavigationManager.LoadPicFromStringAsync(string, MainViewModel)" />
    public async ValueTask OpenLastFile()
    {
        // TODO refactor out of here
        vm.IsLoadingIndicatorShown.Value = true;
        if (await vm.WindowTabs.LoadLastFileAsync())
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is StartUpMenu)
                {
                    vm.WindowTabs.ActiveTab.Value.CurrentView.Value = new ImageViewer();
                }
            });
            TabNavigationInitializer.InitializeNewTab(vm.WindowTabs.ActiveTab.Value, vm);
        }
        vm.IsLoadingIndicatorShown.Value = false;
    }

    /// <inheritdoc cref="NavigationManager.LoadPicFromStringAsync(string, MainViewModel)" />
    public async ValueTask OpenPreviousFileHistoryEntry()
    {
        // await NavigationManager.LoadPicFromStringAsync(FileHistoryManager.GetPreviousEntry(), vm).ConfigureAwait(false);
        return;
    }
   
    /// <inheritdoc cref="NavigationManager.LoadPicFromStringAsync(string, MainViewModel)" />
    public async ValueTask OpenNextFileHistoryEntry()
    {
        // await NavigationManager.LoadPicFromStringAsync(FileHistoryManager.GetNextEntry(), vm).ConfigureAwait(false);
        return;
    }
    
    /// <inheritdoc cref="FileManager.Print(string, MainViewModel)" />
    public async ValueTask Print()
    {
        await FileManager2.Print(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo?.FullName, vm).ConfigureAwait(false);
    }

    /// <inheritdoc cref="FilePicker.SelectAndLoadFile(MainViewModel)" />
    public async ValueTask Open()
    {
        await FilePicker2.SelectAndLoadFile(vm).ConfigureAwait(false);
    }

    /// <inheritdoc cref="FileManager.OpenWith(string, MainViewModel)" />
    public ValueTask OpenWith()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return ValueTask.CompletedTask;
        }

        core.PlatformService.OpenWith(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo?.FullName);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="FileManager.LocateOnDisk(string, MainViewModel)" />
    public ValueTask OpenInExplorer()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return ValueTask.CompletedTask;
        }

        core.PlatformService.LocateOnDisk(vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo?.FullName);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="FileSaverHelper.SaveCurrentFile(MainViewModel)" />
    public async ValueTask Save()
    {
        // await FileSaverHelper.SaveCurrentFile(vm).ConfigureAwait(false);
        return;
    }
    
    /// <inheritdoc cref="FileSaverHelper.SaveFileAs(MainViewModel)" />
    public async ValueTask SaveAs()
    {
        // await FileSaverHelper.SaveFileAs(vm).ConfigureAwait(false);
        return;
    }
    
    /// <inheritdoc cref="FileManager.DeleteFileWithOptionalDialog" />
    public async ValueTask DeleteFile()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        const bool recycle = true;
        await FileManager2
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
        await FileManager2
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
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        await Task.Run(() =>
            FileManager2.ShowFileProperties(vm.WindowTabs.ActiveTab.CurrentValue.Model
                .FileInfo?.FullName)).ConfigureAwait(false);
    }

    #endregion

    #region Copy and Paste functions

    /// <inheritdoc cref="ClipboardFileOperations.CopyFileToClipboard(string, MainViewModel)" />
    public async ValueTask CopyFile()
    {
        await ClipboardFileOperations2.CopyFileToClipboard(vm.WindowTabs.ActiveTab.CurrentValue.Model
            .FileInfo?.FullName).ConfigureAwait(false);
    }
    
    /// <inheritdoc cref="ClipboardTextOperations.CopyTextToClipboard(string)" />
    public async ValueTask CopyFilePath()
    {
        await ClipboardTextOperations2.CopyTextToClipboard(vm.WindowTabs.ActiveTab.CurrentValue.Model
            .FileInfo?.FullName).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ClipboardImageOperations.CopyImageToClipboard(MainViewModel)" />
    public async ValueTask CopyImage()
    {
        // await ClipboardImageOperations.CopyImageToClipboard(vm).ConfigureAwait(false);
        return;
    }

    /// <inheritdoc cref="ClipboardImageOperations.CopyBase64ToClipboard(string, MainViewModel)" />
    public async ValueTask CopyBase64()
    {
        // await ClipboardImageOperations.CopyBase64ToClipboard(vm.PicViewer.FileInfo?.CurrentValue.FullName, vm: vm).ConfigureAwait(false);
        return;
    }

    /// <inheritdoc cref="ClipboardFileOperations.Duplicate(string, MainViewModel)" />
    public async ValueTask DuplicateFile()
    {
        await ClipboardFileOperations2.Duplicate(vm.WindowTabs.ActiveTab.CurrentValue.Model
            .FileInfo?.FullName, vm).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ClipboardFileOperations.CutFile(string, MainViewModel)" />
    public async ValueTask CutFile()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        await ClipboardFileOperations2.CutFile(vm.WindowTabs.ActiveTab.CurrentValue.Model
            .FileInfo?.FullName, core.PlatformService).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ClipboardPasteOperations.Paste(MainViewModel)" />
    public async ValueTask Paste()
    {
        await ClipboardPasteOperations2.Paste(vm).ConfigureAwait(false);
        return;
    }
    
    #endregion

    #region Image Functions
    
    /// <inheritdoc cref="BackgroundManager.ChangeBackground(MainViewModel)" />
    public async ValueTask ChangeBackground()
    {
        // await BackgroundManager.ChangeBackgroundAsync(vm).ConfigureAwait(false);
        return;
    }
    
    /// <inheritdoc cref="SettingsUpdater.ToggleSideBySide(MainViewModel)" />
    public async ValueTask SideBySide()
    {
        await SettingsUpdater2.ToggleSideBySide().ConfigureAwait(false);
    }
    
    /// <inheritdoc cref="ErrorHandling.ReloadAsync(MainViewModel)" />
    public async ValueTask Reload()
    {
        // await ErrorHandling.ReloadAsync(vm).ConfigureAwait(false);
        return;
    }

    public async ValueTask ResizeImage() =>
        await ResizeWindow();

    /// <inheritdoc cref="CropFunctions.StartCropControl(MainViewModel)" />
    public async ValueTask Crop()
    {
        // await CropFunctions.StartCropControlAsync(vm).ConfigureAwait(false);
        return;
    }

    /// <inheritdoc cref="ImageOptimizer.OptimizeImageAsync(MainViewModel)" />
    public async ValueTask OptimizeImage()
    {
        // await ImageOptimizer.OptimizeImageAsync(vm).ConfigureAwait(false);
        return;
    }

    /// <inheritdoc cref="Navigation.Slideshow.StartSlideshow(MainViewModel)" />
    public async ValueTask Slideshow()
    {
        // await Navigation.Slideshow.StartSlideshow(vm).ConfigureAwait(false);
        return;
    }

    public ValueTask ColorPicker()
    {
        throw new NotImplementedException();
    }
    
    #endregion

    #region Sorting

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public async ValueTask SortFilesByName() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.Name).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public async ValueTask SortFilesByCreationTime() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.CreationTime).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public async ValueTask SortFilesByLastAccessTime() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.LastAccessTime).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public async ValueTask SortFilesByLastWriteTime() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.LastWriteTime).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public async ValueTask SortFilesBySize() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.FileSize).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public async ValueTask SortFilesByExtension() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.Extension).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
    public async ValueTask SortFilesRandomly() =>
        await vm.WindowTabs.SortAsync(SortFilesBy.Random).ConfigureAwait(false);

    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, bool)" />
    public async ValueTask SortFilesAscending() =>
        await vm.WindowTabs.SortAsync(ascending: true).ConfigureAwait(false);
    
    /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, bool)" />
    public async ValueTask SortFilesDescending() =>
        await vm.WindowTabs.SortAsync(ascending: false).ConfigureAwait(false);

    #endregion Sorting

    #region Rating

    public async ValueTask Set0Star()
    {
        // => await SetExifRatingHelper.Set0Star(vm);
        return;
    }

    public async ValueTask Set1Star()
    {
        // => await SetExifRatingHelper.Set1Star(vm);
        return;
    }

    public async ValueTask Set2Star()
    {
        // => await SetExifRatingHelper.Set2Star(vm);
        return;
    }

    public async ValueTask Set3Star()
    {
        // => await SetExifRatingHelper.Set3Star(vm);
        return;
    }

    public async ValueTask Set4Star()
    {
        // => await SetExifRatingHelper.Set4Star(vm);
        return;
    }

    public async ValueTask Set5Star()
    {
        // => await SetExifRatingHelper.Set5Star(vm);
        return;
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
    
    public async ValueTask ShowSettingsFile()
    {
        // await Task.Run(() => vm?.PlatformService?.OpenWith(CurrentSettingsPath)).ConfigureAwait(false);
        return;
    }
    
    public async ValueTask ShowKeybindingsFile()
    {
        // await Task.Run(() => vm?.PlatformService?.OpenWith(KeybindingFunctions.CurrentKeybindingsPath)).ConfigureAwait(false);
        return;
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
