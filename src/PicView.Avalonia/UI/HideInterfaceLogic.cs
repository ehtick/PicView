using Avalonia.Threading;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
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
            // vm.MainWindow.IsUIShown.Value = false;
            // Settings.UIProperties.ShowInterface = false;
            // vm.MainWindow.IsTopToolbarShown.Value = false;
            // vm.MainWindow.IsBottomToolbarShown.Value = false;
            // vm.Translation.IsShowingUI.Value = TranslationManager.Translation.ShowUI;
        }
        else
        {
            // vm.MainWindow.IsUIShown.Value = true;
            // vm.MainWindow.IsTopToolbarShown.Value = true;
            // vm.Translation.IsShowingUI.Value = TranslationManager.Translation.HideUI;
            // if (Settings.UIProperties.ShowBottomNavBar)
            // {
            //     vm.MainWindow.IsBottomToolbarShown.Value = true;
            //     vm.MainWindow.BottombarHeight.Value = SizeDefaults.BottombarHeight;
            //     vm.HoverbarViewModel.IsHoverbarVisible.Value = false;
            // }
            // else
            // {
            //     vm.HoverbarViewModel.IsHoverbarVisible.Value = Settings.UIProperties.ShowHoverNavigationBar;
            // }
            // Settings.UIProperties.ShowInterface = true;
            // vm.MainWindow.TitlebarHeight.Value = SizeDefaults.MainTitlebarHeight;
        }
        
       // await WindowResizing.SetSizeAsync(vm);

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
            // vm.MainWindow.IsBottomToolbarShown.Value = false;
            // Settings.UIProperties.ShowBottomNavBar = false;
            // vm.Translation.IsShowingBottomToolbar.Value = TranslationManager.Translation.ShowBottomToolbar;
            // vm.HoverbarViewModel.IsHoverbarVisible.Value = Settings.UIProperties.ShowHoverNavigationBar;
        }
        else
        {
            // vm.MainWindow.IsBottomToolbarShown.Value = true;
            // Settings.UIProperties.ShowBottomNavBar = true;
            // vm.MainWindow.BottombarHeight.Value = SizeDefaults.BottombarHeight;
            // vm.Translation.IsShowingBottomToolbar.Value = TranslationManager.Translation.HideBottomToolbar;
            // vm.HoverbarViewModel.IsHoverbarVisible.Value = false;
        }
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            //WindowResizing.SetSize(vm);
        });
        
        await SaveSettingsAsync();
    }
    
    #endregion
}
