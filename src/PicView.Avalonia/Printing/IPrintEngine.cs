using PicView.Core.ViewModels;

namespace PicView.Avalonia.Printing;

public interface IPrintEngine
{
    ValueTask UpdatePreviewAsync(MainWindowViewModel mainVm, PrintPreviewViewModel previewVm);
    ValueTask RunPrintAsync(MainWindowViewModel mainVm);
}
