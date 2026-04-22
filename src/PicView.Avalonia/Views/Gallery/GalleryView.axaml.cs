using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ViewModels;

namespace PicView.Avalonia.Views.Gallery;

public partial class GalleryAnimationControlView : GalleryAnimationControl
{
    public GalleryAnimationControlView()
    {
        InitializeComponent();
    }

    private void Flyout_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control ctl)
        {
            return;
        }

        FlyoutBase.ShowAttachedFlyout(ctl);
    }

    private void MenuBase_OnOpened(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }
        //GalleryStretchMode.DetermineStretchMode(vm);
    }
}