using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using PicView.Avalonia.UI;

namespace PicView.Avalonia.Views.Gallery;

public partial class GalleryItem2 : UserControl
{
    public GalleryItem2()
    {
        InitializeComponent();
        GalleryContextMenu.Opened += GalleryContextMenuOnOpened;
        GalleryContextMenu.Closed += GalleryContextMenuOnClosed;
        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
    }

    private void GalleryContextMenuOnClosed(object? sender, RoutedEventArgs e)
    {
        InnerBorder.BorderThickness = new Thickness(0);
        InnerBorder.BorderBrush = Brushes.Transparent;
    }

    private void GalleryContextMenuOnOpened(object? sender, RoutedEventArgs e)
    {
        InnerBorder.BorderThickness = new Thickness(2);
        InnerBorder.BorderBrush = UIHelper.GetSolidColorBrush("SecondaryAccentColor");
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        OuterBorder.BorderBrush = UIHelper.GetSolidColorBrush("AccentColor");
    }
    private void OnPointerExited(object? sender, PointerEventArgs e)
     {
         OuterBorder.BorderBrush = Brushes.Transparent;
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
        PointerEntered -= OnPointerEntered;
        PointerExited -= OnPointerExited;
    }
}