using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.MacOS.Views;
using PicView.Avalonia.ViewModels;
using PicView.Core.MacOS.Printing;
using PicView.Core.Printing;

namespace PicView.Avalonia.MacOS.Printing;

public static class MacPrintInitialization
{
    public static void Initialize(MainViewModel vm, string path, PrintPreviewWindow printPreviewWindow)
    {
        // Load image for preview (same as Windows, but using Avalonia Bitmap)
        if (vm.PicViewer.FileInfo.Value != null && File.Exists(path))
        {
            using var fs = File.OpenRead(path);
            vm.PrintPreview.PreviewImage.Value = new Bitmap(fs);
            vm.PrintPreview.PageWidth.Value = 650;
            vm.PrintPreview.PageHeight.Value = 950;
        }

        // 1. Printers via CUPS
        var printers = MacOSPrint.GetAvailablePrinters().ToList(); // includes "Save as PDF" first
        vm.PrintPreview.Printers.Value = printers;

        var defaultPrinter = printers.FirstOrDefault() ?? string.Empty;

        // 2. Paper sizes – simple catalog (A4 / Letter)
        vm.PrintPreview.PaperSizes.Value =
            new List<string>(MacPrintEngine.GetPaperSizes(defaultPrinter));

        // 3. Build initial PrintSettings
        var currentPrintSettings = new PrintSettings
        {
            ImagePath = { Value = vm.PicViewer.FileInfo?.Value?.FullName },
            PrinterName = { Value = defaultPrinter },
            PaperSize = { Value = "A4" },
            ColorMode = { Value = (int)ColorModes.Auto },
            Orientation = { Value = (int)Orientations.Portrait },
            MarginTop = { Value = 10 },     // mm
            MarginBottom = { Value = 10 },
            MarginLeft = { Value = 10 },
            MarginRight = { Value = 10 },
        };

        vm.PrintPreview.PrintSettings.Value = currentPrintSettings;

        Dispatcher.UIThread.InvokeAsync(printPreviewWindow.Initialize);
    }
}