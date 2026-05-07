using Avalonia;
using Avalonia.Controls;
using Avalonia.Svg.Skia;
using ImageMagick;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;
using PicView.Core.Titles;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Navigation;

public static class UpdateImage
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
        var count = tabViewModel.ImageIterator.Files.Count;
        var zoom = tabViewModel.ZoomLevel.CurrentValue;
        var firstInfo = new ImageTitleInfo(firstFile,
            tabViewModel.Model.PixelWidth,
            tabViewModel.Model.PixelHeight,
            index,
            count);
        var secondInfo = new ImageTitleInfo(secondFile,
            tabViewModel.SecondaryModel.PixelWidth,
            tabViewModel.SecondaryModel.PixelHeight,
            secondaryIndex,
            count);
        var titles = ImageTitleFormatter.GenerateTitleForSideBySide(firstInfo, secondInfo,
            zoom,
            files);
        tabViewModel.WindowTitle.Value = titles.TitleWithAppName;
        tabViewModel.Title.Value = titles.BaseTitle;
        tabViewModel.TitleTooltip.Value = titles.FilePathTitle;
    }

    public static void ChangeImage(TabViewModel tabViewModel, MainWindowViewModel vm)
    {
        if (tabViewModel.Model.ImageType is ImageType.Svg)
        {
            tabViewModel.Image.Value = new SvgImage { Source = tabViewModel.Model.Image as SvgSource };
        }
        else
        {
            tabViewModel.Image.Value = tabViewModel.Model.Image;
        }
        tabViewModel.ImageType.Value = tabViewModel.Model.ImageType;
        
        double secondaryWidth, secondaryHeight;
        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            if (tabViewModel.SecondaryModel is null)
            {
#if DEBUG
                DebugHelper.LogDebug(nameof(UpdateImage),
                    nameof(ChangeImage),
                    "SecondaryModel.CurrentValue is null");
#endif
                secondaryWidth = 0;
                secondaryHeight = 0;
                tabViewModel.SecondaryImage.Value = null;
                tabViewModel.SecondaryFileInfo.Value = null;
                tabViewModel.SecondaryImageType.Value = null;
            }
            else
            {
                secondaryWidth = tabViewModel.SecondaryModel.PixelWidth;
                secondaryHeight = tabViewModel.SecondaryModel.PixelHeight;                
                tabViewModel.SecondaryImage.Value = tabViewModel.SecondaryModel.Image;
                tabViewModel.SecondaryImageType.Value = tabViewModel.SecondaryModel.ImageType;
                tabViewModel.SecondaryFileInfo.Value = tabViewModel.SecondaryModel.FileInfo;
            }
        }
        else
        {
            secondaryWidth = secondaryHeight = 0;
        }
        
        if (Settings.WindowProperties.AutoFit)
        {
            WindowResizing.SetSize(tabViewModel.Model.PixelWidth,
                tabViewModel.Model.PixelHeight, 
                secondaryWidth, secondaryHeight,
                WindowResizeReason.Application,
                vm);
        }
        
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is not ImageViewer imageViewer)
        {
            return;
        }
        
        if (Settings.Zoom.ResetZoomOnChange)
        {
            imageViewer.ResetZoomSlim();
            imageViewer.Rotate(0);
        }

        if (tabViewModel.Gallery.IsDockedGalleryVisible.CurrentValue)
        {
            imageViewer.GalleryView.GalleryItemsControl.ScrollToCenterOfCurrentItem();
        }
        tabViewModel.ZoomLevel.Value = Convert.ToInt32(tabViewModel.AspectRatio.CurrentValue * 100);;
        tabViewModel.UpdateTabTitle();
    }
}