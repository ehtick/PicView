using R3;

namespace PicView.Core.ViewModels;

public class TitleViewModel
{
    public BindableReactiveProperty<string>? WindowTitle { get; } = new();
    public BindableReactiveProperty<string>? WindowTitleTooltip { get; } = new();
}