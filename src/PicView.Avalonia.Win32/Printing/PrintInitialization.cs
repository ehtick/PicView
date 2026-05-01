using PicView.Avalonia.Win32.Views;
using PicView.Core.WindowsNT.Printing;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Win32.Printing;

public static class PrintInitialization
{
    public static async ValueTask InitializeAsync(MainWindowViewModel vm, string path,
        PrintPreviewWindow printPreviewWindow)
    {
        var printers = await Task.Run(Win32Print.GetAvailablePrinters);
        var defaultPrinter = await Task.Run(Win32Print.GetDefaultPrinter) ?? printers.FirstOrDefault() ?? string.Empty;

        var paperSizes = !string.IsNullOrEmpty(defaultPrinter)
            ? await Task.Run(() => Win32Print.GetPaperSizes(defaultPrinter))
            : [];

        await PicView.Avalonia.Printing.PrintInitialization.InitializeAsync(
            vm, path, printPreviewWindow, printers, paperSizes, defaultPrinter);

    }
}