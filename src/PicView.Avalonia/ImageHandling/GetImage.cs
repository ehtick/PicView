using Avalonia.Media.Imaging;
using ImageMagick;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using SkiaSharp;

namespace PicView.Avalonia.ImageHandling;

public static class GetImage
{
    public static async Task<Bitmap?> GetStandardBitmapAsync(string file)
    {
        return await GetStandardBitmapAsync(new FileInfo(file)).ConfigureAwait(false);
    }
    
    public static async Task<Bitmap?> GetStandardBitmapAsync(FileInfo fileInfo)
    {
        if (fileInfo is null)
        {
            DebugHelper.LogDebug(nameof(GetImage), nameof(GetStandardBitmapAsync), $"{nameof(fileInfo)} is null");
            return null;
        }
        await using var stream = FileStreamUtils.GetOptimizedFileStream(fileInfo);
        var bitmap = new Bitmap(stream);
        return bitmap;
    }
    
    public static async Task<Bitmap?> GetNonStandardBitmapAsync(FileInfo fileInfo, MagickImage? magickImage)
    {
        var shouldDisposeMagickImage = magickImage is null;
        if (shouldDisposeMagickImage)
        {
            magickImage = new MagickImage();
        }
        await using var stream = FileStreamUtils.GetOptimizedFileStream(fileInfo);
        if (fileInfo.Length >= 2147483648)
        {
            // Fixes "The file is too long. This operation is currently limited to supporting files less than 2 gigabytes in size."
            // ReSharper disable once MethodHasAsyncOverload
            magickImage.Read(stream);
        }
        else
        {
            await magickImage.ReadAsync(stream).ConfigureAwait(false); 
        }

        var bitmap = magickImage.ToWriteableBitmap();
        if (shouldDisposeMagickImage)
        {
            magickImage.Dispose();
        }
        return bitmap;
    }
    
    public static async Task<Bitmap?> GetRawBitmapAsync(FileInfo fileInfo, MagickImage? magickImage)
    {
        var shouldDisposeMagickImage = magickImage is null;
        if (shouldDisposeMagickImage)
        {
            magickImage = new MagickImage();
        }
        // Raw images needs to be loaded by file path, else it just loads thumbnail 
        // https://github.com/Ruben2776/PicView/issues/221
        await magickImage.ReadAsync(fileInfo).ConfigureAwait(false); 
        var bitmap = magickImage.ToWriteableBitmap();
        if (shouldDisposeMagickImage)
        {
            magickImage.Dispose();
        }
        return bitmap;
    }
    
    public static async Task<Bitmap?> GetBase64ImageAsync(FileInfo fileInfo)
    {
        var base64String = await File.ReadAllTextAsync(fileInfo.FullName).ConfigureAwait(false);
        var base64Data = Convert.FromBase64String(base64String);
        var magickImage = new MagickImage
        {
            Quality = 100,
            ColorSpace = ColorSpace.Transparent
        };

        var readSettings = new MagickReadSettings
        {
            Density = new Density(300, 300),
            BackgroundColor = MagickColors.Transparent
        };
        
        await magickImage.ReadAsync(new MemoryStream(base64Data), readSettings).ConfigureAwait(false);
        var bitmap = magickImage.ToWriteableBitmap();
        magickImage.Dispose();
        return bitmap;
    }
}
