using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.CustomControls;

public class MainTitleBar : UserControl, ITitleBar
{
    public MainTitleBar()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        PointerPressed += OnPointerPressed;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        WindowDragAndDoubleClickBehavior(e);
    }

    private void WindowDragAndDoubleClickBehavior(PointerPressedEventArgs e)
    {
        if (VisualRoot is null || DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (vm.IsEditableTitlebarOpen.Value || UIHelper.GetDropDownMenu.IsOpen)
        {
            return;
        }

        if (TopLevel.GetTopLevel(this) is Window window)
        {
            WindowFunctions.WindowDragAndDoubleClickBehavior(window, e, vm.PlatformWindowService);
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Loaded -= OnLoaded;
        PointerPressed -= OnPointerPressed;
    }
}