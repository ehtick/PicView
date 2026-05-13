using Avalonia.Controls;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.ImageTransformations;

public static class ImageStretching
{
    public static async Task ZoomToFit(MainWindowViewModel vm)
    {
        if (Settings.ImageScaling.ZoomToFit)
        {
            Settings.ImageScaling.ZoomToFit = false;
            vm.IsZoomedToFit.Value = false;
        }
        else
        {
            Settings.ImageScaling.ZoomToFit = true;
            vm.IsZoomedToFit.Value = true;
        }

        WindowResizing.SetSize(vm, WindowResizeReason.Layout);
        await SaveSettingsAsync().ConfigureAwait(false);
    }
}