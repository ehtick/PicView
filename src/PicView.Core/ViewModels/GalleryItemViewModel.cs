using PicView.Core.Gallery;
using R3;

namespace PicView.Core.ViewModels;

public class GalleryItemViewModel : IDisposable
{
    public void Dispose()
    {
        Disposable.Dispose(ItemWidth,
            ItemHeight,
            Image,
            FileName,
            FileLocation,
            FileSize,
            FileDate);
    }

    // Layout Properties
    public BindableReactiveProperty<double> ItemWidth { get; } = new(0);
    public BindableReactiveProperty<double> ItemHeight { get; } = new(0);

    // Data Properties
    public BindableReactiveProperty<object?> Image { get; } = new();
    public BindableReactiveProperty<string> FileName { get; } = new();
    public BindableReactiveProperty<string> FileLocation { get; } = new();
    public BindableReactiveProperty<string> FileSize { get; } = new();
    public BindableReactiveProperty<string> FileDate { get; } = new();
    
    public FileInfo? FileInfo { get; set; }
}
