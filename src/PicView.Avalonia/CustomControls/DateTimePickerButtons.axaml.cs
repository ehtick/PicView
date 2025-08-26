using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using R3;

namespace PicView.Avalonia.CustomControls;

public partial class DateTimePickerButtons : UserControl, IDisposable
{
    /// <summary>
    /// Defines the SelectedDateTime dependency property.
    /// This property is two-way bindable and represents the final selected date and time.
    /// </summary>
    public static readonly StyledProperty<DateTime> SelectedDateTimeProperty =
        AvaloniaProperty.Register<DateTimePickerButtons, DateTime>(
            nameof(SelectedDateTime), 
            DateTime.Now,
            defaultBindingMode: BindingMode.TwoWay);

    private CalendarContainer? _calendarContainer;
    private Flyout? _calendarFlyout;
    private AnalogClock? _clock;
    private Flyout? _timePickerFlyout;

    private readonly CompositeDisposable _disposable = new();

    /// <summary>
    /// Gets or sets the selected date and time.
    /// </summary>
    public DateTime SelectedDateTime
    {
        get => GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    public DateTimePickerButtons()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        SelectedDateTimeProperty.Changed.ToObservable().Subscribe(x =>
        {
            var y = x.NewValue.Value;
            DateBox.Text = y.ToString("g");
            _calendarContainer.PartCalendar.SelectedDate = y;
            _clock.SelectedTime = y;
        }).AddTo(_disposable);
        DateBox.Text = SelectedDateTime.ToString("g");
        _calendarContainer = new CalendarContainer();
        _calendarFlyout = new Flyout
        {
            Placement = PlacementMode.Top,
            ShowMode = FlyoutShowMode.Standard,
            Content = _calendarContainer
        };
        FlyoutBase.SetAttachedFlyout(CalendarButton, _calendarFlyout);

        _clock = new AnalogClock();
        _timePickerFlyout = new Flyout
        {
            Placement = PlacementMode.Top,
            ShowMode = FlyoutShowMode.Standard,
            Content = _clock,
            HorizontalOffset = -TimePickerButton.Width / 2 + 5
        };
        FlyoutBase.SetAttachedFlyout(TimePickerButton, _timePickerFlyout);

        CalendarButton.Click += (_, _) => ShowPopUpControl(true);
        TimePickerButton.Click += (_, _) => ShowPopUpControl(false);

        _calendarContainer.Accepted += OnCalendarAccepted;
        _calendarContainer.Cancelled += (_, _) => _calendarFlyout.Hide();
        
        _clock.Accepted += OnClockAccepted;
        _clock.Cancelled += (_, _) => _timePickerFlyout.Hide();
    }

    private void ShowPopUpControl(bool calendar)
    {
        if (calendar)
        {
            _calendarContainer.IsVisible = true;
            _calendarContainer.Opacity = 1;
            _calendarContainer.PartCalendar.SelectedDate = SelectedDateTime;
        }
        else
        {
            _clock.IsVisible = true;
            _clock.Opacity = 1;
            _clock.SelectedTime = SelectedDateTime;
        }
        FlyoutBase.ShowAttachedFlyout(calendar ? CalendarButton : TimePickerButton);
    }

    /// <summary>
    /// Handles the Accepted event from the calendar popup.
    /// It combines the newly selected date with the existing time.
    /// </summary>
    private void OnCalendarAccepted(object? sender, EventArgs e)
    {
        if (!_calendarContainer.SelectedDate.HasValue)
        {
            return;
        }

        var newDate = _calendarContainer.SelectedDate.Value.Date;
        var oldTime = SelectedDateTime.TimeOfDay;
        SelectedDateTime = newDate + oldTime;
    }

    /// <summary>
    /// Handles the Accepted event from the clock popup.
    /// It combines the existing date with the newly selected time.
    /// </summary>
    private void OnClockAccepted(object? sender, EventArgs e)
    {
        var oldDate = SelectedDateTime.Date;
        var newTime = _clock.SelectedTime.TimeOfDay;
        SelectedDateTime = oldDate + newTime;
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}
