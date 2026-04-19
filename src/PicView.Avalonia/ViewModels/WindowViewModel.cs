using PicView.Avalonia.UI;
using PicView.Core.Config;
using PicView.Core.ProcessHandling;

namespace PicView.Avalonia.ViewModels;

public class WindowViewModel
{
    public SettingsWindowConfig? SettingsWindowConfig { get; set; }
    public ImageInfoWindowConfig? ImageInfoWindowConfig { get; set; }
    public KeybindingWindowConfig? KeybindingWindowConfig { get; set; }
    public BatchResizeWindowConfig? BatchResizeWindowConfig { get; set; }
    
    public void NewWindow() => ProcessHelper.StartNewProcess();
    
    public async Task ShowImageInfoWindow()
    {
    }

    public void ShowSettingsWindow()
    {
    }

    public void ShowKeybindingsWindow()
    {
    }

    public void ShowAboutWindow()
    {
    }

    public void ShowConvertWindow()
    {
    }

    public void ShowBatchResizeWindow()
    {
    }

    public void ShowSingleImageResizeWindow()
    {
    }

    public void ShowEffectsWindow()
    {
    }
}