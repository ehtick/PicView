using Avalonia.Controls;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;

namespace PicView.Avalonia.Views.UC.Buttons;

public partial class GalleryShortcut : UserControl
{
    public GalleryShortcut()
    {
        InitializeComponent();
        Loaded += delegate
        {
            if (DataContext is not MainViewModel vm)
            {
                return;
            }
            _ = new HoverFadeButtonHandler(this, DataContext as MainViewModel, InnerButton);
            PointerWheelChanged += async (_, e) => await ImageViewer.PreviewOnPointerWheelChanged(this, e);
        };
    }
}
