using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
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
            () => UIHelper2.GetBottomBar.GetControl<Button>("PreviousButton"),
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.IsNavigationButtonRightClicked,
            clicked => mainWindowVm.IsNavigationButtonRightClicked = clicked,
            () => UIHelper2.GetBottomBar.GetControl<Button>("NextButton"),
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.IsBottomToolbarRightRotationClicked,
            clicked => mainWindowVm.IsNavigationButtonLeftClicked = clicked,
            () => UIHelper2.GetBottomBar.GetControl<Button>("RotateRightButton"),
            new Point(20, 10));

        RepositionCursorIfTriggered(mainWindowVm.IsBottomToolbarLeftRotationClicked,
            clicked => mainWindowVm.IsBottomToolbarLeftRotationClicked = clicked,
            () => UIHelper2.GetBottomBar.GetControl<Button>("RotateLeftButton"),
            new Point(20, 10));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonNextClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonNextClicked = clicked,
            () => UIHelper2.GetHoverBar.GetControl<Button>("NextButton"),
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonPreviousClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonPreviousClicked = clicked,
            () => UIHelper2.GetHoverBar.GetControl<Button>("PreviousButton"),
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.IsClickArrowLeftClicked,
            clicked => mainWindowVm.IsClickArrowLeftClicked = clicked,
            () => UIHelper2.GetClickArrowLeft(mainWindowVm),
            new Point(15, 95));
        
        RepositionCursorIfTriggered(mainWindowVm.IsClickArrowRightClicked,
            clicked => mainWindowVm.IsClickArrowRightClicked = clicked,
            () => UIHelper2.GetClickArrowRight(mainWindowVm),
            new Point(65, 95));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateRightClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateRightClicked = clicked,
            () => UIHelper2.GetHoverBar.GetControl<IconButton>("RotateRightButton"),
            new Point(11, 7));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateLeftClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateLeftClicked = clicked,
            () => UIHelper2.GetHoverBar.GetControl<IconButton>("RotateLeftButton"),
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
                }, DispatcherPriority.Render);

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

    public static void SetSize(double width, double height, WindowResizeReason reason, MainWindowViewModel vm)
    {
        var size = GetSize(width, height, 0, 0, vm.WindowTabs.ActiveTab.CurrentValue.RotationAngle.CurrentValue, vm);

        if (size is null)
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
        if (Settings.WindowProperties.AutoFit)
        {
            if (reason is WindowResizeReason.User)
            {
                vm.ImageWidth.Value =
                    vm.ImageHeight.Value = double.NaN;
            }
            else
            {
                if (rotationAngle is 90 or 270)
                {
                    vm.WindowWidth.Value = size.WindowHeight;
                    vm.WindowHeight.Value = size.WindowWidth;

                    vm.ImageWidth.Value = size.Height;
                    vm.ImageHeight.Value = size.Width;
                }
                else
                {
                    vm.WindowWidth.Value = size.WindowWidth;
                    vm.WindowHeight.Value = size.WindowHeight;

                    vm.ImageWidth.Value = size.Width;
                    vm.ImageHeight.Value = size.Height;
                }
            }
        }
        else
        {
            vm.WindowWidth.Value = Settings.WindowProperties.Width;
            vm.WindowHeight.Value = Settings.WindowProperties.Height;
            vm.ImageWidth.Value =
                vm.ImageHeight.Value = double.NaN;
        }

    }

    public static ImageSize2? GetSize(MainWindowViewModel vm)
    {
        double width, height, secondaryWidth, secondaryHeight;
        if (vm.WindowTabs.SharedCache.TryGet(vm.WindowTabs.ActiveTab.CurrentValue.FileInfo.CurrentValue, out var preloadValue))
        {
            width = preloadValue.ImageModel.PixelWidth;
            height = preloadValue.ImageModel.PixelHeight;
        }
        else
        {
            if (vm.WindowTabs.ActiveTab.CurrentValue.Image.CurrentValue is Bitmap bitmap)
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
            if (vm.WindowTabs.SharedCache.TryGet(vm.WindowTabs.ActiveTab.CurrentValue.SecondaryFileInfo.CurrentValue, out var secondaryPreloadValue))
            {
                secondaryWidth = secondaryPreloadValue.ImageModel.PixelWidth;
                secondaryHeight = secondaryPreloadValue.ImageModel.PixelHeight;
            }
            else
            {
                if (vm.WindowTabs.ActiveTab.CurrentValue.Image.CurrentValue is Bitmap bitmap)
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
        var (containerWidth, containerHeight) = GetContainerSize();

        if (double.IsNaN(width) || double.IsNaN(height) || Application.Current.DataContext is not CoreViewModel core)
        {
            return null;
        }
        var galleryHeight = GalleryHelper.GetGalleryHeight(core.GallerySettings, vm);
        ImageSize2 size;
        if (Settings.ImageScaling.ShowImageSideBySide && secondWidth > 0 && secondHeight > 0)
        {
            size = ImageSizeCalculationHelper2.GetSideBySideImageSize(
                width,
                height,
                secondWidth,
                secondHeight,
                screenSize,
                rotation,
                screenSize.Scaling,
                vm.TitlebarHeight.CurrentValue,
                vm.BottombarHeight.CurrentValue,
                galleryHeight,
                containerWidth,
                containerHeight);
        }
        else
        {
            size = ImageSizeCalculationHelper2.GetImageSize(
                width,
                height,
                screenSize,
                rotation,
                screenSize.Scaling,
                vm.TitlebarHeight.CurrentValue,
                vm.BottombarHeight.CurrentValue,
                galleryHeight,
                containerWidth,
                containerHeight);
        }

        return size;

        (double containerWidth, double containerHeight) GetContainerSize()
        {
            return Dispatcher.CurrentDispatcher.CheckAccess() ? Get() : Dispatcher.CurrentDispatcher.Invoke(Get, DispatcherPriority.Send);

            (double containerWidth, double containerHeight) Get()
            {
                var mainView = UIHelper.GetMainView;

                if (mainView is null)
                {
                    return default;
                }

                containerWidth = mainView.Bounds.Width;
                containerHeight = mainView.Bounds.Height;

                if (double.IsNaN(containerWidth))
                {
                    containerWidth = mainView.Bounds.Width;
                }

                if (double.IsNaN(containerHeight))
                {
                    containerHeight = mainView.Bounds.Height;
                }

                return (containerWidth, containerHeight);
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
            Dispatcher.CurrentDispatcher.InvokeAsync(Set);
        }

        return;

        void Set()
        {
            var top = window.Position.Y;
            var left = window.Position.X;
            Settings.WindowProperties.Top = top;
            Settings.WindowProperties.Left = left;
            Settings.WindowProperties.Width = window.Width;
            Settings.WindowProperties.Height = window.Height;
        }
    }

    #endregion
}