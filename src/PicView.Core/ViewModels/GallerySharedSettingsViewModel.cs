using PicView.Core.Gallery;
using R3;

namespace PicView.Core.ViewModels;

public class GallerySharedSettingsViewModel
{
    public GallerySharedSettingsViewModel()
    {
        SetStretchModeCommand = new ReactiveCommand<string>();
        SetStretchModeCommand.Subscribe(mode =>
        {
            GalleryStretch.Value = mode;
        });
    }
    public BindableReactiveProperty<double> ItemHeight { get; } = new(0);
    public BindableReactiveProperty<double> ItemWidth { get; } = new(0);

    public BindableReactiveProperty<object> GalleryStretch { get; } = new();
    
    public BindableReactiveProperty<double> ItemSpacingSetting { get; } = new(Settings.Gallery.ItemSpacing);
    public BindableReactiveProperty<double> LineSpacingSetting { get; } = new(Settings.Gallery.LineSpacing);
    public ReactiveCommand<string> SetStretchModeCommand { get; }
    
    public BindableReactiveProperty<bool> IsTopDocked { get; } = new();
    public BindableReactiveProperty<bool> IsBottomDocked { get; } = new();
    public BindableReactiveProperty<bool> IsLeftDocked { get; } = new();
    public BindableReactiveProperty<bool> IsRightDocked { get; } = new();
    public BindableReactiveProperty<bool> IsDockedGalleryShownInHiddenUI { get; } =
        new(Settings.Gallery.ShowBottomGalleryInHiddenUI);
    


}