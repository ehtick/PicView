using System.Diagnostics;
using ImageMagick;

namespace PicView.Core.ImageDecoding;

/// <summary>
///     Provides methods for decoding various image formats.
/// </summary>
public static class Base64Decoder
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
    
    /// <summary>
    /// Determines whether a string is a valid Base64 string.
    /// </summary>
    /// <param name="base64">The string to check.</param>
    /// <returns>String as a valid Base64 string; otherwise, "".</returns>
    public static string IsBase64String(string base64)
    {
        if (string.IsNullOrEmpty(base64))
        {
            return "";
        }

        if (base64.StartsWith("data:image/webp;base64,"))
        {
            base64 = base64["data:image/webp;base64,".Length..];
        }

        if (base64.StartsWith("data:image/jpeg;base64,"))
        {
            base64 = base64["data:image/jpeg;base64,".Length..];
        }
        
        if (base64.StartsWith("data:image/png;base64,"))
        {
            base64 = base64["data:image/png;base64,".Length..];
        }
        
        if (base64.StartsWith("data:image/gif;base64,"))
        {
            base64 = base64["data:image/gif;base64,".Length..];
        }

        var buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out _) ? base64 : "";
    }
}