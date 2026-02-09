using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using PicView.Avalonia.CustomControls;

namespace PicView.Avalonia.Views.Gallery;

public partial class GalleryItem2 : NavigateAbleItem
{
    public GalleryItem2()
    {
        InitializeComponent();
        GalleryContextMenu.Opened += GalleryContextMenuOnOpened;
        GalleryContextMenu.Closed += GalleryContextMenuOnClosed;
    }

    private void GalleryContextMenuOnClosed(object? sender, RoutedEventArgs e)
    {
        SetContextMenuOpen(false);
    }

    private void GalleryContextMenuOnOpened(object? sender, RoutedEventArgs e)
    {
        SetContextMenuOpen(true);
    }

    private void Flyout_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control ctl)
        {
            return;
        }

        FlyoutBase.ShowAttachedFlyout(ctl);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        GalleryContextMenu.Opened -= GalleryContextMenuOnOpened;
        GalleryContextMenu.Closed -= GalleryContextMenuOnClosed;
    }
}