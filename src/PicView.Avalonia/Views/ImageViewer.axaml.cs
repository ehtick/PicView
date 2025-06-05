using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.ImageTransformations;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ImageDecoding;
using PicView.Core.ImageTransformations;

namespace PicView.Avalonia.Views;

public partial class ImageViewer : UserControl
{
    private Zoom? _zoom;

    public ImageViewer()
    {
        InitializeComponent();
        TriggerScalingModeUpdate(false);
        AddHandler(PointerWheelChangedEvent, PreviewOnPointerWheelChanged, RoutingStrategies.Tunnel);
        AddHandler(Gestures.PointerTouchPadGestureMagnifyEvent, TouchMagnifyEvent, RoutingStrategies.Bubble);

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        InitializeZoom();
        LostFocus += OnLostFocus;
    }

    private void OnLostFocus(object? sender, EventArgs e)
    {
        _zoom?.Release();
    }

    public void TriggerScalingModeUpdate(bool invalidate)
    {
        var scalingMode = Settings.ImageScaling.IsScalingSetToNearestNeighbor
            ? BitmapInterpolationMode.LowQuality
            : BitmapInterpolationMode.HighQuality;

        RenderOptions.SetBitmapInterpolationMode(MainImage, scalingMode);
        if (invalidate)
        {
            MainImage.InvalidateVisual();
        }
    }

    private void TouchMagnifyEvent(object? sender, PointerDeltaEventArgs e)
    {
        _zoom?.ZoomTo(e.GetPosition(this), e.Delta.X > 0, DataContext as MainViewModel);
    }

    public async Task PreviewOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;
        await HandlePointerWheelChanged(e);
    }

    private async Task HandlePointerWheelChanged(PointerWheelEventArgs e)
    {
        if (DataContext is not MainViewModel mainViewModel)
        {
            return;
        }

        var ctrl = e.KeyModifiers == KeyModifiers.Control;
        var shift = e.KeyModifiers == KeyModifiers.Shift;
        var reverse = e.Delta.Y < 0;

        if (Settings.Zoom.ScrollEnabled)
        {
            if (!shift)
            {
                if (ctrl && !Settings.Zoom.CtrlZoom)
                {
                    if (IsTouchPadOrTouch(e))
                    {
                        return;
                    }

                    await LoadNextPicAsync(reverse, mainViewModel);
                    return;
                }

                if (IsVerticalScrollBarVisible())
                {
                    ScrollVertically(reverse);
                }
                else
                {
                    await LoadNextPicAsync(reverse, mainViewModel);
                }

                return;
            }
        }

        if (Settings.Zoom.CtrlZoom)
        {
            if (ctrl)
            {
                if (IsTouchPadOrTouch(e))
                {
                    return;
                }

                if (reverse)
                {
                    ZoomOut(e);
                }
                else
                {
                    ZoomIn(e);
                }
            }
            else
            {
                await ScrollOrNavigateAsync(e, reverse, mainViewModel);
            }
        }
        else
        {
            if (ctrl)
            {
                await ScrollOrNavigateAsync(e, reverse, mainViewModel);
            }
            else
            {
                if (reverse)
                {
                    ZoomOut(e);
                }
                else
                {
                    ZoomIn(e);
                }
            }
        }
    }

    private static bool IsTouchPadOrTouch(PointerEventArgs e)
        => Settings.Zoom.IsUsingTouchPad || e.Pointer.Type == PointerType.Touch;

    private bool IsVerticalScrollBarVisible()
        => ImageScrollViewer.VerticalScrollBarVisibility is ScrollBarVisibility.Visible or ScrollBarVisibility.Auto;

    private void ScrollVertically(bool reverse)
    {
        if (reverse)
        {
            ImageScrollViewer.LineDown();
        }
        else
        {
            ImageScrollViewer.LineUp();
        }
    }

    private async Task ScrollOrNavigateAsync(PointerWheelEventArgs e, bool reverse, MainViewModel mainViewModel)
    {
        if (!Settings.Zoom.ScrollEnabled || e.KeyModifiers == KeyModifiers.Shift)
        {
            if (IsTouchPadOrTouch(e))
            {
                return;
            }

            await LoadNextPicAsync(reverse, mainViewModel);
        }
        else
        {
            if (IsVerticalScrollBarVisible())
            {
                ScrollVertically(reverse);
            }
            else
            {
                await LoadNextPicAsync(reverse, mainViewModel);
            }
        }
    }

    private static async Task LoadNextPicAsync(bool reverse, MainViewModel mainViewModel)
    {
        if (Settings.Zoom.IsUsingTouchPad)
        {
            return;
        }

        var next = reverse ? Settings.Zoom.HorizontalReverseScroll : !Settings.Zoom.HorizontalReverseScroll;
        await NavigationManager.Navigate(next, mainViewModel).ConfigureAwait(false);
    }

    #region Zoom

    private void InitializeZoom() => _zoom = new Zoom(MainBorder);

    public void ZoomIn(PointerWheelEventArgs e) =>
        _zoom?.ZoomIn(e, this, MainImage, DataContext as MainViewModel);

    public void ZoomOut(PointerWheelEventArgs e) =>
        _zoom?.ZoomOut(e, this, MainImage, DataContext as MainViewModel);

    public void ZoomIn() => _zoom?.ZoomIn(DataContext as MainViewModel);

    public void ZoomOut() => _zoom?.ZoomOut(DataContext as MainViewModel);

    public void ResetZoom(bool enableAnimations = true) =>
        _zoom?.ResetZoom(enableAnimations, DataContext as MainViewModel);

    #endregion

    #region Rotation and Flip

    public void Rotate(bool clockWise)
    {
        if (DataContext is not MainViewModel vm || MainImage.Source is null)
        {
            return;
        }

        if (RotationHelper.IsValidRotation(vm.RotationAngle))
        {
            var nextAngle = RotationHelper.Rotate(vm.RotationAngle, clockWise);
            vm.RotationAngle = nextAngle switch
            {
                360 => 0,
                -90 => 270,
                _ => nextAngle
            };
        }
        else
        {
            vm.RotationAngle = RotationHelper.NextRotationAngle(vm.RotationAngle, true);
        }

        SetImageLayoutTransform(new RotateTransform(vm.RotationAngle));
        WindowResizing.SetSize(vm);
        MainImage.InvalidateVisual();
    }

    public void Rotate(double angle)
    {
        SetImageLayoutTransform(new RotateTransform(angle));
        WindowResizing.SetSize(DataContext as MainViewModel);
        MainImage.InvalidateVisual();
    }

    private void SetImageLayoutTransform(RotateTransform rotateTransform)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            ImageLayoutTransformControl.LayoutTransform = rotateTransform;
        }
        else
        {
            Dispatcher.UIThread.Invoke(() =>
                ImageLayoutTransformControl.LayoutTransform = rotateTransform);
        }
    }

    public void Flip(bool animate)
    {
        if (DataContext is not MainViewModel vm || MainImage.Source is null)
        {
            return;
        }

        var prevScaleX = vm.PicViewer.ScaleX;
        vm.PicViewer.ScaleX = vm.PicViewer.ScaleX == -1 ? 1 : -1;
        vm.Translation.IsFlipped = vm.PicViewer.ScaleX == 1 ? vm.Translation.UnFlip : vm.Translation.Flip;

        if (animate)
        {
            var flipTransform = new ScaleTransform(prevScaleX, 1)
            {
                Transitions =
                [
                    new DoubleTransition
                        { Property = ScaleTransform.ScaleXProperty, Duration = TimeSpan.FromSeconds(.2) }
                ]
            };
            ImageLayoutTransformControl.RenderTransform = flipTransform;
            flipTransform.ScaleX = vm.PicViewer.ScaleX;
        }
        else
        {
            ImageLayoutTransformControl.RenderTransform = new ScaleTransform(vm.PicViewer.ScaleX, 1);
        }
    }

    public void SetTransform(int scaleX, int rotationAngle)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        vm.PicViewer.ScaleX = scaleX;
        vm.RotationAngle = rotationAngle;
        ImageLayoutTransformControl.RenderTransform = new ScaleTransform(vm.PicViewer.ScaleX, 1);
        ImageLayoutTransformControl.LayoutTransform = new RotateTransform(rotationAngle);

        if (_zoom is not null)
        {
            if (_zoom.IsZoomed)
            {
                ResetZoom(false);
            }
        }
    }

    public void SetTransform(EXIFHelper.EXIFOrientation? orientation, MagickFormat? format, bool reset = true)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyOrientationTransform(orientation, format, reset);
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() =>
                ApplyOrientationTransform(orientation, format, reset), DispatcherPriority.Send);
        }
    }

    private void ApplyOrientationTransform(EXIFHelper.EXIFOrientation? orientation, MagickFormat? format, bool reset)
    {
        if (Settings.Zoom.ScrollEnabled)
        {
            ImageScrollViewer.ScrollToHome();
        }

        if (format is MagickFormat.Heic or MagickFormat.Heif)
        {
            if (reset)
            {
                SetTransform(1, 0);
            }

            return;
        }

        switch (orientation)
        {
            case null:
            case EXIFHelper.EXIFOrientation.None:
            case EXIFHelper.EXIFOrientation.Horizontal:
                if (reset)
                {
                    SetTransform(1, 0);
                }

                break;
            case EXIFHelper.EXIFOrientation.MirrorHorizontal:
                SetTransform(-1, 0);
                break;
            case EXIFHelper.EXIFOrientation.Rotate180:
                SetTransform(1, 180);
                break;
            case EXIFHelper.EXIFOrientation.MirrorVertical:
                SetTransform(-1, 180);
                break;
            case EXIFHelper.EXIFOrientation.MirrorHorizontalRotate270Cw:
                SetTransform(-1, 90);
                break;
            case EXIFHelper.EXIFOrientation.Rotate90Cw:
                SetTransform(1, 90);
                break;
            case EXIFHelper.EXIFOrientation.MirrorHorizontalRotate90Cw:
                SetTransform(-1, 270);
                break;
            case EXIFHelper.EXIFOrientation.Rotated270Cw:
                SetTransform(1, 270);
                break;
        }
    }

    #endregion

    #region Events

    private void ImageScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        e.Handled = true;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ResetZoom();
        }
    }

    private void MainImage_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ResetZoom();
        }
        else
        {
            Pressed(e);
        }
    }

    private void MainImage_OnPointerMoved(object? sender, PointerEventArgs e) =>
        _zoom?.Pan(e, this);

    private void Pressed(PointerEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _zoom?.Capture(e);
    }

    private void MainImage_OnPointerReleased(object? sender, PointerReleasedEventArgs e) =>
        _zoom?.Release();

    #endregion
}