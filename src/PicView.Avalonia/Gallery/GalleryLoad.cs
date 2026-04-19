using PicView.Avalonia.ViewModels;
using PicView.Core.Gallery;
using GalleryItem = PicView.Avalonia.Views.Gallery.GalleryItem;

namespace PicView.Avalonia.Gallery;

// TODO deprecated, delete
public static class GalleryLoad
{
    private static string? _currentDirectory;
    private static CancellationTokenSource? _cancellationTokenSource;
    public static bool IsLoading { get; private set; }

    public static async ValueTask LoadGallery(MainViewModel vm, string currentDirectory)
    {
    }


    private static void UpdateGalleryItem(MainViewModel vm, FileInfo fileInfo,
        GalleryThumbInfo.GalleryThumbHolder thumbData, GalleryItem galleryItem)
    {
    }

    private static void CleanupAfterLoading()
    {
        IsLoading = false;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _currentDirectory = null;
    }

    public static async ValueTask ReloadGalleryAsync(MainViewModel vm, string currentDirectory)
    {
    }

    /// <summary>
    ///     Checks and reloads the gallery if necessary based on the provided file info.
    /// </summary>
    /// <param name="fileInfo">The file info to check.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async ValueTask CheckAndReloadGallery(FileInfo fileInfo, MainViewModel vm)
    {
    }

    public static async ValueTask CancelGalleryLoadAsync()
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
        }

        CleanupAfterLoading();
    }
}