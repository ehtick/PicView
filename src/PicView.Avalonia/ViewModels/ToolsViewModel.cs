using Avalonia.Threading;
using PicView.Avalonia.Clipboard;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Functions;
using PicView.Avalonia.ImageTransformations.Rotation;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.SettingsManagement;
using PicView.Avalonia.UI;
using PicView.Avalonia.Wallpaper;
using R3;

namespace PicView.Avalonia.ViewModels;

public class ToolsViewModel : IDisposable
{
    public ReactiveCommand ShowSettingsFileCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ShowSettingsFile();
    });

    public ReactiveCommand CheckForUpdatesCommand { get; } = new(async (_, _) =>
    {
        // var vm = await Dispatcher.UIThread.InvokeAsync(() => UIHelper.GetMainView.DataContext as MainViewModel);
        // if (vm == null)
        // {
        //     return;
        // }
        //
        // await Dispatcher.UIThread.InvokeAsync(() => vm.Window.ShowAboutWindow());
        // await vm.AboutView.UpdateCurrentVersion();
    });

    public ReactiveCommand ShowKeybindingsFileCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ShowSettingsFile();
    });

    public ReactiveCommand ResetZoomCommand { get; } = new(async (_, _) => { await FunctionsMapper.ResetZoom(); });

    // Open related
    public ReactiveCommand OpenFileCommand { get; } = new(async (_, _) => { await FunctionsMapper.Open(); });

    public ReactiveCommand OpenLastFileCommand { get; } =
        new(async (_, _) => { await FunctionsMapper.OpenLastFile(); });

    public ReactiveCommand<string> OpenWithCommand { get; } = new(async (path, _) =>
    {
    });

    // Save related
    public ReactiveCommand<string> SaveFileCommand { get; } =
        new(async (_, _) => { await FunctionsMapper.Save(); });

    public ReactiveCommand<string> SaveFileAsCommand { get; } =
        new(async (_, _) => { await FunctionsMapper.SaveAs(); });

    // File Tasks
    public ReactiveCommand<string> RecycleFileCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand<string> DeleteFilePermanentlyCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand<string> LocateOnDiskCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand<string> FilePropertiesCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand<string> PrintCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand<string> RenameCommand { get; } = new(async (_, _) =>
    {
        await Task.Run(FunctionsMapper.Rename);
    });


    // Copy related
    public ReactiveCommand<string> PasteCommand { get; } = new(async (_, _) =>
    {
        await Task.Run(FunctionsMapper.Paste);
    });

    public ReactiveCommand<string> DuplicateFileCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand<string> CopyImageCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.CopyImage().ConfigureAwait(false);
    });

    public ReactiveCommand<string> CopyFileCommand { get; } = new(async (path, _) =>
    {
        await ClipboardFileOperations.CopyFileToClipboard(path).ConfigureAwait(false);
    });

    public ReactiveCommand<string> CopyFilePathCommand { get; } = new(async (path, _) =>
    {
        await ClipboardTextOperations.CopyTextToClipboard(path).ConfigureAwait(false);
    });

    public ReactiveCommand<string> CopyBase64Command { get; } = new(async (path, _) =>
    {
    });

    // Settings
    public ReactiveCommand ChangeAutoFitCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.AutoFitWindow().ConfigureAwait(false);
    });

    public ReactiveCommand ChangeTopMostCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SetTopMost().ConfigureAwait(false);
    });

    public ReactiveCommand ToggleSubdirectoriesCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ToggleSubdirectories().ConfigureAwait(false);
    });

    public ReactiveCommand ToggleLoopingCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ToggleLooping().ConfigureAwait(false);
    });

    public ReactiveCommand ResetSettingsCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ResetSettings().ConfigureAwait(false);
    });

    public ReactiveCommand RestartCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.Restart().ConfigureAwait(false);
    });

    public ReactiveCommand ToggleOpeningInSameWindowCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ToggleOpeningInSameWindow().ConfigureAwait(false);
    });

    public ReactiveCommand ToggleUsingTouchPadCommand { get; } = new(async (_, _) =>
    {
    });

    // UI
    public ReactiveCommand ToggleUICommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ToggleInterface();
    });

    public ReactiveCommand ToggleBottomNavBarCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ToggleBottomToolbar();
    });

    public ReactiveCommand ToggleBottomGalleryShownInHiddenUICommand { get; } = new(async (_, _) =>
    {
    });

    public ReactiveCommand ToggleFadeInButtonsOnHoverCommand { get; } = new(async (_, _) =>
    {
    });

    public ReactiveCommand ToggleHoverbarCommand { get; } = new(async (_, _) =>
    {
    });

    public ReactiveCommand ChangeCtrlZoomCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ChangeCtrlZoom();
    });

    public ReactiveCommand ToggleTaskbarProgressCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ToggleTaskbarProgress();
    });

    public ReactiveCommand ToggleConstrainBackgroundColorCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ToggleConstrainBackgroundColor();
    });

    // Image related
    public ReactiveCommand RotateLeftCommand { get; } =
        new(async (_, _) => { await FunctionsMapper.RotateLeft(); });

    public ReactiveCommand RotateRightCommand { get; } =
        new(async (_, _) => { await FunctionsMapper.RotateRight(); });

    public ReactiveCommand ZoomInCommand { get; } = new(async (_, _) => { await FunctionsMapper.ZoomIn(); });
    public ReactiveCommand ZoomOutCommand { get; } = new(async (_, _) => { await FunctionsMapper.ZoomOut(); });

    public ReactiveCommand FlipCommand { get; } = new(async (_, _) => { await FunctionsMapper.Flip(); });

    public ReactiveCommand StretchCommand { get; } = new(async (_, _) => { await FunctionsMapper.Stretch(); });

    public ReactiveCommand CropCommand { get; } = new(async (_, _) => { await FunctionsMapper.Crop(); });

    public ReactiveCommand ToggleScrollCommand { get; } =
        new(async (_, _) => { await FunctionsMapper.ToggleScroll(); });

    public ReactiveCommand OptimizeImageCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.OptimizeImage();
    });

    public ReactiveCommand ChangeBackgroundCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ChangeBackground();
    });

    public ReactiveCommand ShowSearchCommand { get; } = new(async (_, _) => { await FunctionsMapper.Search(); });

    public ReactiveCommand ShowSideBySideCommand { get; } =
        new(async (_, _) => { await FunctionsMapper.SideBySide(); });

    // Wallpaper
    public ReactiveCommand<string> SetAsWallpaperCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand<string> SetAsWallpaperTiledCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand<string> SetAsWallpaperStretchedCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand<string> SetAsWallpaperCenteredCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand<string> SetAsWallpaperFilledCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand<string> SetAsWallpaperFittedCommand { get; } = new(async (path, _) =>
    {
    });

    public ReactiveCommand ToggleFileHistoryTask { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.ToggleFileHistory();
    });

    public ReactiveCommand<string> StartSlideShowTask { get; } = new(async (value, _) =>
    {
    });

    public void Dispose()
    {
        Disposable.Dispose(SetAsWallpaperCommand,
            SetAsWallpaperFilledCommand,
            SetAsWallpaperStretchedCommand,
            SetAsWallpaperCenteredCommand,
            SetAsWallpaperFilledCommand,
            SetAsWallpaperFittedCommand);
    }

    public async Task RotateTask(int angle)
    {
    }

    public async Task StretchedCommand()
    {
    }
}