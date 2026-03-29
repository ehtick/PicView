using System.Diagnostics;
using ObservableCollections;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;
using PicView.Core.Navigation;
using R3;

namespace PicView.Core.ViewModels;

public class GalleryViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    public ReactiveCommand<string> SetStretchModeCommand { get; } = new();
    public ReactiveCommand<GalleryMode2> SetGalleryModeCommand { get; } = new();

    public ReactiveCommand<Unit> ToggleGalleryCommand { get; } = new();
    public ReactiveCommand<NavigateTo> NavigateGalleryCommand { get; } = new();
    public ReactiveCommand<int> OpenSelectedItemCommand { get; } = new();

    public ObservableList<GalleryItemViewModel> GalleryItems { get; } = new([]);
    public BindableReactiveProperty<GalleryMode2> GalleryMode { get; } = new(GalleryMode2.Closed);

    public BindableReactiveProperty<bool> IsGalleryExpanded { get; } = new();
    public BindableReactiveProperty<bool> IsDockedGalleryVisible { get; } = new(Settings.Gallery.IsGalleryDocked);
    public BindableReactiveProperty<double> ItemSpacing { get; } = new(Settings.Gallery.ItemSpacing);
    public BindableReactiveProperty<double> LineSpacing { get; } = new(Settings.Gallery.LineSpacing);

    public BindableReactiveProperty<GalleryItemViewModel?> CurrentGalleryItem { get; } = new();
    public BindableReactiveProperty<GalleryItemViewModel?> SelectedGalleryItem { get; } = new();
    public BindableReactiveProperty<int> SelectedGalleryItemIndex { get; } = new(-1);

    public GalleryLoadingState LoadingState { get; set; }

    public void Initialize()
    {
        Debug.Assert(Settings.Gallery is not null);
        Observable.EveryValueChanged(Settings.Gallery, g => g.IsGalleryDocked)
            .Subscribe(isDocked =>
            {
                if (isDocked && GalleryMode.Value == GalleryMode2.Closed)
                {
                    GalleryMode.Value = GalleryMode2.Docked;
                }
                else if (!isDocked && GalleryMode.Value == GalleryMode2.Docked)
                {
                    GalleryMode.Value = GalleryMode2.Closed;
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GalleryViewModel), nameof(Initialize), result.Exception);
                }
#endif
            })
            .AddTo(_disposables);

        Observable.EveryValueChanged(Settings.Gallery, g => g.ItemSpacing)
            .Subscribe(x =>
            {
                if (IsGalleryExpanded.CurrentValue)
                {
                    ItemSpacing.Value = x;
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GalleryViewModel), nameof(Initialize), result.Exception);
                }
#endif
            })
            .AddTo(_disposables);

        Observable.EveryValueChanged(Settings.Gallery, g => g.LineSpacing)
            .Subscribe(x =>
            {
                if (IsGalleryExpanded.CurrentValue)
                {
                    LineSpacing.Value = x;
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GalleryViewModel), nameof(Initialize), result.Exception);
                }
#endif
            })
            .AddTo(_disposables);

        GalleryMode.Subscribe(mode =>
            {
                IsGalleryExpanded.Value = mode == GalleryMode2.Expanded;
                IsDockedGalleryVisible.Value = mode == GalleryMode2.Docked;
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GalleryViewModel), nameof(Initialize), result.Exception);
                }
#endif
            })
            .AddTo(_disposables);
        
        SetGalleryModeCommand.Subscribe(mode =>
            {
                GalleryMode.Value = mode;
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(GalleryViewModel), nameof(Initialize), result.Exception);
                }
#endif
            })
            .AddTo(_disposables);
        
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
        }, result =>
        {
#if DEBUG
            if (result is { IsFailure: true, Exception: not null })
            {
                DebugHelper.LogDebug(nameof(GalleryViewModel), nameof(Initialize), result.Exception);
            }
#endif
        })
        .AddTo(_disposables);
    }

    public void Navigate(NavigateTo direction)
    {
        NavigateGalleryCommand.Execute(direction);
    }
    
    public void Dispose()
    {
        _disposables.Dispose();
        Disposable.Dispose(
            GalleryMode,
            IsGalleryExpanded,
            GalleryMode,
            CurrentGalleryItem,
            SelectedGalleryItem,
            SelectedGalleryItemIndex
        );
    }
}