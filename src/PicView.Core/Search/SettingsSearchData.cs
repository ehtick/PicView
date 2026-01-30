using Cysharp.Text;
using PicView.Core.Localization;
using R3;

namespace PicView.Core.Search;

public class SettingsSearchData : IDisposable
{
    private readonly CompositeDisposable _disposables = new();

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
    public BindableReactiveProperty<string> GalleryDockSearchTags { get; }
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

    public SettingsSearchData()
    {
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
        sb.Append(TranslationManager.Translation.ShowDockedGallery);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ShowDockedGalleryWhenUiIsHidden);
        sb.Append(space);
        sb.Append("Gallery Hide Show UI Docked");
        GalleryVisibilitySearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();

        sb.Append(TranslationManager.Translation.GallerySettings);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Orientation);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Bottom);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Top);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Left);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.Right);
        sb.Append(space);
        sb.Append("Position Dock Gallery");
        GalleryDockSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();

        sb.Append(TranslationManager.Translation.GallerySettings);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.ExpandedGalleryItemSize);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.DockedGalleryItemSize);
        sb.Append(space);
        sb.Append("Size Height Thumbnail Spacing Item Line Margin Padding");
        GallerySizeSearchTags = new BindableReactiveProperty<string>(sb.ToString());
        
        sb.Clear();

        sb.Append(TranslationManager.Translation.GallerySettings);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.GalleryThumbnailStretch);
        sb.Append(space);
        sb.Append(TranslationManager.Translation.DockedGalleryThumbnailStretch);
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
    }

    public void Dispose()
    {
        Disposable.Dispose(_disposables,
            StartUpSearchTags,
            DeleteFileDialogSearchTags,
            SubDirectorySearchTags,
            FileHistorySearchTags,
            WhenDeletingFileSearchTags,
            ThemeSearchTags,
            ColorSearchTags,
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
            GalleryDockSearchTags,
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
}
