using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileHandling;
using PicView.Core.ImageDecoding;

namespace PicView.Avalonia.FileSystem;

public static class FileRenamer
{
    private const int FileInUseRetryCount = 100;
    private const int FileInUseRetryDelayMs = 50;

    public static async Task<bool> AttemptRenameAsync(string oldPath, string newPath, MainViewModel vm)
    {
        vm.IsLoading = true;

        if (Path.GetExtension(newPath) != Path.GetExtension(oldPath))
        {
            return await HandleExtensionChangeAsync(vm, oldPath, newPath);
        }

        return await HandleSimpleRenameAsync(vm, oldPath, newPath);
    }

    private static async Task<bool> HandleExtensionChangeAsync(MainViewModel vm, string oldPath, string newPath)
    {
        var saved = await SaveImageFileHelper
            .SaveImageAsync(null, oldPath, newPath, null, null, null, Path.GetExtension(newPath)).ConfigureAwait(false);

        var attempts = 0;
        while (FileHelper.IsFileInUse(oldPath) && attempts++ < FileInUseRetryCount)
        {
            await Task.Delay(FileInUseRetryDelayMs);
        }

        if (!saved)
        {
            return true;
        }

        var success = await vm.PlatformService.DeleteFile(oldPath, true);
        if (success || File.Exists(newPath))
        {
            return true;
        }

        await ErrorHandling.ReloadAsync(vm);
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