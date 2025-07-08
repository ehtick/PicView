using PicView.Avalonia.Clipboard;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Functions;
using PicView.Avalonia.ImageTransformations.Rotation;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.Wallpaper;
using R3;

namespace PicView.Avalonia.ViewModels;

public class ToolsViewModel : IDisposable
{
    
    // Open related
    public ReactiveCommand OpenFileCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.Open();
    });
    
    public ReactiveCommand OpenLastFileCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.OpenLastFile();
    });
    
    public ReactiveCommand<string> OpenWithCommand { get; } = new(async (path, _) =>
    {
        if (UIHelper.GetMainView.DataContext is MainViewModel vm)
        {
            await FileManager.OpenWith(path, vm).ConfigureAwait(false);
        }
    });
    // Save related
    public ReactiveCommand<string> SaveFileCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.Save();
    });
    
    public ReactiveCommand<string> SaveFileAsCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SaveAs();
    });
    
    // File Tasks
    public ReactiveCommand<string> RecycleFileCommand { get; } = new(async (path, _) =>
    {
        if (UIHelper.GetMainView.DataContext is MainViewModel vm)
        {
            await Task.Run(() => FileManager.DeleteFileWithOptionalDialog(true, path, vm.PlatformService)).ConfigureAwait(false);
        }
    });
    
    public ReactiveCommand<string> DeleteFilePermanentlyCommand { get; } = new(async (path, _) =>
    {
        if (UIHelper.GetMainView.DataContext is MainViewModel vm)
        {
            await Task.Run(() => FileManager.DeleteFileWithOptionalDialog(false, path, vm.PlatformService)).ConfigureAwait(false);
        }
    });
    
    public ReactiveCommand<string> LocateOnDiskCommand { get; } = new(async (path, _) =>
    {
        if (UIHelper.GetMainView.DataContext is MainViewModel vm)
        {
            await FileManager.LocateOnDisk(path, vm).ConfigureAwait(false);
        }
    });
    
    public ReactiveCommand<string> FilePropertiesCommand { get; } = new(async (path, _) =>
    {
        if (UIHelper.GetMainView.DataContext is MainViewModel vm)
        {
            await FileManager.ShowFileProperties(path, vm).ConfigureAwait(false);
        }
    });
    
    public ReactiveCommand<string> PrintCommand { get; } = new(async (path, _) =>
    {
        if (UIHelper.GetMainView.DataContext is MainViewModel vm)
        {
            await FileManager.Print(path, vm).ConfigureAwait(false);
        }
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
        if (UIHelper.GetMainView.DataContext is MainViewModel vm)
        {
            await ClipboardFileOperations.Duplicate(path, vm).ConfigureAwait(false);
        }
    });
    
    public ReactiveCommand<string> CopyImageCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.CopyImage().ConfigureAwait(false);
    });
    
    public ReactiveCommand<string> CopyFileCommand { get; } = new(async (path, _) =>
    {
        if (UIHelper.GetMainView.DataContext is MainViewModel vm)
        {
            await ClipboardFileOperations.CopyFileToClipboard(path, vm).ConfigureAwait(false);
        }
    });
    
    public ReactiveCommand<string> CopyFilePathCommand { get; } = new(async (path, _) =>
    {
        await ClipboardTextOperations.CopyTextToClipboard(path).ConfigureAwait(false);
    });
    
    public ReactiveCommand<string> CutCommand { get; } = new(async (path, _) =>
    {
        if (UIHelper.GetMainView.DataContext is MainViewModel vm)
        {
            await ClipboardFileOperations.CutFile(path, vm).ConfigureAwait(false);
        }
    });
    
    public ReactiveCommand<string> CopyBase64Command { get; } = new(async (path, _) =>
    {
        if (UIHelper.GetMainView.DataContext is MainViewModel vm)
        {
            await ClipboardImageOperations.CopyBase64ToClipboard(path, vm).ConfigureAwait(false);
        }
    });
    
    
    public async Task StartSlideShowTask(int milliseconds) =>
        await Slideshow.StartSlideshow(UIHelper.GetMainView.DataContext as MainViewModel, milliseconds);
    
    public async Task RotateTask(int angle) =>
        await RotationNavigation.RotateTo(UIHelper.GetMainView.DataContext as MainViewModel, angle);
    
    // Wallpaper
    public ReactiveCommand<string> SetAsWallpaperCommand { get; } = new(async (path, _) =>
    {
        await WallpaperManager.SetAsWallpaper(path, WallpaperStyle.Fill, UIHelper.GetMainView.DataContext as MainViewModel).ConfigureAwait(false);
    });
    
    public ReactiveCommand<string> SetAsWallpaperTiledCommand { get; } = new(async (path, _) =>
    {
        await WallpaperManager.SetAsWallpaper(path, WallpaperStyle.Tile, UIHelper.GetMainView.DataContext as MainViewModel).ConfigureAwait(false);
    });
    
    public ReactiveCommand<string> SetAsWallpaperStretchedCommand { get; } = new(async (path, _) =>
    {
        await WallpaperManager.SetAsWallpaper(path, WallpaperStyle.Stretch, UIHelper.GetMainView.DataContext as MainViewModel).ConfigureAwait(false);
    });
    
    public ReactiveCommand<string> SetAsWallpaperCenteredCommand { get; } = new(async (path, _) =>
    {
        await WallpaperManager.SetAsWallpaper(path, WallpaperStyle.Center, UIHelper.GetMainView.DataContext as MainViewModel).ConfigureAwait(false);
    });
    
    public ReactiveCommand<string> SetAsWallpaperFilledCommand { get; } = new(async (path, _) =>
    {
        await WallpaperManager.SetAsWallpaper(path, WallpaperStyle.Fill, UIHelper.GetMainView.DataContext as MainViewModel).ConfigureAwait(false);
    });
    
    public ReactiveCommand<string> SetAsWallpaperFittedCommand { get; } = new(async (path, _) =>
    {
        await WallpaperManager.SetAsWallpaper(path, WallpaperStyle.Fit, UIHelper.GetMainView.DataContext as MainViewModel).ConfigureAwait(false);
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
}