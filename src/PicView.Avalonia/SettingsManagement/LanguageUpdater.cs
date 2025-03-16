using PicView.Core.Localization;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.SettingsManagement;

public static class LanguageUpdater
{
    public static async Task UpdateLanguageAsync(TranslationViewModel translationViewModel, PicViewerModel picViewerModel, bool settingsExists)
    {
        if (settingsExists)
        {
            await TranslationManager.LoadLanguage(Settings.UIProperties.UserLanguage).ConfigureAwait(false);
        }
        else
        {
            await TranslationManager.DetermineAndLoadLanguage().ConfigureAwait(false);
        }

        translationViewModel.UpdateLanguage();

        translationViewModel.IsFlipped = picViewerModel.ScaleX == 1 ? translationViewModel.Flip : translationViewModel.UnFlip;
        
        translationViewModel.IsShowingUI = !Settings.UIProperties.ShowInterface ? translationViewModel.ShowUI : translationViewModel.HideUI;
        
        translationViewModel.IsScrolling = Settings.Zoom.ScrollEnabled ?
            TranslationManager.Translation.ScrollingEnabled : TranslationManager.Translation.ScrollingDisabled;
        
        translationViewModel.IsShowingBottomGallery = Settings.Gallery.IsBottomGalleryShown ?
            TranslationManager.Translation.HideBottomGallery :
            TranslationManager.Translation.ShowBottomGallery;
        
        translationViewModel.IsLooping = Settings.UIProperties.Looping
            ? TranslationManager.Translation.LoopingEnabled
            : TranslationManager.Translation.LoopingDisabled;
        
        translationViewModel.IsCtrlToZoom = Settings.Zoom.CtrlZoom
            ? TranslationManager.Translation.CtrlToZoom
            : TranslationManager.Translation.ScrollToZoom;
        
        translationViewModel.IsShowingBottomToolbar = Settings.UIProperties.ShowBottomNavBar
            ? TranslationManager.Translation.HideBottomToolbar
            : TranslationManager.Translation.ShowBottomToolbar;
        
        translationViewModel.IsShowingFadingUIButtons = Settings.UIProperties.ShowAltInterfaceButtons
            ? TranslationManager.Translation.DisableFadeInButtonsOnHover
            : TranslationManager.Translation.ShowFadeInButtonsOnHover;
        
        translationViewModel.IsUsingTouchpad = Settings.Zoom.IsUsingTouchPad
            ? TranslationManager.Translation.UsingTouchpad
            : TranslationManager.Translation.UsingMouse;
    }
}
