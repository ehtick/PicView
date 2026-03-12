using Avalonia.Media.Imaging;
using PicView.Core.Printing;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Printing;

public static class PrintInitialization
{
    public static void Initialize(MainWindowViewModel vm, string path, IPrintWindow printWindow,
        List<string> installedPrinters, List<string> paperSizes, bool supportsColor, string printerName, PrinterPageSettings pageSettings)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.Image.CurrentValue != null && File.Exists(path))
        {
            using var fs = File.OpenRead(path);
            vm.PrintPreview.PreviewImage.Value = new Bitmap(fs);
            // Prefill page sizes to avoid excessive resize
            vm.PrintPreview.PageWidth.Value = 650;
            vm.PrintPreview.PageHeight.Value = 950;
        }

        // Load installed printers
        vm.PrintPreview.Printers.Value = new List<string>(installedPrinters);
        vm.PrintPreview.PaperSizes.Value = new List<string>(paperSizes);

        var currentPrintSettings =
            new PrintSettings // TODO: Add print settings to its own config class to remember user preference
            {
                ImagePath = { Value = vm.WindowTabs.ActiveTab.CurrentValue.FileInfo?.Value?.FullName },
                PrinterName = { Value = printerName },
                PaperSize = { Value = pageSettings.PaperSize.PaperName },
                ColorMode =
                {
                    Value = supportsColor ? (int)ColorModes.Auto : (int)ColorModes.BlackAndWhite
                },
                Orientation =
                    { Value = pageSettings.Landscape ? (int)Orientations.Landscape : (int)Orientations.Portrait },
                MarginTop = { Value = PrintSettings.HundredthsInchToMm(pageSettings.Margins.Top) },
                MarginBottom = { Value = PrintSettings.HundredthsInchToMm(pageSettings.Margins.Bottom) },
                MarginLeft = { Value = PrintSettings.HundredthsInchToMm(pageSettings.Margins.Left) },
                MarginRight = { Value = PrintSettings.HundredthsInchToMm(pageSettings.Margins.Right) }
            };

        vm.PrintPreview.PrintSettings.Value = currentPrintSettings;
        printWindow.Initialize();
    }
}