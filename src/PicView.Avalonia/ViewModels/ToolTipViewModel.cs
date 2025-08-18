using R3;

namespace PicView.Avalonia.ViewModels;

public class ToolTipViewModel : IDisposable
{
    public BindableReactiveProperty<string?> ToolTipMessageSource { get; } = new();
    public BindableReactiveProperty<bool> ToolTipMessageCentered { get; } = new();
    public ReactiveProperty<TimeSpan> ToolTipMessageInterval { get; } = new(TimeSpan.FromSeconds(2.5));

    public void Dispose()
    {
        ToolTipMessageSource.Dispose();
        ToolTipMessageCentered.Dispose();
        ToolTipMessageInterval.Dispose();
    }
}