using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;

namespace PicView.Avalonia.Views.UC;

public partial class ZoomPreviewWindow : Window
{
    private ZoomPanControl? _zoomPanControl;
    private Control? _childControl;
    private Timer? _hideTimer;

    public ZoomPreviewWindow()
    {
        InitializeComponent();
    }

    public void SetZoomPanControl(ZoomPanControl zoomPanControl)
    {
        _zoomPanControl = zoomPanControl;
        _childControl = zoomPanControl.Child;

        UpdateViewportRect();
    }

    public void UpdatePosition()
    {
        var p = UIHelper.GetMainView.PointToScreen(
            new Point(UIHelper.GetMainView.Bounds.Width - Width - 15,
                UIHelper.GetMainView.Bounds.Height - Height - 25));

        Position = new PixelPoint(p.X, p.Y);

        _hideTimer?.Dispose();
        _hideTimer = new Timer(_ => { Dispatcher.UIThread.Post(Hide); }, null, TimeSpan.FromSeconds(1.3),
            Timeout.InfiniteTimeSpan);
    }

    public void UpdateVisibility()
    {
        if (_zoomPanControl == null)
        {
            IsVisible = false;
            return;
        }

        // Show when zoomed in or out (not at 1.0 scale)
        var shouldShow = Math.Abs(_zoomPanControl.Scale - 1.0) > 0.001;
        IsVisible = shouldShow;

        if (shouldShow)
        {
            UpdateViewportRect();
        }
    }

    internal void UpdateViewportRect()
    {
        if (_zoomPanControl == null || _childControl == null)
        {
            return;
        }

        var viewportRect = GetCurrentViewportRect();

        // Update the viewport border rectangle
        Canvas.SetLeft(ViewportBorder, viewportRect.X);
        Canvas.SetTop(ViewportBorder, viewportRect.Y);
        ViewportBorder.Width = viewportRect.Width;
        ViewportBorder.Height = viewportRect.Height;
    }

    private Rect GetCurrentViewportRect()
    {
        if (_zoomPanControl == null || _childControl == null)
        {
            return new Rect();
        }

        // Get the viewport rectangle in normalized coordinates (0-1)
        var scale = _zoomPanControl.Scale;
        var translateX = _zoomPanControl.TranslateX;
        var translateY = _zoomPanControl.TranslateY;

        var controlBounds = _zoomPanControl.Bounds;
        var childBounds = _childControl.Bounds;

        if (controlBounds.Width == 0 || controlBounds.Height == 0 ||
            childBounds.Width == 0 || childBounds.Height == 0)
        {
            return new Rect();
        }

        // Calculate what portion of the child is visible in the control
        var visibleLeft = Math.Max(0, -translateX / scale) / childBounds.Width;
        var visibleTop = Math.Max(0, -translateY / scale) / childBounds.Height;
        var visibleRight = Math.Min(1, (controlBounds.Width - translateX) / scale / childBounds.Width);
        var visibleBottom = Math.Min(1, (controlBounds.Height - translateY) / scale / childBounds.Height);

        // Convert to preview window coordinates
        var previewWidth = OverlayCanvas.Bounds.Width;
        var previewHeight = OverlayCanvas.Bounds.Height;

        return new Rect(
            visibleLeft * previewWidth,
            visibleTop * previewHeight,
            (visibleRight - visibleLeft) * previewWidth,
            (visibleBottom - visibleTop) * previewHeight
        );
    }

    protected override void OnClosed(EventArgs e)
    {
        _hideTimer?.Dispose();
        base.OnClosed(e);
    }
}