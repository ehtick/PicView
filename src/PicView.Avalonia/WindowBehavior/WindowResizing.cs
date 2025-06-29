using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.DebugTools;
using PicView.Core.Sizing;

namespace PicView.Avalonia.WindowBehavior;

public static class WindowResizing
{
    #region Window Resize Handling

    public static void HandleWindowResize(Window window, AvaloniaPropertyChangedEventArgs<Size> size)
    {
        if (!Settings.WindowProperties.AutoFit)
        {
            return;
        }

        if (!size.OldValue.HasValue || !size.NewValue.HasValue)
        {
            return;
        }

        if (size.OldValue.Value.Width == 0 || size.OldValue.Value.Height == 0 ||
            size.NewValue.Value.Width == 0 || size.NewValue.Value.Height == 0)
        {
            return;
        }

        if (size.Sender != window)
        {
            return;
        }

        var x = (size.OldValue.Value.Width - size.NewValue.Value.Width) / 2;
        var y = (size.OldValue.Value.Height - size.NewValue.Value.Height) / 2;

        window.Position = new PixelPoint(window.Position.X + (int)x, window.Position.Y + (int)y);
    }

    #endregion

    #region Set Window Size

    public static void SetSize(MainViewModel vm)
    {
        var size = GetSize(vm);

        if (size is null)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            SetSize(size.Value, vm);
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() => SetSize(size.Value, vm));
        }
    }

    public static async Task SetSizeAsync(MainViewModel vm)
    {
        var size = GetSize(vm);

        if (size is null)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() => SetSize(size.Value, vm));
    }

    public static void SetSize(double width, double height, MainViewModel vm)
        => SetSize(width, height, 0, 0, vm.RotationAngle, vm);

    public static void SetSize(double width, double height, double secondWidth, double secondHeight, double rotation,
        MainViewModel vm)
    {
        var size = GetSize(width, height, secondWidth, secondHeight, rotation, vm);

        if (size is null)
        {
            return;
        }

        SetSize(size.Value, vm);
    }

    public static void SetSize(ImageSize size, MainViewModel vm)
    {
        vm.TitleMaxWidth = size.TitleMaxWidth;
        vm.PicViewer.ImageWidth.Value = size.Width;
        vm.PicViewer.SecondaryImageWidth.Value = size.SecondaryWidth;
        vm.PicViewer.ImageHeight.Value = size.Height;
        vm.GalleryMargin = new Thickness(0, 0, 0, size.Margin);

        vm.PicViewer.ScrollViewerWidth.Value = size.ScrollViewerWidth;
        vm.PicViewer.ScrollViewerHeight.Value = size.ScrollViewerHeight;

        if (Settings.WindowProperties.AutoFit)
        {
            if (Settings.WindowProperties.Fullscreen ||
                Settings.WindowProperties.Maximized)
            {
                vm.GalleryWidth = double.NaN;
            }
            else
            {
                var scrollbarSize = Settings.Zoom.ScrollEnabled ? SizeDefaults.ScrollbarSize : 0;
                vm.GalleryWidth = vm.RotationAngle is 90 or 270
                    ? Math.Max(size.Height + scrollbarSize, SizeDefaults.WindowMinSize + scrollbarSize)
                    : Math.Max(size.Width + scrollbarSize, SizeDefaults.WindowMinSize + scrollbarSize);
            }
        }
        else
        {
            vm.GalleryWidth = double.NaN;
        }

        vm.PicViewer.AspectRatio.Value = size.AspectRatio;
    }

    public static ImageSize? GetSize(MainViewModel vm)
    {
        double firstWidth, firstHeight;
        var preloadValue = NavigationManager.GetCurrentPreLoadValue();
        if (preloadValue == null)
        {
            if (vm.PicViewer.FileInfo is null)
            {
                if (vm.PicViewer.ImageSource.CurrentValue is Bitmap bitmap)
                {
                    firstWidth = bitmap.PixelSize.Width;
                    firstHeight = bitmap.PixelSize.Height;
                }
                else
                {
                    return null;
                }
            }
            else if (vm.PicViewer.FileInfo?.CurrentValue?.Exists != null)
            {
                try
                {
                    var magickImage = new MagickImage();
                    magickImage.Ping(vm.PicViewer.FileInfo.CurrentValue);
                    firstWidth = magickImage.Width;
                    firstHeight = magickImage.Height;
                }
                catch (Exception e)
                {
                    DebugHelper.LogDebug(nameof(WindowBehavior), nameof(GetSize), e);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        else
        {
            firstWidth = preloadValue.ImageModel?.PixelWidth ?? vm.PicViewer.ImageWidth.CurrentValue;
            firstHeight = preloadValue.ImageModel?.PixelHeight ?? vm.PicViewer.ImageHeight.CurrentValue;
        }

        if (!Settings.ImageScaling.ShowImageSideBySide)
        {
            return GetSize(firstWidth, firstHeight, 0, 0, vm.RotationAngle, vm);
        }

        var secondaryPreloadValue = NavigationManager.GetNextPreLoadValue();
        double secondWidth, secondHeight;
        if (secondaryPreloadValue is { ImageModel: not null })
        {
            secondWidth = secondaryPreloadValue.ImageModel.PixelWidth;
            secondHeight = secondaryPreloadValue.ImageModel.PixelHeight;
        }
        else if (NavigationManager.CanNavigate(vm))
        {
            var nextFileName = NavigationManager.GetNextFileName;
            var magickImage = new MagickImage();
            magickImage.Ping(nextFileName);
            secondWidth = magickImage.Width;
            secondHeight = magickImage.Height;
        }
        else
        {
            secondWidth = 0;
            secondHeight = 0;
        }

        return GetSize(firstWidth, firstHeight, secondWidth, secondHeight, vm.RotationAngle, vm);
    }

    public static ImageSize? GetSize(double width, double height, double secondWidth, double secondHeight,
        double rotation,
        MainViewModel vm)
    {
        width = width == 0 ? vm.PicViewer.ImageWidth.CurrentValue : width;
        height = height == 0 ? vm.PicViewer.ImageHeight.CurrentValue : height;
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return null;
        }

        var mainView = UIHelper.GetMainView;
        if (mainView is null)
        {
            return null;
        }

        var screenSize = ScreenHelper.ScreenSize;
        var desktopMinWidth = desktop.MainWindow.MinWidth;
        var desktopMinHeight = desktop.MainWindow.MinHeight;
        var containerWidth = mainView.Bounds.Width;
        var containerHeight = mainView.Bounds.Height;

        if (double.IsNaN(containerWidth) || double.IsNaN(containerHeight) || double.IsNaN(width) ||
            double.IsNaN(height))
        {
            return null;
        }

        ImageSize size;
        if (Settings.ImageScaling.ShowImageSideBySide && secondWidth > 0 && secondHeight > 0)
        {
            size = ImageSizeCalculationHelper.GetSideBySideImageSize(
                width,
                height,
                secondWidth,
                secondHeight,
                screenSize,
                desktopMinWidth,
                desktopMinHeight,
                vm.PlatformWindowService.CombinedTitleButtonsWidth,
                rotation,
                screenSize.Scaling,
                vm.TitlebarHeight,
                vm.BottombarHeight,
                vm.GalleryHeight,
                containerWidth,
                containerHeight);
        }
        else
        {
            size = ImageSizeCalculationHelper.GetImageSize(
                width,
                height,
                screenSize,
                desktopMinWidth,
                desktopMinHeight,
                vm.PlatformWindowService.CombinedTitleButtonsWidth,
                rotation,
                screenSize.Scaling,
                vm.TitlebarHeight,
                vm.BottombarHeight,
                vm.GalleryHeight,
                containerWidth,
                containerHeight);
        }

        return size;
    }

    public static void SaveSize(Window window)
    {
        if (Settings.WindowProperties.Maximized || Settings.WindowProperties.Fullscreen)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set);
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