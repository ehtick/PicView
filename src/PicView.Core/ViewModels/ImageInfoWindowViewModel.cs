using R3;

namespace PicView.Core.ViewModels;

public class ImageInfoWindowViewModel : IDisposable
{
    public int TextWidth => 100;
    public int CopyButtonWidth => 37;

    public BindableReactiveProperty<double> TextBoxWidth { get; } = new(180);
    public BindableReactiveProperty<double> TextBoxXlWidth { get; } = new(630);
    public BindableReactiveProperty<double> HalfLineWidth { get; } = new(395);

    public BindableReactiveProperty<bool> IsCopyButtonEnabled { get; } = new();
    public BindableReactiveProperty<bool> IsExtraButtonsEnabled { get; } = new();
    
    public BindableReactiveProperty<bool> IsLoading { get; } = new(true);

    public void Dispose()
    {
        Disposable.Dispose(
            TextBoxWidth,
            TextBoxXlWidth,
            IsCopyButtonEnabled);
    }

    public void ResponsiveResizeUpdate(double width, double scrollBarThickness, double panelWidth)
    {
        const int firstBreakPoint = 500;
        const int secondBreakPoint = 800;
        const int thirdBreakPoint = 1550;

        var textWidth = TextWidth;
        var copyBtnWidth = CopyButtonWidth;

        IsCopyButtonEnabled.Value = width >= firstBreakPoint;
        IsExtraButtonsEnabled.Value = width >= thirdBreakPoint;

        const int smallPadding = 10;
        const int largePadding = 40;

        switch (width)
        {
            case <= firstBreakPoint:
                TextBoxWidth.Value = TextBoxXlWidth.Value = width - (textWidth + scrollBarThickness + smallPadding);
                break;
            case >= firstBreakPoint and <= secondBreakPoint:
                TextBoxWidth.Value = TextBoxXlWidth.Value =
                    width - (textWidth + scrollBarThickness + smallPadding + copyBtnWidth);

                break;
            case >= secondBreakPoint and <= thirdBreakPoint:
                var newWidthL = width - width / 2 - (textWidth * 2 + scrollBarThickness + largePadding) +
                                copyBtnWidth * 2;
                TextBoxWidth.Value = newWidthL;
                TextBoxXlWidth.Value = newWidthL * 2 + textWidth * 2 - smallPadding * 2;
                break;
            case >= thirdBreakPoint:
                var newWidthXl = width / 2 - panelWidth - (textWidth * 2 + scrollBarThickness + largePadding) +
                                 copyBtnWidth;
                TextBoxWidth.Value = newWidthXl;
                TextBoxXlWidth.Value = newWidthXl * 2 + textWidth * 2 - smallPadding * 2;
                break;
        }

        HalfLineWidth.Value = width / 2 - (scrollBarThickness + smallPadding);
    }
}