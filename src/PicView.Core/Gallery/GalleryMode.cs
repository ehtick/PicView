namespace PicView.Core.Gallery;

/// <summary>
/// Used for determining the animation to play when switching between gallery modes.
/// </summary>
public enum GalleryMode
{
    /// <summary>
    /// Animates the gallery from full-screen mode to the bottom bar.
    /// </summary>
    FullToBottom,

    /// <summary>
    /// Animates the gallery from full-screen mode to a closed state by fading out.
    /// </summary>
    FullToClosed,

    /// <summary>
    /// Animates the gallery from the bottom bar to full-screen mode.
    /// </summary>
    BottomToFull,

    /// <summary>
    /// Animates the gallery from the bottom bar to a closed state by collapsing its height.
    /// </summary>
    BottomToClosed,

    /// <summary>
    /// Animates the gallery from a closed state to full-screen mode by fading in.
    /// </summary>
    ClosedToFull,

    /// <summary>
    /// Animates the gallery from a closed state to the bottom bar by expanding its height.
    /// </summary>
    ClosedToBottom,

    /// <summary>
    /// Used when the gallery should be closed instantly with no animation.
    /// </summary>
    Closed,

    /// <summary>
    /// Used when the gallery should be opened to the bottom bar with no animation. 
    /// </summary>
    BottomNoAnimation
}