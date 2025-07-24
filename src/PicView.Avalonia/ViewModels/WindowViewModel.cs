using PicView.Avalonia.UI;
using PicView.Core.Config;
using PicView.Core.ProcessHandling;

namespace PicView.Avalonia.ViewModels;

public class WindowViewModel
{
    public SettingsWindowConfig? SettingsWindowConfig { get; set; }
    public ImageInfoWindowConfig? ImageInfoWindowConfig { get; set; }
    
    public void NewWindow() => ProcessHelper.StartNewProcess();
    
    public async Task ShowImageInfoWindow()
    {
        if (UIHelper.TryGetMainViewModel(out var vm))
        {
            await vm.PlatformWindowService.ShowImageInfoWindow();
        }
    }

    public void ShowSettingsWindow()
    {
        if (UIHelper.TryGetMainViewModel(out var vm))
        {
            vm.PlatformWindowService.ShowSettingsWindow();
        }
    }

    public void ShowKeybindingsWindow()
    {
        if (UIHelper.TryGetMainViewModel(out var vm))
        {
            vm.PlatformWindowService.ShowKeybindingsWindow();
        }
    }

    public void ShowAboutWindow()
    {
        if (UIHelper.TryGetMainViewModel(out var vm))
        {
            vm.PlatformWindowService.ShowAboutWindow();
        }
    }

    public void ShowBatchResizeWindow()
    {
        if (UIHelper.TryGetMainViewModel(out var vm))
        {
            vm.PlatformWindowService.ShowBatchResizeWindow();
        }
    }

    public void ShowSingleImageResizeWindow()
    {
        if (UIHelper.TryGetMainViewModel(out var vm))
        {
            vm.PlatformWindowService.ShowSingleImageResizeWindow();
        }
    }

    public void ShowEffectsWindow()
    {
        if (UIHelper.TryGetMainViewModel(out var vm))
        {
            vm.PlatformWindowService.ShowEffectsWindow();
        }
    }
}