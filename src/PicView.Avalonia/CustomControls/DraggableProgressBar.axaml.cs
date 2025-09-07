using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.Navigation;
using R3;

namespace PicView.Avalonia.CustomControls;

public class DraggableProgressBar : TemplatedControl
{
    // Define the Maximum property
    public static readonly StyledProperty<int> MaximumProperty =
        AvaloniaProperty.Register<DraggableProgressBar, int>(nameof(Maximum), 100);

    // Define the CurrentIndex property with two-way binding
    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<DraggableProgressBar, int>(nameof(CurrentIndex),
            defaultBindingMode: BindingMode.TwoWay);

    // Define a property for the thumb's fill color
    public static readonly StyledProperty<IBrush?> ThumbFillProperty =
        AvaloniaProperty.Register<DraggableProgressBar, IBrush?>(nameof(ThumbFill));

    // Define the DragSensitivity property
    public static readonly StyledProperty<double> DragSensitivityProperty =
        AvaloniaProperty.Register<DraggableProgressBar, double>(nameof(DragSensitivity), 1.0);

    private Ellipse? _thumb;
    private Border? _track;
    private Point _dragStartPoint;
    private int _dragStartIndex;

    static DraggableProgressBar()
    {
        // This allows the control to react to property changes
        AffectsRender<DraggableProgressBar>(CurrentIndexProperty, MaximumProperty);
        AffectsMeasure<DraggableProgressBar>(CurrentIndexProperty, MaximumProperty);
    }

    public DraggableProgressBar()
    {
        // Initialize the observable in the constructor.
        // It will observe the CurrentIndexProperty for changes,
        // wait for a slight pause in changes (debounce), and then emit the last value.
        CurrentIndexProperty.Changed.ToObservable()
            .Debounce(TimeSpan.FromMilliseconds(50))
            .SubscribeAwait(async (x, cancel) =>
            {
                // Check if the new value exists and is different from the old one.
                if (x.NewValue.HasValue && x.OldValue.HasValue && x.NewValue.Value != x.OldValue.Value)
                {
                    await NavigationManager.ImageIterator.IterateToIndex(x.NewValue.Value, cancel);
                }
            });
    }

    public bool IsDragging { get; private set; }

    public int Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public int CurrentIndex
    {
        get => GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    public IBrush? ThumbFill
    {
        get => GetValue(ThumbFillProperty);
        set => SetValue(ThumbFillProperty, value);
    }

    public double DragSensitivity
    {
        get => GetValue(DragSensitivityProperty);
        set => SetValue(DragSensitivityProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _track = e.NameScope.Find<Border>("PART_Track");
        _thumb = e.NameScope.Find<Ellipse>("PART_Thumb");

        if (_track is not null && _thumb is not null)
        {
            UpdateThumbPosition();
        }
    }

    // Recalculate thumb position when CurrentIndex or Maximum changes
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if ((change.Property != CurrentIndexProperty && change.Property != MaximumProperty) || IsDragging)
        {
            return;
        }

        if (_track is not null && _thumb is not null)
        {
            UpdateThumbPosition();
        }
    }

    private void UpdateThumbPosition()
    {
        if (_track == null || _thumb == null || Maximum <= 1)
        {
            return;
        }

        var trackWidth = _track.Bounds.Width - _thumb.Bounds.Width;
        var position = (double)CurrentIndex / (Maximum - 1) * trackWidth;

        if (_thumb.RenderTransform is TranslateTransform transform)
        {
            transform.X = position;
        }
        else
        {
            _thumb.RenderTransform = new TranslateTransform(position, 0);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var properties = e.GetCurrentPoint(_track).Properties;
        if (!properties.IsLeftButtonPressed || _track == null)
        {
            return;
        }

        IsDragging = true;
        _dragStartPoint = e.GetPosition(this); // Use 'this' for consistent coordinate space
        _dragStartIndex = CurrentIndex;
        e.Pointer.Capture(_thumb);
        // REMOVED: UpdateIndexFromPosition(e.GetPosition(_track).X);
        // This prevents the immediate jump that causes the feedback loop.
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!IsDragging || _track == null || _thumb == null || Maximum <= 1)
        {
            return;
        }

        var currentPosition = e.GetPosition(this); // Use 'this' for consistent coordinate space
        var deltaX = currentPosition.X - _dragStartPoint.X;

        var trackWidth = _track.Bounds.Width - _thumb.Width;
        var pixelsPerIndex = trackWidth / (Maximum - 1);

        // Avoid division by zero if track has no width
        if (Math.Abs(pixelsPerIndex) < 0.001)
        {
            return;
        }

        // Apply sensitivity
        var sensitiveDragPerIndex = pixelsPerIndex * DragSensitivity;
        if (Math.Abs(sensitiveDragPerIndex) < 0.001)
        {
            return; // Avoid division by zero
        }

        var indexChange = deltaX / sensitiveDragPerIndex;

        var newIndex = _dragStartIndex + indexChange;
        var clampedIndex = (int)Math.Round(Math.Clamp(newIndex, 0, Maximum - 1));

        if (CurrentIndex == clampedIndex)
        {
            return;
        }

        CurrentIndex = clampedIndex;
        UpdateThumbPosition();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!IsDragging)
        {
            return;
        }

        IsDragging = false;
        e.Pointer.Capture(null);
    }

    private void UpdateIndexFromPosition(double x)
    {
        if (_track == null || _thumb == null || Maximum <= 1)
        {
            return;
        }

        var trackWidth = _track.Bounds.Width - _thumb.Width;
        var thumbWidth = _thumb.Width;

        // Clamp the position within the track bounds
        var clampedX = Math.Clamp(x - thumbWidth / 2, 0, trackWidth);

        var percentage = clampedX / trackWidth;
        var newIndex = (int)Math.Round(percentage * (Maximum - 1));

        if (CurrentIndex == newIndex)
        {
            return;
        }

        CurrentIndex = newIndex;
        UpdateThumbPosition(); // Visually update while dragging
    }

    // Ensure the thumb is in the correct position when the control is resized
    protected override Size ArrangeOverride(Size finalSize)
    {
        var arrangedSize = base.ArrangeOverride(finalSize);
        UpdateThumbPosition();
        return arrangedSize;
    }
}