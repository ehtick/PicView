using PicView.Core.Config;

namespace PicView.Core.Gallery;

public static class GalleryManager
{
    public static async ValueTask UpdateGalleryDockedStatusAsync(bool isDocked, CancellationToken ct)
    {
        Settings.Gallery.IsGalleryDocked = isDocked;
        // Wait for animation to finish
        await Task.Delay(TimeSpan.FromSeconds(GalleryDefaults.VeryFastAnimationSpeed), ct);
        Settings.Gallery.DockPosition = GalleryDockPosition.Closed;
        await SaveSettingsAsync();
    }
}