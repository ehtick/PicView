using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views;
using Point = Avalonia.Point;

namespace PicView.Avalonia.ImageTransformations;

public class Zoom
{
    private bool _captured;
    private Point _origin;

    private ScaleTransform? _scaleTransform;

    private Point _start;
    private TranslateTransform? _translateTransform;
    
    public bool IsZoomed { get; private set; }

    public Zoom(Border border)
    {
        InitializeZoom(border);
    }

    /// <summary>
    /// Initialize the necessary transforms for zooming
    /// </summary>
    private void InitializeZoom(Border border)
    {
        border.RenderTransform = new TransformGroup
        {
            Children =
            [
                new ScaleTransform(),
                new TranslateTransform()
            ]
        };

        _scaleTransform = (ScaleTransform)((TransformGroup)border.RenderTransform)
            .Children.First(tr => tr is ScaleTransform);

        _translateTransform = (TranslateTransform)((TransformGroup)border.RenderTransform)
            .Children.First(tr => tr is TranslateTransform);

        border.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative);
        border.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative);
    }

    public void ZoomIn(PointerWheelEventArgs e, Control parent, Control content, MainViewModel? vm = null)
    {
        ZoomTo(e, true, parent, content, vm);
    }

    public void ZoomOut(PointerWheelEventArgs e, Control parent, Control content, MainViewModel? vm = null)
    {
        ZoomTo(e, false, parent, content, vm);
    }

    public void ZoomIn(MainViewModel? vm = null)
    {
        ZoomTo(_start, true, vm);
    }

    public void ZoomOut(MainViewModel? vm = null)
    {
        ZoomTo(_start, false, vm);
    }

    private Point GetRelativePosition(Control parent, Control content)
    {
        // Get center of the ImageViewer control
        var centerX = parent.Bounds.Width / 2;
        var centerY = parent.Bounds.Height / 2;

        // Convert to MainImage's coordinate space
        return parent.TranslatePoint(new Point(centerX, centerY), content)
               ?? new Point(content.Bounds.Width / 2, content.Bounds.Height / 2);
    }

    public void ZoomTo(PointerWheelEventArgs e, bool isZoomIn, Control parent, Control content,
        MainViewModel? vm = null)
    {
        var relativePosition = !content.IsPointerOver ? GetRelativePosition(parent, content) : e.GetPosition(content);
        ZoomTo(relativePosition, isZoomIn, vm);
    }

    public void ZoomTo(Point point, bool isZoomIn, MainViewModel? vm = null)
    {
        if (_scaleTransform == null || _translateTransform == null)
        {
            return;
        }

        var currentZoom = _scaleTransform.ScaleX;
        var zoomSpeed = Settings.Zoom.ZoomSpeed;

        switch (currentZoom)
        {
            // Increase speed based on the current zoom level
            case > 15 when isZoomIn:
                return;

            case > 4:
                zoomSpeed += 1;
                break;

            case > 3.2:
                zoomSpeed += 0.8;
                break;

            case > 1.6:
                zoomSpeed += 0.5;
                break;
        }

        if (!isZoomIn)
        {
            zoomSpeed = -zoomSpeed;
        }

        currentZoom += zoomSpeed;
        currentZoom = Math.Max(0.09, currentZoom); // Fix for zooming out too much
        if (Settings.Zoom.AvoidZoomingOut && currentZoom < 1.0)
        {
            ResetZoom(true, vm);
        }
        else
        {
            if (currentZoom is > 0.95 and < 1.05 or > 1.0 and < 1.05)
            {
                ResetZoom(true, vm);
            }
            else
            {
                ZoomTo(point, currentZoom, true, vm);
            }
        }
    }

    public void ZoomTo(Point point, double zoomValue, bool enableAnimations, MainViewModel? vm = null)
    {
        if (_scaleTransform == null || _translateTransform == null)
        {
            return;
        }

        if (enableAnimations)
        {
            _scaleTransform.Transitions ??=
            [
                new DoubleTransition { Property = ScaleTransform.ScaleXProperty, Duration = TimeSpan.FromSeconds(.25) },
                new DoubleTransition { Property = ScaleTransform.ScaleYProperty, Duration = TimeSpan.FromSeconds(.25) }
            ];
            _translateTransform.Transitions ??=
            [
                new DoubleTransition { Property = TranslateTransform.XProperty, Duration = TimeSpan.FromSeconds(.25) },
                new DoubleTransition { Property = TranslateTransform.YProperty, Duration = TimeSpan.FromSeconds(.25) }
            ];
        }
        else
        {
            _scaleTransform.Transitions = null;
            _translateTransform.Transitions = null;
        }

        var absoluteX = point.X * _scaleTransform.ScaleX + _translateTransform.X;
        var absoluteY = point.Y * _scaleTransform.ScaleY + _translateTransform.Y;

        var newTranslateValueX = Math.Abs(zoomValue - 1) > .2 ? absoluteX - point.X * zoomValue : 0;
        var newTranslateValueY = Math.Abs(zoomValue - 1) > .2 ? absoluteY - point.Y * zoomValue : 0;

        _scaleTransform.ScaleX = zoomValue;
        _scaleTransform.ScaleY = zoomValue;
        _translateTransform.X = newTranslateValueX;
        _translateTransform.Y = newTranslateValueY;

        IsZoomed = zoomValue != 0;
        if (vm is null)
        {
            return;
        }

        vm.ZoomValue = zoomValue;
        if (!IsZoomed)
        {
            return;
        }

        TitleManager.SetTitle(vm);
        _ = TooltipHelper.ShowTooltipMessageAsync($"{Math.Floor(zoomValue * 100)}%", true, TimeSpan.FromSeconds(1));
    }

    public void ResetZoom(bool enableAnimations, MainViewModel? vm = null)
    {
        if (_scaleTransform == null || _translateTransform == null)
        {
            return;
        }

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (enableAnimations)
            {
                _scaleTransform.Transitions ??=
                [
                    new DoubleTransition
                        { Property = ScaleTransform.ScaleXProperty, Duration = TimeSpan.FromSeconds(.25) },
                    new DoubleTransition
                        { Property = ScaleTransform.ScaleYProperty, Duration = TimeSpan.FromSeconds(.25) }
                ];
                _translateTransform.Transitions ??=
                [
                    new DoubleTransition
                        { Property = TranslateTransform.XProperty, Duration = TimeSpan.FromSeconds(.25) },
                    new DoubleTransition
                        { Property = TranslateTransform.YProperty, Duration = TimeSpan.FromSeconds(.25) }
                ];
            }
            else
            {
                _scaleTransform.Transitions = null;
                _translateTransform.Transitions = null;
            }

            _scaleTransform.ScaleX = 1;
            _scaleTransform.ScaleY = 1;
            _translateTransform.X = 0;
            _translateTransform.Y = 0;
        }, DispatcherPriority.Send);

        IsZoomed = false;

        if (vm is null)
        {
            return;
        }

        vm.ZoomValue = 1;
        TooltipHelper.StopTooltipMessage();
        TitleManager.SetTitle(vm);
    }

    public void Capture(PointerEventArgs e)
    {
        if (_captured)
        {
            return;
        }

        if (_scaleTransform == null || _translateTransform == null)
        {
            return;
        }

        var mainView = UIHelper.GetMainView;

        var point = e.GetCurrentPoint(mainView);
        var x = point.Position.X;
        var y = point.Position.Y;
        _start = new Point(x, y);
        _origin = new Point(_translateTransform.X, _translateTransform.Y);
        _captured = true;
    }

    public void Pan(PointerEventArgs e, ImageViewer imageViewer)
    {
        if (!_captured || _scaleTransform == null || !IsZoomed)
        {
            return;
        }

        var dragMousePosition = _start - e.GetPosition(imageViewer);

        // Get the current rotation angle from the ViewModel
        var vm = imageViewer.DataContext as MainViewModel;
        var rotationAngle = vm?.RotationAngle ?? 0;

        // Apply rotation transformation to the mouse movement
        var rotationRadians = rotationAngle * Math.PI / 180.0;
        var cos = Math.Cos(rotationRadians);
        var sin = Math.Sin(rotationRadians);

        double rotatedX;
        double rotatedY;

        switch (rotationAngle)
        {
            case 90:
            case 270:
                rotatedX = -(dragMousePosition.X * cos - dragMousePosition.Y * sin);
                rotatedY = -(dragMousePosition.X * sin + dragMousePosition.Y * cos);
                break;
            default:
                rotatedX = dragMousePosition.X * cos - dragMousePosition.Y * sin;
                rotatedY = dragMousePosition.X * sin + dragMousePosition.Y * cos;
                break;
        }

        var newXproperty = _origin.X - rotatedX;
        var newYproperty = _origin.Y - rotatedY;

        // #185
        if (Settings.WindowProperties.Fullscreen || Settings.WindowProperties.Maximized ||
            !Settings.WindowProperties.AutoFit)
        {
            // TODO: figure out how to pan when not auto fitting window while keeping it in bounds
            _translateTransform.Transitions = null;
            _translateTransform.X = newXproperty;
            _translateTransform.Y = newYproperty;
            e.Handled = true;
            return;
        }

        var actualScrollWidth = imageViewer.ImageScrollViewer.Bounds.Width;
        var actualBorderWidth = imageViewer.MainBorder.Bounds.Width;
        var actualScrollHeight = imageViewer.ImageScrollViewer.Bounds.Height;
        var actualBorderHeight = imageViewer.MainBorder.Bounds.Height;

        var isXOutOfBorder = actualScrollWidth < actualBorderWidth * _scaleTransform.ScaleX;
        var isYOutOfBorder = actualScrollHeight < actualBorderHeight * _scaleTransform.ScaleY;
        var maxX = actualScrollWidth - actualBorderWidth * _scaleTransform.ScaleX;
        var maxY = actualScrollHeight - actualBorderHeight * _scaleTransform.ScaleY;

        // Clamp X translation
        if ((isXOutOfBorder && newXproperty < maxX) || (!isXOutOfBorder && newXproperty > maxX))
        {
            newXproperty = maxX;
        }

        if ((isXOutOfBorder && newXproperty > 0) || (!isXOutOfBorder && newXproperty < 0))
        {
            newXproperty = 0;
        }

        // Clamp Y translation
        if ((isYOutOfBorder && newYproperty < maxY) || (!isYOutOfBorder && newYproperty > maxY))
        {
            newYproperty = maxY;
        }

        if ((isYOutOfBorder && newYproperty > 0) || (!isYOutOfBorder && newYproperty < 0))
        {
            newYproperty = 0;
        }

        _translateTransform.Transitions = null;
        _translateTransform.X = newXproperty;
        _translateTransform.Y = newYproperty;
        e.Handled = true;
    }

    public void Release()
    {
        _captured = false;
    }
}