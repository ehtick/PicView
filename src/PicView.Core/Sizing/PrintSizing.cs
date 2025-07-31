using System.Globalization;
using PicView.Core.Localization;

namespace PicView.Core.Sizing;

public static class PrintSizing
{
    /// <summary>
    /// Calculates and returns print sizes based on pixel dimensions and DPI values.
    /// </summary>
    /// <param name="pixelWidth">The width of the image in pixels.</param>
    /// <param name="pixelHeight">The height of the image in pixels.</param>
    /// <param name="dpiX">The horizontal DPI (dots per inch) of the image.</param>
    /// <param name="dpiY">The vertical DPI (dots per inch) of the image.</param>
    /// <returns>An instance of the PrintSizes structure containing print dimensions in centimeters, print dimensions in inches, and the image size in megapixels.</returns>
    public static PrintSizes GetPrintSizes(int pixelWidth, int pixelHeight, double dpiX, double dpiY)
    {
        var cm = TranslationManager.Translation.Centimeters;
        var mp = TranslationManager.Translation.MegaPixels;
        var inches = TranslationManager.Translation.Inches;
        var inchesWidth = pixelWidth / dpiX;
        var inchesHeight = pixelHeight / dpiY;
        var printSizeInch =
            $"{inchesWidth.ToString("0.##", CultureInfo.CurrentCulture)} x {inchesHeight.ToString("0.##", CultureInfo.CurrentCulture)} {inches}";

        var cmWidth = pixelWidth / dpiX * 2.54;
        var cmHeight = pixelHeight / dpiY * 2.54;
        var printSizeCm =
            $"{cmWidth.ToString("0.##", CultureInfo.CurrentCulture)} x {cmHeight.ToString("0.##", CultureInfo.CurrentCulture)} {cm}";
        var sizeMp =
            $"{((float)pixelHeight * pixelWidth / 1000000).ToString("0.##", CultureInfo.CurrentCulture)} {mp}";

        return new PrintSizes(printSizeCm, printSizeInch, sizeMp);
    }

    /// <summary>
    /// Represents the calculated dimensions and size of an image for printing purposes.
    /// </summary>
    /// <remarks>
    /// This struct is used to define the print sizes of an image in multiple units,
    /// including centimeters, inches, and megapixels. It is commonly utilized when
    /// determining the output characteristics of an image based on its pixel dimensions
    /// and DPI (dots per inch).
    /// </remarks>
    public readonly struct PrintSizes(string printSizeCm, string printSizeInch, string sizeMp)
    {
        public string PrintSizeCm { get; } = printSizeCm;
        public string PrintSizeInch { get; } = printSizeInch;
        public string SizeMp { get; } = sizeMp;
    }
}