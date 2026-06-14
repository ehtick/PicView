using Avalonia;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.Views.UC;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Gallery;

public static class GalleryHelper
{
    public static (double width, double height) GetGallerySize(MainWindowViewModel main)
    {
        if (!Settings.Gallery.IsGalleryDocked || Slideshow.IsRunning || 
            !Settings.Gallery.ShowDockedGalleryInHiddenUI && !main.IsUIShown.CurrentValue)
        {
            return (0, 0);
        }

        Rect galleryBounds;
        if (main.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer imageViewer)
        {
            galleryBounds = imageViewer.GalleryView.Bounds;
        }
        else
        {
            return (0, 0);
        }

        if (main.WindowTabs.ActiveTab.CurrentValue.Gallery.IsLeftDocked.CurrentValue || 
            main.WindowTabs.ActiveTab.CurrentValue.Gallery.IsRightDocked.CurrentValue)
        {
            return (galleryBounds.Width, 0);
        }

        return (0, galleryBounds.Height);
    }

    public static void CenterGallery(MainWindowViewModel main)
    {
        if (main.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is not ImageViewer imageViewer)
        {
            return;
        }

        imageViewer.GalleryView.GalleryItemsControl.ScrollToCenterOfCurrentItem();
    }
}