using Avalonia;
using PicView.Avalonia.ViewModels;
using PicView.Core.ISettings;

namespace PicView.Avalonia.Services;

public class ImageSettingsService : IImageSettingsService
{
    public void TriggerScalingModeUpdate(bool nearestNeighbor)
    {
        var context = Application.Current?.DataContext;
        if (context is MainViewModel vm)
        {
            vm.ImageViewer?.TriggerScalingModeUpdate(nearestNeighbor);
        }
        // In CoreViewModel architecture, ImageViewer might be handled differently or we need to find it.
        // Assuming legacy support or hybrid for now.
    }
}
