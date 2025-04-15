namespace PicView.Core.Sizing;

/// <summary>
/// Represents screen dimensions and scaling information.
/// </summary>
public readonly record struct ScreenSize
{
    /// <summary>
    /// Gets the width of the screen's working area in device-independent pixels.
    /// </summary>
    public double WorkingAreaWidth { get; init; }
    
    /// <summary>
    /// Gets the height of the screen's working area in device-independent pixels.
    /// </summary>
    public double WorkingAreaHeight { get; init; }
    
    public double Width { get; init; }
    public double Height { get; init; }
    
    public double X { get; init; }
    public double Y { get; init; }
    
    /// <summary>
    /// Gets the DPI scaling factor of the screen.
    /// </summary>
    public double Scaling { get; init; }
}