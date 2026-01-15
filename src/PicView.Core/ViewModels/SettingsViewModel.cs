using PicView.Core.Config;
using PicView.Core.Localization;
using R3;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace PicView.Core.ViewModels;

public class SettingsViewModel : IDisposable
{
    private readonly Stack<NavigationState> _backStack = new();
    private readonly CompositeDisposable _disposables = new();
    private readonly Stack<NavigationState> _forwardStack = new();
    private NavigationState _currentState;
    private bool _isNavigatingHistory;

    public SettingsViewModel()
    {
        MouseDoubleClickBehaviors = new BindableReactiveProperty<string[]>(
        [
            TranslationManager.Translation.None!,
            TranslationManager.Translation.ResetZoom!,
            TranslationManager.Translation.ToggleFullscreen!
        ]);
        MouseDoubleClickBehaviorIndex = new BindableReactiveProperty<int>(Settings.UIProperties.DoubleClickBehavior);

        NavigateToCategoryCommand = new ReactiveCommand<SettingsCategory>();
        NavigateToCategoryCommand.Subscribe(category =>
        {
            SelectedCategory.Value = category;
            IsOverviewVisible.Value = false;
        }).AddTo(_disposables);

        IsOverviewVisible.Subscribe(_ => OnStateChanged()).AddTo(_disposables);
        SelectedCategory.Subscribe(_ => OnStateChanged()).AddTo(_disposables);

        _currentState = GetCurrentState();

        GoBackCommand = IsBackButtonEnabled.ToReactiveCommand(_ => GoBack()).AddTo(_disposables);
        GoForwardCommand = IsForwardButtonEnabled.ToReactiveCommand(_ => GoForward()).AddTo(_disposables);
        GoHomeCommand = IsHome.ToReactiveCommand(_ => GoHome()).AddTo(_disposables);

        UpdateNavigationProperties();
    }

    public SettingsWindowConfig? SettingsWindowConfig { get; set; }

    public BindableReactiveProperty<bool> IsShowingRecycleDialog { get; } =
        new(Settings.UIProperties.ShowRecycleConfirmation);

    public BindableReactiveProperty<bool> IsShowingPermanentDeletionDialog { get; } =
        new(Settings.UIProperties.ShowPermanentDeletionConfirmation);

    public BindableReactiveProperty<bool> IsBottomGalleryShownInHiddenUI { get; } =
        new(Settings.Gallery.ShowBottomGalleryInHiddenUI);

    public BindableReactiveProperty<bool> IsOpeningInSameWindow { get; } = new(Settings.UIProperties.OpenInSameWindow);

    public BindableReactiveProperty<bool> IsShowingConfirmationOnEsc { get; } =
        new(Settings.UIProperties.ShowConfirmationOnEsc);

    public BindableReactiveProperty<bool> IsStayingCentered { get; } = new(Settings.WindowProperties.KeepCentered);

    public BindableReactiveProperty<bool> IsUsingTouchpad { get; } = new(Settings.Zoom.IsUsingTouchPad);

    public BindableReactiveProperty<bool> IsConstrainingBackgroundColor { get; } =
        new(Settings.UIProperties.IsConstrainBackgroundColorEnabled);

    public BindableReactiveProperty<bool> IsAvoidingZoomingOut { get; } = new(Settings.Zoom.AvoidZoomingOut);
    public BindableReactiveProperty<bool> IsResettingZoomOnImageChange { get; } = new(Settings.Zoom.ResetZoomOnChange);

    public BindableReactiveProperty<bool> IsZoomAnimated { get; } = new(Settings.Zoom.IsZoomAnimated);

    public BindableReactiveProperty<bool> IsShowingZoomPercentagePopup { get; } =
        new(Settings.Zoom.IsShowingZoomPercentagePopup);

    public BindableReactiveProperty<double> WindowMargin { get; } = new(Settings.WindowProperties.Margin);

    public BindableReactiveProperty<double> NavSpeed { get; } = new(Settings.UIProperties.NavSpeed);
    public BindableReactiveProperty<double> GetNavSpeed { get; } = new();

    public BindableReactiveProperty<double> ZoomSpeed { get; } = new(Settings.Zoom.ZoomSpeed);
    public BindableReactiveProperty<double> GetZoomSpeed { get; } = new();

    public BindableReactiveProperty<double> SlideshowSpeed { get; } = new(Settings.UIProperties.SlideShowTimer);
    public BindableReactiveProperty<double> GetSlideshowSpeed { get; } = new();

    public BindableReactiveProperty<string[]> MouseDoubleClickBehaviors { get; }
    public BindableReactiveProperty<int> MouseDoubleClickBehaviorIndex { get; }

    public BindableReactiveProperty<bool> IsOverviewVisible { get; } = new(true);
    public BindableReactiveProperty<SettingsCategory> SelectedCategory { get; } = new(SettingsCategory.General);
    public ReactiveCommand<SettingsCategory> NavigateToCategoryCommand { get; }

    public void Dispose()
    {
        Disposable.Dispose(_disposables,
            GetNavSpeed,
            GetSlideshowSpeed,
            GetZoomSpeed,
            GoBackCommand,
            GoForwardCommand,
            IsAvoidingZoomingOut,
            IsBackButtonEnabled,
            IsBottomGalleryShownInHiddenUI,
            IsConstrainingBackgroundColor,
            IsForwardButtonEnabled,
            IsOpeningInSameWindow,
            IsShowingConfirmationOnEsc,
            IsShowingPermanentDeletionDialog,
            IsShowingRecycleDialog,
            IsStayingCentered,
            IsUsingTouchpad,
            NavSpeed,
            SlideshowSpeed,
            WindowMargin,
            ZoomSpeed,
            MouseDoubleClickBehaviorIndex,
            MouseDoubleClickBehaviors,
            NavigateToCategoryCommand,
            IsOverviewVisible,
            SelectedCategory);
    }

    /// <summary>
    /// Subscribes to settings properties changes and updates relevant application settings and UI state.
    /// This method binds observable properties in the <see cref="SettingsViewModel"/> to the corresponding
    /// settings in the application's configuration, ensuring real-time updates.
    /// </summary>
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

        Observable.EveryValueChanged(this, x => x.IsShowingPermanentDeletionDialog.CurrentValue)
            .SubscribeAwait(async (x, _) =>
            {
                Settings.UIProperties.ShowPermanentDeletionConfirmation = x;
                await SaveSettingsAsync();
            }).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.IsShowingRecycleDialog.CurrentValue)
            .SubscribeAwait(async (x, _) =>
            {
                Settings.UIProperties.ShowRecycleConfirmation = x;
                await SaveSettingsAsync();
            }).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.IsResettingZoomOnImageChange.CurrentValue)
            .Subscribe(x => Settings.Zoom.ResetZoomOnChange = x).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.IsAvoidingZoomingOut.CurrentValue)
            .Subscribe(x => Settings.Zoom.AvoidZoomingOut = x).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.IsZoomAnimated.CurrentValue)
            .Subscribe(x => Settings.Zoom.IsZoomAnimated = x).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.IsShowingZoomPercentagePopup.CurrentValue)
            .Subscribe(x => Settings.Zoom.IsShowingZoomPercentagePopup = x).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.IsUsingTouchpad.CurrentValue)
            .Subscribe(x => Settings.Zoom.IsUsingTouchPad = x).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.IsStayingCentered.CurrentValue)
            .Subscribe(x => Settings.WindowProperties.KeepCentered = x).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.IsShowingConfirmationOnEsc.CurrentValue)
            .Subscribe(x => Settings.UIProperties.ShowConfirmationOnEsc = x).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.IsOpeningInSameWindow.CurrentValue)
            .Subscribe(x => Settings.UIProperties.OpenInSameWindow = x).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.WindowMargin.CurrentValue)
            .Subscribe(x => Settings.WindowProperties.Margin = x).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.MouseDoubleClickBehaviorIndex.CurrentValue)
            .Subscribe(x => Settings.UIProperties.DoubleClickBehavior = x).AddTo(_disposables);
    }

    private readonly record struct NavigationState(bool IsOverview, SettingsCategory Category);

    #region Tab history navigation

    public BindableReactiveProperty<bool> IsBackButtonEnabled { get; } = new();
    public BindableReactiveProperty<bool> IsForwardButtonEnabled { get; } = new();
    public BindableReactiveProperty<bool> IsHome { get; } = new();

    public ReactiveCommand GoHomeCommand { get; }
    public ReactiveCommand GoForwardCommand { get; }
    public ReactiveCommand GoBackCommand { get; }

    public void RestoreLastTab(int lastTab)
    {
        _isNavigatingHistory = true;
        try
        {
            if (lastTab <= 0)
            {
                IsOverviewVisible.Value = true;
            }
            else
            {
                var categoryIndex = lastTab - 1;
                if (Enum.IsDefined(typeof(SettingsCategory), categoryIndex))
                {
                    SelectedCategory.Value = (SettingsCategory)categoryIndex;
                    IsOverviewVisible.Value = false;
                }
                else
                {
                    IsOverviewVisible.Value = true;
                }
            }

            _currentState = GetCurrentState();
            _backStack.Clear();
            _forwardStack.Clear();
            UpdateNavigationProperties();
        }
        finally
        {
            _isNavigatingHistory = false;
        }
    }

    public int GetLastTabId()
    {
        if (IsOverviewVisible.Value)
        {
            return 0;
        }

        return (int)SelectedCategory.Value + 1;
    }

    private void GoBack()
    {
        if (_backStack.Count == 0)
        {
            return;
        }

        var targetState = _backStack.Pop();
        _forwardStack.Push(GetCurrentState());

        ApplyState(targetState);
        UpdateNavigationProperties();
    }

    private void GoForward()
    {
        if (_forwardStack.Count == 0)
        {
            return;
        }

        var targetState = _forwardStack.Pop();
        _backStack.Push(GetCurrentState());

        ApplyState(targetState);
        UpdateNavigationProperties();
    }

    private void GoHome()
    {
        if (IsOverviewVisible.Value)
        {
            return;
        }

        IsOverviewVisible.Value = true;
    }

    private void ApplyState(NavigationState state)
    {
        _isNavigatingHistory = true;
        try
        {
            if (state.IsOverview)
            {
                IsOverviewVisible.Value = true;
            }
            else
            {
                SelectedCategory.Value = state.Category;
                IsOverviewVisible.Value = false;
            }

            _currentState = state;
        }
        finally
        {
            _isNavigatingHistory = false;
        }
    }

    private void OnStateChanged()
    {
        if (_isNavigatingHistory)
        {
            return;
        }

        var newState = GetCurrentState();
        if (newState == _currentState)
        {
            return;
        }

        _backStack.Push(_currentState);
        _forwardStack.Clear();
        _currentState = newState;

        UpdateNavigationProperties();
    }

    private NavigationState GetCurrentState() =>
        IsOverviewVisible.Value
            ? new NavigationState(true, default)
            : new NavigationState(false, SelectedCategory.Value);

    private void UpdateNavigationProperties()
    {
        IsBackButtonEnabled.Value = _backStack.Count > 0;
        IsForwardButtonEnabled.Value = _forwardStack.Count > 0;
        IsHome.Value = !IsOverviewVisible.Value;
    }

    #endregion
}

public enum SettingsCategory
{
    General,
    Appearance,
    Interface,
    Image,
    Navigation,
    Gallery,
    Slideshow,
    Window,
    Zoom,
    Mouse,
    Language,
    FileAssociations
}