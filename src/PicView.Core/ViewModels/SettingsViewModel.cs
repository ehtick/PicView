using PicView.Core.Config;
using PicView.Core.Localization;
using PicView.Core.ColorHandling;
using PicView.Core.Gallery;
using PicView.Core.ISettings;
using PicView.Core.Search;
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
    
    // Services
    private IThemeService? _themeService;
    private ILanguageService? _languageService;
    private IImageSettingsService? _imageSettingsService;

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
        
        // Navigation Properties
        IsIncludingSubdirectories.Subscribe(x => Settings.Sorting.IncludeSubDirectories = x).AddTo(_disposables);
        IsLooping.Subscribe(x => Settings.UIProperties.Looping = x).AddTo(_disposables);
        IsShowingTaskbarProgress.Subscribe(x => Settings.UIProperties.IsTaskbarProgressEnabled = x).AddTo(_disposables);
        IsFileHistoryEnabled.Subscribe(x => Settings.Navigation.IsFileHistoryEnabled = x).AddTo(_disposables);
        
        ToggleUsingTouchpadCommand = new ReactiveCommand(_ => IsUsingTouchpad.Value = !IsUsingTouchpad.Value).AddTo(_disposables);

        // Search Initialization
        SearchData = new SettingsSearchData();
        var allSuggestions = SettingsSearchIndex.Build(SearchData);
        
        ClearSearchCommand = new ReactiveCommand(_ => SearchQuery.Value = string.Empty).AddTo(_disposables);

        SearchQuery.Subscribe(query =>
        {
            var hasText = !string.IsNullOrEmpty(query);
            IsSearchVisible.Value = hasText;

            if (hasText)
            {
                IsOverviewVisible.Value = false;
                Suggestions.Value = allSuggestions
                    .Where(x => x.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase) ||
                                x.Tags.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }
            else
            {
                Suggestions.Value = [];
            }

        }).AddTo(_disposables);

        SelectedSuggestion.Subscribe(item =>
        {
            if (item == null)
            {
                return;
            }

            SearchQuery.Value = item.Name;
            Suggestions.Value = [];
        }).AddTo(_disposables);

        UpdateNavigationProperties();
    }
    
    public void Initialize(IThemeService themeService, ILanguageService languageService, IImageSettingsService imageSettingsService)
    {
        _themeService = themeService;
        _languageService = languageService;
        _imageSettingsService = imageSettingsService;
        
        AvailableLanguages.Value = _languageService.GetAvailableLanguages()
            .Select(x => new LanguageItem(x.Code, x.DisplayName)).ToList();
        
        SubscriptionSettingsUpdate();
    }

    public SettingsWindowConfig? SettingsWindowConfig { get; set; }

    // General
    public BindableReactiveProperty<bool> OpenLastFile { get; } = new(Settings.StartUp.OpenLastFile);
    public BindableReactiveProperty<int> StartUpIndex { get; } = new(Settings.StartUp.OpenLastFile ? 1 : 0);
    
    public BindableReactiveProperty<bool> IsNavigatingBackwardsWhenDeleting { get; } = new(Settings.Navigation.IsNavigatingBackwardsWhenDeleting);
    public BindableReactiveProperty<int> DeletionIndex { get; } = new(Settings.Navigation.IsNavigatingBackwardsWhenDeleting ? 1 : 0);

    // Appearance
    public BindableReactiveProperty<int> ThemeIndex { get; } = new(Settings.Theme.GlassTheme ? 2 : Settings.Theme.Dark ? 0 : 1);
    public BindableReactiveProperty<int> ColorThemeIndex { get; } = new(Settings.Theme.ColorTheme);
    public BindableReactiveProperty<int> BackgroundChoice { get; } = new(Settings.UIProperties.BgColorChoice);
    
    // Image
    public BindableReactiveProperty<bool> IsScalingSetToNearestNeighbor { get; } = new(Settings.ImageScaling.IsScalingSetToNearestNeighbor);
    public BindableReactiveProperty<int> ImageScalingIndex { get; } = new(Settings.ImageScaling.IsScalingSetToNearestNeighbor ? 1 : 0);

    // Zoom
    public BindableReactiveProperty<bool> CtrlZoom { get; } = new(Settings.Zoom.CtrlZoom);
    public BindableReactiveProperty<bool> HorizontalReverseScroll { get; } = new(Settings.Zoom.HorizontalReverseScroll);
    public BindableReactiveProperty<int> ScrollDirectionIndex { get; } = new(Settings.Zoom.HorizontalReverseScroll ? 0 : 1);

    // Mouse
    public BindableReactiveProperty<int> MouseSideButtonBehavior { get; } = new(
        Settings.Navigation.IsNavigatingFileHistory ? 0 : 
        Settings.Navigation.IsNavigatingBetweenDirectories ? 1 : 2);
    
    public BindableReactiveProperty<int> GalleryMouseWheelBehavior { get; } = new((int)Settings.Gallery.GalleryMouseWheelBehavior);
    
    public BindableReactiveProperty<int> MouseWheelBehavior { get; } = new(Settings.Zoom.CtrlZoom ? 0 : 1);

    // Language
    public BindableReactiveProperty<List<LanguageItem>> AvailableLanguages { get; } = new();
    public BindableReactiveProperty<string> UserLanguage { get; } = new(Settings.UIProperties.UserLanguage);

    // Navigation
    public BindableReactiveProperty<bool> IsIncludingSubdirectories { get; } = new(Settings.Sorting.IncludeSubDirectories);
    public BindableReactiveProperty<bool> IsLooping { get; } = new(Settings.UIProperties.Looping);
    public BindableReactiveProperty<bool> IsShowingTaskbarProgress { get; } = new(Settings.UIProperties.IsTaskbarProgressEnabled);
    public BindableReactiveProperty<bool> IsFileHistoryEnabled { get; } = new(Settings.Navigation.IsFileHistoryEnabled);

    public BindableReactiveProperty<bool> IsShowingRecycleDialog { get; } =
        new(Settings.UIProperties.ShowRecycleConfirmation);

    public BindableReactiveProperty<bool> IsShowingPermanentDeletionDialog { get; } =
        new(Settings.UIProperties.ShowPermanentDeletionConfirmation);

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

    public BindableReactiveProperty<bool> IsShowingZoomPreviewer { get; } =
        new(Settings.Zoom.IsShowingZoomPreviewer);

    public BindableReactiveProperty<double> WindowMargin { get; } = new(Settings.WindowProperties.Margin);

    public BindableReactiveProperty<double> NavSpeed { get; } = new(Settings.UIProperties.NavSpeed);
    public BindableReactiveProperty<double> GetNavSpeed { get; } = new();

    public BindableReactiveProperty<double> ZoomSpeed { get; } = new(Settings.Zoom.ZoomSpeed);
    public BindableReactiveProperty<double> GetZoomSpeed { get; } = new();

    public BindableReactiveProperty<double> SlideshowSpeed { get; } = new(Settings.UIProperties.SlideShowTimer);
    public BindableReactiveProperty<double> GetSlideshowSpeed { get; } = new();

    public BindableReactiveProperty<string[]> MouseDoubleClickBehaviors { get; }
    public BindableReactiveProperty<int> MouseDoubleClickBehaviorIndex { get; }
    
    // Commands for simple toggles or actions
    public ReactiveCommand<ColorOptions> SetColorThemeCommand { get; } = new();
    public ReactiveCommand<BackgroundType> SetBackgroundCommand { get; } = new();
    public ReactiveCommand ToggleUsingTouchpadCommand { get; }

    
    public BindableReactiveProperty<bool> IsSearchVisible { get; } = new(false);
    public BindableReactiveProperty<string> SearchQuery { get; } = new(string.Empty);
    public ReactiveCommand ClearSearchCommand { get; }

    // Search properties (Tags and Visibility)
    public SettingsSearchData SearchData { get; }
    public BindableReactiveProperty<List<SettingsSearchItem>> Suggestions { get; } = new();
    public BindableReactiveProperty<SettingsSearchItem?> SelectedSuggestion { get; } = new();

    public BindableReactiveProperty<bool> IsOverviewVisible { get; } = new(false);
    public BindableReactiveProperty<SettingsCategory> SelectedCategory { get; } = new(SettingsCategory.General);
    public ReactiveCommand<SettingsCategory> NavigateToCategoryCommand { get; }

    public void Dispose()
    {
        SearchData.Dispose();
        Disposable.Dispose(_disposables,
            GetNavSpeed,
            GetSlideshowSpeed,
            GetZoomSpeed,
            GoBackCommand,
            GoForwardCommand,
            IsAvoidingZoomingOut,
            IsBackButtonEnabled,
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
            SelectedCategory,
            OpenLastFile,
            StartUpIndex,
            IsNavigatingBackwardsWhenDeleting,
            DeletionIndex,
            ThemeIndex,
            ColorThemeIndex,
            BackgroundChoice,
            IsScalingSetToNearestNeighbor,
            ImageScalingIndex,
            CtrlZoom,
            HorizontalReverseScroll,
            ScrollDirectionIndex,
            MouseSideButtonBehavior,
            MouseWheelBehavior,
            UserLanguage,
            IsIncludingSubdirectories,
            IsLooping,
            IsShowingTaskbarProgress,
            IsFileHistoryEnabled,
            SetColorThemeCommand,
            SetBackgroundCommand,
            ToggleUsingTouchpadCommand,
            SearchQuery,
            IsSearchVisible,
            ClearSearchCommand
            );
    }

    /// <summary>
    /// Subscribes to settings properties changes and updates relevant application settings and UI state.
    /// This method binds observable properties in the <see cref="SettingsViewModel"/> to the corresponding
    /// settings in the application's configuration, ensuring real-time updates.
    /// </summary>
    public void SubscriptionSettingsUpdate()
    {
        // Set initial value
        Observable.EveryValueChanged(this, x => x.NavSpeed.CurrentValue)
            .Subscribe(x =>
            {
                Settings.UIProperties.NavSpeed = x;
                GetNavSpeed.Value = Math.Round(Settings.UIProperties.NavSpeed, 2);
            }).AddTo(_disposables);
        // Set initial value
        Observable.EveryValueChanged(this, x => x.ZoomSpeed.CurrentValue)
            .Subscribe(x =>
            {
                Settings.Zoom.ZoomSpeed = x;
                GetZoomSpeed.Value = Math.Round(Settings.Zoom.ZoomSpeed, 2);
            }).AddTo(_disposables);
        // Set initial value
        Observable.EveryValueChanged(this, x => x.SlideshowSpeed.CurrentValue)
            .Subscribe(x =>
            {
                var roundedValue = Math.Round(x, 2);
                Settings.UIProperties.SlideShowTimer = roundedValue;
                GetSlideshowSpeed.Value = roundedValue;
            }).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.IsShowingPermanentDeletionDialog.CurrentValue)
            .Skip(1) // Skip initial
            .SubscribeAwait(async (x, _) =>
            {
                Settings.UIProperties.ShowPermanentDeletionConfirmation = x;
                await SaveSettingsAsync();
            }).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.IsShowingRecycleDialog.CurrentValue)
            .Skip(1) // Skip initial
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

        Observable.EveryValueChanged(this, x => x.IsShowingZoomPreviewer.CurrentValue)
            .Subscribe(x => Settings.Zoom.IsShowingZoomPreviewer = x).AddTo(_disposables);

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
        
        // General
        Observable.EveryValueChanged(this, x => x.StartUpIndex.CurrentValue)
            .Skip(1) // Skip initial
             .SubscribeAwait(async (x, _) => {
                 OpenLastFile.Value = x == 1;
                 Settings.StartUp.OpenLastFile = x == 1;
                 await SaveSettingsAsync();
             }).AddTo(_disposables);
             
        Observable.EveryValueChanged(this, x => x.DeletionIndex.CurrentValue)
            .Skip(1) // Skip initial
             .SubscribeAwait(async (x, _) => {
                 IsNavigatingBackwardsWhenDeleting.Value = x == 1;
                 Settings.Navigation.IsNavigatingBackwardsWhenDeleting = x == 1;
                 await SaveSettingsAsync();
             }).AddTo(_disposables);
             
        // Appearance
        Observable.EveryValueChanged(this, x => x.ThemeIndex.CurrentValue)
            .Skip(1) // Skip initial
            .Subscribe(x => _themeService?.SetTheme(x)).AddTo(_disposables);
            
        SetColorThemeCommand
            .Skip(1) // Skip initial
            .Subscribe(x => {
            ColorThemeIndex.Value = (int)x;
            _themeService?.SetColorTheme((int)x);
        }).AddTo(_disposables);

        SetBackgroundCommand
            .Skip(1) // Skip initial
            .Subscribe(x => {
            BackgroundChoice.Value = (int)x;
            _themeService?.SetBackground((int)x);
        }).AddTo(_disposables);

        // Image
        Observable.EveryValueChanged(this, x => x.ImageScalingIndex.CurrentValue)
            .Skip(1) // Skip initial
            .SubscribeAwait(async (x, _) => {
                var isNearestNeighbor = x == 1;
                IsScalingSetToNearestNeighbor.Value = isNearestNeighbor;
                Settings.ImageScaling.IsScalingSetToNearestNeighbor = isNearestNeighbor;
                _imageSettingsService?.TriggerScalingModeUpdate(isNearestNeighbor);
                await SaveSettingsAsync();
            }).AddTo(_disposables);
            
        // Zoom
        Observable.EveryValueChanged(this, x => x.CtrlZoom.CurrentValue)
            .Skip(1) // Skip initial
            .SubscribeAwait(async (x, _) => {
                Settings.Zoom.CtrlZoom = x;
                // Sync MouseWheelBehavior if needed
                MouseWheelBehavior.Value = x ? 0 : 1;
                await SaveSettingsAsync();
            }).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.ScrollDirectionIndex.CurrentValue)
            .Skip(1) // Skip initial 
            .SubscribeAwait(async (x, _) => {
                 var reverse = x == 0;
                 HorizontalReverseScroll.Value = reverse;
                 Settings.Zoom.HorizontalReverseScroll = reverse;
                 await SaveSettingsAsync();
             }).AddTo(_disposables);
             
        // Mouse
        Observable.EveryValueChanged(this, x => x.MouseSideButtonBehavior.CurrentValue)
            .Skip(1) // Skip initial
            .SubscribeAwait(async (x, _) => {
                switch(x)
                {
                    case 0:
                        Settings.Navigation.IsNavigatingFileHistory = true;
                        Settings.Navigation.IsNavigatingBetweenDirectories = false;
                        break;
                    case 1:
                        Settings.Navigation.IsNavigatingFileHistory = false;
                        Settings.Navigation.IsNavigatingBetweenDirectories = true;
                        break;
                    case 2:
                        Settings.Navigation.IsNavigatingFileHistory = false;
                        Settings.Navigation.IsNavigatingBetweenDirectories = false;
                        break;
                }
                await SaveSettingsAsync();
            }).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.MouseWheelBehavior.CurrentValue)
            .Skip(1) // Skip initial
            .SubscribeAwait(async (x, _) => {
                var ctrlZoom = x == 0;
                Settings.Zoom.CtrlZoom = ctrlZoom;
                if (CtrlZoom.Value != ctrlZoom) CtrlZoom.Value = ctrlZoom;
                await SaveSettingsAsync();
            }).AddTo(_disposables);
            
        // Language
        Observable.EveryValueChanged(this, x => x.UserLanguage.CurrentValue)
            .Skip(1) // Skip initial
            .SubscribeAwait(async (x, _) => {
                if (Settings.UIProperties.UserLanguage != x)
                {
                    Settings.UIProperties.UserLanguage = x;
                    if (_languageService != null)
                    {
                        await _languageService.UpdateLanguageAsync(x);
                    }
                }
            }).AddTo(_disposables);
        
        GalleryMouseWheelBehavior
            .Skip(1) // Skip initial
            .Subscribe(x => {
                Settings.Gallery.GalleryMouseWheelBehavior = (GalleryMouseWheel)x;
            }).AddTo(_disposables);
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

public record LanguageItem(string Code, string DisplayName);
