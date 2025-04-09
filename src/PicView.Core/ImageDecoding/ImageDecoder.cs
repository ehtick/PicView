using System.Diagnostics;
using ImageMagick;

namespace PicView.Core.ImageDecoding;

/// <summary>
///     Provides methods for decoding various image formats.
/// </summary>
public static class ImageDecoder
{
    public static MagickImage? Base64ToMagickImage(string base64)
    {
        try
        {
            var base64Data = Convert.FromBase64String(base64);
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

            magickImage.Read(new MemoryStream(base64Data), readSettings);
            return magickImage;
        }
        catch (Exception e)
        {
#if DEBUG
            Trace.WriteLine($"{nameof(Base64ToMagickImage)} exception:\n{e.Message}");
#endif
            return null;
        }
    }

    public static async Task<MagickImage?> Base64ToMagickImage(FileInfo fileInfo)
    {
        var base64String = await File.ReadAllTextAsync(fileInfo.FullName).ConfigureAwait(false);
        return Base64ToMagickImage(base64String);
    }
}