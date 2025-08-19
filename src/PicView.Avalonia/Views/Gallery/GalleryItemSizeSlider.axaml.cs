using Avalonia;
using Avalonia.Controls;
using PicView.Avalonia.Gallery;
using PicView.Core.Gallery;

namespace PicView.Avalonia.Views.Gallery;

public partial class GalleryItemSizeSlider : UserControl
{
    public GalleryItemSizeSlider()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetMaxAndMin();
    }

    private void SetMaxAndMin()
    {
        if (GalleryFunctions.IsFullGalleryOpen)
        {
            CustomSlider.Maximum = GalleryDefaults.MaxFullGalleryItemHeight;
            CustomSlider.Minimum = GalleryDefaults.MinFullGalleryItemHeight;
        }
        else
        {
            CustomSlider.Maximum = GalleryDefaults.MaxBottomGalleryItemHeight;
            CustomSlider.Minimum = GalleryDefaults.MinBottomGalleryItemHeight;
        }
    }
}
