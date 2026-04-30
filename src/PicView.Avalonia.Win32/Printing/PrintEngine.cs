using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Printing;
using PicView.Core.Localization;
using PicView.Core.Printing;
using PicView.Core.WindowsNT.Printing;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace PicView.Avalonia.Win32.Printing;

public static class PrintEngine
{
    private const float PrintDpi = 300f; // physical print DPI

    public static async Task RunPrintJob(PrintSettings settings, Bitmap avaloniaBmp)
    {
        ArgumentNullException.ThrowIfNull(avaloniaBmp);

        // 1. Grayscale
        if (settings.ColorMode.Value == (int)ColorModes.BlackAndWhite)
        {
            avaloniaBmp = PrintCore.ToGrayScale(avaloniaBmp, PrintDpi);
        }

        // 2. Resolve paper size
        var paperInfo = ResolvePaper(settings.PrinterName.Value, settings.PaperSize.Value,
            settings.Orientation.Value == (int)Orientations.Landscape);

        // 3. Compute layout
        var layout = PrintCore.ComputeLayout(
            paperInfo.WidthMm, paperInfo.HeightMm, settings,
            avaloniaBmp.PixelSize.Width, avaloniaBmp.PixelSize.Height, PrintDpi);

        // 4. Render to RenderTargetBitmap
        var pageSize = new PixelSize((int)Math.Round(layout.PageWidthPx), (int)Math.Round(layout.PageHeightPx));
        var rtb = new RenderTargetBitmap(pageSize);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            using var ctx = rtb.CreateDrawingContext();
            ctx.FillRectangle(Brushes.White, new Rect(0, 0, layout.PageWidthPx, layout.PageHeightPx));
            var dest = new Rect(layout.DrawX, layout.DrawY, layout.DrawWidth, layout.DrawHeight);
            ctx.DrawImage(avaloniaBmp, new Rect(0, 0, avaloniaBmp.PixelSize.Width, avaloniaBmp.PixelSize.Height), dest);
        });

        // 5. Handle Printing or PDF Export
        var printerName = settings.PrinterName.Value ?? "";
        if (printerName == TranslationManager.Translation.SaveAsPdf)
        {
            var outputFilename = Path.GetFileNameWithoutExtension(settings.ImagePath.Value) + ".pdf";
            await PdfExport.SavePdfWithFilePicker(outputFilename, rtb);
        }
        else
        {
            // Convert to a format Win32 can handle (Bgra8888)
            var width = rtb.PixelSize.Width;
            var height = rtb.PixelSize.Height;
            var stride = width * 4;
            var bufferSize = height * stride;
            var bufferPtr = Marshal.AllocHGlobal(bufferSize);
            try
            {
                rtb.CopyPixels(new PixelRect(0, 0, width, height), bufferPtr, bufferSize, stride);
                var title = Path.GetFileName(settings.ImagePath.Value) ?? "PicView";
                Win32Print.Print(printerName, title, settings.Copies.Value, width, height, bufferPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPtr);
            }
        }
    }

    public static PaperInfo ResolvePaper(string? printerName, string? requestedName, bool landscape)
    {
        var normalized = requestedName ?? "A4";
        var (w, h) = PaperSizeHelper.GetMmSize(normalized);

        return landscape
            ? new PaperInfo(normalized, h, w)
            : new PaperInfo(normalized, w, h);
    }

    public static IEnumerable<string> GetPaperSizes(string printerName)
    {
        return Win32Print.GetPaperSizes(printerName);
    }

    public record PaperInfo(string Name, double WidthMm, double HeightMm);
}
