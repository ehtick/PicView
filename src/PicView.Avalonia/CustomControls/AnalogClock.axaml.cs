using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using PicView.Avalonia.Animations;
using PicView.Avalonia.UI;

namespace PicView.Avalonia.CustomControls;

public partial class AnalogClock : UserControl
{
    public static readonly StyledProperty<DateTime> SelectedTimeProperty =
        AvaloniaProperty.Register<AnalogClock, DateTime>(
            nameof(SelectedTime),
            DateTime.Now,
            coerce: CoerceSelectedTime);

    private const double ClockMargin = 25;
    private double ClockRadius => ClockContainer.Width / 2;

    // ReSharper disable once InconsistentNaming
    private static bool _isPM;

    private Point _centerPoint;
    private Arc? _elapsedHoursArc;
    private Arc? _elapsedMinutesArc;
    private Border? _hourHand;

    // Field to track the previous angle of the minute hand during a drag.
    private double _previousMinuteAngle = -1;

    private readonly bool _is24Hour;

    private bool _isDraggingHourHand;
    private bool _isDraggingHours;
    private bool _isDraggingMinuteHand;
    private bool _isDraggingMinutes;
    private Border? _minuteHand;
    private Arc? _remainingHoursArc;
    private Arc? _remainingMinutesArc;
    private DateTime _initialTime;

    public event EventHandler? Accepted;
    public event EventHandler? Cancelled;

    private bool _isGenerated; // Prevent creating extra hands

    public AnalogClock()
    {
        InitializeComponent();
        _initialTime = SelectedTime;
        _isPM = SelectedTime.Hour >= 12;
        _is24Hour = !DateTimeFormatInfo.CurrentInfo.ShortTimePattern.Contains("tt");

        Loaded += (_, _) =>
        {
            if (_isGenerated)
            {
                UpdateHands(SelectedTime);
                return;
            }
            _isGenerated = true;
            
            EnsureProperSize();

            CancelButton.Click += async (_, _) => await Cancel();
            AcceptButton.Click += async (_, _) => await Accept();

            GenerateClockFace();

            UpdateHands(SelectedTime);
        };
    }

    public DateTime SelectedTime
    {
        get => GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    // This callback is triggered whenever the SelectedTime property changes.
    private static DateTime CoerceSelectedTime(AvaloniaObject instance, DateTime value)
    {
        if (instance is not AnalogClock clock)
        {
            return value;
        }

        // When the property is set externally, update the clock's visual state.
        clock.UpdateHands(value);
        clock._initialTime = value; // Update initial time for cancel functionality

        return value;
    }

    #region Create clock

    private void EnsureProperSize()
    {
        double clockWidth;
        if (double.IsNaN(ClockContainer.Width) || double.IsNaN(ClockContainer.Height))
        {
            ClockContainer.Width = ClockContainer.Height = clockWidth = 300;
        }
        else if (Math.Abs(Width - Height) > .1)
        {
            var highestNumber = Math.Max(Width, Height);
            ClockContainer.Width = ClockContainer.Height = clockWidth = highestNumber;
        }
        else
        {
            clockWidth = ClockContainer.Width;
        }

        var newWidth = clockWidth / 2 + OuterBorder.Padding.Left + OuterBorder.Padding.Right;
        AcceptButton.Width = CancelButton.Width = newWidth;
        DownArrow.Margin = new Thickness(newWidth, -1, 0, 0);
    }

    private void GenerateClockFace()
    {
        DigitalTime.Margin = new Thickness(0, 0, 0, ClockMargin * 3.2);

        const double arcHalfThickness = 4;
        const double textFontSize = 14;
        var numberRadius = ClockRadius - arcHalfThickness - ClockMargin;
        var diameter = ClockRadius * 2;
        var panel = new Panel();

        _centerPoint = new Point(ClockRadius, ClockRadius);


        _remainingHoursArc = GetArc(0, 360, diameter - ClockMargin * 4, false, 0.3, "MainBorderColor");
        _remainingHoursArc.Name = "remainingHoursArc";

        _remainingMinutesArc = GetArc(0, 360, diameter, false, 0.3, "MainBorderColor");
        _remainingMinutesArc.Name = "remainingMinutesArc";

        _elapsedHoursArc = GetArc(-90, 0, diameter - ClockMargin * 4, true, 1, "AccentColor");
        _elapsedHoursArc.Name = "elapsedHoursArc";
        _elapsedHoursArc.Cursor = new Cursor(StandardCursorType.Hand);

        _elapsedMinutesArc = GetArc(-90, 0, diameter, true, 0.7, "AccentColor");
        _elapsedMinutesArc.Name = "elapsedMinutesArc";
        _elapsedMinutesArc.Cursor = new Cursor(StandardCursorType.Hand);

        // Add A border to hours arcs
        var borderArc = GetArc(0, 360, diameter - ClockMargin * 4 + 2, false, 1, "MainBorderColor");
        MainPanel.Children.Add(borderArc);

        // Add pointer events
        _elapsedHoursArc.PointerPressed += ElapsedHoursArc_PointerPressed;
        _elapsedHoursArc.PointerReleased += ElapsedArc_PointerReleased;
        _elapsedHoursArc.PointerMoved += ElapsedHoursArc_PointerMoved;

        _elapsedMinutesArc.PointerPressed += ElapsedMinutesArc_PointerPressed;
        _elapsedMinutesArc.PointerReleased += ElapsedArc_PointerReleased;
        _elapsedMinutesArc.PointerMoved += ElapsedMinutesArc_PointerMoved;

        MainPanel.Children.Add(_remainingHoursArc);
        MainPanel.Children.Add(_remainingMinutesArc);
        MainPanel.Children.Add(_elapsedHoursArc);
        MainPanel.Children.Add(_elapsedMinutesArc);

        // --- Numbers ---
        var canvas = new Canvas();
        for (var h = 1; h <= 12; h++)
        {
            double angleDeg = h * 30 - 90;
            var angleRad = angleDeg * Math.PI / 180;
            var x = ClockRadius + numberRadius * Math.Cos(angleRad);
            var y = ClockRadius + numberRadius * Math.Sin(angleRad);
            var text = new TextBlock { Text = h.ToString(), FontSize = textFontSize, Classes = { "txt" } };
            text.Measure(Size.Infinity);
            var size = text.DesiredSize;
            Canvas.SetLeft(text, x - size.Width / 2);
            Canvas.SetTop(text, y - size.Height / 2);
            canvas.Children.Add(text);
        }

        panel.Children.Add(canvas);
        MainPanel.Children.Add(panel);

        CreateClockHands();
    }

    private static Arc GetArc(double startAngle, double sweepAngle, double diameter, bool fill, double opacity,
        string colorResource)
    {
        var stroke = fill ? UIHelper.GetSolidColorBrush(colorResource) : UIHelper.GetBrush(colorResource);
        return new Arc
        {
            Stroke = stroke,
            StrokeThickness = 8,
            StrokeJoin = PenLineJoin.Round,
            StrokeLineCap = PenLineCap.Round,
            StartAngle = startAngle,
            SweepAngle = sweepAngle,
            Width = diameter,
            Height = diameter,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = opacity
        };
    }

    private void CreateClockHands()
    {
        // Hour hand
        _hourHand = new Border
        {
            Width = 8,
            Height = 45,
            Background = UIHelper.GetBrush("MainTextColorFaded"),
            CornerRadius = new CornerRadius(10, 0),
            RenderTransformOrigin = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            RenderTransform = new RotateTransform(),
            Cursor = new Cursor(StandardCursorType.Hand) // Add cursor indicator
        };

        // Minute hand
        _minuteHand = new Border
        {
            Width = 5,
            Height = 60,
            Background = UIHelper.GetBrush("MainTextColorFaded"),
            CornerRadius = new CornerRadius(10, 0),
            RenderTransformOrigin = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            RenderTransform = new RotateTransform(),
            Cursor = new Cursor(StandardCursorType.Hand) // Add cursor indicator
        };

        // Add pointer events to hands
        _hourHand.PointerPressed += HourHand_PointerPressed;
        _hourHand.PointerReleased += Hand_PointerReleased;
        _hourHand.PointerMoved += HourHand_PointerMoved;

        _minuteHand.PointerPressed += MinuteHand_PointerPressed;
        _minuteHand.PointerReleased += Hand_PointerReleased;
        _minuteHand.PointerMoved += MinuteHand_PointerMoved;

        // Position both hands at center bottom
        Canvas.SetLeft(_hourHand, ClockRadius - _hourHand.Width / 2);
        Canvas.SetTop(_hourHand, ClockRadius - _hourHand.Height);

        Canvas.SetLeft(_minuteHand, ClockRadius - _minuteHand.Width / 2);
        Canvas.SetTop(_minuteHand, ClockRadius - _minuteHand.Height);

        // Add to existing canvas
        var canvas = MainPanel.Children.OfType<Panel>().First().Children.OfType<Canvas>().First();
        canvas.Children.Add(_hourHand);
        canvas.Children.Add(_minuteHand);
    }

    #endregion

    #region Update clock

    // This method now contains the logic to advance/rewind the hour.
    private void UpdateArcFromPoint(Point point, bool isHours)
    {
        if (_elapsedHoursArc is null || _remainingHoursArc is null)
        {
            return;
        }

        var angle = Math.Atan2(point.Y - _centerPoint.Y, point.X - _centerPoint.X);
        // Convert to degrees and adjust to start from top (90 degrees)
        var degrees = (angle * 180 / Math.PI + 90) % 360;
        if (degrees < 0)
        {
            degrees += 360;
        }

        if (isHours)
        {
            // Old sweep before updating
            var oldSweep = _elapsedHoursArc.SweepAngle;

            _elapsedHoursArc.SweepAngle = degrees;
            _remainingHoursArc.StartAngle = -90 + degrees;
            _remainingHoursArc.SweepAngle = 360 - degrees;

            switch (oldSweep)
            {
                // Detect crossing 12 (0°)
                // CCW crossing 12 → back into AM
                case < 30 when degrees > 330:
                // CW crossing 12 → into PM
                case > 330 when degrees < 30:
                    _isPM = !_isPM;
                    break;
            }

            // Update hour hand
            if (_hourHand != null)
            {
                ((RotateTransform)_hourHand.RenderTransform!).Angle = degrees;
            }
        }
        else
        {
            // Initialize previous angle if it's the first time dragging
            if (_previousMinuteAngle < 0)
            {
                _previousMinuteAngle = _elapsedMinutesArc.SweepAngle;
            }

            switch (_previousMinuteAngle)
            {
                // Check for clockwise wrap-around (e.g., from 59 mins to 0 mins)
                case > 270 when degrees < 90:
                    SetValue(SelectedTimeProperty, SelectedTime.AddHours(1));
                    break;
                // Check for counter-clockwise wrap-around (e.g., from 0 mins to 59 mins)
                case < 90 when degrees > 270:
                    SetValue(SelectedTimeProperty, SelectedTime.AddHours(-1));
                    break;
            }

            // Update the previous angle for the next move event
            _previousMinuteAngle = degrees;

            _elapsedMinutesArc.SweepAngle = degrees;
            _remainingMinutesArc.StartAngle = -90 + degrees;
            _remainingMinutesArc.SweepAngle = 360 - degrees;

            // Update minute hand
            if (_minuteHand != null)
            {
                ((RotateTransform)_minuteHand.RenderTransform!).Angle = degrees;
            }
        }

        // Update digital time display
        UpdateDigitalTimeFromAngles();
    }

    private void UpdateDigitalTimeFromAngles()
    {
        if (_elapsedHoursArc == null || _elapsedMinutesArc == null)
        {
            return;
        }

        var rawHours = (int)(_elapsedHoursArc.SweepAngle / 30) % 12;
        if (rawHours == 0)
        {
            rawHours = 12;
        }

        var minutes = (int)(_elapsedMinutesArc.SweepAngle / 6) % 60;

        int displayHours;

        if (_is24Hour)
        {
            displayHours = rawHours % 12 + (_isPM ? 12 : 0);
            if (rawHours == 12)
            {
                displayHours = _isPM ? 12 : 0;
            }
        }
        else
        {
            displayHours = rawHours;
        }

        // Construct the new time while preserving the original date
        var currentDate = SelectedTime.Date;
        var newTime = currentDate.AddHours(displayHours).AddMinutes(minutes);

        // Update the property. SetCurrentValue is used to avoid breaking bindings.
        SetCurrentValue(SelectedTimeProperty, newTime);

        // Update the digital display text from the new property value
        DigitalTime.Text = newTime.ToShortTimeString();

        UpdateArcOpacity();
    }

    private void UpdateArcOpacity()
    {
        if (_elapsedHoursArc == null || _remainingHoursArc == null)
        {
            return;
        }

        if (_isPM)
        {
            // Both arcs use AccentColor, but with different opacity
            _elapsedHoursArc.Stroke = UIHelper.GetSolidColorBrush("AccentColor");
            _remainingHoursArc.Stroke = UIHelper.GetSolidColorBrush("AccentColor");
        }
        else
        {
            // AM: elapsed = accent, remaining = greyed
            _elapsedHoursArc.Stroke = UIHelper.GetSolidColorBrush("AccentColor");
            _remainingHoursArc.Stroke = UIHelper.GetBrush("MainBorderColor");
        }

        _elapsedHoursArc.Opacity = 1.0;
        _remainingHoursArc.Opacity = 0.3;
    }



    private void UpdateHands(DateTime time)
    {
        if (_hourHand == null || _minuteHand == null ||
            _elapsedHoursArc == null || _elapsedMinutesArc == null ||
            _remainingHoursArc == null || _remainingMinutesArc == null)
        {
            return;
        }

        DigitalTime.Text = time.ToShortTimeString();

        // Update hour arcs
        var hourWithFraction = time.Hour % 12 + time.Minute / 60.0;
        var elapsedHoursAngle = hourWithFraction * 30;
        _elapsedHoursArc.StartAngle = -90;
        _elapsedHoursArc.SweepAngle = elapsedHoursAngle;

        _remainingHoursArc.StartAngle = -90 + elapsedHoursAngle;
        _remainingHoursArc.SweepAngle = 360 - elapsedHoursAngle;

        // Update minute arcs
        double elapsedMinutesAngle = time.Minute * 6;
        _elapsedMinutesArc.StartAngle = -90;
        _elapsedMinutesArc.SweepAngle = elapsedMinutesAngle;

        _remainingMinutesArc.StartAngle = -90 + elapsedMinutesAngle;
        _remainingMinutesArc.SweepAngle = 360 - elapsedMinutesAngle;

        // Apply rotations to hands
        ((RotateTransform)_hourHand.RenderTransform!).Angle = elapsedHoursAngle;
        ((RotateTransform)_minuteHand.RenderTransform!).Angle = elapsedMinutesAngle;

        // Update AM/PM state based on DateTime
        _isPM = time.Hour >= 12;
        UpdateArcOpacity();
    }

    #endregion

    #region Events

    private async Task Accept()
    {
        const double speed = .3;
        var closeAnimation = AnimationsHelper.OpacityAnimation(1, 0, speed);
        await closeAnimation.RunAsync(this);
        Accepted?.Invoke(this, EventArgs.Empty);
        await Task.Delay(TimeSpan.FromSeconds(speed * 3));
        IsVisible = false;      // Hide the control
    }

    private async Task Cancel()
    {
        SelectedTime = _initialTime; // Reset to the original time
        var closeAnimation = AnimationsHelper.OpacityAnimation(1, 0, .3);
        await closeAnimation.RunAsync(this);
        IsVisible = false;      // Hide the control
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    private void ElapsedHoursArc_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDraggingHours = true;
        e.Pointer.Capture(sender as IInputElement);
    }

    // Reset the tracking field when a drag starts.
    private void ElapsedMinutesArc_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDraggingMinutes = true;
        _previousMinuteAngle = -1;
        e.Pointer.Capture(sender as IInputElement);
    }

    // Reset the tracking field when a drag ends.
    private void ElapsedArc_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDraggingHours = false;
        _isDraggingMinutes = false;
        _previousMinuteAngle = -1;
        e.Pointer.Capture(null);
    }

    private void ElapsedHoursArc_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDraggingHours)
        {
            return;
        }

        var point = e.GetPosition(MainPanel);
        UpdateArcFromPoint(point, true);
    }

    private void ElapsedMinutesArc_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDraggingMinutes)
        {
            return;
        }

        var point = e.GetPosition(MainPanel);
        UpdateArcFromPoint(point, false);
    }

    private void HourHand_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDraggingHourHand = true;
        e.Pointer.Capture(_hourHand);
        e.Handled = true;
    }

    // Reset the tracking field when a drag starts.
    private void MinuteHand_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDraggingMinuteHand = true;
        _previousMinuteAngle = -1;
        e.Pointer.Capture(_minuteHand);
        e.Handled = true;
    }

    // Reset the tracking field when a drag ends.
    private void Hand_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDraggingHourHand = false;
        _isDraggingMinuteHand = false;
        _previousMinuteAngle = -1;
        e.Pointer.Capture(null);
    }

    private void HourHand_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDraggingHourHand)
        {
            return;
        }

        var point = e.GetPosition(MainPanel);
        UpdateArcFromPoint(point, true);
        e.Handled = true;
    }

    private void MinuteHand_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDraggingMinuteHand)
        {
            return;
        }

        var point = e.GetPosition(MainPanel);
        UpdateArcFromPoint(point, false);
        e.Handled = true;
    }

    #endregion
}
