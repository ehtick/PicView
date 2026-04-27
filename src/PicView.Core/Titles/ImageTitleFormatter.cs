using Cysharp.Text;
using PicView.Core.Extensions;
using PicView.Core.Localization;

namespace PicView.Core.Titles;

/// <summary>
/// Provides methods for generating window titles for image display,
/// including the ability to format titles with image properties such as
/// file name, resolution, zoom level, and aspect ratio.
/// </summary>
public static class ImageTitleFormatter
{
    private const double NormalZoomLevel = 100;
    private const double NoZoomLevel = 0.0;

    public static WindowTitles GenerateTitleStrings(int width, int height, int index, FileInfo? fileInfo,
        double zoomValue, IReadOnlyList<FileInfo> filesList)
        => GenerateTitleStrings(width, height, index, fileInfo.Name, fileInfo, zoomValue, filesList);

    /// <summary>
    /// Generates window title strings based on provided image and display-related metadata.
    /// The resulting titles include information such as the file name, image dimensions, file size,
    /// aspect ratio, zoom percentage, and application name.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="index">The zero-based index of the current file in the list.</param>
    /// <param name="namePart">The base name of the file used in the title.</param>
    /// <param name="fileInfo">The file information of the image file.</param>
    /// <param name="zoomValue">The current zoom level of the image as a double value.</param>
    /// <param name="filesList">A read-only list of all files in the current file collection.</param>
    /// <returns>
    /// A <see cref="WindowTitles"/> struct containing the base title, title with the application name appended,
    /// and a title using the full file path.
    /// </returns>
    public static WindowTitles GenerateTitleStrings(int width, int height, int index, string namePart,
        FileInfo? fileInfo, double zoomValue, IReadOnlyList<FileInfo> filesList)
    {
        using var sb = ZString.CreateStringBuilder(true);

        sb.Append(namePart);
        sb.Append(' ');
        sb.Append(index + 1);
        sb.Append('/');
        sb.Append(filesList.Count);
        sb.Append(' ');
        sb.Append(filesList.Count == 1 ? TranslationManager.Translation?.File : TranslationManager.Translation?.Files);
        sb.Append(" (");
        sb.Append(width);
        sb.Append(" x ");
        sb.Append(height);
        sb.Append(AspectRatioFormatter.FormatAspectRatio(width, height));
        sb.Append(") "); 
        sb.Append(fileInfo.Length.GetReadableFileSize());

        var zoomString = FormatZoomPercentage(zoomValue);
        if (zoomString is not null)
        {
            sb.Append(", ");
            sb.Append(zoomString);
        }

        var baseTitle = sb.ToString();

        sb.Append(" - ");
        sb.Append(StringExtensions.AppName);
        var fullTitle = sb.ToString();
        var filePathTitle = baseTitle.Replace(namePart, fileInfo.FullName);

        return new WindowTitles
        {
            BaseTitle = baseTitle,
            TitleWithAppName = fullTitle,
            FilePathTitle = filePathTitle
        };
    }
    
    public static WindowTitles GenerateTitleStrings(ImageTitleInfo info, IReadOnlyList<FileInfo> filesList)
    {
        using var sb = ZString.CreateStringBuilder(true);

        sb.Append(info.FileInfo.Name);
        sb.Append(' ');
        sb.Append(info.Index + 1);
        sb.Append('/');
        sb.Append(filesList.Count);
        sb.Append(' ');
        sb.Append(filesList.Count == 1 ? TranslationManager.Translation?.File : TranslationManager.Translation?.Files);
        sb.Append(" (");
        sb.Append(info.Width);
        sb.Append(" x ");
        sb.Append(info.Height);
        sb.Append(AspectRatioFormatter.FormatAspectRatio(info.Width, info.Height));
        sb.Append(") "); 
        if (info.FileInfo is not null)
        {
            sb.Append(info.FileInfo.Length.GetReadableFileSize());
        }

        var zoomString = FormatZoomPercentage(info.ZoomValue);
        if (zoomString is not null)
        {
            sb.Append(", ");
            sb.Append(zoomString);
        }

        var baseTitle = sb.ToString();

        sb.Append(" - ");
        sb.Append(StringExtensions.AppName);
        var fullTitle = sb.ToString();
        var filePathTitle = baseTitle.Replace(info.FileInfo.Name, info.FileInfo.FullName);

        return new WindowTitles
        {
            BaseTitle = baseTitle,
            TitleWithAppName = fullTitle,
            FilePathTitle = filePathTitle
        };
    }
    
        /// <summary>
    /// Generates window title strings for two images displayed side-by-side.
    /// The resulting format is similar to: image.png (753 x 1090) 209.99 KB ⇜ || ⇝ image2.jpg (1024 x 1440) 201.8 KB [99 & 100 / 100 Files]
    /// </summary>
    /// <param name="first">Information about the first image.</param>
    /// <param name="second">Information about the second image.</param>
    /// <param name="filesList">A read-only list of all files in the current file collection.</param>
    /// <returns>A <see cref="WindowTitles"/> struct containing the generated side-by-side titles.</returns>
    public static WindowTitles GenerateTitleForSideBySide(ImageTitleInfo first, ImageTitleInfo second, int firstIndex, int secondIndex, IReadOnlyList<FileInfo> filesList)
    {
        using var sb = ZString.CreateStringBuilder(true);

        // First image details
        sb.Append(first.FileInfo.Name);
        sb.Append(" (");
        sb.Append(first.Width);
        sb.Append(" x ");
        sb.Append(first.Height);
        sb.Append(AspectRatioFormatter.FormatAspectRatio(first.Width, first.Height));
        sb.Append(") ");
        sb.Append(first.FileInfo.Length.GetReadableFileSize());

        var firstZoomString = FormatZoomPercentage(first.ZoomValue);
        if (firstZoomString is not null)
        {
            sb.Append(", ");
            sb.Append(firstZoomString);
        }

        // Separator
        sb.Append(" \u21dc || \u21dd ");

        // Second image details
        sb.Append(second.FileInfo.Name);
        sb.Append(" (");
        sb.Append(second.Width);
        sb.Append(" x ");
        sb.Append(second.Height);
        sb.Append(AspectRatioFormatter.FormatAspectRatio(second.Width, second.Height));
        sb.Append(") ");
        if (second.FileInfo is not null)
        {
            sb.Append(second.FileInfo.Length.GetReadableFileSize());
        }

        var secondZoomString = FormatZoomPercentage(second.ZoomValue);
        if (secondZoomString is not null)
        {
            sb.Append(", ");
            sb.Append(secondZoomString);
        }

        // File count group
        sb.Append(" [");
        sb.Append(firstIndex);
        sb.Append(" & ");
        sb.Append(secondIndex);
        sb.Append(" / ");
        sb.Append(filesList.Count);
        sb.Append(' ');
        sb.Append(filesList.Count == 1 ? TranslationManager.Translation?.File : TranslationManager.Translation?.Files);
        sb.Append(']');

        var baseTitle = sb.ToString();
        var fullTitle = $"{baseTitle} - {StringExtensions.AppName}";
        
        // Construct FilePathTitle replacing the file names with their full paths
        var filePathTitle = baseTitle;
        filePathTitle = filePathTitle.Replace(first.FileInfo.Name, first.FileInfo.FullName);
        filePathTitle = filePathTitle.Replace(second.FileInfo.Name, second.FileInfo.FullName);

        return new WindowTitles
        {
            BaseTitle = baseTitle,
            TitleWithAppName = fullTitle,
            FilePathTitle = filePathTitle
        };
    }

    public static WindowTitles GenerateTiffTitleStrings(int width, int height, int index, FileInfo? fileInfo, double zoomValue, IReadOnlyList<FileInfo> filesList, int? currentPage, int? pageCount)
    {
        var namePart = $"{fileInfo.Name} [{currentPage + 1}/{pageCount}]";
        return GenerateTitleStrings(width, height,  index, namePart, fileInfo, zoomValue, filesList);
    }

    /// <summary>
    /// Formats the zoom percentage for display, omitting the zoom information if it's 0 or 100%.
    /// </summary>
    /// <param name="zoomValue">The current zoom level of the image as a double value.</param>
    /// <returns>A formatted string representing the zoom percentage, or null if the zoom is 0 or 100%.</returns>
    private static string? FormatZoomPercentage(double zoomValue) =>
        zoomValue is NoZoomLevel or NormalZoomLevel ? null : $"{Math.Floor(zoomValue)}%";


    /// <summary>
    /// Generates a window title for a single image, including its name, resolution, aspect ratio, and zoom level.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="name">Display name of the image.</param>
    /// <param name="zoomValue">The current zoom level of the image.</param>
    /// <returns>A <see cref="WindowTitles"/> struct containing the generated titles for the single image.</returns>
    public static WindowTitles GenerateTitleForSingleImage(int width, int height, string name, double zoomValue)
    {
        using var sb = ZString.CreateStringBuilder(true);

        // Build the base title (common parts)
        sb.Append(name);
        sb.Append(" (");
        sb.Append(width);
        sb.Append(" x ");
        sb.Append(height);
        sb.Append(AspectRatioFormatter.FormatAspectRatio(width, height));
        sb.Append(") "); 

        // Add zoom information if applicable
        var zoomString = FormatZoomPercentage(zoomValue);
        if (zoomString is not null)
        {
            sb.Append(", ");
            sb.Append(zoomString);
        }

        var baseTitle = sb.ToString(); // Save the base title (without AppName)

        // Full title with AppName
        var fullTitle = $"{baseTitle} - {StringExtensions.AppName}";

        return new WindowTitles
        {
            BaseTitle = baseTitle,
            TitleWithAppName = fullTitle,
            FilePathTitle = baseTitle
        };
    }

}