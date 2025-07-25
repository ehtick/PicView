using System.Diagnostics;
using Avalonia.Media.Imaging;
using ImageMagick;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;

namespace PicView.Avalonia.ImageHandling;

public static class GetThumbnails
{
    public static async Task<Bitmap?> GetThumbAsync(FileInfo? fileInfo, uint height)
    {
        try
        {
            using var magick = new MagickImage();
            try
            {
                magick.Ping(fileInfo);
            }
            catch (Exception e)
            {
                DebugHelper.LogDebug(nameof(GetThumbnails), nameof(GetThumbAsync), e);
                return await CreateThumbAsync(magick, fileInfo, height).ConfigureAwait(false);
            }
            var profile = magick.GetExifProfile();
            if (profile == null)
            {
                return await CreateThumbAsync(magick, fileInfo, height).ConfigureAwait(false);
            }

            var thumbnail = profile.CreateThumbnail();
            if (thumbnail == null)
            {
                return await CreateThumbAsync(magick, fileInfo, height).ConfigureAwait(false);
            }

            thumbnail.AutoOrient();
            return thumbnail.ToWriteableBitmap();
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(GetThumbnails), nameof(GetThumbAsync), e);
            return null;
        }
    }

    public static WriteableBitmap? GetExifThumb(string path)
    {
        using var magick = new MagickImage();
        try
        {
            magick.Ping(path);
        }
        catch (Exception e)
        {
#if DEBUG
            Trace.WriteLine(
                $"\n{nameof(GetExifThumb)} ping exception: \n{e.Message}\n{e.StackTrace}");
#endif
            return null;
        }

        var profile = magick.GetExifProfile();
        var thumbnail = profile?.CreateThumbnail();
        thumbnail?.AutoOrient();
        return thumbnail?.ToWriteableBitmap();
    }

    public static async Task<Bitmap?> CreateThumbAsync(MagickImage magick, FileInfo fileInfo, uint height)
    {
        // TODO: extract thumbnails from PlatformService and convert to Avalonia image,
        // I.E. https://boldena.com/article/64006
        // https://github.com/AvaloniaUI/Avalonia/discussions/16703
        // https://stackoverflow.com/a/42178963/2923736 convert to DLLImport to LibraryImport, source generation & AOT support
        
        await using var fileStream = FileStreamUtils.GetOptimizedFileStream(fileInfo);

        if (fileInfo.Length >= 2147483648)
        {
            // Fixes "The file is too long. This operation is currently limited to supporting files less than 2 gigabytes in size."
            // ReSharper disable once MethodHasAsyncOverload
            magick.Read(fileStream);
        }
        else
        {
            await magick.ReadAsync(fileStream).ConfigureAwait(false);
        }

        var geometry = new MagickGeometry(0, height);
        magick.AutoOrient();
        magick.Thumbnail(geometry);
        return magick.ToWriteableBitmap();
    }
}

