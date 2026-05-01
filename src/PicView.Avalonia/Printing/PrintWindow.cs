using PicView.Core.ViewModels;

namespace PicView.Avalonia.Printing;

public interface IPrintWindow
{
    ValueTask UpdatePreviewAsync(PrintPreviewViewModel vm);
    
    ValueTask RunPrintAsync(MainWindowViewModel vm);
}