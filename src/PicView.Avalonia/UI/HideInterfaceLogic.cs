using Avalonia.Threading;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Gallery;
using PicView.Core.Localization;
using PicView.Core.Sizing;

namespace PicView.Avalonia.UI;

public static class HideInterfaceLogic
{
    #region Toggle UI
    /// <summary>
    /// Toggle between showing the full interface and hiding it
    /// </summary>
    /// <param name="vm">The view model. </param>
    public static async Task ToggleUI(MainViewModel vm)
    {
        if (Settings.UIProperties.ShowInterface)
        {
            vm.IsUIShown = false;
            Settings.UIProperties.ShowInterface = false;
            vm.IsTopToolbarShown = false;
            vm.IsBottomToolbarShown = false;
            vm.Translation.IsShowingUI = TranslationManager.Translation.ShowUI;
            if (!GalleryFunctions.IsFullGalleryOpen)
            {
                if (!Settings.Gallery.ShowBottomGalleryInHiddenUI)
                {
                    vm.GalleryMode = GalleryMode.Closed;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (UIHelper.GetGalleryView.Bounds.Height > 0)
                        {
                            vm.GalleryMode = GalleryMode.BottomToClosed;
                        }
                    });
                    vm.IsBottomGalleryShown = false;
                }
                else
                {
                    vm.IsBottomGalleryShown = Settings.Gallery.ShowBottomGalleryInHiddenUI;
                }
            }
        }
        else
        {
            vm.IsUIShown = true;
            vm.IsTopToolbarShown = true;
            vm.Translation.IsShowingUI = TranslationManager.Translation.HideUI;
            if (Settings.UIProperties.ShowBottomNavBar)
            {
                vm.IsBottomToolbarShown = true;
                vm.BottombarHeight = SizeDefaults.BottombarHeight;
            }
            Settings.UIProperties.ShowInterface = true;
            vm.TitlebarHeight = SizeDefaults.MainTitlebarHeight;
            if (!GalleryFunctions.IsFullGalleryOpen)
            {
                if (Settings.Gallery.IsBottomGalleryShown)
                {
                    if (NavigationManager.CanNavigate(vm))
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            if (UIHelper.GetGalleryView.Bounds.Height <= 0)
                            {
                                vm.GalleryMode = GalleryMode.Closed;
                                GalleryFunctions.OpenBottomGallery(vm);
                            }
                        });
                        _ = GalleryLoad.LoadGallery(vm, vm.PicViewer.FileInfo.CurrentValue.DirectoryName);
                    }

                    vm.IsBottomGalleryShown = true;
                }
                else
                {
                    vm.IsBottomGalleryShown = false;
                }
            }
        }
        
        MenuManager.CloseMenus(vm);
        await WindowResizing.SetSizeAsync(vm);

        await SaveSettingsAsync();
    }
    
    /// <summary>
    /// Toggle between showing the bottom toolbar and hiding it
    /// </summary>
    /// <param name="vm">The view model. </param>
    public static async Task ToggleBottomToolbar(MainViewModel vm)
    {
        if (Settings.UIProperties.ShowBottomNavBar)
        {
            vm.IsBottomToolbarShown = false;
            Settings.UIProperties.ShowBottomNavBar = false;
            vm.Translation.IsShowingBottomToolbar = TranslationManager.Translation.ShowBottomToolbar;
        }
        else
        {
            vm.IsBottomToolbarShown = true;
            Settings.UIProperties.ShowBottomNavBar = true;
            vm.BottombarHeight = SizeDefaults.BottombarHeight;
            vm.Translation.IsShowingBottomToolbar = TranslationManager.Translation.HideBottomToolbar;
        }
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            WindowResizing.SetSize(vm);
        });
        
        await SaveSettingsAsync();
    }
    
    #endregion

    public static async Task ToggleBottomGalleryShownInHiddenUI(MainViewModel vm)
    {
        Settings.Gallery.ShowBottomGalleryInHiddenUI = !Settings.Gallery
            .ShowBottomGalleryInHiddenUI;
        vm.IsBottomGalleryShownInHiddenUI = Settings.Gallery.ShowBottomGalleryInHiddenUI;

        if (!GalleryFunctions.IsFullGalleryOpen)
        {
            if (!Settings.UIProperties.ShowInterface && !Settings.Gallery
                    .ShowBottomGalleryInHiddenUI)
            {
                vm.IsBottomGalleryShown = false;
            }
            else
            {
                vm.IsBottomGalleryShown = Settings.Gallery.IsBottomGalleryShown;
            }
        }
        
        await SaveSettingsAsync();
    }

    public static async Task ToggleFadeInButtonsOnHover(MainViewModel vm)
    {
        Settings.UIProperties.ShowAltInterfaceButtons = !Settings
            .UIProperties.ShowAltInterfaceButtons;
        
        vm.Translation.IsShowingFadingUIButtons = Settings.UIProperties.ShowAltInterfaceButtons
            ? TranslationManager.Translation.DisableFadeInButtonsOnHover
            : TranslationManager.Translation.ShowFadeInButtonsOnHover;
        
        await SaveSettingsAsync();
    }
}
