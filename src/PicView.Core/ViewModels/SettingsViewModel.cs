using Cysharp.Text;
using PicView.Core.Config;
using PicView.Core.Localization;
using PicView.Core.ColorHandling;
using PicView.Core.ISettings;
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
        const string space = " ";
        var sb = ZString.CreateUtf8StringBuilder();
        
        sb.Append(TranslationManager.Translation.ApplicationStartup);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Start);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Open);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.OpenLastFile);
        sb.Append(space);
        sb.Append("Boot");
        sb.Append(space);
        sb.Append("Launch");
        StartUpSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.DeletedFile);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.DeleteFilePermanently);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.PermanentlyDelete);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ShowConfirmationDialogWhenMovingFileToRecycleBin);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ShowConfirmationDialogWhenPermanentlyDeletingFile);
        DeleteFileDialogSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.SearchSubdirectory);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Folder);
        sb.Append(space);
        sb.Append("File system");
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Navigation);
        SubDirectorySearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Append(TranslationManager.Translation.OpenFileHistory);
        sb.Append(space);
        sb.Append("File system");
        FileHistorySearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.DeletedFile);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.DeleteFilePermanently);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.PermanentlyDelete);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.NavigateBackwards);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.NavigateForwards);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Navigation);
        WhenDeletingFileSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.InterfaceConfiguration);
        sb.Append(space);
        sb.Append("UI");
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Color);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Theme);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.HighlightColor);
        ColorSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Append(TranslationManager.Translation.DarkTheme);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.GlassTheme);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.LightTheme);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Theme);
        ThemeSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Append(TranslationManager.Translation.ChangeBackground);
        sb.Append(space);
        sb.Append("Background Texture Wallpaper");
        BackgroundSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.InterfaceConfiguration);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ShowBottomToolbar);
        sb.Append(space);
        sb.Append("Interface UI Toolbar");
        ShowBottomToolbarSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.InterfaceConfiguration);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ShowUI);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.HideUI);
        sb.Append(space);
        sb.Append("Interface UI Hidden");
        ShowUISearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.InterfaceConfiguration);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ShowFadeInButtonsOnHover);
        sb.Append(space);
        sb.Append("Interface UI Buttons Fade");
        ShowFadeInButtonsSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.InterfaceConfiguration);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ShowHoverNavigationBar);
        sb.Append(space);
        sb.Append("Interface UI Hover Navigation");
        ShowHoverNavigationBarSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Image);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Stretch);
        sb.Append(space);
        sb.Append("Stretch");
        ImageStretchSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Image);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Scrolling);
        sb.Append(space);
        sb.Append("Scroll");
        ImageScrollingSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Image);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.SideBySide);
        sb.Append(space);
        sb.Append("SideBySide");
        ImageSideBySideSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.ImageAliasing);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.HighQuality);
        sb.Append(space);
        sb.Append("Scaling Pixelated Nearest Neighbor");
        ImageScalingSearchTags = new BindableReactiveProperty<string>(sb.ToString());

        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Navigation);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.SearchSubdirectory);
        sb.Append(space);
        sb.Append("Folder File system");
        NavigationSubdirectorySearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Navigation);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ToggleLooping);
        sb.Append(space);
        sb.Append("Loop");
        NavigationLoopSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Navigation);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ToggleTaskbarProgress);
        sb.Append(space);
        sb.Append("Taskbar Progress");
        NavigationTaskbarSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Navigation);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.AdjustNavSpeed);
        sb.Append(space);
        sb.Append("Speed Time");
        NavigationSpeedSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.GallerySettings);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ShowBottomGallery);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ShowBottomGalleryWhenUiIsHidden);
        sb.Append(space);
        sb.Append("Gallery Hide Show UI");
        GalleryVisibilitySearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();

        sb.Append(TranslationManager.Translation.GallerySettings);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ExpandedGalleryItemSize);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.BottomGalleryItemSize);
        sb.Append(space);
        sb.Append("Size Height Thumbnail");
        GallerySizeSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();

        sb.Append(TranslationManager.Translation.GallerySettings);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.GalleryThumbnailStretch);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.BottomGalleryThumbnailStretch);
        sb.Append(space);
        sb.Append("Stretch Fill Uniform");
        GalleryStretchSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();

        sb.Append(TranslationManager.Translation.Slideshow);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.AdjustTimingForSlideshow);
        sb.Append(space);
        sb.Append("Timer Speed Presentation");
        SlideshowSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();

        sb.Append(TranslationManager.Translation.WindowScaling);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.AutoFitWindow);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.WindowMargin);
        sb.Append(space);
        sb.Append("Fit Margin");
        WindowScalingSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();

        sb.Append(TranslationManager.Translation.WindowManagement);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.StayTopMost);
        sb.Append(space);
        sb.Append("TopOn");
        WindowTopMostSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.WindowManagement);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.StayCentered);
        sb.Append(space);
        sb.Append("Center");
        WindowCenteredSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.WindowManagement);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.OpenInSameWindow);
        sb.Append(space);
        sb.Append("New Window");
        WindowSameWindowSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.WindowManagement);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ShowConfirmationOnEsc);
        sb.Append(space);
        sb.Append("Escape Exit");
        WindowEscSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();

        sb.Append(TranslationManager.Translation.Zoom);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ResetZoomOnChange);
        sb.Append(space);
        sb.Append("Reset");
        ZoomResetSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Zoom);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.AllowZoomOut);
        sb.Append(space);
        sb.Append("Out");
        ZoomOutSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Zoom);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.UseAnimatedZoom);
        sb.Append(space);
        sb.Append("Animation");
        ZoomAnimationSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Zoom);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ShowZoomPercentagePopup);
        sb.Append(space);
        sb.Append("Percentage Popup");
        ZoomPopupSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Zoom);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.AdjustTimingForZoom);
        sb.Append(space);
        sb.Append("Speed Time");
        ZoomSpeedSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();

        sb.Append(TranslationManager.Translation.DoubleClick);
        sb.Append(space);
        sb.Append("Click");
        MouseDoubleClickSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.MouseSideButtons);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.NavigateFileHistory);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.NavigateBetweenDirectories);
        MouseNavigationSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.MouseWheel);
        sb.Append(space);
        sb.Append("Wheel Scroll");
        MouseWheelBehaviorSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.ScrollDirection);
        sb.Append(space);
        sb.Append("Scroll Direction Reverse");
        MouseScrollDirectionSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.UsingTouchpad);
        sb.Append(space);
        sb.Append("Trackpad Touchpad");
        MouseTouchpadSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.Language);
        sb.Append(space);
        sb.Append("Translate Locale");
        LanguageSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();
        
        sb.Append(TranslationManager.Translation.ConstrainBackgroundToImage);
        sb.Append(space);
        sb.Append("Background Constrain");
        BackgroundConstrainSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        ClearSearchCommand = new ReactiveCommand(_ => SearchQuery.Value = string.Empty).AddTo(_disposables);

        SearchQuery.Subscribe(query =>
        {
            var hasText = !string.IsNullOrEmpty(query);
            IsSearchVisible.Value = hasText;

            if (hasText)
            {
                IsOverviewVisible.Value = false;
            }

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
    
    // Commands for simple toggles or actions
    public ReactiveCommand<ColorOptions> SetColorThemeCommand { get; } = new ReactiveCommand<ColorOptions>();
    public ReactiveCommand<BackgroundType> SetBackgroundCommand { get; } = new ReactiveCommand<BackgroundType>();
    public ReactiveCommand ToggleUsingTouchpadCommand { get; }

    
    public BindableReactiveProperty<bool> IsSearchVisible { get; } = new(false);
    public BindableReactiveProperty<string> SearchQuery { get; } = new(string.Empty);
    public ReactiveCommand ClearSearchCommand { get; }

    // Search properties (Tags and Visibility)
    public BindableReactiveProperty<string> StartUpSearchTags { get; }
    public BindableReactiveProperty<string> DeleteFileDialogSearchTags { get; }
    public BindableReactiveProperty<string> SubDirectorySearchTags { get; }
    public BindableReactiveProperty<string> FileHistorySearchTags { get; }
    public BindableReactiveProperty<string> WhenDeletingFileSearchTags { get; }
    public BindableReactiveProperty<string> ThemeSearchTags { get; }
    public BindableReactiveProperty<string> ColorSearchTags { get; }
    public BindableReactiveProperty<string> BackgroundSearchTags { get; }
    public BindableReactiveProperty<string> BackgroundConstrainSearchTags { get; }
    
    public BindableReactiveProperty<string> ShowBottomToolbarSearchTags { get; }
    public BindableReactiveProperty<string> ShowUISearchTags { get; }
    public BindableReactiveProperty<string> ShowFadeInButtonsSearchTags { get; }
    public BindableReactiveProperty<string> ShowHoverNavigationBarSearchTags { get; }
    
    public BindableReactiveProperty<string> ImageStretchSearchTags { get; }
    public BindableReactiveProperty<string> ImageScrollingSearchTags { get; }
    public BindableReactiveProperty<string> ImageSideBySideSearchTags { get; }
    public BindableReactiveProperty<string> ImageScalingSearchTags { get; }
    public BindableReactiveProperty<string> NavigationSubdirectorySearchTags { get; }
    public BindableReactiveProperty<string> NavigationLoopSearchTags { get; }
    public BindableReactiveProperty<string> NavigationTaskbarSearchTags { get; }
    public BindableReactiveProperty<string> NavigationSpeedSearchTags { get; }
    public BindableReactiveProperty<string> GalleryVisibilitySearchTags { get; }
    public BindableReactiveProperty<string> GallerySizeSearchTags { get; }
    public BindableReactiveProperty<string> GalleryStretchSearchTags { get; }
    public BindableReactiveProperty<string> SlideshowSearchTags { get; }
    public BindableReactiveProperty<string> WindowScalingSearchTags { get; }
    public BindableReactiveProperty<string> WindowTopMostSearchTags { get; }
    public BindableReactiveProperty<string> WindowCenteredSearchTags { get; }
    public BindableReactiveProperty<string> WindowSameWindowSearchTags { get; }
    public BindableReactiveProperty<string> WindowEscSearchTags { get; }
    
    public BindableReactiveProperty<string> ZoomResetSearchTags { get; }
    public BindableReactiveProperty<string> ZoomOutSearchTags { get; }
    public BindableReactiveProperty<string> ZoomAnimationSearchTags { get; }
    public BindableReactiveProperty<string> ZoomPopupSearchTags { get; }
    public BindableReactiveProperty<string> ZoomSpeedSearchTags { get; }
    
    public BindableReactiveProperty<string> MouseDoubleClickSearchTags { get; }
    public BindableReactiveProperty<string> MouseNavigationSearchTags { get; }
    public BindableReactiveProperty<string> MouseWheelBehaviorSearchTags { get; }
    public BindableReactiveProperty<string> MouseScrollDirectionSearchTags { get; }
    public BindableReactiveProperty<string> MouseTouchpadSearchTags { get; }
    public BindableReactiveProperty<string> LanguageSearchTags { get; }
    
    

    public BindableReactiveProperty<bool> IsOverviewVisible { get; } = new(false);
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
            ClearSearchCommand,
            SubDirectorySearchTags,
            FileHistorySearchTags,
            WhenDeletingFileSearchTags,
            BackgroundSearchTags,
            BackgroundConstrainSearchTags,
            ShowBottomToolbarSearchTags,
            ShowUISearchTags,
            ShowFadeInButtonsSearchTags,
            ShowHoverNavigationBarSearchTags,
            ImageStretchSearchTags,
            ImageScrollingSearchTags,
            ImageSideBySideSearchTags,
            ImageScalingSearchTags,
            NavigationSubdirectorySearchTags,
            NavigationLoopSearchTags,
            NavigationTaskbarSearchTags,
            NavigationSpeedSearchTags,
            GalleryVisibilitySearchTags,
            GallerySizeSearchTags,
            GalleryStretchSearchTags,
            SlideshowSearchTags,
            WindowScalingSearchTags,
            WindowTopMostSearchTags,
            WindowCenteredSearchTags,
            WindowSameWindowSearchTags,
            WindowEscSearchTags,
            ZoomResetSearchTags,
            ZoomOutSearchTags,
            ZoomAnimationSearchTags,
            ZoomPopupSearchTags,
            ZoomSpeedSearchTags,
            MouseDoubleClickSearchTags,
            MouseNavigationSearchTags,
            MouseWheelBehaviorSearchTags,
            MouseScrollDirectionSearchTags,
            MouseTouchpadSearchTags,
            LanguageSearchTags
            );
    }

    /// <summary>
    /// Subscribes to settings properties changes and updates relevant application settings and UI state.
    /// This method binds observable properties in the <see cref="SettingsViewModel"/> to the corresponding
    /// settings in the application's configuration, ensuring real-time updates.
    /// </summary>
    public void SubscriptionSettingsUpdate()
    {
        // Existing logic
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
        
        // General
        Observable.EveryValueChanged(this, x => x.StartUpIndex.CurrentValue)
             .SubscribeAwait(async (x, _) => {
                 OpenLastFile.Value = x == 1;
                 Settings.StartUp.OpenLastFile = x == 1;
                 await SaveSettingsAsync();
             }).AddTo(_disposables);
             
        Observable.EveryValueChanged(this, x => x.DeletionIndex.CurrentValue)
             .SubscribeAwait(async (x, _) => {
                 IsNavigatingBackwardsWhenDeleting.Value = x == 1;
                 Settings.Navigation.IsNavigatingBackwardsWhenDeleting = x == 1;
                 await SaveSettingsAsync();
             }).AddTo(_disposables);
             
        // Appearance
        Observable.EveryValueChanged(this, x => x.ThemeIndex.CurrentValue)
            .Subscribe(x => _themeService?.SetTheme(x)).AddTo(_disposables);
            
        SetColorThemeCommand.Subscribe(x => {
            ColorThemeIndex.Value = (int)x;
            _themeService?.SetColorTheme((int)x);
        }).AddTo(_disposables);

        SetBackgroundCommand.Subscribe(x => {
            BackgroundChoice.Value = (int)x;
            _themeService?.SetBackground((int)x);
        }).AddTo(_disposables);

        // Image
        Observable.EveryValueChanged(this, x => x.ImageScalingIndex.CurrentValue)
            .SubscribeAwait(async (x, _) => {
                var isNearestNeighbor = x == 1;
                IsScalingSetToNearestNeighbor.Value = isNearestNeighbor;
                Settings.ImageScaling.IsScalingSetToNearestNeighbor = isNearestNeighbor;
                _imageSettingsService?.TriggerScalingModeUpdate(isNearestNeighbor);
                await SaveSettingsAsync();
            }).AddTo(_disposables);
            
        // Zoom
        Observable.EveryValueChanged(this, x => x.CtrlZoom.CurrentValue)
            .SubscribeAwait(async (x, _) => {
                Settings.Zoom.CtrlZoom = x;
                // Sync MouseWheelBehavior if needed
                MouseWheelBehavior.Value = x ? 0 : 1;
                await SaveSettingsAsync();
            }).AddTo(_disposables);

        Observable.EveryValueChanged(this, x => x.ScrollDirectionIndex.CurrentValue)
             .SubscribeAwait(async (x, _) => {
                 var reverse = x == 0;
                 HorizontalReverseScroll.Value = reverse;
                 Settings.Zoom.HorizontalReverseScroll = reverse;
                 await SaveSettingsAsync();
             }).AddTo(_disposables);
             
        // Mouse
        Observable.EveryValueChanged(this, x => x.MouseSideButtonBehavior.CurrentValue)
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
            .SubscribeAwait(async (x, _) => {
                var ctrlZoom = x == 0;
                Settings.Zoom.CtrlZoom = ctrlZoom;
                if (CtrlZoom.Value != ctrlZoom) CtrlZoom.Value = ctrlZoom;
                await SaveSettingsAsync();
            }).AddTo(_disposables);
            
        // Language
        Observable.EveryValueChanged(this, x => x.UserLanguage.CurrentValue)
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
