using System.Collections.Generic;
using PicView.Core.Localization;

namespace PicView.Core.Search;

public static class SettingsSearchIndex
{
    public static List<SettingsSearchItem> Build(SettingsSearchData data)
    {
        var list = new List<SettingsSearchItem>();
        var t = TranslationManager.Translation;

        if (t == null) return list;

        // Startup
        if (t.OpenLastFile != null)
            list.Add(new SettingsSearchItem(t.OpenLastFile, data.StartUpSearchTags.Value));
        if (t.ApplicationStartup != null)
            list.Add(new SettingsSearchItem(t.ApplicationStartup, data.StartUpSearchTags.Value));

        // Delete File Dialog
        if (t.ShowConfirmationDialogWhenPermanentlyDeletingFile != null)
            list.Add(new SettingsSearchItem(t.ShowConfirmationDialogWhenPermanentlyDeletingFile, data.DeleteFileDialogSearchTags.Value));
        if (t.ShowConfirmationDialogWhenMovingFileToRecycleBin != null)
            list.Add(new SettingsSearchItem(t.ShowConfirmationDialogWhenMovingFileToRecycleBin, data.DeleteFileDialogSearchTags.Value));
        if (t.PermanentlyDelete != null)
            list.Add(new SettingsSearchItem(t.PermanentlyDelete, data.DeleteFileDialogSearchTags.Value));
        if (t.DeletedFile != null)
            list.Add(new SettingsSearchItem(t.DeletedFile, data.DeleteFileDialogSearchTags.Value));

        // SubDirectory
        if (t.SearchSubdirectory != null)
            list.Add(new SettingsSearchItem(t.SearchSubdirectory, data.SubDirectorySearchTags.Value));

        // File History
        if (t.OpenFileHistory != null)
            list.Add(new SettingsSearchItem(t.OpenFileHistory, data.FileHistorySearchTags.Value));

        // When Deleting File (Navigation)
        if (t.WhenDeletingAFile != null)
            list.Add(new SettingsSearchItem(t.WhenDeletingAFile, data.WhenDeletingFileSearchTags.Value));
        if (t.NavigateForwards != null)
            list.Add(new SettingsSearchItem(t.NavigateForwards, data.WhenDeletingFileSearchTags.Value));
        if (t.NavigateBackwards != null)
            list.Add(new SettingsSearchItem(t.NavigateBackwards, data.WhenDeletingFileSearchTags.Value));

        // Theme / Appearance
        if (t.InterfaceConfiguration != null)
            list.Add(new SettingsSearchItem(t.InterfaceConfiguration, data.ColorSearchTags.Value));
        if (t.Color != null)
            list.Add(new SettingsSearchItem(t.Color, data.ColorSearchTags.Value));
        if (t.Theme != null)
            list.Add(new SettingsSearchItem(t.Theme, data.ThemeSearchTags.Value));
        if (t.DarkTheme != null)
            list.Add(new SettingsSearchItem(t.DarkTheme, data.ThemeSearchTags.Value));
        if (t.LightTheme != null)
            list.Add(new SettingsSearchItem(t.LightTheme, data.ThemeSearchTags.Value));
        if (t.GlassTheme != null)
            list.Add(new SettingsSearchItem(t.GlassTheme, data.ThemeSearchTags.Value));

        // Background
        if (t.ChangeBackground != null)
            list.Add(new SettingsSearchItem(t.ChangeBackground, data.BackgroundSearchTags.Value));
        if (t.ConstrainBackgroundToImage != null)
            list.Add(new SettingsSearchItem(t.ConstrainBackgroundToImage, data.BackgroundConstrainSearchTags.Value));

        // UI Configuration
        if (t.ShowBottomToolbar != null)
            list.Add(new SettingsSearchItem(t.ShowBottomToolbar, data.ShowBottomToolbarSearchTags.Value));
        if (t.ShowUI != null)
            list.Add(new SettingsSearchItem(t.ShowUI, data.ShowUISearchTags.Value));
        if (t.HideUI != null)
            list.Add(new SettingsSearchItem(t.HideUI, data.ShowUISearchTags.Value));
        if (t.ShowFadeInButtonsOnHover != null)
            list.Add(new SettingsSearchItem(t.ShowFadeInButtonsOnHover, data.ShowFadeInButtonsSearchTags.Value));
        if (t.ShowHoverNavigationBar != null)
            list.Add(new SettingsSearchItem(t.ShowHoverNavigationBar, data.ShowHoverNavigationBarSearchTags.Value));

        // Image Settings
        if (t.Stretch != null)
            list.Add(new SettingsSearchItem(t.Stretch, data.ImageStretchSearchTags.Value));
        if (t.Scrolling != null)
            list.Add(new SettingsSearchItem(t.Scrolling, data.ImageScrollingSearchTags.Value));
        if (t.SideBySide != null)
            list.Add(new SettingsSearchItem(t.SideBySide, data.ImageSideBySideSearchTags.Value));
        if (t.ImageAliasing != null)
            list.Add(new SettingsSearchItem(t.ImageAliasing, data.ImageScalingSearchTags.Value));
        if (t.HighQuality != null)
            list.Add(new SettingsSearchItem(t.HighQuality, data.ImageScalingSearchTags.Value));
        if (t.NearestNeighbor != null)
            list.Add(new SettingsSearchItem(t.NearestNeighbor, data.ImageScalingSearchTags.Value));

        // Navigation
        // Subdirectory already covered? data.NavigationSubdirectorySearchTags seems duplicate of SubDirectorySearchTags but for Navigation section?
        if (t.Navigation != null && t.SearchSubdirectory != null)
            list.Add(new SettingsSearchItem(t.Navigation + " " + t.SearchSubdirectory, data.NavigationSubdirectorySearchTags.Value));
        if (t.ToggleLooping != null)
            list.Add(new SettingsSearchItem(t.ToggleLooping, data.NavigationLoopSearchTags.Value));
        if (t.ToggleTaskbarProgress != null)
            list.Add(new SettingsSearchItem(t.ToggleTaskbarProgress, data.NavigationTaskbarSearchTags.Value));
        if (t.AdjustNavSpeed != null)
            list.Add(new SettingsSearchItem(t.AdjustNavSpeed, data.NavigationSpeedSearchTags.Value));

        // Gallery
        if (t.ShowBottomGallery != null)
            list.Add(new SettingsSearchItem(t.ShowBottomGallery, data.GalleryVisibilitySearchTags.Value));
        if (t.ShowBottomGalleryWhenUiIsHidden != null)
            list.Add(new SettingsSearchItem(t.ShowBottomGalleryWhenUiIsHidden, data.GalleryVisibilitySearchTags.Value));
        if (t.ExpandedGalleryItemSize != null)
            list.Add(new SettingsSearchItem(t.ExpandedGalleryItemSize, data.GallerySizeSearchTags.Value));
        if (t.BottomGalleryItemSize != null)
            list.Add(new SettingsSearchItem(t.BottomGalleryItemSize, data.GallerySizeSearchTags.Value));
        if (t.GalleryThumbnailStretch != null)
            list.Add(new SettingsSearchItem(t.GalleryThumbnailStretch, data.GalleryStretchSearchTags.Value));
        if (t.BottomGalleryThumbnailStretch != null)
            list.Add(new SettingsSearchItem(t.BottomGalleryThumbnailStretch, data.GalleryStretchSearchTags.Value));

        // Slideshow
        if (t.AdjustTimingForSlideshow != null)
            list.Add(new SettingsSearchItem(t.AdjustTimingForSlideshow, data.SlideshowSearchTags.Value));

        // Window
        if (t.WindowScaling != null)
            list.Add(new SettingsSearchItem(t.WindowScaling, data.WindowScalingSearchTags.Value));
        if (t.AutoFitWindow != null)
            list.Add(new SettingsSearchItem(t.AutoFitWindow, data.WindowScalingSearchTags.Value));
        if (t.WindowMargin != null)
            list.Add(new SettingsSearchItem(t.WindowMargin, data.WindowScalingSearchTags.Value));
        if (t.StayTopMost != null)
            list.Add(new SettingsSearchItem(t.StayTopMost, data.WindowTopMostSearchTags.Value));
        if (t.StayCentered != null)
            list.Add(new SettingsSearchItem(t.StayCentered, data.WindowCenteredSearchTags.Value));
        if (t.OpenInSameWindow != null)
            list.Add(new SettingsSearchItem(t.OpenInSameWindow, data.WindowSameWindowSearchTags.Value));
        if (t.ShowConfirmationOnEsc != null)
            list.Add(new SettingsSearchItem(t.ShowConfirmationOnEsc, data.WindowEscSearchTags.Value));

        // Zoom
        if (t.ResetZoomOnChange != null)
            list.Add(new SettingsSearchItem(t.ResetZoomOnChange, data.ZoomResetSearchTags.Value));
        if (t.AllowZoomOut != null)
            list.Add(new SettingsSearchItem(t.AllowZoomOut, data.ZoomOutSearchTags.Value));
        if (t.UseAnimatedZoom != null)
            list.Add(new SettingsSearchItem(t.UseAnimatedZoom, data.ZoomAnimationSearchTags.Value));
        if (t.ShowZoomPercentagePopup != null)
            list.Add(new SettingsSearchItem(t.ShowZoomPercentagePopup, data.ZoomPopupSearchTags.Value));
        if (t.AdjustTimingForZoom != null)
            list.Add(new SettingsSearchItem(t.AdjustTimingForZoom, data.ZoomSpeedSearchTags.Value));

        // Mouse
        if (t.DoubleClick != null)
            list.Add(new SettingsSearchItem(t.DoubleClick, data.MouseDoubleClickSearchTags.Value));
        if (t.NavigateFileHistory != null)
            list.Add(new SettingsSearchItem(t.NavigateFileHistory, data.MouseNavigationSearchTags.Value));
        if (t.NavigateBetweenDirectories != null)
            list.Add(new SettingsSearchItem(t.NavigateBetweenDirectories, data.MouseNavigationSearchTags.Value));
        if (t.MouseWheel != null)
            list.Add(new SettingsSearchItem(t.MouseWheel, data.MouseWheelBehaviorSearchTags.Value));
        if (t.ScrollDirection != null)
            list.Add(new SettingsSearchItem(t.ScrollDirection, data.MouseScrollDirectionSearchTags.Value));
        if (t.UsingTouchpad != null)
            list.Add(new SettingsSearchItem(t.UsingTouchpad, data.MouseTouchpadSearchTags.Value));

        // Language
        if (t.Language != null)
            list.Add(new SettingsSearchItem(t.Language, data.LanguageSearchTags.Value));

        return list;
    }
}
