using R3;

namespace PicView.Core.ViewModels;

public class HoverbarViewModel : IDisposable
{
    public void Dispose()
    {
        Disposable.Dispose(IsHoverbarVisible);
    }

    public bool IsHoverRotateLeftClicked { get; set; }
    public bool IsHoverRotateRightClicked { get; set; }

    public bool IsHoverNavigationButtonLeftClicked { get; set; }
    public bool IsHoverNavigationButtonRightClicked { get; set; }

    public BindableReactiveProperty<bool> IsHoverbarVisible { get; } = new();
}