using Avalonia.Controls;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Localization;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.UI;

public static class ToggleUIVisibility
{
    public static async ValueTask ToggleBottomBar(MainWindowViewModel vm)
    {
        if (Settings.UIProperties.ShowBottomNavBar)
        {
            vm.IsBottomToolbarShown.Value = false;
            Settings.UIProperties.ShowBottomNavBar = false;
            vm.Translation.IsShowingBottomToolbar.Value = TranslationManager.Translation.ShowBottomToolbar;
            vm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverbarVisible.Value = Settings.UIProperties.ShowHoverNavigationBar;
        }
        else
        {
            vm.IsBottomToolbarShown.Value = true;
            Settings.UIProperties.ShowBottomNavBar = true;
            vm.BottombarHeight.Value = SizeDefaults.BottombarHeight;
            vm.Translation.IsShowingBottomToolbar.Value = TranslationManager.Translation.HideBottomToolbar;
            vm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverbarVisible.Value = false;
        }
        
        WindowResizing.SetSize(vm, WindowResizeReason.Layout);
        
        await SaveSettingsAsync();
    }
}