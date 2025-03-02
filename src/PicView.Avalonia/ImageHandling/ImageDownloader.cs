using System.Diagnostics;

namespace PicView.Avalonia.ImageHandling;

/// <summary>
/// Handles downloading images from URLs
/// </summary>
public static class ImageDownloader
{
    /// <summary>
    /// Downloads an image from a URL to a local temporary file
    /// </summary>
    /// <param name="url">URL of the image to download</param>
    /// <returns>Local path to the downloaded image or empty string if download failed</returns>
    public static async Task<string> DownloadImageFromUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }
        
        try
        {
            var extension = Path.GetExtension(url);
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);

            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                await using var fs = new FileStream(tempPath, FileMode.Create);
                await response.Content.CopyToAsync(fs);
                return tempPath;
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"{nameof(DownloadImageFromUrlAsync)} Error downloading image: {ex.Message}");
#endif
            
        }

        return string.Empty;
    }
}