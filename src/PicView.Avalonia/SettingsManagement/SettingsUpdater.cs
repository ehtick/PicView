using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Gallery;
using PicView.Core.Localization;
using PicView.Core.Models;
using PicView.Core.Sizing;

namespace PicView.Avalonia.SettingsManagement;
public static class SettingsUpdater
{
    public static void ValidateGallerySettings(MainViewModel vm, bool settingsExists)
    {
        if (vm.Gallery is not {} gallery)
        {
            return;
        }
        vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.Value  = Settings.Gallery.ExpandedGalleryItemSize;
        vm.Gallery.GalleryItem.BottomGalleryItemHeight.Value = Settings.Gallery.BottomGalleryItemSize;
        if (!settingsExists)
        {
            vm.Gallery.GalleryItem.BottomGalleryItemHeight.Value = GalleryDefaults.DefaultBottomGalleryHeight;
            vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.Value = GalleryDefaults.DefaultFullGalleryHeight;
        }

        // Set default gallery sizes if they are out of range or upgrading from an old version
        if (vm.Gallery.GalleryItem.BottomGalleryItemHeight.CurrentValue < GalleryDefaults.MinBottomGalleryItemHeight ||
            vm.Gallery.GalleryItem.BottomGalleryItemHeight.CurrentValue > GalleryDefaults.MaxBottomGalleryItemHeight)
        {
            vm.Gallery.GalleryItem.BottomGalleryItemHeight.Value = GalleryDefaults.DefaultBottomGalleryHeight;
        }

        if (vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.CurrentValue < GalleryDefaults.MinFullGalleryItemHeight ||
            vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.CurrentValue > GalleryDefaults.MaxFullGalleryItemHeight)
        {
            vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.Value = GalleryDefaults.DefaultFullGalleryHeight;
        }

        if (settingsExists)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Settings.Gallery.BottomGalleryStretchMode))
        {
            Settings.Gallery.BottomGalleryStretchMode = "UniformToFill";
        }

        if (string.IsNullOrWhiteSpace(Settings.Gallery.FullGalleryStretchMode))
        {
            Settings.Gallery.FullGalleryStretchMode = "UniformToFill";
        }
    }

    public static void InitializeSettings(MainViewModel vm)
    {
        // Set corner radius on macOS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            vm.MainWindow.BottomCornerRadius.Value = new CornerRadius(0, 0, 8, 8);
        }
        
        vm.MainWindow.TitlebarHeight.Value = Settings.WindowProperties.Fullscreen
                                       || !Settings.UIProperties.ShowInterface
            ? 0
            : SizeDefaults.MainTitlebarHeight;
        vm.MainWindow.BottombarHeight.Value = Settings.WindowProperties.Fullscreen
                                              || !Settings.UIProperties.ShowInterface
            ? 0
            : SizeDefaults.BottombarHeight;
        vm.PicViewer.IsShowingSideBySide.Value = Settings.ImageScaling.ShowImageSideBySide;
        vm.IsAvoidingZoomingOut  = Settings.Zoom.AvoidZoomingOut;
        vm.IsUIShown  = Settings.UIProperties.ShowInterface;
        vm.IsTopToolbarShown  = Settings.UIProperties.ShowInterface;
        vm.IsBottomToolbarShown   = Settings.UIProperties.ShowBottomNavBar &&
                                    Settings.UIProperties.ShowInterface;
        vm.IsShowingTaskbarProgress  = Settings.UIProperties.IsTaskbarProgressEnabled;
        vm.IsFullscreen  = Settings.WindowProperties.Fullscreen;
        vm.IsTopMost  = Settings.WindowProperties.TopMost;
        vm.IsIncludingSubdirectories = Settings.Sorting.IncludeSubDirectories;
        vm.IsStretched = Settings.ImageScaling.StretchImage;
        vm.IsLooping  = Settings.UIProperties.Looping;
        vm.IsAutoFit  = Settings.WindowProperties.AutoFit;
        vm.IsStayingCentered  = Settings.WindowProperties.KeepCentered;
        vm.IsOpeningInSameWindow  = Settings.UIProperties.OpenInSameWindow;
        vm.IsShowingConfirmationOnEsc  = Settings.UIProperties.ShowConfirmationOnEsc;   
        vm.IsUsingTouchpad  = Settings.Zoom.IsUsingTouchPad;
        vm.IsAscending  = Settings.Sorting.Ascending;
        vm.MainWindow.BackgroundChoice.Value = Settings.UIProperties.BgColorChoice;
        vm.IsConstrainingBackgroundColor = Settings.UIProperties.IsConstrainBackgroundColorEnabled;
    }
    
    public static async Task ResetSettings(MainViewModel vm)
    {
        vm.IsLoading = true;

        try
        {
            ResetDefaults();

            ThemeManager.DetermineTheme(Application.Current, false);
        
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                TurnOffUsingTouchpad(vm);
            }
            else
            {
                TurnOffUsingTouchpad(vm);
            }

            if (vm.Gallery is not null)
            {
                vm.Gallery.GalleryItem.BottomGalleryItemHeight.Value = GalleryDefaults.DefaultBottomGalleryHeight;
                vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.Value = GalleryDefaults.DefaultFullGalleryHeight;
            }
            
            if (string.IsNullOrWhiteSpace(Settings.Gallery.BottomGalleryStretchMode))
            {
                Settings.Gallery.BottomGalleryStretchMode = "UniformToFill";
            }

            if (string.IsNullOrWhiteSpace(Settings.Gallery.FullGalleryStretchMode))
            {
                Settings.Gallery.FullGalleryStretchMode = "UniformToFill";
            }
        
            await TurnOffSubdirectories(vm);
        
            TurnOffSideBySide(vm);
            TurnOffScroll(vm);
            TurnOffCtrlZoom(vm);
            TurnOffLooping(vm);
        
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                vm.PlatformService.StopTaskbarProgress();
                if (NavigationManager.CanNavigate(vm))
                {
                    vm.PlatformService.SetTaskbarProgress((ulong)NavigationManager.GetCurrentIndex,
                        (ulong)NavigationManager.GetCount);
                }
                WindowResizing.SetSize(vm);
            });
            
            await SaveSettingsAsync();
        }
        finally
        {
            TitleManager.SetTitle(vm);
            vm.IsLoading = false;
        }
    }

    public static async Task ToggleUsingTouchpad(MainViewModel vm)
    {
        if (Settings.Zoom.IsUsingTouchPad)
        {
            TurnOffUsingTouchpad(vm);
        }
        else
        {
            TurnOnUsingTouchpad(vm);
        }
    
        await SaveSettingsAsync();
    }
    
    public static void TurnOffUsingTouchpad(MainViewModel vm)
    {
        Settings.Zoom.IsUsingTouchPad = false;
        vm.Translation.IsUsingTouchpad.Value = TranslationManager.Translation.UsingMouse;
        vm.IsUsingTouchpad = false;
    }
    
    public static void TurnOnUsingTouchpad(MainViewModel vm)
    {
        Settings.Zoom.IsUsingTouchPad = true;
        vm.Translation.IsUsingTouchpad.Value = TranslationManager.Translation.UsingTouchpad;
        vm.IsUsingTouchpad = true;
    }
    
    public static async Task ToggleSubdirectories(MainViewModel vm)
    {
        if (Settings.Sorting.IncludeSubDirectories)
        {
            await TurnOffSubdirectories(vm).ConfigureAwait(false);
        }
        else
        {
            await TurnOnSubdirectories(vm).ConfigureAwait(false);
        }
        await SaveSettingsAsync();
    }
    
    public static async Task TurnOffSubdirectories(MainViewModel vm)
    {
        vm.IsIncludingSubdirectories = false;
        Settings.Sorting.IncludeSubDirectories = false;

        if (!NavigationManager.CanNavigate(vm))
        {
            return;
        }
        
        await NavigationManager.ReloadFileListAsync().ConfigureAwait(false);
        TitleManager.SetTitle(vm);
    }
    
    public static async Task TurnOnSubdirectories(MainViewModel vm)
    {
        vm.IsIncludingSubdirectories = true;
        Settings.Sorting.IncludeSubDirectories = true;
        
        if (!NavigationManager.CanNavigate(vm))
        {
            return;
        }
        
        await NavigationManager.ReloadFileListAsync().ConfigureAwait(false);
        TitleManager.SetTitle(vm);
    }
    
    public static async Task ToggleTaskbarProgress(MainViewModel vm)
    {
        if (Settings.UIProperties.IsTaskbarProgressEnabled)
        {
            Settings.UIProperties.IsTaskbarProgressEnabled = false;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                vm.PlatformService.StopTaskbarProgress();
            });
        }
        else
        {
            Settings.UIProperties.IsTaskbarProgressEnabled = true;
            if (NavigationManager.CanNavigate(vm))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    vm.PlatformService.SetTaskbarProgress((ulong)NavigationManager.GetCurrentIndex,
                        (ulong)NavigationManager.GetCount);
                });
            }
        }

        await SaveSettingsAsync();
    }
    
    public static async Task ToggleConstrainBackgroundColor(MainViewModel vm)
    {
        if (Settings.UIProperties.IsConstrainBackgroundColorEnabled)
        {
            Settings.UIProperties.IsConstrainBackgroundColorEnabled = false;
            vm.IsConstrainingBackgroundColor = false;
        }
        else
        {
            Settings.UIProperties.IsConstrainBackgroundColorEnabled = true;
            vm.IsConstrainingBackgroundColor = true;
        }
        
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            BackgroundManager.SetBackground(vm);
        });
        await SaveSettingsAsync();
    }

    public static async Task ToggleOpeningInSameWindow(MainViewModel vm)
    {
        if (Settings.UIProperties.OpenInSameWindow)
        {
            _ = IPC.StartListeningForArguments(vm);
            Settings.UIProperties.OpenInSameWindow = true;
        }
        else
        {
            IPC.StopListening();
            Settings.UIProperties.OpenInSameWindow = false;
        }

        await SaveSettingsAsync();
    }
    
    #region Image settings

    public static async Task ToggleSideBySide(MainViewModel vm)
    {
        if (vm is null)
        {
            return;
        }
        
        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            TurnOffSideBySide(vm);
        }
        else
        {
            await TurnOnSideBySide(vm);
        }

        await SaveSettingsAsync();
    }

    public static void TurnOffSideBySide(MainViewModel vm)
    {
        Settings.ImageScaling.ShowImageSideBySide = false;
        vm.PicViewer.IsShowingSideBySide.Value = false;
        vm.PicViewer.SecondaryImageSource.Value = null;
        WindowResizing.SetSize(vm);
        TitleManager.SetTitle(vm);
    }
    
    public static async Task TurnOnSideBySide(MainViewModel vm)
    {
        Settings.ImageScaling.ShowImageSideBySide = true;
        vm.PicViewer.IsShowingSideBySide.Value = true;
        if (NavigationManager.CanNavigate(vm))
        {
            var preloadValue = await NavigationManager.GetNextPreLoadValueAsync();
            if (preloadValue is null)
            {
#if DEBUG
                Console.WriteLine($"{nameof(TurnOnSideBySide)} {nameof(preloadValue)} is null");       
#endif
                return;
            }
            vm.PicViewer.SecondaryImageSource.Value = preloadValue.ImageModel.Image;
            var imageModel1 = new ImageModel
            {
                FileInfo = vm.PicViewer.FileInfo.CurrentValue,
                PixelWidth = (int)vm.PicViewer.ImageWidth.CurrentValue,
                PixelHeight = (int)vm.PicViewer.ImageHeight.CurrentValue,
                ImageType = vm.PicViewer.ImageType.CurrentValue,
                Image = vm.PicViewer.ImageSource,
                EXIFOrientation = vm.PicViewer.ExifOrientation.CurrentValue
            };
            var imageModel2 = new ImageModel
            {
                FileInfo = preloadValue.ImageModel.FileInfo,
                PixelWidth = preloadValue.ImageModel.PixelWidth,
                PixelHeight = preloadValue.ImageModel.PixelHeight,
                ImageType = preloadValue.ImageModel.ImageType,
                Image = preloadValue.ImageModel.Image,
                EXIFOrientation = preloadValue.ImageModel.EXIFOrientation
            };
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                WindowResizing.SetSize(vm.PicViewer.ImageWidth.CurrentValue, vm.PicViewer.ImageHeight.CurrentValue, preloadValue.ImageModel.PixelWidth,
                    preloadValue.ImageModel.PixelHeight, vm.RotationAngle, vm);
                TitleManager.SetSideBySideTitle(vm, imageModel1, imageModel2);
            });
        }
    }
    
    public static async Task ToggleScroll(MainViewModel vm)
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
        
        WindowResizing.SetSize(vm);
        
        await SaveSettingsAsync();
    }
    
    public static void TurnOffScroll(MainViewModel vm)
    {
        vm.ToggleScrollBarVisibility = ScrollBarVisibility.Disabled;
        vm.Translation.IsScrolling.Value = TranslationManager.Translation.ScrollingDisabled;
        vm.IsScrollingEnabled = false;
        Settings.Zoom.ScrollEnabled = false;
        vm.MainWindow.RightControlOffSetMargin.Value = new Thickness(0);
    }
    
    public static void TurnOnScroll(MainViewModel vm)
    {
        vm.ToggleScrollBarVisibility = ScrollBarVisibility.Visible;
        vm.Translation.IsScrolling.Value = TranslationManager.Translation.ScrollingEnabled;
        vm.IsScrollingEnabled = true;
        Settings.Zoom.ScrollEnabled = true;
        vm.MainWindow.RightControlOffSetMargin.Value = new Thickness(0,0,30,0);
    }
    
    public static async Task ToggleCtrlZoom(MainViewModel vm)
    {
        if (vm is null)
        {
            return;
        }
        
        Settings.Zoom.CtrlZoom = !Settings.Zoom.CtrlZoom;
        vm.Translation.IsCtrlToZoom.Value = Settings.Zoom.CtrlZoom
            ? TranslationManager.Translation.CtrlToZoom
            : TranslationManager.Translation.ScrollToZoom;
        
        // Set source for ChangeCtrlZoomImage
        if (!Application.Current.TryGetResource("ScanEyeImage", Application.Current.RequestedThemeVariant, out var scanEyeImage ))
        {
            return;
        }
        if (!Application.Current.TryGetResource("LeftRightArrowsImage", Application.Current.RequestedThemeVariant, out var leftRightArrowsImage ))
        {
            return;
        }
        var isNavigatingWithCtrl = Settings.Zoom.CtrlZoom;
        vm.ChangeCtrlZoomImage = isNavigatingWithCtrl ? leftRightArrowsImage as DrawingImage : scanEyeImage as DrawingImage;
        await SaveSettingsAsync().ConfigureAwait(false);
    }
    
    public static void TurnOffCtrlZoom(MainViewModel vm)
    {
        Settings.Zoom.CtrlZoom = false;
        vm.Translation.IsCtrlToZoom.Value = TranslationManager.Translation.ScrollToZoom;
        if (!Application.Current.TryGetResource("ScanEyeImage", Application.Current.RequestedThemeVariant, out var scanEyeImage ))
        {
            return;
        }
        vm.ChangeCtrlZoomImage = scanEyeImage as DrawingImage;
    }
    
    public static async Task ToggleLooping(MainViewModel vm)
    {
        if (vm is null)
        {
            return;
        }
        
        var value = !Settings.UIProperties.Looping;
        Settings.UIProperties.Looping = value;
        vm.Translation.IsLooping.Value = value
            ? TranslationManager.Translation.LoopingEnabled
            : TranslationManager.Translation.LoopingDisabled;
        vm.IsLooping = value;

        var msg = value
            ? TranslationManager.Translation.LoopingEnabled
            : TranslationManager.Translation.LoopingDisabled;
        await TooltipHelper.ShowTooltipMessageAsync(msg);

        await SaveSettingsAsync();
    }
    
    public static void TurnOffLooping(MainViewModel vm)
    {
        Settings.UIProperties.Looping = false;
        vm.Translation.IsLooping.Value = TranslationManager.Translation.LoopingDisabled;
        vm.IsLooping = false;
    }
    
    #endregion
}
