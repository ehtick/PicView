using System.Runtime.InteropServices;
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (fileInfo is null || !fileInfo.Exists)
                {
                    return null;
                }
                await using var stream = FileStreamUtils.GetOptimizedFileStream(fileInfo);
                var thumb = Bitmap.DecodeToHeight(stream, (int)height);
                return thumb;
            }

            using var magick = new MagickImage();
            magick.Ping(fileInfo);
            var profile = magick.GetExifProfile();
            if (profile == null)
            {
                return await CreateThumbAsync(magick, fileInfo, height).ConfigureAwait(false);
            }

            var thumbnail = profile.CreateThumbnail();
            if (thumbnail == null || thumbnail.Height < height)
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
            DebugHelper.LogDebug(nameof(GetThumbnails), nameof(GetExifThumb), e);
            return null;
        }

        var profile = magick.GetExifProfile();
        var thumbnail = profile?.CreateThumbnail();
        thumbnail?.AutoOrient();
        return thumbnail?.ToWriteableBitmap();
    }

    private static async Task<Bitmap?> CreateThumbAsync(MagickImage magick, FileInfo fileInfo, uint height)
    {
        // TODO: extract thumbnails from PlatformService and convert to Avalonia image,
        // I.E. https://boldena.com/article/64006
        // https://github.com/AvaloniaUI/Avalonia/discussions/16703
        // https://stackoverflow.com/a/42178963/2923736 convert to DLLImport to LibraryImport, source generation & AOT support

        switch (magick.Format)
        {
            case MagickFormat.WebP:
            case MagickFormat.WebM:
            case MagickFormat.Gif:
            case MagickFormat.Gif87:
            case MagickFormat.Png:
            case MagickFormat.Png00:
            case MagickFormat.Png8:
            case MagickFormat.Png24:
            case MagickFormat.Png32:
            case MagickFormat.Png48:
            case MagickFormat.Png64:
            case MagickFormat.APng:
            case MagickFormat.Jpe:
            case MagickFormat.Jpeg:
            case MagickFormat.Pjpeg:
            case MagickFormat.Bmp:
            case MagickFormat.Tif:
            case MagickFormat.Tiff:
            case MagickFormat.Ico:
            case MagickFormat.Icon:
            case MagickFormat.Wbmp:
            {
                await using var stream = FileStreamUtils.GetOptimizedFileStream(fileInfo);
                var thumb = Bitmap.DecodeToHeight(stream, (int)height);
                return thumb;
            }

            case MagickFormat.Svg:
            case MagickFormat.Svgz:
                return null;
            default:
            {
                await using var fileStream = FileStreamUtils.GetOptimizedFileStream(fileInfo);

                if (fileInfo.Length >= 2147483648)
                {
                    await Task.Run(() =>
                    {
                        // Fixes "The file is too long. This operation is currently limited to supporting files less than 2 gigabytes in size."
                        // ReSharper disable once MethodHasAsyncOverload
                        magick.Read(fileStream);
                    });
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
    }
}