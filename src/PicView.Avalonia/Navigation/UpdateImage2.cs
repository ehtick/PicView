using Avalonia.Controls;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Localization;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Navigation;

public static class UpdateImage2
{
    public static void UpdateFileInfo(TabViewModel tabViewModel, FileInfo? file)
    {
        if (tabViewModel.Model.Image is null || tabViewModel.Model.PixelHeight is 0 || tabViewModel.Model.PixelWidth is 0)
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

        // Trigger file changes to UI
        tabViewModel.FileInfo.Value = file;
                        
        tabViewModel.UpdateTabTitle();
        
        tabViewModel.Format.Value = tabViewModel.Model.Format;
    }

    public static void ChangeImage(TabViewModel tabViewModel, object image, MainWindowViewModel mainWindowViewModel)
    {
        // Trigger image change to UI
        tabViewModel.Image.Value = image;

        if (Settings.WindowProperties.AutoFit)
        {
            WindowResizing2.SetSize(tabViewModel.Model.PixelWidth,
                tabViewModel.Model.PixelHeight,
                WindowResizeReason.Application,
                mainWindowViewModel);
        }

        // Update tiff title if appropriate (there are no file changes in this instance
        if (tabViewModel.Model.TiffNavigation is null)
        {
            return;
        }
        // Update title to reflect tiff navigation changes
        tabViewModel.UpdateTabTitle();
    }
}