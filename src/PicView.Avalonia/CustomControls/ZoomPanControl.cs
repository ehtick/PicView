using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using R3;

namespace PicView.Avalonia.CustomControls;

public class ZoomPanControl : Decorator
{
    // Bindable properties so you can bind to them if needed.
    public static readonly StyledProperty<double> ScaleProperty =
        AvaloniaProperty.Register<ZoomPanControl, double>(nameof(Scale), 1.0);

    public static readonly StyledProperty<double> RotationProperty =
        AvaloniaProperty.Register<ZoomPanControl, double>(nameof(Rotation));

    public static readonly StyledProperty<double> TranslateXProperty =
        AvaloniaProperty.Register<ZoomPanControl, double>(nameof(TranslateX));

    public static readonly StyledProperty<double> TranslateYProperty =
        AvaloniaProperty.Register<ZoomPanControl, double>(nameof(TranslateY));

    // Deadzone configuration
    public static readonly StyledProperty<double> DeadzoneToleranceProperty =
        AvaloniaProperty.Register<ZoomPanControl, double>(nameof(DeadzoneTolerance), 0.05);

    public static readonly StyledProperty<bool> EnableDeadzoneProperty =
        AvaloniaProperty.Register<ZoomPanControl, bool>(nameof(EnableDeadzone), true);

    private ZoomPreviewer? _zoomPreviewer;

    // Private fields for panning
    private bool _isPanning;
    private Point _panStartPointer;
    private Point _panStartTranslate;

    // Persistent transform objects for animations to work
    private ScaleTransform? _scaleTransform;
    private RotateTransform? _rotateTransform;
    private TranslateTransform? _translateTransform;
    private TransformGroup? _transformGroup;

    /// <summary>
    /// Represents the current zoom level as a percentage.
    /// A value of 100 corresponds to the default zoom level, while values higher or lower indicate zoomed-in or zoomed-out states, respectively.
    /// </summary>
    public double ZoomLevel { get; private set; } = 100;

    // Accessors
    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    /// <summary>Rotation in degrees (clockwise)</summary>
    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    public double TranslateX
    {
        get => GetValue(TranslateXProperty);
        set => SetValue(TranslateXProperty, value);
    }

    public double TranslateY
    {
        get => GetValue(TranslateYProperty);
        set => SetValue(TranslateYProperty, value);
    }

    /// <summary>
    /// The tolerance range around 1.0 where zoom will snap to reset (1.0).
    /// For example, 0.05 means zoom values between 0.95 and 1.05 will snap to 1.0.
    /// </summary>
    public double DeadzoneTolerance
    {
        get => GetValue(DeadzoneToleranceProperty);
        set => SetValue(DeadzoneToleranceProperty, Math.Max(0, value));
    }

    /// <summary>
    /// Whether the deadzone snap-to-reset feature is enabled.
    /// </summary>
    public bool EnableDeadzone
    {
        get => GetValue(EnableDeadzoneProperty);
        set => SetValue(EnableDeadzoneProperty, value);
    }

    public void Initialize()
    {
        // Pointer handling for panning
        AddHandler(PointerPressedEvent, HandlePointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, HandlePointerMoved, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, HandlePointerReleased, RoutingStrategies.Tunnel);

        // When the child changes, ensure transforms are applied
        ChildProperty.Changed.ToObservable().Skip(1).Subscribe(_ => UpdateChildTransform());

        _zoomPreviewer = new ZoomPreviewer
        {
            DataContext = DataContext,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 25, 25),
            IsVisible = false
        };
        _zoomPreviewer.SetZoomPanControl(this);
        UIHelper.GetMainView.MainGrid.Children.Add(_zoomPreviewer);

        ScaleProperty.Changed.ToObservable().Skip(1).Subscribe(_ =>
        {
            UpdateChildTransform();
            UpdatePreviewWindow();
        });
        TranslateXProperty.Changed.ToObservable().Skip(1).Subscribe(_ =>
        {
            UpdateChildTransform();
            UpdatePreviewWindow();
        });
        TranslateYProperty.Changed.ToObservable().Skip(1).Subscribe(_ =>
        {
            UpdateChildTransform();
            UpdatePreviewWindow();
        });
    }

    private void UpdatePreviewWindow()
    {
        if (_zoomPreviewer == null)
        {
            return;
        }

        // Update visibility based on zoom state
        _zoomPreviewer.UpdateVisibility();

        // Update viewport rectangle
        _zoomPreviewer.UpdateViewportRect();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Show preview window when attached
        if (_zoomPreviewer is not { IsVisible: false })
        {
            return;
        }

        _zoomPreviewer.IsVisible = true;
        UpdatePreviewWindow();
    }
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        // After layout, ensure transforms are constrained
        ConstrainTranslationToBounds();
        UpdateChildTransform();
        return base.ArrangeOverride(finalSize);
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Child == null)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ResetZoom(Settings.Zoom.IsZoomAnimated);
            return;
        }

        var p = e.GetPosition(this);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || !(Math.Abs(Scale) > 1.0001))
        {
            return;
        }

        _isPanning = true;
        _panStartPointer = p;
        _panStartTranslate = new Point(TranslateX, TranslateY);
        e.Pointer.Capture(this);
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPanning || Child == null)
        {
            return;
        }

        var p = e.GetPosition(this);
        var delta = p - _panStartPointer;

        // delta is in control coordinates; we need to convert that into translate change respecting rotation/scale
        // Given we compose transforms as: Result = Translate + Rotate( Scale * childPoint )
        // The translate we manipulate is in control coordinates directly, so we can add the delta to it,
        // but rotation means dragging direction should rotate together (so we rotate delta by -Rotation to convert?)
        // Simpler and correct: update TranslateX/Y by delta (works because translate is last transform).
        var newTx = _panStartTranslate.X + delta.X;
        var newTy = _panStartTranslate.Y + delta.Y;

        TranslateX = newTx;
        TranslateY = newTy;

        ConstrainTranslationToBounds();
        UpdateChildTransform();
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isPanning)
        {
            return;
        }

        _isPanning = false;
        e.Pointer.Capture(null);
    }

    /// <summary>
    /// Applies deadzone logic to snap zoom values close to 1.0 back to exactly 1.0.
    /// Also resets translation when snapping to reset zoom.
    /// </summary>
    private double ApplyDeadzone(double targetScale, bool animated, Point? zoomPoint = null)
    {
        if (!EnableDeadzone || DeadzoneTolerance <= 0)
        {
            return targetScale;
        }

        const double resetZoom = 1.0;
        var lowerBound = resetZoom - DeadzoneTolerance;
        var upperBound = resetZoom + DeadzoneTolerance;

        // Check if target scale is within deadzone
        if (!(targetScale >= lowerBound) || !(targetScale <= upperBound))
        {
            return targetScale;
        }

        // Snap to reset zoom and center the content
        SetTransitions(animated);
        Scale = resetZoom;
        TranslateX = 0;
        TranslateY = 0;

        ZoomLevel = resetZoom * 100;

        // If we have a specific zoom point and child is available, center properly
        if (zoomPoint.HasValue && Child != null)
        {
            var center = CenterPoint();
            SetScaleImmediate(resetZoom, center);
        }
        else
        {
            SetScaleImmediate(resetZoom, CenterPoint());
        }

        return resetZoom;

    }

    public void ResetZoom(bool animated)
    {
        if (Child == null)
        {
            return;
        }

        _zoomPreviewer.IsVisible = false;

        SetTransitions(animated);
        Scale = TranslateX = TranslateY = 1.0;
        SetScaleImmediate(1.0, CenterPoint());

        ZoomLevel = 100;
    }

    /// <summary>
    /// Handles zooming functionality using the mouse pointer wheel. Zooms in or out based on the scroll direction.
    /// </summary>
    /// <param name="e">The event arguments containing details about the pointer wheel input.</param>
    public void ZoomWithPointerWheel(PointerWheelEventArgs e) =>
        ZoomWithPointerWheelCore(e.Delta.Y > 0, e.GetPosition(this));

    /// <inheritdoc cref="ZoomWithPointerWheel(PointerWheelEventArgs)"/>
    public void ZoomWithPointerWheel(PointerDeltaEventArgs e) =>
        ZoomWithPointerWheelCore(e.Delta.Y > 0, e.GetPosition(this));

    private void ZoomWithPointerWheelCore(bool isZoomIn, Point pos)
    {
        var step = isZoomIn ? Settings.Zoom.ZoomSpeed : -Math.Abs(Settings.Zoom.ZoomSpeed);
        ZoomBy(step, Settings.Zoom.IsZoomAnimated, pos);
    }

    /// <summary>
    /// Adjusts the zoom scale by a specified multiplier, with optional animation and zoom origin point.
    /// </summary>
    /// <param name="multiplier">The amount by which to adjust the current zoom scale. Positive values zoom in, negative values zoom out.</param>
    /// <param name="animated">Specifies whether the zoom adjustment should include an animation.</param>
    /// <param name="zoomAtPoint">The point where the zoom operation is centered. If null, the control's center is used.</param>
    public void ZoomBy(double multiplier, bool animated = true, Point? zoomAtPoint = null)
    {
        var center = zoomAtPoint ?? CenterPoint();
        var targetScale = Math.Max(0.09, Scale + multiplier);

        if (Settings.Zoom.AvoidZoomingOut && targetScale < 1)
        {
            ResetZoom(animated);
            return;
        }

        // Apply deadzone logic
        targetScale = ApplyDeadzone(targetScale, animated, center);

        // Only animate if deadzone didn't handle the zoom
        if (Math.Abs(targetScale - Scale) > 1e-9)
        {
            AnimateScaleTo(targetScale, center, animated);
        }

        ZoomLevel = targetScale * 100;
    }

    /// <summary>
    /// Zooms in the content by increasing the scale based on the specified multiplier.
    /// Updates the zoom level and optionally animates the zoom effect while focusing on a specific point.
    /// </summary>
    /// <param name="multiplier">The factor by which the scale is increased. Defaults to 1.2.</param>
    /// <param name="zoomAtCursorPoint">The point to zoom around. Defaults to the center if null.</param>
    public void ZoomIn(double multiplier = 1.2, Point? zoomAtCursorPoint = null)
    {
        var center = zoomAtCursorPoint ?? CenterPoint();
        var targetScale = Scale * multiplier;

        // Apply deadzone logic
        targetScale = ApplyDeadzone(targetScale, false, center);

        if (Math.Abs(targetScale - Scale) > 1e-9)
        {
            AnimateScaleTo(targetScale, center, false);
        }

        ZoomLevel = targetScale * 100;
    }

    /// <summary>
    /// Zooms out the view by a specified multiplier, applying deadzone logic
    /// and animation if enabled.
    /// </summary>
    /// <param name="multiplier">The factor by which to decrease the zoom level. For example, a multiplier of 1/1.2 reduces the scale.</param>
    public void ZoomOut(double multiplier = 1.0 / 1.2)
    {
        var center = CenterPoint();
        var targetScale = Scale * multiplier;

        // Apply deadzone logic
        targetScale = ApplyDeadzone(targetScale, false, center);

        if (Math.Abs(targetScale - Scale) > 1e-9)
        {
            AnimateScaleTo(targetScale, center, false);
        }

        ZoomLevel = targetScale * 100;
    }

    /// <summary>
    /// Sets the scale of the control immediately, optionally focusing the scaling around a specific point.
    /// Updates the internal zoom level and applies the necessary transformations to the child element.
    /// </summary>
    /// <param name="newScale">The new scale value to apply.</param>
    /// <param name="around">The point around which the scaling should occur. If null, the scaling is applied around the center of the control.</param>
    public void SetScaleImmediate(double newScale, Point? around = null)
    {
        var center = around ?? CenterPoint();
        ApplyScaleAroundPoint(newScale, center);
        ConstrainTranslationToBounds();
        UpdateChildTransform();

        ZoomLevel = newScale * 100;

        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        vm.GlobalSettings.ZoomValue.Value = ZoomLevel;

        TitleManager.SetTitle(vm);
        if (Settings.Zoom.IsShowingZoomPercentagePopup)
        {
            _ = TooltipHelper.ShowTooltipMessageContinuallyAsync($"{Math.Floor(ZoomLevel)}%", true,
                TimeSpan.FromSeconds(1));
        }
    }

    private Point CenterPoint() => new(Bounds.Width / 2.0, Bounds.Height / 2.0);

    private void AnimateScaleTo(double targetScale, Point center, bool animated)
    {
        SetTransitions(animated);
        SetScaleImmediate(targetScale, center);
    }

    private void SetTransitions(bool isAnimated)
    {
        if (_scaleTransform == null || _rotateTransform == null || _translateTransform == null)
        {
            // Transforms not yet initialized
            return;
        }

        if (!isAnimated)
        {
            _scaleTransform.Transitions = null;
            _rotateTransform.Transitions = null;
            _translateTransform.Transitions = null;
        }
        else
        {
            // Apply transitions to the persistent transform objects
            _scaleTransform.Transitions ??=
            [
                new DoubleTransition
                {
                    Property = ScaleTransform.ScaleXProperty,
                    Duration = TimeSpan.FromSeconds(.25)
                },

                new DoubleTransition
                {
                    Property = ScaleTransform.ScaleYProperty,
                    Duration = TimeSpan.FromSeconds(.25)
                }
            ];

            _translateTransform.Transitions ??=
            [
                new DoubleTransition
                {
                    Property = TranslateTransform.XProperty,
                    Duration = TimeSpan.FromSeconds(.20)
                },

                new DoubleTransition
                {
                    Property = TranslateTransform.YProperty,
                    Duration = TimeSpan.FromSeconds(.20)
                }
            ];
        }
    }

    /// <summary>
    /// Applies the scale change so that the child point under `controlPoint` remains fixed in control coordinates.
    /// Takes rotation and flipping into account.
    /// Transform order used: Result = Translate + Rotate( Scale * childPoint ).
    /// </summary>
    private void ApplyScaleAroundPoint(double newScale, Point controlPoint)
    {
        if (Child == null)
        {
            return;
        }

        // Current params
        var s = Scale;
        var sNew = newScale;
        if (Math.Abs(s - sNew) < 1e-9)
        {
            return;
        }

        var angleDeg = Rotation;
        var angleRad = angleDeg * Math.PI / 180.0;

        // Current translate
        var tx = TranslateX;
        var ty = TranslateY;

        // We want child point pChild such that: controlPoint = (tx,ty) + R( s * pChild )
        // => pChild = (1/s) * R^{-1}( controlPoint - t )
        // after scale: t' = controlPoint - R( sNew * pChild )
        // compute:
        var cpMinusT = new Point(controlPoint.X - tx, controlPoint.Y - ty);

        // R^{-1} rotate by -angle
        var cos = Math.Cos(-angleRad);
        var sin = Math.Sin(-angleRad);
        var px = (cpMinusT.X * cos - cpMinusT.Y * sin) / s;
        var py = (cpMinusT.X * sin + cpMinusT.Y * cos) / s;

        // Now compute new translation so that R( sNew * pChild ) + t' = controlPoint
        var cos2 = Math.Cos(angleRad);
        var sin2 = Math.Sin(angleRad);
        var rotatedX = sNew * (px * cos2 - py * sin2);
        var rotatedY = sNew * (px * sin2 + py * cos2);

        var newTx = controlPoint.X - rotatedX;
        var newTy = controlPoint.Y - rotatedY;

        // Commit
        Scale = sNew;
        TranslateX = newTx;
        TranslateY = newTy;
    }

    /// <summary>
    /// Applies the RenderTransform on the child according to current properties.
    /// Transform order: Scale (including flipping) -> Rotate -> Translate.
    /// </summary>
    private void UpdateChildTransform()
    {
        if (Child == null)
        {
            return;
        }

        // Initialize transforms only once
        if (_transformGroup == null)
        {
            _scaleTransform = new ScaleTransform();
            _rotateTransform = new RotateTransform();
            _translateTransform = new TranslateTransform();

            _transformGroup = new TransformGroup();
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_rotateTransform);
            _transformGroup.Children.Add(_translateTransform);

            Child.RenderTransform = _transformGroup;
            Child.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
        }

        // Update the properties of the existing transform objects
        // This is what makes animations work!
        _scaleTransform.ScaleX = Scale;
        _scaleTransform.ScaleY = Scale;
        _rotateTransform.Angle = Rotation;
        _translateTransform.X = TranslateX;
        _translateTransform.Y = TranslateY;

        // Update preview window after transform change
        UpdatePreviewWindow();
    }

    /// <summary>
    /// Ensures the transformed child covers the control area (i.e. prevents panning away until whitespace appears).
    /// Works when rotated/flipped because we compute transformed corners and clamp them against control bounds.
    /// </summary>
    private void ConstrainTranslationToBounds()
    {
        if (Child == null)
        {
            return;
        }

        // We need the child's size in local coordinates
        var childSize = Child.Bounds.Size;
        if (childSize.Width <= 0 || childSize.Height <= 0 || double.IsNaN(childSize.Width) ||
            double.IsNaN(childSize.Height))
        {
            // Fallback to desired size
            childSize = Child.DesiredSize;
        }

        if (childSize.Width <= 0 || childSize.Height <= 0)
        {
            return;
        }

        // Transform the 4 corners through our transform (Scale + Rotate + Translate)
        var angleRad = Rotation * Math.PI / 180.0;
        var cos = Math.Cos(angleRad);
        var sin = Math.Sin(angleRad);

        var corners = new[]
        {
            TransformPointLocal(new Point(0, 0)),
            TransformPointLocal(new Point(childSize.Width, 0)),
            TransformPointLocal(new Point(childSize.Width, childSize.Height)),
            TransformPointLocal(new Point(0, childSize.Height))
        };

        var minX = corners.Min(c => c.X);
        var maxX = corners.Max(c => c.X);
        var minY = corners.Min(c => c.Y);
        var maxY = corners.Max(c => c.Y);

        var controlWidth = Bounds.Width;
        var controlHeight = Bounds.Height;

        // If transformed content is smaller than control in any axis, center it (so user sees content)
        var desiredTx = TranslateX;
        var desiredTy = TranslateY;

        // Horizontal
        var contentWidth = maxX - minX;
        if (contentWidth <= controlWidth)
        {
            // center horizontally
            var centerOffset = (controlWidth - contentWidth) / 2.0 - minX;
            desiredTx += centerOffset;
        }
        else
        {
            // ensure minX <= 0 and maxX >= controlWidth
            if (minX > 0)
            {
                desiredTx -= minX;
            }

            if (maxX < controlWidth)
            {
                desiredTx += controlWidth - maxX;
            }
        }

        // Vertical
        var contentHeight = maxY - minY;
        if (contentHeight <= controlHeight)
        {
            var centerOffset = (controlHeight - contentHeight) / 2.0 - minY;
            desiredTy += centerOffset;
        }
        else
        {
            if (minY > 0)
            {
                desiredTy -= minY;
            }

            if (maxY < controlHeight)
            {
                desiredTy += controlHeight - maxY;
            }
        }

        // Apply clamped translation
        TranslateX = desiredTx;
        TranslateY = desiredTy;
        return;

        Point TransformPointLocal(Point p)
        {
            // Scale
            var sx = Scale * p.X;
            var sy = Scale * p.Y;

            // Rotate
            var rx = sx * cos - sy * sin;
            var ry = sx * sin + sy * cos;

            // Translate
            return new Point(rx + TranslateX, ry + TranslateY);
        }
    }

    /// <summary>
    /// Sets translation values and ensures they are constrained to bounds.
    /// This method should be used by external controls (like <see cref="Views.UC.ZoomPreviewer"/>) to ensure consistent behavior.
    /// </summary>
    public void SetConstrainedTranslation(double translateX, double translateY)
    {
        TranslateX = translateX;
        TranslateY = translateY;
        ConstrainTranslationToBounds();
        UpdateChildTransform();
    }
}