using PicView.Avalonia.Win32.Views;
using PicView.Core.Config;
using PicView.Core.Printing;
using PicView.Core.WindowsNT.Printing;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Win32.Printing;

public static class PrintInitialization
{
    public static async ValueTask InitializeAsync(MainWindowViewModel vm, string path, PrintPreviewWindow printPreviewWindow)
    {
        if (vm.PrintPreview.PrintWindowConfig is null)
        {
            vm.PrintPreview.PrintWindowConfig = new PrintWindowConfig();
            await vm.PrintPreview.PrintWindowConfig.LoadAsync();
        }

        var configProps = vm.PrintPreview.PrintWindowConfig.PrintProperties;
        
        var printers = await Task.Run(Win32Print.GetAvailablePrinters);
        var defaultPrinter = await Task.Run(Win32Print.GetDefaultPrinter) ?? printers.FirstOrDefault();

        vm.PrintPreview.Printers.Value = printers;

        var paperSizes = !string.IsNullOrEmpty(defaultPrinter)
            ? await Task.Run(() => Win32Print.GetPaperSizes(defaultPrinter)) : [];

        vm.PrintPreview.PaperSizes.Value = paperSizes;

        var currentPrintSettings = new PrintSettings
        {
            ImagePath = { Value = path },
            PrinterName = { Value = defaultPrinter },
            ColorMode = { Value = configProps?.ColorMode ?? (int)ColorModes.Auto },
            Orientation = { Value = configProps?.Orientation ?? (int)Orientations.Portrait },
            ScaleMode = { Value = configProps?.ScaleMode ?? (int)ScaleModes.Fit },
            Copies = { Value = configProps?.Copies ?? 1 },
            MarginTop = { Value = configProps?.MarginTop ?? 10 },     // mm
            MarginBottom = { Value = configProps?.MarginBottom ?? 10 },
            MarginLeft = { Value = configProps?.MarginLeft ?? 10 },
            MarginRight = { Value = configProps?.MarginRight ?? 10 },
        };

        vm.PrintPreview.PrintSettings.Value = currentPrintSettings;
        printPreviewWindow.Initialize();
    }
}