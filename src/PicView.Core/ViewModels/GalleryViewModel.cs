using ObservableCollections;
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
        
        Observable.EveryValueChanged(Settings.Gallery, g => g.ItemSpacing)
            .Subscribe(x =>
            {
                if (IsGalleryExpanded.CurrentValue)
                {
                    ItemSpacing.Value = x;
                }
            })
            .AddTo(_disposables);

        Observable.EveryValueChanged(Settings.Gallery, g => g.LineSpacing)
            .Subscribe(x =>
            {
                if (IsGalleryExpanded.CurrentValue)
                {
                    LineSpacing.Value = x;
                }
            })
            .AddTo(_disposables);

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
            Settings.Gallery.IsGalleryDocked = true;
            Settings.Gallery.DockPosition = pos;
            GalleryMode.Value = GalleryMode2.Docked;
        }).AddTo(_disposables);

        ToggleGalleryCommand = new ReactiveCommand<Unit>();
        ToggleGalleryCommand.Subscribe(_ =>
        {
            if (Settings.Gallery.IsGalleryDocked && IsGalleryExpanded.CurrentValue)
            {
                GalleryMode.Value = GalleryMode2.Docked;
            }
            else if (IsGalleryExpanded.CurrentValue)
            {
                GalleryMode.Value = GalleryMode2.Closed;
            }
            else
            {
                GalleryMode.Value = GalleryMode2.Expanded;
            }
            //GalleryMode.Value = GalleryMode.Value == GalleryMode2.Expanded ? GalleryMode2.Docked : GalleryMode2.Expanded;
        }).AddTo(_disposables);
        
        CloseGalleryCommand = new ReactiveCommand<Unit>();
        CloseGalleryCommand.Subscribe(_ =>
        {
            Settings.Gallery.IsGalleryDocked = false;
            Settings.Gallery.DockPosition = GalleryDockPosition.Closed;
            GalleryMode.Value = GalleryMode2.Closed;
        }).AddTo(_disposables);
    }
    
    public ReactiveCommand<string> SetStretchModeCommand { get; }
    
    public ReactiveCommand<GalleryMode2> SetGalleryModeCommand { get; }
    public ReactiveCommand<GalleryDockPosition> SetDockPositionCommand { get; }
    public ReactiveCommand<Unit> ToggleGalleryCommand { get; }
    public ReactiveCommand<Unit> CloseGalleryCommand { get; }

    public BindableReactiveProperty <ObservableList<GalleryItemViewModel>> GalleryItems { get; } = new([]);
    public BindableReactiveProperty<GalleryMode2> GalleryMode { get; }
    public BindableReactiveProperty<object> GalleryVerticalAlignment { get; } = new();
    
    public BindableReactiveProperty<bool> IsGalleryExpanded { get; } = new();
    public BindableReactiveProperty<bool> IsDockedGalleryVisible { get; } = new(Settings.Gallery.IsGalleryDocked);
    public BindableReactiveProperty<double> ItemSpacing { get; } = new(Settings.Gallery.ItemSpacing);
    public BindableReactiveProperty<double> LineSpacing { get; } = new(Settings.Gallery.LineSpacing);
    
    public BindableReactiveProperty<GalleryItemViewModel?> CurrentGalleryItem { get; } = new();
    public BindableReactiveProperty<GalleryItemViewModel?> SelectedGalleryItem { get; } = new();

    public GalleryLoadingState LoadingState { get; set; }

    public void Dispose()
    {
        _disposables.Dispose();
        Disposable.Dispose(
            GalleryItems,
            GalleryMode,
            GalleryVerticalAlignment,
            IsGalleryExpanded,
            GalleryMode,
            CurrentGalleryItem,
            SelectedGalleryItem
        );
    }
}
