namespace PicView.Core.Sizing;

public readonly record struct ImageSize2(
    double WindowWidth,
    double WindowHeight,
    double Width,
    double Height,
    double ScrollViewerWidth,
    double ScrollViewerHeight,
    double Margin,
    double AspectRatio);