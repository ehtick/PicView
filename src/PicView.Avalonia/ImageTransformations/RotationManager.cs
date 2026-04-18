using Avalonia.Controls;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.ImageTransformations;

public static class RotationManager
{
    public static void ResetZoomAndRotations(MainWindowViewModel vm)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
        {
            imageViewer.ResetZoomSlim();
            imageViewer.Rotate(0);
        }
        
        if (Settings.WindowProperties.AutoFit)
        {
            WindowResizing2.SetSize(vm, WindowResizeReason.Layout);
        }
    }
    
    public static void ResetZoom(MainWindowViewModel vm)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
        {
            imageViewer.ResetZoom(Settings.Zoom.IsZoomAnimated);
        }
    }
    
    public static void Rotate(MainWindowViewModel vm, int angle)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
        {
            imageViewer.Rotate(angle);
        }
    }
    
    public static void RotateRight(MainWindowViewModel vm)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
        {
            imageViewer.Rotate(true);
        }
    }

    public static void RotateLeft(MainWindowViewModel vm)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
        {
            imageViewer.Rotate(false);
        }
    }
    
    public static void Flip(MainWindowViewModel vm)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
        {
            imageViewer.Flip(true);
        }
    }
}