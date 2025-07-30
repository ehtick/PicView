using PicView.Core.ImageDecoding;

namespace PicView.Core.FileHandling;

public static class FileRenamer
{
    private const int FileInUseRetryCount = 100;
    private const int FileInUseRetryDelayMs = 50;

    public static async Task<bool> AttemptRenameAsync(string oldPath, string newPath, Task reload, Task<bool> deleteFile, uint? width = null, uint? height = null, uint? quality = null)
    {
        if (Path.GetExtension(newPath) != Path.GetExtension(oldPath))
        {
            return await HandleExtensionAsync(oldPath, newPath, reload, deleteFile, width, height, quality);
        }

        return await HandleSimpleRenameAsync(oldPath, newPath, reload);
    }

    private static async Task<bool> HandleExtensionAsync(string oldPath, string newPath, Task reload, Task<bool> deleteFile, uint? width = null, uint? height = null, uint? quality = null)
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
            if (!File.Exists(newPath))
            {
                return true;
            }
            var success = await deleteFile;
            if (success || File.Exists(newPath))
            {
                return true;
            }

            await reload;
        }
        
        return false;
    }

    private static async Task<bool> HandleSimpleRenameAsync(string oldPath, string newPath, Task reload)
    {
        var renamed = FileHelper.RenameFile(oldPath, newPath);
        if (renamed || File.Exists(newPath))
        {
            return true;
        }

        await reload;
        return false;
    }
}