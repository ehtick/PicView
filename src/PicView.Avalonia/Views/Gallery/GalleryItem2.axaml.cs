using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
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
        InnerBorder.BorderBrush = UIHelper.GetSolidColorBrush("MainBorderColor");
        if (!GalleryContextMenu.IsOpen)
        {
            OuterBorder.BorderBrush = UIHelper.GetSolidColorBrush("MainBorderColor");
        }
    }

    private void GalleryContextMenuOnOpened(object? sender, RoutedEventArgs e)
    {
        var secondaryBrush = UIHelper.GetSolidColorBrush("SecondaryAccentColor");
        InnerBorder.BorderBrush = secondaryBrush;
        OuterBorder.BorderBrush = secondaryBrush;
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        OuterBorder.BorderBrush = UIHelper.GetSolidColorBrush("AccentColor");
    }
    private void OnPointerExited(object? sender, PointerEventArgs e)
     {
         if (!GalleryContextMenu.IsOpen)
         {
             OuterBorder.BorderBrush = UIHelper.GetSolidColorBrush("MainBorderColor");
         }
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