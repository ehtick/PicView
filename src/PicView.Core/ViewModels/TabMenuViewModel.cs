using R3;

namespace PicView.Core.ViewModels;

public class TabMenuViewModel
{
    public BindableReactiveProperty<int> MenuCarouselIndex { get; } = new(0);
    public BindableReactiveProperty<bool> IsGalleryOptionsVisible { get; } = new(false);

    public void OpenGalleryOptions()
    {
        MenuCarouselIndex.Value = 1;
        IsGalleryOptionsVisible.Value = true;
    }

    public async ValueTask CloseGalleryOptions()
    {
        MenuCarouselIndex.Value = 0;
        await Task.Delay(95);
        IsGalleryOptionsVisible.Value = false;
    }
}