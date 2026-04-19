using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.UI;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.WindowBehavior;

public static class WindowResizing2
{
    #region Window Resize Handling

    public static bool KeepWindowSize(Window window, AvaloniaPropertyChangedEventArgs<Size> size)
    {
        if (!size.OldValue.HasValue || !size.NewValue.HasValue ||
            size.Sender != window || size.OldValue.Value.Width == 0 || size.OldValue.Value.Height == 0 ||
            size.NewValue.Value.Width == 0 || size.NewValue.Value.Height == 0)
        {
            return false;
        }
        
        var oldSize = size.OldValue.Value;
        var newSize = size.NewValue.Value;

        var x = (oldSize.Width - newSize.Width) / 2;
        var y = (oldSize.Height - newSize.Height) / 2;

        window.Position = new PixelPoint(window.Position.X + (int)x, window.Position.Y + (int)y);
        
        return true;
    }

    public static void HandleWindowResize(Window window, AvaloniaPropertyChangedEventArgs<Size> size)
    {
        if (!Settings.WindowProperties.AutoFit)
        {
            return;
        }

        var isWindowResized = KeepWindowSize(window, size);
        if (!isWindowResized)
        {
            return;
        }
        
        if (window.DataContext is not MainWindowViewModel mainWindowVm)
        {
            return;
        }

        RepositionCursorIfTriggered(mainWindowVm.IsNavigationButtonLeftClicked,
            clicked => mainWindowVm.IsNavigationButtonLeftClicked = clicked,
            () => UIHelper.GetBottomBar.PreviousButton,
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.IsNavigationButtonRightClicked,
            clicked => mainWindowVm.IsNavigationButtonRightClicked = clicked,
            () => UIHelper.GetBottomBar.NextButton,
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.IsBottomToolbarRightRotationClicked,
            clicked => mainWindowVm.IsBottomToolbarRightRotationClicked = clicked,
            () => UIHelper.GetBottomBar.RotateRightButton,
            new Point(20, 10));

        RepositionCursorIfTriggered(mainWindowVm.IsBottomToolbarLeftRotationClicked,
            clicked => mainWindowVm.IsBottomToolbarLeftRotationClicked = clicked,
            () => UIHelper.GetBottomBar.RotateLeftButton,
            new Point(20, 10));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonNextClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonNextClicked = clicked,
            () => UIHelper.GetHoverBar().NextButton,
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonPreviousClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonPreviousClicked = clicked,
            () => UIHelper.GetHoverBar().PreviousButton,
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.IsClickArrowLeftClicked,
            clicked => mainWindowVm.IsClickArrowLeftClicked = clicked,
            () => UIHelper.GetClickArrowLeft(mainWindowVm),
            new Point(15, 95));
        
        RepositionCursorIfTriggered(mainWindowVm.IsClickArrowRightClicked,
            clicked => mainWindowVm.IsClickArrowRightClicked = clicked,
            () => UIHelper.GetClickArrowRight(mainWindowVm),
            new Point(65, 95));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateRightClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateRightClicked = clicked,
            () => UIHelper.GetHoverBar().RotateRightButton,
            new Point(11, 7));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateLeftClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateLeftClicked = clicked,
            () => UIHelper.GetHoverBar().RotateLeftButton,
            new Point(11, 7));
    }

    private static void RepositionCursorIfTriggered(
        bool isTriggered,
        Action<bool> setTrigger,
        Func<Control?> controlProvider,
        Point offset)
    {
        if (!isTriggered)   
        {
            return;
        }
        var control = controlProvider();
        if (control is not null && Application.Current.DataContext is CoreViewModel core)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Dispatcher.CurrentDispatcher.Post(() =>
                {
                    var screenPoint = control.PointToScreen(offset);
                    core.PlatformService.SetCursorPos(screenPoint.X, screenPoint.Y);
                }, DispatcherPriority.Loaded + 1);

            }
            else
            {
                var screenPoint = control.PointToScreen(offset);
                core.PlatformService.SetCursorPos(screenPoint.X, screenPoint.Y);
            }
        }

        setTrigger(false);
    }

    #endregion
    
    #region Set Window Size

    public static void SetSize(MainWindowViewModel vm, WindowResizeReason reason)
    {
        var size = GetSize(vm);

        if (size is null)
        {
            return;
        }

        SetSize(size.Value, reason, vm);
    }

    public static void SetSize(double width, double height, double secondWidth, double secondHeight, WindowResizeReason reason, MainWindowViewModel vm)
    {
        var size = GetSize(width, height, secondWidth, secondHeight, vm.WindowTabs.ActiveTab.CurrentValue.RotationAngle.CurrentValue, vm);

        if (size is null || size.Value.WindowWidth == 0 || size.Value.WindowHeight == 0)
        {
            return;
        }

        SetSize(size.Value, reason, vm);
    }

    public static void SetSize(ImageSize2 size, WindowResizeReason reason, MainWindowViewModel vm)
    {
        vm.ScrollViewerWidth.Value = size.ScrollViewerWidth;
        vm.ScrollViewerHeight.Value = size.ScrollViewerHeight;
        var rotationAngle = vm.WindowTabs.ActiveTab.CurrentValue.RotationAngle.CurrentValue;
        var isRotated = rotationAngle is 90 or 270;

        var imageWidth = isRotated ? size.Height : size.Width;
        var imageHeight = isRotated ? size.Width : size.Height;

        if (Settings.WindowProperties.Fullscreen)
        {
            vm.WindowMaxWidth.Value = ScreenHelper.ScreenSize.Width;
            vm.WindowMaxHeight.Value = ScreenHelper.ScreenSize.Height;
            vm.ImageWidth.Value = imageWidth;
            vm.ImageHeight.Value = imageHeight;
        }
        else if (Settings.WindowProperties.Maximized)
        {
            vm.WindowMaxWidth.Value = ScreenHelper.ScreenSize.WorkingAreaWidth;
            vm.WindowMaxHeight.Value = ScreenHelper.ScreenSize.WorkingAreaHeight;
            vm.ImageWidth.Value = imageWidth;
            vm.ImageHeight.Value = imageHeight;
        }
        else if (Settings.WindowProperties.AutoFit)
        {
            if (reason is WindowResizeReason.User)
            {
                vm.ImageWidth.Value =
                    vm.ImageHeight.Value = double.NaN;
            }
            else
            {
                vm.WindowMaxWidth.Value = isRotated ? size.WindowHeight : size.WindowWidth;
                vm.WindowMaxHeight.Value = isRotated ? size.WindowWidth : size.WindowHeight;
                vm.ImageWidth.Value = imageWidth;
                vm.ImageHeight.Value = imageHeight;
            }
        }
        else
        {
            vm.WindowMaxWidth.Value = Settings.WindowProperties.Width;
            vm.WindowMaxHeight.Value = Settings.WindowProperties.Height;
            vm.ImageWidth.Value =
                vm.ImageHeight.Value = double.NaN;
        }

    }

    public static ImageSize2? GetSize(MainWindowViewModel vm)
    {
        double width, height, secondaryWidth, secondaryHeight;
        if (vm.WindowTabs.SharedCache?.TryGet(vm.WindowTabs.ActiveTab.CurrentValue.Model.CurrentValue.FileInfo, out var preloadValue) ?? false)
        {
            width = preloadValue.ImageModel.PixelWidth;
            height = preloadValue.ImageModel.PixelHeight;
        }
        else
        {
            if (vm.WindowTabs.ActiveTab.CurrentValue.Model.CurrentValue.Image is Bitmap bitmap)
            {
                width = bitmap.PixelSize.Width;
                height = bitmap.PixelSize.Height;
            }
            else
            {
                return null;
            }
        }

        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            if (vm.WindowTabs.SharedCache?.TryGet(vm.WindowTabs.ActiveTab.CurrentValue.SecondaryModel.CurrentValue.FileInfo, out var secondaryPreloadValue) ?? false)
            {
                secondaryWidth = secondaryPreloadValue.ImageModel.PixelWidth;
                secondaryHeight = secondaryPreloadValue.ImageModel.PixelHeight;
            }
            else
            {
                if (vm.WindowTabs.ActiveTab.CurrentValue.Model.CurrentValue.Image is Bitmap bitmap)
                {
                    secondaryWidth = bitmap.PixelSize.Width;
                    secondaryHeight = bitmap.PixelSize.Height;
                }
                else
                {
                    return null;
                }
            }
        }
        else
        {
            secondaryWidth = secondaryHeight = 0;
        }

        return GetSize(width, height, secondaryWidth, secondaryHeight, vm.WindowTabs.ActiveTab.CurrentValue.RotationAngle.CurrentValue,
            vm);
    }

    public static ImageSize2? GetSize(double width, double height, double secondWidth, double secondHeight,
        double rotation,
        MainWindowViewModel vm)
    {
        var screenSize = ScreenHelper.ScreenSize;
        var (uiBottomSize, uiTopSize, galleryWidth, galleryHeight) = GetContainerSize();

        if (double.IsNaN(width) || double.IsNaN(height))
        {
            return null;
        }
        
        if (Settings.ImageScaling.ShowImageSideBySide && secondWidth > 0 && secondHeight > 0)
        {
            return ImageSizeCalculationHelper2.GetSideBySideImageSize(
                width,
                height,
                secondWidth,
                secondHeight,
                screenSize,
                rotation,
                uiTopSize,
                uiBottomSize,
                galleryWidth,
                galleryHeight);
        }
        return ImageSizeCalculationHelper2.GetImageSize(
                width,
                height,
                screenSize,
                rotation,
                uiTopSize,
                uiBottomSize,
                galleryWidth,
                galleryHeight);

        (double, double, double, double) GetContainerSize()
        {
            return Dispatcher.CurrentDispatcher.CheckAccess() ? Get() : Dispatcher.CurrentDispatcher.Invoke(Get, DispatcherPriority.Send);

            (double, double, double, double) Get()
            {
                var (gW, gH) = GalleryHelper.GetGallerySize(vm);
                if (vm.WindowTabs.Tabs.CurrentValue.Count > 1)
                {
                    uiTopSize = SizeDefaults.TabHeight + vm.TitlebarHeight.CurrentValue;
                }
                else
                {
                    uiTopSize = vm.TitlebarHeight.CurrentValue;
                }

                return (UIHelper.GetBottomBar.Bounds.Height, uiTopSize, gW, gH);
            }
        }
    }

    public static void SaveSize(Window window)
    {
        if (Settings.WindowProperties.Maximized || Settings.WindowProperties.Fullscreen)
        {
            return;
        }

        if (Dispatcher.CurrentDispatcher.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.CurrentDispatcher.Invoke(Set);
        }

        return;

        void Set()
        {
            var top = window.Position.Y;
            var left = window.Position.X;
            Settings.WindowProperties.Top = top;
            Settings.WindowProperties.Left = left;
            Settings.WindowProperties.Width = window.Bounds.Width;
            Settings.WindowProperties.Height = window.Bounds.Height;
        }
    }

    #endregion
}