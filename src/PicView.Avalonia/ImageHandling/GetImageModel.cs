using Avalonia.Media.Imaging;
using ImageMagick;
using PicView.Core.DebugTools;
using PicView.Core.ImageDecoding;
using PicView.Core.Models;

namespace PicView.Avalonia.ImageHandling;

public static class GetImageModel
{
    // Group extensions by how they should be handled
    private static readonly Dictionary<string, Func<FileInfo, ImageModel, Task>> ExtensionHandlers = new()
    {
        { ".webp", ProcessWebpAsync },
        { ".gif", ProcessGifAsync },
        { ".png", ProcessStandardBitmapAsync },
        { ".jpg", ProcessStandardBitmapAsync },
        { ".jpeg", ProcessStandardBitmapAsync },
        { ".jpe", ProcessStandardBitmapAsync },
        { ".bmp", ProcessStandardBitmapAsync },
        { ".jfif", ProcessStandardBitmapAsync },
        { ".ico", ProcessStandardBitmapAsync },
        { ".wbmp", ProcessStandardBitmapAsync },
        { ".svg", ProcessSvgAsync },
        { ".svgz", ProcessSvgAsync },
        { ".b64", ProcessBase64Async }
    };
    
    /// <inheritdoc cref="GetImageModelAsync(System.IO.FileInfo, MagickImage)"/>
    public static async Task<ImageModel> GetImageModelAsync(FileInfo fileInfo) =>
        await GetImageModelAsync(fileInfo, null).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously retrieves an <see cref="ImageModel"/> instance based on the provided file and optional <see cref="MagickImage"/>.
    /// </summary>
    /// <param name="fileInfo">The file information of the image to process.</param>
    /// <param name="magickImage">An optional <see cref="MagickImage"/> instance. If null, a new instance will be created internally.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the constructed <see cref="ImageModel"/>.</returns>
    public static async Task<ImageModel> GetImageModelAsync(FileInfo fileInfo, MagickImage? magickImage)
    {
        if (fileInfo is null)
        {
            DebugHelper.LogDebug(nameof(GetImageModel), nameof(GetImageModelAsync), "fileInfo is null");
            return CreateErrorImageModel(null);
        }

        var imageModel = new ImageModel { FileInfo = fileInfo };
        var shouldDisposeMagickImage = magickImage is null;

        try
        {
            // Initialize MagickImage if not provided
            magickImage ??= CreateAndPingMagickImage(fileInfo);

            // Extract metadata
            imageModel.EXIFOrientation = EXIFHelper.GetImageOrientation(magickImage);
            imageModel.Format = magickImage.Format;

            // Process the image based on extension
            var ext = fileInfo.Extension.ToLowerInvariant();
            var processor = ExtensionHandlers.GetValueOrDefault(ext, ProcessDefaultImageAsync);
            await processor(fileInfo, imageModel).ConfigureAwait(false);

            return imageModel;
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(GetImageModel), nameof(GetImageModelAsync), e);
            return CreateErrorImageModel(fileInfo);
        }
        finally
        {
            if (shouldDisposeMagickImage)
            {
                magickImage?.Dispose();
            }
        }
    }

    private static MagickImage CreateAndPingMagickImage(FileInfo fileInfo)
    {
        var magickImage = new MagickImage();
        magickImage.Ping(fileInfo);
        return magickImage;
    }

    private static void SetBitmapProperties(Bitmap? bitmap, ImageModel imageModel, ImageType imageType = ImageType.Bitmap)
    {
        imageModel.Image = bitmap;
        imageModel.PixelWidth = bitmap?.PixelSize.Width ?? 0;
        imageModel.PixelHeight = bitmap?.PixelSize.Height ?? 0;
        imageModel.ImageType = imageType;
    }

    private static ImageModel CreateErrorImageModel(FileInfo? fileInfo)
    {
        return new ImageModel
        {
            FileInfo = fileInfo,
            ImageType = ImageType.Invalid,
            Image = null, // TODO replace with error image
            PixelHeight = 0,
            PixelWidth = 0,
            EXIFOrientation = EXIFHelper.EXIFOrientation.None
        };
    }

    #region Image Processing Methods

    private static async Task ProcessStandardBitmapAsync(FileInfo fileInfo, ImageModel imageModel)
    {
        var bitmap = await GetImage.GetStandardBitmapAsync(fileInfo).ConfigureAwait(false);
        SetBitmapProperties(bitmap, imageModel);
    }

    private static async Task ProcessWebpAsync(FileInfo fileInfo, ImageModel imageModel)
    {
        var bitmap = await GetImage.GetStandardBitmapAsync(fileInfo).ConfigureAwait(false);
        var imageType = ImageAnalyzer.IsAnimated(fileInfo) ? ImageType.AnimatedWebp : ImageType.Bitmap;
        SetBitmapProperties(bitmap, imageModel, imageType);
    }

    private static async Task ProcessGifAsync(FileInfo fileInfo, ImageModel imageModel)
    {
        var bitmap = await GetImage.GetStandardBitmapAsync(fileInfo).ConfigureAwait(false);
        var imageType = ImageAnalyzer.IsAnimated(fileInfo) ? ImageType.AnimatedGif : ImageType.Bitmap;
        SetBitmapProperties(bitmap, imageModel, imageType);
    }

    private static Task ProcessSvgAsync(FileInfo fileInfo, ImageModel imageModel)
    {
        using var svg = new MagickImage();
        svg.Ping(fileInfo.FullName);
        
        imageModel.PixelWidth = (int)svg.Width;
        imageModel.PixelHeight = (int)svg.Height;
        imageModel.ImageType = ImageType.Svg;
        imageModel.Image = fileInfo.FullName;
        
        return Task.CompletedTask;
    }

    private static async Task ProcessBase64Async(FileInfo fileInfo, ImageModel imageModel)
    {
        var bitmap = await GetImage.GetBase64ImageAsync(fileInfo).ConfigureAwait(false);
        SetBitmapProperties(bitmap, imageModel);
    }

    private static async Task ProcessDefaultImageAsync(FileInfo fileInfo, ImageModel imageModel)
    {
        var bitmap = await GetImage.GetDefaultBitmapAsync(fileInfo).ConfigureAwait(false);
        SetBitmapProperties(bitmap, imageModel);
    }

    #endregion
}