namespace PicView.Avalonia.Printing;

public readonly record struct PrinterPageSettings(bool Landscape, PaperSize PaperSize, Margins Margins);

public readonly record struct PaperSize(string PaperName);

public readonly record struct Margins(int Top, int Bottom, int Left, int Right);