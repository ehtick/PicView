using PicView.Core.Gallery;
using R3;

namespace PicView.Core.ViewModels;

public class GalleryItemViewModel : IDisposable
{
    public void Dispose()
    {
        Disposable.Dispose(ItemWidth,
            ItemHeight,
            ItemMargin,
            ExpandedGalleryItemWidth,
            ExpandedGalleryItemHeight,
            BottomGalleryItemWidth,
            BottomGalleryItemHeight,
            Image,
            FileName,
            FileLocation,
            FileSize,
            FileDate);
    }

    // Layout Properties
    public BindableReactiveProperty<double> ItemWidth { get; } = new(0);
    public BindableReactiveProperty<double> ItemHeight { get; } = new(0);

    public BindableReactiveProperty<object> ItemMargin { get; } = new();

    public BindableReactiveProperty<double> ExpandedGalleryItemWidth { get; } = new(0);
    public BindableReactiveProperty<double> ExpandedGalleryItemHeight { get; } = new(0);
    
    public BindableReactiveProperty<double> BottomGalleryItemWidth { get; } = new(0);
    public BindableReactiveProperty<double> BottomGalleryItemHeight { get; } = new(0);

    // Data Properties
    public BindableReactiveProperty<object?> Image { get; } = new();
    public BindableReactiveProperty<string> FileName { get; } = new();
    public BindableReactiveProperty<string> FileLocation { get; } = new();
    public BindableReactiveProperty<string> FileSize { get; } = new();
    public BindableReactiveProperty<string> FileDate { get; } = new();
    
    public FileInfo? FileInfo { get; set; }

    public double MaxExpandedGalleryItemHeight => GalleryDefaults.MaxFullGalleryItemHeight;
    public double MinExpandedGalleryItemHeight => GalleryDefaults.MinFullGalleryItemHeight;

    public double MaxGalleryItemHeight => GalleryDefaults.MaxBottomGalleryItemHeight;
    public double MinGalleryItemHeight => GalleryDefaults.MinBottomGalleryItemHeight;
}
