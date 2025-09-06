using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.ImageTransformations;
using PicView.Avalonia.ImageTransformations.Rotation;
using PicView.Avalonia.Input;
using PicView.Avalonia.ViewModels;
using PicView.Core.Exif;

namespace PicView.Avalonia.Views.UC;

public partial class ImageViewer : UserControl
{
    private RotationTransformer? _imageTransformer;
    private Zoom? _zoom;

    public ImageViewer()
    {
        InitializeComponent();
        InitializeImageTransformer();
        AddHandler(PointerWheelChangedEvent, PreviewOnPointerWheelChanged, RoutingStrategies.Tunnel);
        AddHandler(Gestures.PointerTouchPadGestureMagnifyEvent, TouchMagnifyEvent, RoutingStrategies.Bubble);
        AddHandler(Gestures.PinchEvent, TouchMagnifyEvent, RoutingStrategies.Bubble);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ImageControlHelper.TriggerScalingModeUpdate(MainImage, false);
        InitializeZoom();
        InitializeMouseInputHelper();
        LostFocus += OnLostFocus;
    }

    private void OnLostFocus(object? sender, EventArgs e) => _zoom?.Release();

    public void TriggerScalingModeUpdate(bool invalidate) =>
        ImageControlHelper.TriggerScalingModeUpdate(MainImage, invalidate);

    private void TouchMagnifyEvent(object? sender, PointerDeltaEventArgs e) =>
        _zoom?.ZoomTo(e.GetPosition(this), e.Delta.X > 0, Settings.Zoom.ZoomSpeed, DataContext as MainViewModel);

    public static async Task PreviewOnPointerWheelChanged(object? sender, PointerWheelEventArgs e) =>
        await MouseShortcuts.HandlePointerWheelChanged(e);

    private void InitializeZoom() => _zoom = new Zoom(MainBorder);

    public void InitializeImageTransformer()
    {
        _imageTransformer = new RotationTransformer(
            ImageLayoutTransformControl,
            MainImage,
            () => DataContext,
            () =>
            {
                if (_zoom?.IsZoomed == true)
                {
                    ResetZoom(false);
                }
            });
    }

    private void InitializeMouseInputHelper() =>
        MouseShortcuts.InitializeMouseShortcuts(
            ImageScrollViewer,
            async e => { await Dispatcher.UIThread.InvokeAsync(() => { ZoomIn(e); }); },
            async e => { await Dispatcher.UIThread.InvokeAsync(() => { ZoomOut(e); }); });

    #region Zoom

    /// <inheritdoc cref="Zoom.ZoomIn(MainViewModel)"/>
    private void ZoomIn(PointerWheelEventArgs e) =>
        _zoom?.ZoomIn(e, this, MainImage, DataContext as MainViewModel);
    
    /// <inheritdoc cref="Zoom.ZoomOut(MainViewModel)"/>
    private void ZoomOut(PointerWheelEventArgs e) =>
        _zoom?.ZoomOut(e, this, MainImage, DataContext as MainViewModel);

    /// <inheritdoc cref="Zoom.ZoomIn(MainViewModel)"/>
    public void ZoomIn() => _zoom?.ZoomIn(this, MainImage,DataContext as MainViewModel);

    /// <inheritdoc cref="Zoom.ZoomOut(MainViewModel)"/>
    public void ZoomOut() => _zoom?.ZoomOut(this, MainImage, DataContext as MainViewModel);
    /// <inheritdoc cref="Zoom.ResetZoom(bool, MainViewModel)"/>
    public void ResetZoom(bool enableAnimations = true) =>
        _zoom?.ResetZoom(enableAnimations, DataContext as MainViewModel);

    #endregion

    #region Image Transformation

    public void Rotate(bool clockWise) => _imageTransformer?.Rotate(clockWise);

    public void Rotate(double angle) => _imageTransformer?.Rotate(angle);

    public void Flip(bool animate) => _imageTransformer?.Flip(animate);

    public void SetTransform(ExifOrientation? orientation, MagickFormat? format, bool reset = true) =>
        _imageTransformer?.SetTransform(orientation, format, reset);

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