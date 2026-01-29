using Avalonia.Input;
using PicView.Avalonia.CustomControls;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.Gallery;

public partial class GalleryView2 : GalleryAnimationControl
{
    public GalleryView2()
    {
        InitializeComponent();
    }

    private void GalleryScrollViewer_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is not TabViewModel vm)
        {
            return;
        }

        var gallery = vm.Gallery;
        var isHorizontal = (gallery.IsTopDocked.Value || gallery.IsBottomDocked.Value) && !gallery.IsGalleryExpanded.Value;

        if (isHorizontal)
        {
            if (Settings.Zoom.HorizontalReverseScroll)
            {
                if (e.Delta.Y < 0)
                {
                    GalleryScrollViewer.LineRight();
                }
                else
                {
                    GalleryScrollViewer.LineLeft();
                }
            }
            else
            {
                if (e.Delta.Y > 0)
                {
                    GalleryScrollViewer.LineRight();
                }
                else
                {
                    GalleryScrollViewer.LineLeft();
                }
            }
        }
        else
        {
            if (Settings.Zoom.HorizontalReverseScroll)
            {
                if (e.Delta.Y < 0)
                {
                    GalleryScrollViewer.LineUp();
                }
                else
                {
                    GalleryScrollViewer.LineDown();
                }
                e.Handled = true;
            }
        }
    }
}