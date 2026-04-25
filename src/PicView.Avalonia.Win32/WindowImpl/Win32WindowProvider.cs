using Avalonia.Controls;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.Win32.PlatformUpdate;
using PicView.Avalonia.Win32.Views;
using PicView.Avalonia.Win32.Printing;
using PicView.Core.Update;
using PicView.Core.ViewModels;
using PicView.Core.Config;

namespace PicView.Avalonia.Win32.WindowImpl;

public class Win32WindowProvider : IWindowProvider
{
    public Window CreateAboutWindow() => new AboutWindow();

    public Window CreateBatchResizeWindow(BatchResizeWindowConfig config) => new BatchResizeWindow(config);

    public Window CreateConvertWindow() => new ConvertWindow();

    public Window CreateEffectsWindow() => new EffectsWindow();

    public Window CreateImageInfoWindow(MainWindowViewModel vm) => new ImageInfoWindow(vm);

    public Window CreateKeybindingsWindow(KeybindingWindowConfig config) => new KeybindingsWindow(config);

    public Window CreateSettingsWindow(SettingsWindowConfig config) => new SettingsWindow(config);

    public Window CreateSingleImageResizeWindow() => new SingleImageResizeWindow();

    public Window CreatePrintPreviewWindow(MainWindowViewModel vm) => new PrintPreviewWindow();

    public async Task InitializePrintAsync(MainWindowViewModel vm, string path, Window printPreviewWindow)
    {
        if (printPreviewWindow is PrintPreviewWindow win)
        {
            await PrintInitialization.InitializeAsync(vm, path, win);
        }
    }

    public async Task RunPrintAsync(Window printPreviewWindow, MainWindowViewModel vm)
    {
        if (printPreviewWindow is PrintPreviewWindow win)
        {
            await win.RunPrintAsync(vm);
        }
    }

    public async Task HandlePlatformUpdate(UpdateInfo updateInfo, string tempPath)
    {
        await WinUpdateHelper.HandleWindowsUpdate(updateInfo, tempPath);
    }
}
