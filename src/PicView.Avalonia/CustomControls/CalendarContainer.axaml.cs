using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PicView.Avalonia.Animations;

namespace PicView.Avalonia.CustomControls;

public partial class CalendarContainer : UserControl
{
    public event EventHandler? Accepted;
    public event EventHandler? Cancelled;
    
    /// <summary>
    /// Gets or sets the date currently selected in the calendar.
    /// </summary>
    public DateTime? SelectedDate
    {
        get => PartCalendar.SelectedDate;
        set => PartCalendar.SelectedDate = value;
    }
    
    private DateTime? _initialDate;
    
    public CalendarContainer()
    {
        InitializeComponent();
        
        Loaded += (_, _) => { _initialDate = SelectedDate; };

        AcceptButton.Click += async (_, _) => await Accept();
        CancelButton.Click += async (_, _) => await Cancel();
    }
    
    private async Task Accept()
    {
        var closeAnimation = AnimationsHelper.OpacityAnimation(1, 0, .3);
        await closeAnimation.RunAsync(this);
        IsVisible = false;      // Hide the control
        Accepted?.Invoke(this, EventArgs.Empty);
    }

    private async Task Cancel()
    {
        SelectedDate = _initialDate; // Reset to the original time
        var closeAnimation = AnimationsHelper.OpacityAnimation(1, 0, .3);
        await closeAnimation.RunAsync(this);
        IsVisible = false;      // Hide the control
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
}