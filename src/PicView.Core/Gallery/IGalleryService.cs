namespace PicView.Core.Gallery;

public interface IGalleryService
{
    /// <summary>
    /// Asynchronously performs the initial load of the gallery with the specified folder path.
    /// </summary>
    /// <param name="folderPath">The path of the folder to load.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task InitialLoadAsync(string folderPath, CancellationToken ct);

    /// <summary>
    /// Asynchronously reloads the gallery with the specified folder path.
    /// </summary>
    /// <param name="folderPath">The path of the folder to reload.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ReloadAsync(string folderPath, CancellationToken ct);

    void Close();
    void OpenFullscreen();
    void OpenDocked();
    void Toggle();

    /// <summary>
    /// Current gallery state.
    /// </summary>
    GalleryState State { get; }

    /// <summary>
    /// True if the gallery is open and filling the entire window.
    /// </summary>
    bool IsFullscreen => State == GalleryState.Fullscreen;

    /// <summary>
    /// True if the gallery is open and docked to an edge.
    /// </summary>
    bool IsDocked => State == GalleryState.Docked;

    /// <summary>
    /// True if the gallery is not visible.
    /// </summary>
    bool IsClosed => State == GalleryState.Closed;
}