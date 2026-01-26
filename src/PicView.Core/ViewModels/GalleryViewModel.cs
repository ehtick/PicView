using System.Collections.ObjectModel;
using PicView.Core.Gallery;
using R3;

namespace PicView.Core.ViewModels;

public class GalleryViewModel : IDisposable
{
    public BindableReactiveProperty <ObservableCollection<GalleryItemViewModel>> GalleryItems { get; } = new([]);

    public BindableReactiveProperty<object> GalleryDockPosition { get; } = new();

    public BindableReactiveProperty<GalleryMode> GalleryMode { get; } = new(Core.Gallery.GalleryMode.Closed);

    public BindableReactiveProperty<object> GalleryStretch { get; } = new();
    public BindableReactiveProperty<object> GalleryVerticalAlignment { get; } = new();
    public BindableReactiveProperty<object> GalleryOrientation { get; } = new();

    public BindableReactiveProperty<bool> IsGalleryExpanded { get; } = new();
    
    public BindableReactiveProperty<bool> IsDockedGalleryVisible { get; } = new(Settings.Gallery.IsBottomGalleryShown);
    public BindableReactiveProperty<bool> IsDockedGalleryHiddenUI { get; } =
        new(Settings.Gallery.ShowBottomGalleryInHiddenUI);

    public void Dispose()
    {
        Disposable.Dispose(GalleryItems,
            GalleryDockPosition,
            IsDockedGalleryVisible,
            IsDockedGalleryHiddenUI,
            GalleryMode,
            GalleryStretch,
            GalleryVerticalAlignment,
            GalleryOrientation,
            IsGalleryExpanded
            );
    }
}