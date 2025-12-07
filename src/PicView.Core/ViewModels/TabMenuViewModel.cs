using R3;

namespace PicView.Core.ViewModels;

public class TabMenuViewModel
{
    public BindableReactiveProperty<int> MenuCarouselIndex { get; } = new(0);
    public BindableReactiveProperty<bool> IsGalleryOptionsVisible { get; } = new(false);
    public ReactiveCommand OpenGalleryOptionsCommand { get; }
    public ReactiveCommand CloseGalleryOptionsCommand { get; }

    public TabMenuViewModel()
    {
        OpenGalleryOptionsCommand = new ReactiveCommand(_ =>
        {
            IsGalleryOptionsVisible.Value = true;
            MenuCarouselIndex.Value = 1;
        });
        CloseGalleryOptionsCommand = new ReactiveCommand(_ =>
        {
            IsGalleryOptionsVisible.Value = false;
            MenuCarouselIndex.Value = 0;
        });
    }
}