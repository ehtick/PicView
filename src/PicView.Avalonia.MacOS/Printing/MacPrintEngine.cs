using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Printing;
using PicView.Core.Localization;
using PicView.Core.MacOS.Printing;
using PicView.Core.Printing;

namespace PicView.Avalonia.MacOS.Printing;

public static class MacPrintEngine
{
    private const float PrintDpi = 300f; // physical print DPI

    public static async ValueTask RunPrintJob(PrintSettings settings, Bitmap avaloniaBmp)
    {
        ArgumentNullException.ThrowIfNull(avaloniaBmp);

        // 1. Convert grayscale when requested
        if (settings.ColorMode.Value == (int)ColorModes.BlackAndWhite)
        {
            avaloniaBmp = PrintCore.ToGrayScale(avaloniaBmp, PrintDpi);
        }

        // 2. Determine paper size (mm) using shared helper
        var paper = ResolvePaper(settings.PaperSize.Value, settings.Orientation.Value);

        var pageWidthMm = paper.WidthMm;
        var pageHeightMm = paper.HeightMm;

        // 3. Compute final print layout (same logic as preview)
        var layout = PrintCore.ComputeLayout(
            pageWidthMm,
            pageHeightMm,
            settings,
            avaloniaBmp.PixelSize.Width,
            avaloniaBmp.PixelSize.Height,
            PrintDpi);

        // 4. Render the final image at print DPI
        var pageSize = new PixelSize(
            (int)Math.Round(layout.PageWidthPx),
            (int)Math.Round(layout.PageHeightPx));

        var rtb = new RenderTargetBitmap(pageSize);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            using var ctx = rtb.CreateDrawingContext();

            // background
            ctx.FillRectangle(Brushes.White,
                new Rect(0, 0, layout.PageWidthPx, layout.PageHeightPx));

            var dest = new Rect(layout.DrawX, layout.DrawY, layout.DrawWidth, layout.DrawHeight);

            ctx.DrawImage(
                avaloniaBmp,
                new Rect(0, 0, avaloniaBmp.PixelSize.Width, avaloniaBmp.PixelSize.Height),
                dest);
        });

        // 5. Save temporary PNG file
        var tempFile = await SaveToTempPng(rtb);

        try
        {
            // 6. Handle PDF exporting or printing
            var printerName = settings.PrinterName.Value ?? "";
            var saveAsPdf = TranslationManager.GetTranslation("SaveAsPdf");

            switch (string.IsNullOrWhiteSpace(printerName))
            {
                case false
                    when string.Equals(printerName, saveAsPdf, StringComparison.Ordinal):
                    await PdfExport.SavePdfWithFilePicker(null, rtb);
                    break;
                case false:
                {
                    var title = Path.GetFileName(settings.ImagePath.Value) ?? "PicView";
                    var copies = settings.Copies.Value;

                    MacOSPrint.Print(printerName, tempFile, title, copies);
                    break;
                }
            }
        }
        finally
        {
            try { File.Delete(tempFile); } catch { /* ignore */ }
        }
    }

    public static PaperInfo ResolvePaper(string? requestedName, int orientation)
    {
        requestedName ??= "A4";

        // Pull width/height from helper (in mm)
        var (widthMm, heightMm) = PaperSizeHelper.GetMmSize(requestedName);

        var isLandscape = orientation == (int)Orientations.Landscape;

        return isLandscape
            ? new PaperInfo(requestedName, heightMm, widthMm)   // swap
            : new PaperInfo(requestedName, widthMm, heightMm);
    }

    private static async ValueTask<string> SaveToTempPng(RenderTargetBitmap rtb)
    {
        var tempDir = Path.GetTempPath();
        var path = Path.Combine(tempDir, $"picview-print-{Guid.NewGuid():N}.png");

        await using var fs = File.Create(path);
        rtb.Save(fs);

        return path;
    }

    public static IEnumerable<string> GetPaperSizes(string printerName)
        => PaperSizeHelper.GetAllNames();

    public readonly record struct PaperInfo(string Name, double WidthMm, double HeightMm);
}
