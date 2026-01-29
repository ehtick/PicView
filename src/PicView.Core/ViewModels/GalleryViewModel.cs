using System.Collections.ObjectModel;
using PicView.Core.Config;
using PicView.Core.Gallery;
using R3;

namespace PicView.Core.ViewModels;

public class GalleryViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    public GalleryViewModel()
    {
        SetStretchModeCommand = new ReactiveCommand<string>();
        SetStretchModeCommand.Subscribe(mode => GalleryStretchService.SetStretch(this, mode)).AddTo(_disposables);

        GalleryItems.Subscribe(_ =>
        {
            GalleryStretchService.SetStretch(this, Settings.Gallery.BottomGalleryStretchMode);
        }).AddTo(_disposables);
        
        // Initial set
        GalleryStretchService.SetStretch(this, Settings.Gallery.BottomGalleryStretchMode);

        GalleryMode = new BindableReactiveProperty<GalleryMode2>(GalleryMode2.Closed);

        // Sync old properties with new Mode for backward compatibility/UI binding
        GalleryMode.Subscribe(mode =>
        {
            IsGalleryExpanded.Value = mode == GalleryMode2.Expanded;
            IsDockedGalleryVisible.Value = mode == GalleryMode2.Docked;
        }).AddTo(_disposables);

        // Commands
        SetGalleryModeCommand = new ReactiveCommand<GalleryMode2>();
        SetGalleryModeCommand.Subscribe(mode => GalleryMode.Value = mode).AddTo(_disposables);
        
        SetDockPositionCommand = new ReactiveCommand<GalleryDockPosition>();
        SetDockPositionCommand.Subscribe(pos =>
        {
            Settings.Gallery.DockPosition = pos;
            GalleryMode.Value = GalleryMode2.Docked;
        }).AddTo(_disposables);

        ToggleGalleryCommand = new ReactiveCommand<Unit>();
        ToggleGalleryCommand.Subscribe(_ =>
        {
            GalleryMode.Value = GalleryMode.Value == GalleryMode2.Expanded ? GalleryMode2.Docked : GalleryMode2.Expanded;
        }).AddTo(_disposables);
        
        CloseGalleryCommand = new ReactiveCommand<Unit>();
        CloseGalleryCommand.Subscribe(_ => GalleryMode.Value = GalleryMode2.Closed).AddTo(_disposables);
    }

    public ReactiveCommand<string> SetStretchModeCommand { get; }
    public ReactiveCommand<GalleryMode2> SetGalleryModeCommand { get; }
    public ReactiveCommand<GalleryDockPosition> SetDockPositionCommand { get; }
    public ReactiveCommand<Unit> ToggleGalleryCommand { get; }
    public ReactiveCommand<Unit> CloseGalleryCommand { get; }

    public BindableReactiveProperty <ObservableCollection<GalleryItemViewModel>> GalleryItems { get; } = new([]);
    public BindableReactiveProperty<GalleryMode2> GalleryMode { get; }

    public BindableReactiveProperty<object> GalleryStretch { get; } = new();
    public BindableReactiveProperty<object> GalleryVerticalAlignment { get; } = new();

    public BindableReactiveProperty<bool> IsGalleryExpanded { get; } = new();
    public BindableReactiveProperty<bool> IsTopDocked { get; } = new();
    public BindableReactiveProperty<bool> IsBottomDocked { get; } = new();
    public BindableReactiveProperty<bool> IsLeftDocked { get; } = new();
    public BindableReactiveProperty<bool> IsRightDocked { get; } = new();
    
    public BindableReactiveProperty<bool> IsDockedGalleryVisible { get; } = new(Settings.Gallery.IsGalleryDocked);
    public BindableReactiveProperty<bool> IsDockedGalleryShownInHiddenUI { get; } =
        new(Settings.Gallery.ShowBottomGalleryInHiddenUI);

    public void Dispose()
    {
        _disposables.Dispose();
        Disposable.Dispose(GalleryItems,
            IsDockedGalleryVisible,
            IsDockedGalleryShownInHiddenUI,
            GalleryMode,
            GalleryStretch,
            GalleryVerticalAlignment,
            IsGalleryExpanded,
            GalleryMode
            );
    }
}
