using System.Collections.Specialized;
using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using ObservableCollections;
using PicView.Avalonia.CustomControls;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.Gallery;

public partial class GalleryView2 : GalleryAnimationControl
{
    public GalleryView2()
    {
        InitializeComponent();
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        core.MainWindows.ActiveWindow.CurrentValue.WindowTabs.ActiveTab.CurrentValue.Gallery.GalleryItems.CurrentValue.CollectionChanged += CurrentValueOnCollectionChanged;
         if (Settings.Gallery.IsGalleryDocked)
         {
             Height = Settings.Gallery.BottomGalleryItemSize + 2 + SizeDefaults.ScrollbarSize;
         }
         else
         {
             Height = 0;
         }
    }

    private void CurrentValueOnCollectionChanged(in NotifyCollectionChangedEventArgs<GalleryItemViewModel> e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.IsSingleItem)
                {
                    var newItem = e.NewItem;
                    Dispatcher.UIThread.Post(() =>
                    {
                        GalleryItemsControl.Items.Add(newItem);
                    },DispatcherPriority.Background);
                }
                else
                {
                    foreach (var item in e.NewItems)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            GalleryItemsControl.Items.Add(item);
                        },DispatcherPriority.Background);
                    }
                }
                break;
            // Remove, Replace, Move, Reset
            default:
                break;
        }
    }

    private void GalleryScrollViewer_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
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
}