// using Avalonia;
// using Avalonia.Controls.ApplicationLifetimes;
// using Avalonia.Threading;
// using PicView.Avalonia.Clipboard;
// using PicView.Avalonia.ColorManagement;
// using PicView.Avalonia.Crop;
// using PicView.Avalonia.FileSystem;
// using PicView.Avalonia.Gallery;
// using PicView.Avalonia.ImageHandling;
// using PicView.Avalonia.ImageTransformations.Rotation;
// using PicView.Avalonia.Navigation;
// using PicView.Avalonia.SettingsManagement;
// using PicView.Avalonia.UI;
// using PicView.Avalonia.ViewModels;
// using PicView.Avalonia.Views.UC;
// using PicView.Avalonia.Input;
// using PicView.Avalonia.WindowBehavior;
// using PicView.Core.FileHistory;
// using PicView.Core.FileSorting;
// using PicView.Core.Keybindings;
// using PicView.Core.Navigation;
// using PicView.Core.ProcessHandling;
//
// namespace PicView.Avalonia.Functions;
//
// /// <summary>
// /// Used to map functions to their names, used for keyboard shortcuts
// /// </summary>
// public class FunctionsMapper2
// {
//
//     public FunctionsMapper2(MainViewModel? vm)
//     {
//         _vm = vm;
//     }
//
//     private static MainViewModel? _vm;
//
//     public static Func<ValueTask>? GetFunctionByName(string functionName)
//     {
//         // Remember to have exact matching names, or it will be null
//         return functionName switch
//         {
//             // Navigation values
//             "Next" => Next,
//             "Prev" => Prev,
//             
//             "NextFolder" => NextFolder,
//             "PrevFolder" => PrevFolder,
//             
//             "Up" => Up,
//             "Down" => Down,
//             
//             "Last" => Last,
//             "First" => First,
//             
//             "Next10" => Next10,
//             "Prev10" => Prev10,
//             
//             "Next100" => Next100,
//             "Prev100" => Prev100,
//
//             "Search" => Search,
//             
//             // Rotate
//             "RotateLeft" => RotateLeft,
//             "RotateRight" => RotateRight,
//
//             // Scroll
//             "ScrollUp" => ScrollUp,
//             "ScrollDown" => ScrollDown,
//             "ScrollToTop" => ScrollToTop,
//             "ScrollToBottom" => ScrollToBottom,
//
//             // Zoom
//             "ZoomIn" => ZoomIn,
//             "ZoomOut" => ZoomOut,
//             "ResetZoom" => ResetZoom,
//             "ChangeCtrlZoom" => ChangeCtrlZoom,
//
//             // Toggles
//             "ToggleScroll" => ToggleScroll,
//             "ToggleLooping" => ToggleLooping,
//             "ToggleGallery" => ToggleGallery,
//
//             // Scale Window
//             "AutoFitWindow" => AutoFitWindow,
//             "NormalWindow" => NormalWindow,
//
//             // Window functions
//             "Fullscreen" => Fullscreen,
//             "ToggleFullscreen" => ToggleFullscreen,
//             "SetTopMost" => SetTopMost,
//             "Close" => Close,
//             "ToggleInterface" => ToggleInterface,
//             "NewWindow" => NewWindow,
//             "Center" => Center,
//             "Maximize" => Maximize,
//             "Restore" => Restore,
//
//             // Windows
//             "AboutWindow" => AboutWindow,
//             "EffectsWindow" => EffectsWindow,
//             "ImageInfoWindow" => ImageInfoWindow,
//             "ResizeWindow" => ResizeWindow,
//             "SettingsWindow" => SettingsWindow,
//             "KeybindingsWindow" => KeybindingsWindow,
//             "BatchResizeWindow" => BatchResizeWindow,
//             "ConvertWindow" => ConvertWindow,
//
//             // Open functions
//             "Open" => Open,
//             "OpenWith" => OpenWith,
//             "OpenInExplorer" => OpenInExplorer,
//             "Save" => Save,
//             "SaveAs" => SaveAs,
//             "Print" => Print,
//             "Reload" => Reload,
//
//             // Copy functions
//             "CopyFile" => CopyFile,
//             "CopyFilePath" => CopyFilePath,
//             "CopyImage" => CopyImage,
//             "CopyBase64" => CopyBase64,
//             "DuplicateFile" => DuplicateFile,
//             "CutFile" => CutFile,
//             "Paste" => Paste,
//
//             // File functions
//             "DeleteFile" => DeleteFile,
//             "DeleteFilePermanently" => DeleteFilePermanently,
//             "Rename" => Rename,
//             "ShowFileProperties" => ShowFileProperties,
//             "ShowSettingsFile" => ShowSettingsFile,
//             "ShowKeybindingsFile" => ShowKeybindingsFile,
//             
//             // Sorting functions
//             "SortFilesByName" => SortFilesByName,
//             "SortFilesByCreationTime" => SortFilesByCreationTime,
//             "SortFilesByLastAccessTime" => SortFilesByLastAccessTime,
//             "SortFilesByLastWriteTime" => SortFilesByLastWriteTime,
//             "SortFilesBySize" => SortFilesBySize,
//             "SortFilesByExtension" => SortFilesByExtension,
//             "SortFilesRandomly" => SortFilesRandomly,
//             
//             "SortFilesAscending" => SortFilesAscending,
//             "SortFilesDescending" => SortFilesDescending,
//             
//             // Image functions
//             "ResizeImage" => ResizeImage,
//             "Crop" => Crop,
//             "Flip" => Flip,
//             "OptimizeImage" => OptimizeImage,
//             "Stretch" => Stretch,
//
//             // Set stars
//             "Set0Star" => Set0Star,
//             "Set1Star" => Set1Star,
//             "Set2Star" => Set2Star,
//             "Set3Star" => Set3Star,
//             "Set4Star" => Set4Star,
//             "Set5Star" => Set5Star,
//             
//             // Background and lock screen image
//             "SetAsLockScreen" => SetAsLockScreen,
//             "SetAsLockscreenCentered" => SetAsLockscreenCentered,
//             "SetAsWallpaper" => SetAsWallpaper,
//             "SetAsWallpaperFitted" => SetAsWallpaperFitted,
//             "SetAsWallpaperStretched" => SetAsWallpaperStretched,
//             "SetAsWallpaperFilled" => SetAsWallpaperFilled,
//             "SetAsWallpaperCentered" => SetAsWallpaperCentered,
//             "SetAsWallpaperTiled" => SetAsWallpaperTiled,
//             
//             // Tabs
//             "NewTab" => NewTab,
//             "CloseTab" => CloseTab,
//
//             // Misc
//             "ChangeBackground" => ChangeBackground,
//             "SideBySide" => SideBySide,
//             "GalleryClick" => GalleryClick,
//             "Slideshow" => Slideshow,
//             "ColorPicker" => ColorPicker,
//             "Restart" => Restart,
//             "OpenDrowDownMenu" => OpenDropDownMenu,
//
//             _ => null
//         };
//     }
//
//     #region Functions
//
//     #region Navigation, zoom and rotation
//
//     /// <inheritdoc cref="Core.ViewModels.TabOverviewViewModel.NextFile()" />
//     public static async ValueTask Next() =>
//         await _vm.Tabs.NavigateDirectionalAsync(MainKeyboardShortcuts.IsKeyHeldDown,
//             NavigateTo.Next).ConfigureAwait(false);
//
//     /// <inheritdoc cref="NavigationManager.NavigateBetweenDirectories(bool, MainViewModel)" />
//     public static async ValueTask NextFolder() =>
//         await NavigationManager.NavigateBetweenDirectories(true, _vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="NavigationManager.NavigateFirstOrLast(bool, MainViewModel)" />
//     public static async ValueTask Last() =>
//         await _vm.Tabs.FirstFile().ConfigureAwait(false);
//
//     /// <inheritdoc cref="Core.ViewModels.TabOverviewViewModel.PrevFile()" />
//     public static async ValueTask Prev() =>
//         await _vm.Tabs.NavigateDirectionalAsync(MainKeyboardShortcuts.IsKeyHeldDown,
//             NavigateTo.Previous).ConfigureAwait(false);
//
//     /// <inheritdoc cref="NavigationManager.NavigateBetweenDirectories(bool, MainViewModel)" />
//     public static async ValueTask PrevFolder() =>
//         await NavigationManager.NavigateBetweenDirectories(false, _vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="NavigationManager.NavigateFirstOrLast(bool, MainViewModel)" />
//     public static async ValueTask First() =>
//         await _vm.Tabs.FirstFile().ConfigureAwait(false);
//     
//     /// <inheritdoc cref="Core.ViewModels.TabOverviewViewModel.Next10()" />
//     public static async ValueTask Next10() =>
//         await _vm.Tabs.Next10().ConfigureAwait(false);
//
//     /// <inheritdoc cref="Core.ViewModels.TabOverviewViewModel.Next100()" />
//     public static async ValueTask Next100() =>
//         await _vm.Tabs.Next100().ConfigureAwait(false);
//     
//     /// <inheritdoc cref="NavigationManager.Prev10(MainViewModel)" />
//     public static async ValueTask Prev10() =>
//         await _vm.Tabs.Prev10().ConfigureAwait(false);
//     
//     /// <inheritdoc cref="NavigationManager.Prev100(MainViewModel)" />
//     public static async ValueTask Prev100() =>
//         await NavigationManager.Prev100(_vm).ConfigureAwait(false);
//     
//     public static void StopRepeatedNavigation()
//     {
//         _vm?.Tabs?.StopRepeatedNavigation();
//     }
//
//     public static async ValueTask Search() =>
//         await Dispatcher.UIThread.InvokeAsync(DialogManager.AddFileSearchDialog);
//     
//
//     /// <inheritdoc cref="RotationNaRotationNavigationp(MainViewModel)" />
//     public static async ValueTask Up() =>
//         await RotationNavigation.NavigateUp(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="RotationNavigation.RotateRight(MainViewModel)" />
//     public static async ValueTask RotateRight() =>
//         await RotationNavigation.RotateRight(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="RotationNavigation.RotateLeft(MainViewModel)" />
//     public static async ValueTask RotateLeft() =>
//         await RotationNavigation.RotateLeft(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="RotationNavigation.NavigateDown(MainViewModel)" />
//     public static async ValueTask Down() =>
//         await RotationNavigation.NavigateDown(_vm).ConfigureAwait(false);
//     
//     public static async ValueTask ScrollDown()
//     {
//         // TODO: ImageViewer Needs refactor
//         await Dispatcher.UIThread.InvokeAsync(() =>
//         {
//             _vm.ImageViewer.ImageScrollViewer.LineDown();
//         });
//     }
//     
//     public static async ValueTask ScrollUp()
//     {
//         // TODO: ImageViewer Needs refactor
//         await Dispatcher.UIThread.InvokeAsync(() =>
//         {
//             _vm.ImageViewer.ImageScrollViewer.LineUp();
//         });
//     }
//
//     public static async ValueTask ScrollToTop()
//     {
//         // TODO: ImageViewer Needs refactor
//         await Dispatcher.UIThread.InvokeAsync(() =>
//         {
//             _vm.ImageViewer.ImageScrollViewer.ScrollToHome();
//         });
//     }
//
//     public static async ValueTask ScrollToBottom()
//     {
//         // TODO: ImageViewer Needs refactor
//         await Dispatcher.UIThread.InvokeAsync(() =>
//         {
//             _vm.ImageViewer.ImageScrollViewer.ScrollToEnd();
//         });
//     }
//
//     public static ValueTask ZoomIn()
//     {
//         if (_vm.Tabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
//         {
//             imageViewer.ZoomIn();
//         }
//         return ValueTask.CompletedTask;
//     }
//
//     public static ValueTask ZoomOut()
//     {
//         if (_vm.Tabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
//         {
//             imageViewer.ZoomOut();
//         }
//         return ValueTask.CompletedTask;
//     }
//
//     public static async ValueTask ResetZoom()
//     {
//         // TODO: ImageViewer Needs refactor
//         if (_vm is null)
//         {
//             return;
//         }
//
//         await Dispatcher.UIThread.InvokeAsync(() => _vm.ImageViewer.ResetZoom(Settings.Zoom.IsZoomAnimated));
//     }
//     
//     #endregion
//
//     #region Toggle UI functions
//
//     /// <inheritdoc cref="SettingsUpdater.ToggleScroll(MainViewModel)" />
//     public static async ValueTask ToggleScroll() =>
//         await SettingsUpdater.ToggleScroll(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="SettingsUpdater.ToggleCtrlZoom(MainViewModel)" />
//     public static async ValueTask ChangeCtrlZoom() =>
//         await SettingsUpdater.ToggleCtrlZoom(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="SettingsUpdater.ToggleLooping(MainViewModel)" />
//     public static async ValueTask ToggleLooping() =>
//         await SettingsUpdater.ToggleLooping(_vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="HideInterfaceLogic.ToggleUI(MainViewModel)" />
//     public static async ValueTask ToggleInterface() =>
//         await HideInterfaceLogic.ToggleUI(_vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="SettingsUpdater.ToggleSubdirectories(MainViewModel)" />
//     public static async ValueTask ToggleSubdirectories() =>
//         await SettingsUpdater.ToggleSubdirectories(vm: _vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="HideInterfaceLogic.ToggleBottomToolbar(MainViewModel)" />
//     public static async ValueTask ToggleBottomToolbar() =>
//         await HideInterfaceLogic.ToggleBottomToolbar(_vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="SettingsUpdater.ToggleValueTaskbarProgress(MainViewModel)" />
//     public static async ValueTask ToggleTaskbarProgress() =>
//         await SettingsUpdater.ToggleTaskbarProgress(_vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="SettingsUpdater.ToggleConstrainBackgroundColor(MainViewModel)" />
//     public static async ValueTask ToggleConstrainBackgroundColor() =>
//         await SettingsUpdater.ToggleConstrainBackgroundColor(_vm).ConfigureAwait(false);
//
//     public static ValueTask OpenDropDownMenu()
//     {
//         _vm.MainWindow.TopTitlebarViewModel.OpenMenu();
//         return ValueTask.CompletedTask;
//     }
//     
//     public static ValueTask ToggleDropDownMenu()
//     {
//         _vm.MainWindow.TopTitlebarViewModel.ToggleMenu();
//         return ValueTask.CompletedTask;
//     }
//         
//     
//     #endregion
//
//     #region Gallery functions
//
//     /// <inheritdoc cref="GalleryFunctions.ToggleGallery(MainViewModel)" />
//     public static async ValueTask ToggleGallery() =>
//         await Task.Run(() => GalleryFunctions.ToggleGallery(_vm));
//
//     /// <inheritdoc cref="GalleryFunctions.OpenCloseBottomGallery(MainViewModel)" />
//     public static async ValueTask OpenCloseBottomGallery() =>
//         await Task.Run(() => GalleryFunctions.OpenCloseBottomGallery(_vm));
//     
//     /// <inheritdoc cref="GalleryFunctions.CloseGallery(MainViewModel)" />
//     public static ValueTask CloseGallery()
//     {
//         GalleryFunctions.CloseGallery(_vm);
//         return ValueTask.CompletedTask;
//     }
//
//     /// <inheritdoc cref="GalleryNavigation.GalleryClick(MainViewModel)" />
//     public static async ValueTask GalleryClick() =>
//         await GalleryNavigation.GalleryClick(_vm).ConfigureAwait(false);
//
//     #endregion
//     
//     #region Windows and window functions
//
//     public static async Task ShowStartUpMenu()
//     {
//         //TODO: Needs refactor, add async overload for ShowStartUpMenu
//         await Dispatcher.UIThread.InvokeAsync(() =>
//         {
//             ErrorHandling.ShowStartUpMenu(_vm);
//         });
//     }
//     
//     /// <inheritdoc cref="DialogManager.HandleShouldClosing" />
//     public static async ValueTask Close() =>
//         await DialogManager.HandleShouldClosing(_vm).ConfigureAwait(false);
//
//     public static async ValueTask Center() =>
//         await UIHelper.CenterAsync(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="Interfaces.IPlatformWindowService.MaximizeRestore" />
//     public static async ValueTask Maximize()
//     {
//         await _vm.PlatformWindowService.MaximizeRestore();
//     }
//     
//     /// <inheritdoc cref="Interfaces.IPlatformWindowService.Restore" />
//     public static async ValueTask Restore()
//     {
//         await _vm.PlatformWindowService.Restore();
//     }
//
//     /// <inheritdoc cref="ProcessHelper.StartNewProcess()" />
//     public static async ValueTask NewWindow() =>
//         await Task.Run(ProcessHelper.StartNewProcess).ConfigureAwait(false);
//
//     public static async ValueTask AboutWindow() =>
//         await Dispatcher.UIThread.InvokeAsync(() => _vm?.PlatformWindowService?.ShowAboutWindow());
//
//     public static async ValueTask ConvertWindow() =>
//         await Dispatcher.UIThread.InvokeAsync(() => _vm?.PlatformWindowService?.ShowConvertWindow());
//
//     public static async ValueTask KeybindingsWindow() =>
//         await Dispatcher.UIThread.InvokeAsync(() => _vm?.PlatformWindowService?.ShowKeybindingsWindow());
//
//     public static async ValueTask EffectsWindow() =>
//         await Dispatcher.UIThread.InvokeAsync(() =>
//             _vm?.PlatformWindowService?.ShowEffectsWindow());
//
//     public static async ValueTask ImageInfoWindow() =>
//         await _vm?.PlatformWindowService?.ShowImageInfoWindow();
//
//     public static async ValueTask ResizeWindow() =>
//         await Dispatcher.UIThread.InvokeAsync(() => _vm?.PlatformWindowService?.ShowSingleImageResizeWindow());
//
//     public static async ValueTask BatchResizeWindow() =>
//         await _vm?.PlatformWindowService?.ShowBatchResizeWindow();
//
//     public static async ValueTask SettingsWindow() =>
//         await _vm?.PlatformWindowService?.ShowSettingsWindow();
//
//     #endregion Windows
//
//     #region Image Scaling and Window Behavior
//     
//     /// <inheritdoc cref="WindowFunctions.Stretch(MainViewModel)" />
//     public static async ValueTask Stretch() =>
//         await WindowFunctions.Stretch(_vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="WindowFunctions.ToggleAutoFit(MainViewModel)" />
//     public static async ValueTask AutoFitWindow() =>
//         await WindowFunctions.ToggleAutoFit(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="WindowFunctions.NormalWindow(MainViewModel)" />
//     public static async ValueTask NormalWindow() =>
//         await WindowFunctions.NormalWindow(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="Interfaces.IPlatformWindowService.ToggleFullscreen" />
//     public static async ValueTask ToggleFullscreen() =>
//         await _vm.PlatformWindowService.ToggleFullscreen().ConfigureAwait(false);
//     
//     // This shouldn't be here, but keep as alias and backwards compatibility.
//     public static ValueTask Fullscreen() => ToggleFullscreen();
//
//     /// <inheritdoc cref="WindowFunctions.ToggleTopMost(MainViewModel)" />
//     public static async ValueTask SetTopMost() =>
//
//         await WindowFunctions.ToggleTopMost(_vm).ConfigureAwait(false);
//
//     #endregion
//
//     #region File funnctions
//
//     /// <inheritdoc cref="NavigationManager.LoadPicFromStringAsync(string, MainViewModel)" />
//     public static async Task OpenLastFile() =>
//         await NavigationManager.LoadLastFileAsync(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="NavigationManager.LoadPicFromStringAsync(string, MainViewModel)" />
//     public static async Task OpenPreviousFileHistoryEntry() =>
//         await NavigationManager.LoadPicFromStringAsync(FileHistoryManager.GetPreviousEntry(), _vm).ConfigureAwait(false);
//    
//     /// <inheritdoc cref="NavigationManager.LoadPicFromStringAsync(string, MainViewModel)" />
//     public static async Task OpenNextFileHistoryEntry() =>
//         await NavigationManager.LoadPicFromStringAsync(FileHistoryManager.GetNextEntry(), _vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="FileManager.Print(string, MainViewModel)" />
//     public static async ValueTask Print() =>
//         await FileManager.Print(_vm.PicViewer.FileInfo?.CurrentValue?.FullName, _vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="FilePicker.SelectAndLoadFile(MainViewModel)" />
//     public static async ValueTask Open() =>
//         await FilePicker.SelectAndLoadFile(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="FileManager.OpenWith(string, MainViewModel)" />
//     public static async ValueTask OpenWith() =>
//         await Task.Run(() => _vm?.PlatformService?.OpenWith(_vm.PicViewer.FileInfo?.CurrentValue?.FullName))
//             .ConfigureAwait(false);
//     
//     /// <inheritdoc cref="FileManager.LocateOnDisk(string, MainViewModel)" />
//     public static async ValueTask OpenInExplorer()=>
//         await Task.Run(() => _vm?.PlatformService?.LocateOnDisk(_vm.Tabs.ActiveTab.CurrentValue.Model.CurrentValue.FileInfo?.FullName))
//             .ConfigureAwait(false);
//
//     /// <inheritdoc cref="FileSaverHelper.SaveCurrentFile(MainViewModel)" />
//     public static async ValueTask Save() =>
//         await FileSaverHelper.SaveCurrentFile(_vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="FileSaverHelper.SaveFileAs(MainViewModel)" />
//     public static async ValueTask SaveAs() =>
//         await FileSaverHelper.SaveFileAs(_vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="FileManager.DeleteFileWithOptionalDialog" />
//     public static async ValueTask DeleteFile() =>
//         await FileManager
//             .DeleteFileWithOptionalDialog(true, _vm.PicViewer?.FileInfo?.CurrentValue?.FullName, _vm.PlatformService)
//             .ConfigureAwait(false);
//     
//     /// <inheritdoc cref="FileManager.DeleteFileWithOptionalDialog" />
//     public static async ValueTask DeleteFilePermanently() =>
//         await FileManager
//             .DeleteFileWithOptionalDialog(false, _vm.PicViewer?.FileInfo?.CurrentValue?.FullName, _vm.PlatformService)
//             .ConfigureAwait(false);
//
//     public static async ValueTask Rename()
//     {
//         // TODO: Needs refactor for selecting file name
//         await Dispatcher.UIThread.InvokeAsync(() =>
//         {
//             UIHelper.GetEditableTitlebar.SelectFileName();
//         });
//     }
//     
//     /// <inheritdoc cref="FileManager.ShowFileProperties(string, MainViewModel)" />
//     public static async ValueTask ShowFileProperties() =>
//         await Task.Run(() => _vm?.PlatformService?.ShowFileProperties(_vm.PicViewer.FileInfo?.CurrentValue.FullName)).ConfigureAwait(false);
//     
//     #endregion
//
//     #region Copy and Paste functions
//
//     /// <inheritdoc cref="ClipboardFileOperations.CopyFileToClipboard(string, MainViewModel)" />
//     public static async ValueTask CopyFile() =>
//         await ClipboardFileOperations.CopyFileToClipboard(_vm?.PicViewer.FileInfo?.CurrentValue.FullName).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="ClipboardTextOperations.CopyTextToClipboard(string)" />
//     public static async ValueTask CopyFilePath() => 
//         await ClipboardTextOperations.CopyTextToClipboard(_vm?.PicViewer.FileInfo?.CurrentValue.FullName).ConfigureAwait(false);
//
//     /// <inheritdoc cref="ClipboardImageOperations.CopyImageToClipboard(MainViewModel)" />
//     public static async ValueTask CopyImage() => 
//         await ClipboardImageOperations.CopyImageToClipboard(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="ClipboardImageOperations.CopyBase64ToClipboard(string, MainViewModel)" />
//     public static async ValueTask CopyBase64() =>
//         await ClipboardImageOperations.CopyBase64ToClipboard(_vm.PicViewer.FileInfo?.CurrentValue.FullName, vm: _vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="ClipboardFileOperations.Duplicate(string, MainViewModel)" />
//     public static async ValueTask DuplicateFile() => 
//         await ClipboardFileOperations.Duplicate(_vm.PicViewer.FileInfo?.CurrentValue.FullName, _vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="ClipboardFileOperations.CutFile(string, MainViewModel)" />
//     public static async ValueTask CutFile() =>
//         await ClipboardFileOperations.CutFile(_vm.PicViewer.FileInfo.CurrentValue.FullName, _vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="ClipboardPasteOperations.Paste(MainViewModel)" />
//     public static async ValueTask Paste() =>
//         await ClipboardPasteOperations.Paste(_vm).ConfigureAwait(false);
//     
//     #endregion
//
//     #region Image Functions
//     
//     /// <inheritdoc cref="BackgroundManager.ChangeBackground(MainViewModel)" />
//     public static async ValueTask ChangeBackground() =>
//         await BackgroundManager.ChangeBackgroundAsync(_vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="SettingsUpdater.ToggleSideBySide(MainViewModel)" />
//     public static async ValueTask SideBySide() =>
//         await SettingsUpdater.ToggleSideBySide(_vm).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="ErrorHandling.ReloadAsync(MainViewModel)" />
//     public static async ValueTask Reload() =>
//         await ErrorHandling.ReloadAsync(_vm).ConfigureAwait(false);
//
//     public static async ValueTask ResizeImage() =>
//         await ResizeWindow();
//
//     /// <inheritdoc cref="CropFunctions.StartCropControl(MainViewModel)" />
//     public static async ValueTask Crop() =>
//         await CropFunctions.StartCropControlAsync(_vm).ConfigureAwait(false);
//
//     public static async ValueTask Flip() =>
//         await Dispatcher.UIThread.InvokeAsync(() => RotationNavigation.Flip(_vm));
//
//     /// <inheritdoc cref="ImageOptimizer.OptimizeImageAsync(MainViewModel)" />
//     public static async ValueTask OptimizeImage() =>
//         await ImageOptimizer.OptimizeImageAsync(_vm).ConfigureAwait(false);
//
//     /// <inheritdoc cref="Navigation.Slideshow.StartSlideshow(MainViewModel)" />
//     public static async ValueTask Slideshow() =>
//         await Navigation.Slideshow.StartSlideshow(_vm).ConfigureAwait(false);
//
//     public static ValueTask ColorPicker()
//     {
//         throw new NotImplementedException();
//     }
//     
//     #endregion
//
//     #region Sorting
//
//     /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
//     public static async ValueTask SortFilesByName() =>
//         await _vm.Tabs.SortAsync(SortFilesBy.Name).ConfigureAwait(false);
//
//     /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
//     public static async ValueTask SortFilesByCreationTime() =>
//         await _vm.Tabs.SortAsync(SortFilesBy.CreationTime).ConfigureAwait(false);
//
//     /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
//     public static async ValueTask SortFilesByLastAccessTime() =>
//         await _vm.Tabs.SortAsync(SortFilesBy.LastAccessTime).ConfigureAwait(false);
//
//     /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
//     public static async ValueTask SortFilesByLastWriteTime() =>
//         await _vm.Tabs.SortAsync(SortFilesBy.LastWriteTime).ConfigureAwait(false);
//
//     /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
//     public static async ValueTask SortFilesBySize() =>
//         await _vm.Tabs.SortAsync(SortFilesBy.FileSize).ConfigureAwait(false);
//
//     /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
//     public static async ValueTask SortFilesByExtension() =>
//         await _vm.Tabs.SortAsync(SortFilesBy.Extension).ConfigureAwait(false);
//
//     /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, SortFilesBy)" />
//     public static async ValueTask SortFilesRandomly() =>
//         await _vm.Tabs.SortAsync(SortFilesBy.Random).ConfigureAwait(false);
//
//     /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, bool)" />
//     public static async ValueTask SortFilesAscending() =>
//         await _vm.Tabs.SortAsync(ascending: true).ConfigureAwait(false);
//     
//     /// <inheritdoc cref="FileListManager.UpdateFileList(PicView.Avalonia.Interfaces.IPlatformSpecificService, MainViewModel, bool)" />
//     public static async ValueTask SortFilesDescending() =>
//         await _vm.Tabs.SortAsync(ascending: false).ConfigureAwait(false);
//
//     #endregion Sorting
//
//     #region Rating
//
//     public static async ValueTask Set0Star()
//         => await SetExifRatingHelper.Set0Star(_vm);
//
//     public static async ValueTask Set1Star()
//         => await SetExifRatingHelper.Set1Star(_vm);
//
//     public static async ValueTask Set2Star()
//         => await SetExifRatingHelper.Set2Star(_vm);
//
//     public static async ValueTask Set3Star()
//         => await SetExifRatingHelper.Set3Star(_vm);
//
//     public static async ValueTask Set4Star()
//         => await SetExifRatingHelper.Set4Star(_vm);
//
//     public static async ValueTask Set5Star()
//         => await SetExifRatingHelper.Set5Star(_vm);
//
//     #endregion
//
//     #region Open GPS link
//
//     public static async Task OpenGoogleMaps()
//     {
//         // TODO: Needs refactoring into its own method
//         if (_vm is null)
//         {
//             return;
//         }
//         if (string.IsNullOrEmpty(_vm.Exif.GoogleLink.CurrentValue))
//         {
//             return;
//         }
//
//         await Task.Run(() => ProcessHelper.OpenLink(_vm.Exif.GoogleLink.CurrentValue));
//     }
//     
//     public static async Task OpenBingMaps()
//     {
//         // TODO: Needs refactoring into its own method
//         if (_vm is null)
//         {
//             return;
//         }
//         if (string.IsNullOrEmpty(_vm.Exif.BingLink.CurrentValue))
//         {
//             return;
//         }
//
//         await Task.Run(() => ProcessHelper.OpenLink(_vm.Exif.BingLink.CurrentValue));
//     }
//
//     #endregion
//
//     #region Wallpaper and lockscreen image
//
//     public static async ValueTask SetAsWallpaper() =>
//         await SetAsWallpaperFilled();
//
//     public static async ValueTask SetAsWallpaperTiled() =>
//         await Task.Run(() => _vm.PlatformService.SetAsWallpaper(_vm.PicViewer.FileInfo.CurrentValue.FullName, 0)).ConfigureAwait(false);
//     
//     public static async ValueTask SetAsWallpaperCentered() =>
//         await Task.Run(() => _vm.PlatformService.SetAsWallpaper(_vm.PicViewer.FileInfo.CurrentValue.FullName, 1)).ConfigureAwait(false);
//     
//     public static async ValueTask SetAsWallpaperStretched() =>
//         await Task.Run(() => _vm.PlatformService.SetAsWallpaper(_vm.PicViewer.FileInfo.CurrentValue.FullName, 2)).ConfigureAwait(false);
//     
//     public static async ValueTask SetAsWallpaperFitted() =>
//         await Task.Run(() => _vm.PlatformService.SetAsWallpaper(_vm.PicViewer.FileInfo.CurrentValue.FullName, 3)).ConfigureAwait(false);
//     
//     public static async ValueTask SetAsWallpaperFilled() =>
//         await Task.Run(() => _vm.PlatformService.SetAsWallpaper(_vm.PicViewer.FileInfo.CurrentValue.FullName, 4)).ConfigureAwait(false);
//     
//     public static async ValueTask SetAsLockscreenCentered() =>
//         await Task.Run(() => _vm.PlatformService.SetAsLockScreen(_vm.PicViewer.FileInfo.CurrentValue.FullName)).ConfigureAwait(false);
//     
//     public static async ValueTask SetAsLockScreen() =>
//         await Task.Run(() => _vm.PlatformService.SetAsLockScreen(_vm.PicViewer.FileInfo.CurrentValue.FullName)).ConfigureAwait(false);
//
//     #endregion
//
//     #region Tabs
//
//     public static ValueTask NewTab()
//     {
//         _vm.Tabs.CreateTab();
//         return ValueTask.CompletedTask;
//     }
//     
//     public static async ValueTask CloseTab()
//     {
//         await _vm.Tabs.CloseTabAsync();
//     }
//
//     #endregion
//
//     #region Other settings
//
//     /// <inheritdoc cref="SettingsUpdater.ResetSettings(MainViewModel)" />
//     public static async ValueTask ResetSettings() =>
//         await SettingsUpdater.ResetSettings(_vm).ConfigureAwait(false);
//     
//     public static async ValueTask Restart()
//     {
//         // TODO: Needs refactoring into its own method
//         var openFile = string.Empty;
//         var getFromArgs = false;
//         if (_vm?.PicViewer.FileInfo is not null)
//         {
//             if (_vm.PicViewer.FileInfo.CurrentValue.Exists)
//             {
//                 openFile = _vm.PicViewer.FileInfo.CurrentValue.FullName;
//             }
//             else
//             {
//                 getFromArgs = true;
//             }
//         }
//         else
//         {
//             getFromArgs = true;
//         }
//         if (getFromArgs)
//         {
//             var args = Environment.GetCommandLineArgs();
//             if (args is not null && args.Length > 0)
//             {
//                 openFile = args[1];
//             }
//         }
//         ProcessHelper.RestartApp(openFile);
//
//         if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
//         {
//             Environment.Exit(0);
//             return;
//         }
//         await Dispatcher.UIThread.InvokeAsync(() =>
//         {
//             desktop.MainWindow?.Close();
//         });
//     }
//     
//     public static async ValueTask ShowSettingsFile() =>
//         await Task.Run(() => _vm?.PlatformService?.OpenWith(CurrentSettingsPath)).ConfigureAwait(false);
//     
//     public static async ValueTask ShowKeybindingsFile() =>
//         await Task.Run(() => _vm?.PlatformService?.OpenWith(KeybindingFunctions.CurrentKeybindingsPath)).ConfigureAwait(false);
//     
//     public static async ValueTask ShowRecentHistoryFile() =>
//         await Task.Run(() => _vm?.PlatformService?.OpenWith(FileHistoryManager.CurrentFileHistoryFile)).ConfigureAwait(false);
//     
//     public static async ValueTask ToggleOpeningInSameWindow() =>
//         await SettingsUpdater.ToggleOpeningInSameWindow(_vm).ConfigureAwait(false);
//     
//     public static async ValueTask ToggleFileHistory() =>
//         await SettingsUpdater.ToggleFileHistory(_vm).ConfigureAwait(false);
//
//     #endregion
//     
//     #endregion
// }