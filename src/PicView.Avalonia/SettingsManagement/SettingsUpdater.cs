using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ColorHandling;
using PicView.Core.Localization;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.SettingsManagement;
public static class SettingsUpdater
{
    // public static void ValidateGallerySettings(MainViewModel vm, bool settingsExists)
    // {
    //     if (vm.Gallery is not {} gallery)
    //     {
    //         return;
    //     }
    //
    //     if (!settingsExists)
    //     {
    //         gallery.GalleryItem.BottomGalleryItemHeight.Value = GalleryDefaults.DefaultBottomGalleryHeight;
    //         gallery.GalleryItem.ExpandedGalleryItemHeight.Value = GalleryDefaults.DefaultFullGalleryHeight;
    //     }
    //     else
    //     {
    //         gallery.GalleryItem.ExpandedGalleryItemHeight.Value  = Settings.Gallery.ExpandedGalleryItemSize;
    //         gallery.GalleryItem.BottomGalleryItemHeight.Value = Settings.Gallery.BottomGalleryItemSize;
    //     }
    //
    //     // Set default gallery sizes if they are out of range or upgrading from an old version
    //     if (gallery.GalleryItem.BottomGalleryItemHeight.CurrentValue is < GalleryDefaults.MinBottomGalleryItemHeight or > GalleryDefaults.MaxBottomGalleryItemHeight)
    //     {
    //         gallery.GalleryItem.BottomGalleryItemHeight.Value = GalleryDefaults.DefaultBottomGalleryHeight;
    //     }
    //
    //     if (gallery.GalleryItem.ExpandedGalleryItemHeight.CurrentValue is < GalleryDefaults.MinFullGalleryItemHeight or > GalleryDefaults.MaxFullGalleryItemHeight)
    //     {
    //         gallery.GalleryItem.ExpandedGalleryItemHeight.Value = GalleryDefaults.DefaultFullGalleryHeight;
    //     }
    //
    //     if (settingsExists)
    //     {
    //         return;
    //     }
    //
    //     if (string.IsNullOrWhiteSpace(Settings.Gallery.BottomGalleryStretchMode))
    //     {
    //         Settings.Gallery.BottomGalleryStretchMode = "UniformToFill";
    //     }
    //
    //     if (string.IsNullOrWhiteSpace(Settings.Gallery.FullGalleryStretchMode))
    //     {
    //         Settings.Gallery.FullGalleryStretchMode = "UniformToFill";
    //     }
    // }



    public static void InitializeSettings(MainWindowViewModel vm, bool settingsExists)
    {
        Task.Run(() => LanguageUpdater.UpdateLanguageAsync(vm.Translation, settingsExists));
        
        //MainWindowViewModel.GetAndSetWindowMinSize(vm);
        

        
        vm.TitlebarHeight.Value = Settings.WindowProperties.Fullscreen
                                       || !Settings.UIProperties.ShowInterface
            ? 0
            : SizeDefaults.MainTitlebarHeight;
        vm.BottombarHeight.Value = Settings.WindowProperties.Fullscreen
                                              || !Settings.UIProperties.ShowInterface
            ? 0
            : SizeDefaults.BottombarHeight;
        // vm.PicViewer.IsShowingSideBySide.Value = Settings.ImageScaling.ShowImageSideBySide;
        vm.IsUIShown.Value  = Settings.UIProperties.ShowInterface;
        vm.IsTopToolbarShown.Value  = Settings.UIProperties.ShowInterface;
        vm.IsBottomToolbarShown.Value   = Settings.UIProperties.ShowBottomNavBar &&
                                    Settings.UIProperties.ShowInterface;
        vm.IsFullscreen.Value  = Settings.WindowProperties.Fullscreen;
        vm.GlobalSettings.BackgroundChoice.Value = Settings.UIProperties.BgColorChoice;
    }
    
    public static async Task ResetSettings(MainWindowViewModel vm)
    {
        // TODO
    }
    
    public static async ValueTask ToggleZoomToFit(MainWindowViewModel vm)
    {
        if (Settings.ImageScaling.ZoomToFit)
        {
            Settings.ImageScaling.ZoomToFit = false;
            vm.IsZoomedToFit.Value = false;
        }
        else
        {
            Settings.ImageScaling.ZoomToFit = true;
            vm.IsZoomedToFit.Value = true;
        }

        WindowResizing.SetSize(vm, WindowResizeReason.Layout);
        await SaveSettingsAsync().ConfigureAwait(false);
    }
    
    public static async ValueTask ToggleSubdirectories(MainWindowViewModel vm)
    {
        if (Settings.Sorting.IncludeSubDirectories)
        {
            TurnOffSubdirectories(vm);
        }
        else
        {
            TurnOnSubdirectories(vm);
        }

        var windowTabs = vm.WindowTabs;
        var tab = windowTabs.ActiveTab.CurrentValue;
        if (tab.ImageIterator?.Files.Count > 0)
        {
            var ct = tab.GetTabCancellation();
            await windowTabs.SharedNavigation.RepopulateIterator(tab.FileInfo.CurrentValue, tab, ct).ConfigureAwait(false);
            await tab.ImageIterator.ReloadAsync(ct).ConfigureAwait(false);
            tab.UpdateTabTitle();
        }
        
        await SaveSettingsAsync();
    }
    
    public static void TurnOffSubdirectories(MainWindowViewModel vm)
    {
        vm.GlobalSettings.IsIncludingSubdirectories.Value = false;
        Settings.Sorting.IncludeSubDirectories = false;
    }
    
    public static void TurnOnSubdirectories(MainWindowViewModel vm)
    {
        vm.GlobalSettings.IsIncludingSubdirectories.Value = true;
        Settings.Sorting.IncludeSubDirectories = true;
    }
    
    public static async ValueTask ToggleTaskbarProgress(MainWindowViewModel vm)
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        if (Settings.UIProperties.IsTaskbarProgressEnabled)
        {
            Settings.UIProperties.IsTaskbarProgressEnabled = false;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                core.PlatformService.StopTaskbarProgress();
            });
        }
        else
        {
            Settings.UIProperties.IsTaskbarProgressEnabled = true;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                core.PlatformService.SetTaskbarProgress(
                    (ulong)vm.WindowTabs.ActiveTab.CurrentValue.ImageIterator.CurrentIndex,
                    (ulong)vm.WindowTabs.ActiveTab.CurrentValue.ImageIterator.Files.Count);
            });
        }

        await SaveSettingsAsync();
    }
    
    public static async Task ToggleConstrainBackgroundColor()
    {
        Settings.UIProperties.IsConstrainBackgroundColorEnabled =
            !Settings.UIProperties.IsConstrainBackgroundColorEnabled;
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
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
        await SaveSettingsAsync();
    }

    public static async Task ToggleOpeningInSameWindow(MainWindowViewModel vm)
    {
        if (Settings.UIProperties.OpenInSameWindow)
        {
            IPC.StopListening();
            Settings.UIProperties.OpenInSameWindow = false;
        }
        else
        {
            //_ = IPC.StartListeningForArguments(vm);
            Settings.UIProperties.OpenInSameWindow = true;
        }

        await SaveSettingsAsync();
    }

    public static async Task ToggleFileHistory(MainWindowViewModel vm)
    {
        if (Settings.Navigation.IsFileHistoryEnabled)
        {
            vm.GlobalSettings.IsFileHistoryEnabled.Value = false;
            Settings.Navigation.IsFileHistoryEnabled = false;
            
            vm.Translation.ToggleFileHistory.Value = TranslationManager.Translation.FileHistoryDisabled;
        }
        else
        {
            vm.GlobalSettings.IsFileHistoryEnabled.Value = true;
            Settings.Navigation.IsFileHistoryEnabled = true;
            
            vm.Translation.ToggleFileHistory.Value = TranslationManager.Translation.FileHistoryEnabled;
        }
        
        await SaveSettingsAsync();
    }
    
    #region Image settings

    public static async Task ToggleSideBySide()
    {
        Settings.ImageScaling.ShowImageSideBySide = !Settings.ImageScaling.ShowImageSideBySide;
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        var window = core.MainWindows.ActiveWindow.Value;
        var tab = window.WindowTabs.ActiveTab.Value;
        window.IsSideBySide.Value = Settings.ImageScaling.ShowImageSideBySide;
        await tab.ImageIterator.ReloadAsync(tab.GetTabCancellation()).ConfigureAwait(false);
        WindowResizing.SetSize(window, WindowResizeReason.Application);

        await SaveSettingsAsync();
    }
    
    public static async Task ToggleScroll(MainWindowViewModel vm)
    {
        if (vm is null)
        {
            return;
        }
        
        if (Settings.Zoom.ScrollEnabled)
        {
            TurnOffScroll(vm);
        }
        else
        {
            TurnOnScroll(vm);
        }
        
        WindowResizing.SetSize(vm, WindowResizeReason.Application);
        
        await SaveSettingsAsync();
    }
    
    public static void TurnOffScroll(MainWindowViewModel vm)
    {
        //vm.ToggleScrollBarVisibility.Value = ScrollBarVisibility.Disabled;
        vm.Translation.IsScrolling.Value = TranslationManager.Translation.ScrollingDisabled;
        vm.IsScrollingEnabled.Value = false;
        Settings.Zoom.ScrollEnabled = false;
    }
    
    public static void TurnOnScroll(MainWindowViewModel vm)
    {
        // vm.ToggleScrollBarVisibility.Value = ScrollBarVisibility.Visible;
        vm.Translation.IsScrolling.Value = TranslationManager.Translation.ScrollingEnabled;
        // vm.GlobalSettings.IsScrollingEnabled.Value = true;
        Settings.Zoom.ScrollEnabled = true;
        // vm.MainWindow.RightControlOffSetMargin.Value = new Thickness(0,0,30,0);
    }
    
    public static async Task ToggleCtrlZoom(MainWindowViewModel vm)
    {
        // if (vm is null)
        // {
        //     return;
        // }
        //
        // Settings.Zoom.CtrlZoom = !Settings.Zoom.CtrlZoom;
        // vm.Translation.IsCtrlToZoom.Value = Settings.Zoom.CtrlZoom
        //     ? TranslationManager.Translation.CtrlToZoom
        //     : TranslationManager.Translation.ScrollToZoom;
        //
        // // Set source for ChangeCtrlZoomImage
        // if (!Application.Current.TryGetResource("ScanEyeImage", Application.Current.RequestedThemeVariant, out var scanEyeImage ))
        // {
        //     return;
        // }
        // if (!Application.Current.TryGetResource("LeftRightArrowsImage", Application.Current.RequestedThemeVariant, out var leftRightArrowsImage ))
        // {
        //     return;
        // }
        // var isNavigatingWithCtrl = Settings.Zoom.CtrlZoom;
        // vm.MainWindow.ChangeCtrlZoomImage.Value = isNavigatingWithCtrl ? leftRightArrowsImage as DrawingImage : scanEyeImage as DrawingImage;
        // await SaveSettingsAsync().ConfigureAwait(false);
    }
    
    public static void TurnOffCtrlZoom(MainWindowViewModel vm)
    {
        // Settings.Zoom.CtrlZoom = false;
        // vm.Translation.IsCtrlToZoom.Value = TranslationManager.Translation.ScrollToZoom;
        // if (!Application.Current.TryGetResource("ScanEyeImage", Application.Current.RequestedThemeVariant, out var scanEyeImage ))
        // {
        //     return;
        // }
        // vm.MainWindow.ChangeCtrlZoomImage.Value = scanEyeImage as DrawingImage;
    }
    
    public static async Task ToggleLooping(MainWindowViewModel vm)
    {
        var value = !Settings.UIProperties.Looping;
        Settings.UIProperties.Looping = value;
        vm.Translation.IsLooping.Value = value
            ? TranslationManager.Translation.LoopingEnabled
            : TranslationManager.Translation.LoopingDisabled;
        vm.GlobalSettings.IsLooping.Value = value;

        var msg = value
            ? TranslationManager.Translation.LoopingEnabled
            : TranslationManager.Translation.LoopingDisabled;
        TooltipHelper.ShowTooltipMessage(msg);

        var windowTabs = vm.WindowTabs;
        var tab = windowTabs.ActiveTab.CurrentValue;
        if (tab.ImageIterator?.Files.Count > 0)
        {
            var isLooping = Settings.UIProperties.Looping;
            var index = tab.ImageIterator.CurrentIndex;
            var count = tab.ImageIterator.Files.Count;
            if (Settings.ImageScaling.ShowImageSideBySide)
            {
                tab.CanNavigateForwards.Value = isLooping || index < count - 2;
            }
            else
            {
                tab.CanNavigateForwards.Value = isLooping || index < count - 1;
            }

            tab.CanNavigateBackwards.Value = isLooping || index > 0;
        }
        
        await SaveSettingsAsync();
    }
    
    #endregion
}
