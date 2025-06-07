using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PicView.Avalonia.ImageTransformations;

public static class ImageControlHelper
{
    public static void TriggerScalingModeUpdate(Control image, bool invalidate)
    {
        var scalingMode = Settings.ImageScaling.IsScalingSetToNearestNeighbor
            ? BitmapInterpolationMode.LowQuality
            : BitmapInterpolationMode.HighQuality;

        RenderOptions.SetBitmapInterpolationMode(image, scalingMode);
        if (invalidate)
        {
            image.InvalidateVisual();
        }
    }
}