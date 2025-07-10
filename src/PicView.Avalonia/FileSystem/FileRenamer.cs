using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileHandling;
using PicView.Core.ImageDecoding;

namespace PicView.Avalonia.FileSystem;

public static class FileRenamer
{
    private const int FileInUseRetryCount = 100;
    private const int FileInUseRetryDelayMs = 50;

    public static async Task<bool> AttemptRenameAsync(string oldPath, string newPath, MainViewModel vm, uint? width = null, uint? height = null, uint? quality = null)
    {
        vm.MainWindow.IsLoadingIndicatorShown.Value = true;

        if (Path.GetExtension(newPath) != Path.GetExtension(oldPath))
        {
            return await HandleExtensionAsync(vm, oldPath, newPath, width, height, quality);
        }

        return await HandleSimpleRenameAsync(vm, oldPath, newPath);
    }

    private static async Task<bool> HandleExtensionAsync(MainViewModel vm, string oldPath, string newPath, uint? width = null, uint? height = null, uint? quality = null)
    {
        var saved = await SaveImageFileHelper
            .SaveImageAsync(null, oldPath, newPath, width, height, quality, Path.GetExtension(newPath)).ConfigureAwait(false);

        var attempts = 0;
        while (FileHelper.IsFileInUse(oldPath) && attempts++ < FileInUseRetryCount)
        {
            await Task.Delay(FileInUseRetryDelayMs);
        }

        // ReSharper disable once InvertIf
        if (saved)
        {
            var success = await vm.PlatformService.DeleteFile(oldPath, true);
            if (success || File.Exists(newPath))
            {
                return true;
            }

            await ErrorHandling.ReloadAsync(vm);
        }
        
        return false;
    }

    private static async Task<bool> HandleSimpleRenameAsync(MainViewModel vm, string oldPath, string newPath)
    {
        var renamed = FileHelper.RenameFile(oldPath, newPath);
        if (renamed || File.Exists(newPath))
        {
            return true;
        }

        await ErrorHandling.ReloadAsync(vm);
        return false;
    }
}