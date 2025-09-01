using Avalonia.Media.Imaging;
using ImageMagick;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.ImageReading;

namespace PicView.Avalonia.ImageHandling;

public static class GetImage
{
    public static async ValueTask<object?> GetImageCore(FileInfo fileInfo, MagickImage? magickImage = null)
    {
        if (fileInfo is null)
        {
            return null;
        }

        var shouldDisposeMagickImage = magickImage is null;

        try
        {
            // Initialize MagickImage if not provided
            magickImage ??= CreateAndPingMagickImage(fileInfo);
            
            if (fileInfo.Extension.Equals(".b64", StringComparison.InvariantCultureIgnoreCase))
            {
                return await GetBase64ImageAsync(fileInfo).ConfigureAwait(false);
            }

            // Process the image based on type
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (magickImage.Format)
            {
                case MagickFormat.WebP: 
                case MagickFormat.WebM:
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
                    return await GetSkBitmapAsync(fileInfo).ConfigureAwait(false);
                
                case MagickFormat.Arw:
                case MagickFormat.Nef:
                case MagickFormat.Dng:
                case MagickFormat.Cr2:
                case MagickFormat.Rw2:
                    return await GetRawBitmapAsync(fileInfo, magickImage).ConfigureAwait(false);

                default:
                    return await GetNonStandardBitmapAsync(fileInfo, magickImage).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(GetImage), nameof(GetImageCore), e);
            return null;
        }
        finally
        {
            if (shouldDisposeMagickImage)
            {
                magickImage?.Dispose();
            }
        }
    }
    
    public static async ValueTask<Bitmap?> GetSkBitmapAsync(string file) =>
        await GetSkBitmapAsync(new FileInfo(file)).ConfigureAwait(false);

    public static async ValueTask<Bitmap?> GetSkBitmapAsync(FileInfo fileInfo)
    {
        if (fileInfo is null)
        {
            DebugHelper.LogDebug(nameof(GetImage), nameof(GetSkBitmapAsync), $"{nameof(fileInfo)} is null");
            return null;
        }
        await using var stream = FileStreamUtils.GetOptimizedFileStream(fileInfo);
        var bitmap = new Bitmap(stream);
        return bitmap;
    }
    
    public static async ValueTask<Bitmap?> GetNonStandardBitmapAsync(FileInfo fileInfo, MagickImage? magickImage)
    {
        var shouldDisposeMagickImage = magickImage is null;
        if (shouldDisposeMagickImage)
        {
            magickImage = new MagickImage();
        }
        magickImage = await MagickPerformanceReader.ReadMagickImageWithSpanAsync(fileInfo, magickImage);

        var bitmap = magickImage.ToWriteableBitmap();
        if (shouldDisposeMagickImage)
        {
            magickImage.Dispose();
        }
        return bitmap;
    }
    
    public static async ValueTask<Bitmap?> GetRawBitmapAsync(FileInfo fileInfo, MagickImage? magickImage)
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
    
    public static async ValueTask<Bitmap?> GetBase64ImageAsync(FileInfo fileInfo)
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
    
    public static MagickImage CreateAndPingMagickImage(FileInfo fileInfo)
    {
        var magickImage = new MagickImage();
        magickImage.Ping(fileInfo);
        return magickImage;
    }
}
