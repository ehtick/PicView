using Avalonia.Controls;
using PicView.Core.Config;
using PicView.Core.Update;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Interfaces;

public interface IWindowProvider
{
    Window CreateAboutWindow();
    Window CreateBatchResizeWindow(BatchResizeWindowConfig config);
    Window CreateConvertWindow();
    Window CreateEffectsWindow();
    Window CreateImageInfoWindow(MainWindowViewModel vm);
    Window CreateKeybindingsWindow(KeybindingWindowConfig config);
    Window CreateSettingsWindow(SettingsWindowConfig config);
    Window CreateSingleImageResizeWindow();
    Window CreatePrintPreviewWindow(MainWindowViewModel vm);

    Task InitializePrintAsync(MainWindowViewModel vm, string path, Window printPreviewWindow);

    Task RunPrintAsync(Window printPreviewWindow, MainWindowViewModel vm);

    Task HandlePlatformUpdate(UpdateInfo updateInfo, string tempPath);
}
