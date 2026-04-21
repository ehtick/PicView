using Avalonia;
using Avalonia.Controls;
using ImageMagick;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.Localization;
using PicView.Core.Titles;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Navigation;

public static class UpdateImage2
{
    public static void UpdateFileInfo(TabViewModel tabViewModel, FileInfo? file)
    {
        if (tabViewModel.Model?.Image is null || tabViewModel.Model.PixelHeight is 0 ||
            tabViewModel.Model.PixelWidth is 0)
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
        tabViewModel.FileInfo.Value = file;
        tabViewModel.UpdateTabTitle();

        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        core.Effects?.ProcessedImage = new MagickImage(file);
    }
    
    public static void UpdateTabSideBySideTitles(TabViewModel tabViewModel,
        int index,
        int secondaryIndex,
        FileInfo firstFile,
        FileInfo secondFile,
        List<FileInfo> files)
    {
        tabViewModel.FileInfo.Value = firstFile;
        tabViewModel.SecondaryFileInfo.Value = secondFile;
        var firstInfo = new ImageTitleInfo(tabViewModel.Model.PixelWidth,
            tabViewModel.Model.PixelHeight,
            index,
            firstFile,
            100);
        var secondInfo = new ImageTitleInfo(tabViewModel.SecondaryModel.PixelWidth,
            tabViewModel.SecondaryModel.PixelHeight,
            secondaryIndex,
            secondFile,
            100);
        var titles = ImageTitleFormatter.GenerateTitleForSideBySide(firstInfo,
            secondInfo,
            index,
            secondaryIndex,
            files);
        tabViewModel.WindowTitle.Value = titles.TitleWithAppName;
        tabViewModel.Title.Value = titles.BaseTitle;
        tabViewModel.TitleTooltip.Value = titles.FilePathTitle;
    }

    public static void ChangeImage(TabViewModel tabViewModel, MainWindowViewModel vm)
    {
        tabViewModel.Image.Value = tabViewModel.Model.Image;
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
            if (tabViewModel.SecondaryModel is null)
            {
#if DEBUG
                DebugHelper.LogDebug(nameof(UpdateImage2),
                    nameof(ChangeImage),
                    "SecondaryModel.CurrentValue is null");
#endif
                secondaryWidth = 0;
                secondaryHeight = 0;
                tabViewModel.SecondaryImage.Value = null;
            }
            else
            {
                secondaryWidth = tabViewModel.SecondaryModel.PixelWidth;
                secondaryHeight = tabViewModel.SecondaryModel.PixelHeight;
                tabViewModel.SecondaryImage.Value = tabViewModel.SecondaryModel.Image;
            }
        }
        else
        {
            secondaryWidth = secondaryHeight = 0;
        }
        
        if (Settings.WindowProperties.AutoFit)
        {
            WindowResizing2.SetSize(tabViewModel.Model.PixelWidth,
                tabViewModel.Model.PixelHeight, 
                secondaryWidth, secondaryHeight,
                WindowResizeReason.Application,
                vm);
        }

        // Update tiff title if appropriate (there are no file changes in this instance
        if (tabViewModel.Model?.TiffNavigation is null)
        {
            return;
        }
        // Update title to reflect tiff navigation changes
        tabViewModel.UpdateTabTitle();
    }
}