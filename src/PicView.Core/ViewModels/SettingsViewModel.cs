using R3;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace PicView.Core.ViewModels;

public class SettingsViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    public void Dispose()
    {
        Disposable.Dispose(_disposables,
            IsBackButtonEnabled,
            IsForwardButtonEnabled,
            GoForwardCommand,
            GoBackCommand,
            IsShowingRecycleDialog,
            IsShowingPermanentDeletionDialog,
            IsBottomGalleryShownInHiddenUI,
            WindowMargin,
            NavSpeed,
            GetNavSpeed,
            ZoomSpeed,
            GetZoomSpeed,
            SlideshowSpeed,
            GetSlideshowSpeed);
    }

    #region Tab history navigation

    public BindableReactiveProperty<bool> IsBackButtonEnabled { get; } = new();

    public BindableReactiveProperty<bool> IsForwardButtonEnabled { get; } = new();
    public ReactiveCommand? GoForwardCommand { get; set; }

    public ReactiveCommand? GoBackCommand { get; set; }

    public void InitializeNavigation (Action goBack, Action goForward)
    {
        GoForwardCommand = IsForwardButtonEnabled.ToReactiveCommand(_ => { goForward(); });
        GoBackCommand = IsBackButtonEnabled.ToReactiveCommand(_ => { goBack(); });
    }

    #endregion

    public BindableReactiveProperty<bool> IsShowingRecycleDialog { get; } =
        new(Settings.UIProperties.ShowRecycleConfirmation);

    public BindableReactiveProperty<bool> IsShowingPermanentDeletionDialog { get; } =
        new(Settings.UIProperties.ShowPermanentDeletionConfirmation);
    
    public BindableReactiveProperty<bool> IsBottomGalleryShownInHiddenUI { get; } =
        new(Settings.Gallery.ShowBottomGalleryInHiddenUI);
    
    public BindableReactiveProperty<double> WindowMargin { get; } = new();
    
    public BindableReactiveProperty<double> NavSpeed { get; } = new(Settings.UIProperties.NavSpeed);
    public BindableReactiveProperty<double> GetNavSpeed { get; } = new();
    
    public BindableReactiveProperty<double> ZoomSpeed { get; } = new(Settings.Zoom.ZoomSpeed);
    public BindableReactiveProperty<double> GetZoomSpeed { get; } = new();
    
    public BindableReactiveProperty<double> SlideshowSpeed { get; } = new(Settings.UIProperties.SlideShowTimer);
    public BindableReactiveProperty<double> GetSlideshowSpeed { get; } = new();
    
    public void SubscriptionSettingsUpdate()
    {
        Observable.EveryValueChanged(this, x => x.NavSpeed.CurrentValue)
            .Subscribe(x =>
            {
                Settings.UIProperties.NavSpeed = x;
                GetNavSpeed.Value = Math.Round(Settings.UIProperties.NavSpeed, 2);
            }).AddTo(_disposables);
        Observable.EveryValueChanged(this, x => x.ZoomSpeed.CurrentValue)
            .Subscribe(x =>
            {
                Settings.Zoom.ZoomSpeed = x;
                GetZoomSpeed.Value = Math.Round(Settings.Zoom.ZoomSpeed, 2);
            }).AddTo(_disposables);
        Observable.EveryValueChanged(this, x => x.SlideshowSpeed.CurrentValue)
            .Subscribe(x =>
            {
                var roundedValue = Math.Round(x, 2);
                Settings.UIProperties.SlideShowTimer = roundedValue;
                GetSlideshowSpeed.Value = roundedValue;
            }).AddTo(_disposables);
    }
}