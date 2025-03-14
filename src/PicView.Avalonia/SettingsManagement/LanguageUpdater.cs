using PicView.Avalonia.ViewModels;
using PicView.Core.Localization;

namespace PicView.Avalonia.SettingsManagement;

public static class LanguageUpdater
{
    public static async Task UpdateLanguageAsync(MainViewModel vm, bool settingsExists)
    {
        if (settingsExists)
        {
            await TranslationManager.LoadLanguage(Settings.UIProperties.UserLanguage).ConfigureAwait(false);
        }
        else
        {
            await TranslationManager.DetermineAndLoadLanguage().ConfigureAwait(false);
        }

        vm.UpdateLanguage();

        vm.GetIsFlippedTranslation = vm.ScaleX == 1 ? vm.Flip : vm.UnFlip;
        
        vm.GetIsShowingUITranslation = !Settings.UIProperties.ShowInterface ? vm.ShowUI : vm.HideUI;
        
        vm.GetIsScrollingTranslation = Settings.Zoom.ScrollEnabled ?
            TranslationManager.Translation.ScrollingEnabled : TranslationManager.Translation.ScrollingDisabled;
        
        vm.GetIsShowingBottomGalleryTranslation = Settings.Gallery.IsBottomGalleryShown ?
            TranslationManager.Translation.HideBottomGallery :
            TranslationManager.Translation.ShowBottomGallery;
        
        vm.GetIsLoopingTranslation = Settings.UIProperties.Looping
            ? TranslationManager.Translation.LoopingEnabled
            : TranslationManager.Translation.LoopingDisabled;
        
        vm.GetIsCtrlZoomTranslation = Settings.Zoom.CtrlZoom
            ? TranslationManager.Translation.CtrlToZoom
            : TranslationManager.Translation.ScrollToZoom;
        
        vm.GetIsShowingBottomToolbarTranslation = Settings.UIProperties.ShowBottomNavBar
            ? TranslationManager.Translation.HideBottomToolbar
            : TranslationManager.Translation.ShowBottomToolbar;
        
        vm.GetIsShowingFadingUIButtonsTranslation = Settings.UIProperties.ShowAltInterfaceButtons
            ? TranslationManager.Translation.DisableFadeInButtonsOnHover
            : TranslationManager.Translation.ShowFadeInButtonsOnHover;
        
        vm.GetIsUsingTouchpadTranslation = Settings.Zoom.IsUsingTouchPad
            ? TranslationManager.Translation.UsingTouchpad
            : TranslationManager.Translation.UsingMouse;
    }
}
