using System.Diagnostics;
using ImageMagick;
using PicView.Core.DebugTools;

namespace PicView.Core.ImageDecoding;

/// <summary>
/// Provides methods to analyze image properties
/// </summary>
public static class ImageAnalyzer
{
    /// <summary>
    ///     Gets the number of frames in an image.
    /// </summary>
    /// <param name="file">The path to the image file.</param>
    /// <returns>The number of frames in the image. Returns 0 if an error occurs.</returns>
    /// <remarks>
    ///     This method uses the Magick.NET library to load the image and retrieve the frame count.
    /// </remarks>
    public static int GetImageFrames(string file)
    {
        try
        {
            using var magickImageCollection = new MagickImageCollection();
            magickImageCollection.Ping(file);
            return magickImageCollection.Count;
        }
#if DEBUG
        catch (MagickCorruptImageErrorException ex)
        {
            // This can happen with malformed or unsupported image files.
            // We'll treat it as a non-animated image with 0 frames.

            Trace.WriteLine($"Caught a corrupt image exception for file '{file}'. Treating as 0 frames. Details: {ex.Message}");
            DebugHelper.LogDebug(nameof(ImageAnalyzer),nameof(GetImageFrames), ex);
            return 0;
        }
#endif
        catch (MagickException ex)
        {
            DebugHelper.LogDebug(nameof(ImageAnalyzer),nameof(GetImageFrames), ex);
            return 0;
        }
    }
    
    /// <summary>
    /// Determines if the specified image file is animated
    /// </summary>
    /// <param name="fileInfo">File information for the image</param>
    /// <returns>True if the image is animated; otherwise, false</returns>
    public static bool IsAnimated(FileInfo fileInfo) => 
        GetImageFrames(fileInfo.FullName) > 1;

    /// <summary>
    ///     Retrieves the compression quality of the specified image file.
    /// </summary>
    /// <param name="file">The path to the image file.</param>
    /// <returns>The compression quality of the image, as a percentage (0-100).</returns>
    /// <remarks>
    ///     This method uses the Magick.NET library to load the image and retrieve the compression quality.
    /// </remarks>
    public static uint GetCompressionQuality(string file)
    {
        using var magickImage = new MagickImage();
        magickImage.Ping(file);
        return magickImage.Quality;
    }
}