using PicView.Avalonia.ViewModels;
using PicView.Core.DebugTools;
using PicView.Core.ImageDecoding;

namespace PicView.Avalonia.Converters;

internal static class ConversionHelper
{
    public static async Task<bool> ResizeByWidth(FileInfo fileInfo, double width)
    {
        if (width <= 0)
        {
            return false;
        }

        return await SaveImageFileHelper.ResizeImageAsync(fileInfo, (uint)width, 0).ConfigureAwait(false);
    }

    public static async Task<bool> ResizeByHeight(FileInfo fileInfo, double height)
    {
        if (height <= 0)
        {
            return false;
        }

        return await SaveImageFileHelper.ResizeImageAsync(fileInfo, 0, (uint)height).ConfigureAwait(false);
    }

    public static void DetermineIfOptimizeImageShouldBeEnabled(MainViewModel vm)
    {
        if (vm.PicViewer.FileInfo is null)
        {
            vm.ShouldOptimizeImageBeEnabled = false;
            return;
        }

        try
        {
            if (vm.PicViewer.FileInfo.CurrentValue.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase)
                || vm.PicViewer.FileInfo.CurrentValue.Extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                || vm.PicViewer.FileInfo.CurrentValue.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase)
                || vm.PicViewer.FileInfo.CurrentValue.Extension.Equals(".gif", StringComparison.InvariantCultureIgnoreCase))
            {
                vm.ShouldOptimizeImageBeEnabled = true;
            }
            else
            {
                vm.ShouldOptimizeImageBeEnabled = false;
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(ConversionHelper), nameof(DetermineIfOptimizeImageShouldBeEnabled), e);
            vm.ShouldOptimizeImageBeEnabled = false;
        }
    }
}