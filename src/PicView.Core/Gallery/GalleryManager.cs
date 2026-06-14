namespace PicView.Core.Gallery;

public static class GalleryManager
{
    public static async ValueTask CloseDockedGalleryAsync(CancellationToken ct)
    {
        Settings.Gallery.IsGalleryDocked = false;
        // Wait for animation to finish
        await Task.Delay(TimeSpan.FromSeconds(GalleryDefaults.VeryFastAnimationSpeed), ct);
        Settings.Gallery.DockPosition = GalleryDockPosition.Closed;
        await SaveSettingsAsync();
    }
}