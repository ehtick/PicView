using PicView.Avalonia.Win32.Views;
using PicView.Core.FileHandling;
using PicView.Core.Printing;
using System.Drawing;
using System.Drawing.Printing;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Win32.Printing;

public static class PrintInitialization2
{
    public static async ValueTask InitializeAsync(MainWindowViewModel vm, string path, PrintPreviewWindow2 printPreviewWindow)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.Image.CurrentValue != null && File.Exists(path))
        {
            await using var fs = FileStreamUtils.GetOptimizedFileStream(new FileInfo(path));
            vm.PrintPreview.PreviewImage.Value = new Bitmap(fs);
        }
        
        var printerSettings = new PrinterSettings();

        // Load installed printers
        vm.PrintPreview.Printers.Value = new List<string>(PrinterSettings.InstalledPrinters);
        vm.PrintPreview.PaperSizes.Value = new List<string>(PrintEngine.GetPaperSizes(printerSettings.PrinterName));


        // Pre-select default printer settings
        var pageSettings = printerSettings.DefaultPageSettings;

        var currentPrintSettings =
            new PrintSettings // TODO: Add print settings to its own config class to remember user preference
            {
                ImagePath = { Value = vm.WindowTabs.ActiveTab.CurrentValue.FileInfo?.Value?.FullName },
                PrinterName = { Value = printerSettings.PrinterName },
                PaperSize = { Value = pageSettings.PaperSize.PaperName },
                ColorMode =
                {
                    Value = printerSettings.SupportsColor ? (int)ColorModes.Auto : (int)ColorModes.BlackAndWhite
                },
                Orientation =
                    { Value = pageSettings.Landscape ? (int)Orientations.Landscape : (int)Orientations.Portrait },
                MarginTop = { Value = PrintSettings.HundredthsInchToMm(pageSettings.Margins.Top) },
                MarginBottom = { Value = PrintSettings.HundredthsInchToMm(pageSettings.Margins.Bottom) },
                MarginLeft = { Value = PrintSettings.HundredthsInchToMm(pageSettings.Margins.Left) },
                MarginRight = { Value = PrintSettings.HundredthsInchToMm(pageSettings.Margins.Right) }
            };

        vm.PrintPreview.PrintSettings.Value = currentPrintSettings;
        printPreviewWindow.Initialize();
    }
}