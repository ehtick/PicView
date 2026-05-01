using PicView.Core.Config;
using PicView.Core.Printing;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Printing;

public static class PrintInitialization
{
    public static async ValueTask InitializeAsync(
        MainWindowViewModel vm,
        string path,
        IPrintWindow printWindow,
        List<string> printers,
        List<string> paperSizes,
        string defaultPrinter,
        string? defaultPaperSize = null)
    {
        if (vm.PrintPreview.PrintWindowConfig is null)
        {
            vm.PrintPreview.PrintWindowConfig = new PrintWindowConfig();
            await vm.PrintPreview.PrintWindowConfig.LoadAsync();
        }

        var configProps = vm.PrintPreview.PrintWindowConfig.PrintProperties;

        vm.PrintPreview.Printers.Value = printers;
        vm.PrintPreview.PaperSizes.Value = paperSizes;

        var printerToUse = defaultPrinter;
        var configPrinter = configProps?.PrinterName;
        if (!string.IsNullOrWhiteSpace(configPrinter) && printers.Contains(configPrinter))
        {
            printerToUse = configPrinter;
        }

        var paperSizeToUse = defaultPaperSize ?? "A4";
        var configPaperSize = configProps?.PaperSize;
        if (!string.IsNullOrWhiteSpace(configPaperSize) && paperSizes.Contains(configPaperSize))
        {
            paperSizeToUse = configPaperSize;
        }
        else if (string.IsNullOrEmpty(defaultPaperSize) && paperSizes.Count != 0)
        {
            paperSizeToUse = paperSizes.First();
        }

        var currentPrintSettings = new PrintSettings
        {
            ImagePath = { Value = path },
            PrinterName = { Value = printerToUse },
            PaperSize = { Value = paperSizeToUse },
            ColorMode = { Value = configProps?.ColorMode ?? (int)ColorModes.Auto },
            Orientation = { Value = configProps?.Orientation ?? (int)Orientations.Portrait },
            ScaleMode = { Value = configProps?.ScaleMode ?? (int)ScaleModes.Fit },
            Copies = { Value = configProps?.Copies ?? 1 },
            MarginTop = { Value = configProps?.MarginTop ?? 10 }, // mm
            MarginBottom = { Value = configProps?.MarginBottom ?? 10 },
            MarginLeft = { Value = configProps?.MarginLeft ?? 10 },
            MarginRight = { Value = configProps?.MarginRight ?? 10 },
        };

        vm.PrintPreview.PrintSettings.Value = currentPrintSettings;
        
        var ps = vm.PrintPreview.PrintSettings.Value;

        // Printer change
        ps.PrinterName
            .AsObservable()
            .DistinctUntilChanged()
            .ObserveOnThreadPool()
            .SubscribeAwait(async (_, _) =>
            {
                await printWindow.UpdatePreviewAsync(vm.PrintPreview);
            })
            .AddTo(vm.PrintPreview.Disposables);
        
        // Any setting change triggers preview update
        // ReSharper disable once InvokeAsExtensionMethod
        Observable.CombineLatest(
                ps.Orientation.AsObservable(),
                ps.MarginTop.AsObservable(),
                ps.MarginBottom.AsObservable(),
                ps.MarginLeft.AsObservable(),
                ps.MarginRight.AsObservable(),
                ps.ScaleMode.AsObservable(),
                ps.ColorMode.AsObservable(),
                ps.PaperSize.AsObservable(),
                (orientation, top, bottom, left, right, scale, color, paper)
                    => (orientation, top, bottom, left, right, scale, color, paper))
            .ObserveOnThreadPool()
            .SubscribeAwait(async (_, _) =>
            {
                await printWindow.UpdatePreviewAsync(vm.PrintPreview);
            })
            .AddTo(vm.PrintPreview.Disposables);
    }
}
