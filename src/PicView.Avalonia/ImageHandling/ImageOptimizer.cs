using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.DebugTools;

namespace PicView.Avalonia.ImageHandling;

/// <summary>
/// Provides image optimization functionality
/// </summary>
public static class ImageOptimizer
{
    /// <summary>
    /// Optimizes the current image in the view model
    /// </summary>
    /// <param name="vm">The main view model</param>
    public static async Task OptimizeImageAsync(MainViewModel vm)
    {
        if (!NavigationManager.CanNavigate(vm) || vm.PicViewer.FileInfo == null)
        {
            return;
        }
        
        await Task.Run(() =>
        {
            try
            {
                var optimizer = new ImageMagick.ImageOptimizer
                {
                    OptimalCompression = true
                };
                optimizer.LosslessCompress(vm.PicViewer.FileInfo.CurrentValue.FullName);
            }
            catch (Exception ex)
            {
                DebugHelper.LogDebug(nameof(ImageOptimizer), nameof(OptimizeImageAsync), ex);
            }
        });
        
        TitleManager.SetTitle(vm);
    }
}