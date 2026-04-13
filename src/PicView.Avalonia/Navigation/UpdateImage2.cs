using Avalonia.Controls;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.Localization;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Navigation;

public static class UpdateImage2
{
    public static void UpdateFileInfo(TabViewModel tabViewModel, FileInfo? file)
    {
        if (tabViewModel.Model.CurrentValue.Image is null || tabViewModel.Model.CurrentValue.PixelHeight is 0 || tabViewModel.Model.CurrentValue.PixelWidth is 0)
        {
            return;
        }
        
        if (file is null || file.Length is 0)
        {
            var noImage = TranslationManager.Translation?.NoImage;
            if (string.IsNullOrEmpty(noImage))
            {
                return;
            }

            tabViewModel.TabTitle.Value = noImage;
            tabViewModel.TabTooltip.Value = noImage;
            return;
        }
                        
        tabViewModel.UpdateTabTitle();
    }

    public static void ChangeImage(TabViewModel tabViewModel, MainWindowViewModel vm)
    {
        if (Settings.Zoom.ResetZoomOnChange)
        {
            if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
            {
                imageViewer.ResetZoomSlim();
                imageViewer.Rotate(0);
            }
        }
        
        double secondaryWidth, secondaryHeight;
        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            if (tabViewModel.SecondaryModel.CurrentValue is null)
            {
#if DEBUG
                DebugHelper.LogDebug(nameof(UpdateImage2), nameof(ChangeImage), $"SecondaryModel.CurrentValue is null");
#endif
                secondaryWidth = 0;
                secondaryHeight = 0;
            }
            else
            {
                secondaryWidth = tabViewModel.SecondaryModel.CurrentValue.PixelWidth;
                secondaryHeight = tabViewModel.SecondaryModel.CurrentValue.PixelHeight;
            }
        }
        else
        {
            secondaryWidth = secondaryHeight = 0;
        }
        
        if (Settings.WindowProperties.AutoFit)
        {
            WindowResizing2.SetSize(tabViewModel.Model.CurrentValue.PixelWidth,
                tabViewModel.Model.CurrentValue.PixelHeight, 
                secondaryWidth, secondaryHeight,
                WindowResizeReason.Application,
                vm);
        }

        // Update tiff title if appropriate (there are no file changes in this instance
        if (tabViewModel.Model.CurrentValue.TiffNavigation is null)
        {
            return;
        }
        // Update title to reflect tiff navigation changes
        tabViewModel.UpdateTabTitle();
    }
}