using PicView.Avalonia.UI;
using PicView.Avalonia.Wallpaper;
using R3;

namespace PicView.Avalonia.ViewModels;

public class ToolsViewModel : IDisposable
{
    
    // Open related
    public ReactiveCommand<string> OpenFileCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand OpenLastFileCommand { get; } = new(async (_, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> OpenWithCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    // Save related
    public ReactiveCommand<string> SaveFileCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> SaveFileAsCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    // File Tasks
    public ReactiveCommand<string> RecycleFileCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> LocateOnDiskCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> FilePropertiesCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> PrintCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> RenameCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> ResizeCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> ConvertCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });

    
    // Copy related
    public ReactiveCommand<string> PasteCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> DuplicateFileCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> CopyImageCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> CopyFileCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> CopyFilePathCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> CutCommand { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
    public ReactiveCommand<string> CopyBase64Command { get; } = new(async (path, _) =>
    {
        await Task.Delay(1); // TODO implement
    });
    
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