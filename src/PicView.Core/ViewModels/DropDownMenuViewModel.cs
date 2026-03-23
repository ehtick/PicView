using R3;

namespace PicView.Core.ViewModels;

public class DropDownMenuViewModel
{
    public BindableReactiveProperty<bool> IsDropDownMenuVisible { get; } = new(false);
    public BindableReactiveProperty<bool> IsExpandedOptionsOpened { get; } = new(false);
    
    public BindableReactiveProperty<int> GalleryCarouselIndex { get; } = new(0);
    public BindableReactiveProperty<bool> IsGalleryCarouselVisible { get; } = new(true);

    public BindableReactiveProperty<int> SlideshowCarouselIndex { get; } = new(0);
    public BindableReactiveProperty<bool> IsSlideshowCarouselVisible { get; } = new(true);

    private const int DefaultDelay = 95;
    public void OpenGalleryOptions()
    {
        GalleryCarouselIndex.Value = 1;
        SlideshowCarouselIndex.Value = 0;
        IsGalleryCarouselVisible.Value = true;
        IsExpandedOptionsOpened.Value = true;
        IsSlideshowCarouselVisible.Value = false;
    }

    public async ValueTask CloseGalleryOptions()
    {
        GalleryCarouselIndex.Value = 0;
        await CloseCarousel();
    }

    public void OpenSlideshowOptions()
    {
        SlideshowCarouselIndex.Value = 1;
        GalleryCarouselIndex.Value = 0;
        IsExpandedOptionsOpened.Value = true;
        IsSlideshowCarouselVisible.Value = true;
        IsGalleryCarouselVisible.Value = false;
    }

    public async ValueTask CloseSlideshowOptions()
    {
        SlideshowCarouselIndex.Value = 0;
        await CloseCarousel();
    }

    public async ValueTask CloseCarousel()
    {
        await Task.Delay(DefaultDelay);
        IsExpandedOptionsOpened.Value = false;
        IsGalleryCarouselVisible.Value = true;
        IsSlideshowCarouselVisible.Value = true;
    }

}