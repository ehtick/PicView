using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacOSTitlebar : UserControl
{
    public MacOSTitlebar()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (!Settings.Theme.GlassTheme)
            {
                return;
            }

            TopWindowBorder.Background = Brushes.Transparent;

            EditableTitlebar.Background = Brushes.Transparent;
            EditableTitlebar.BorderThickness = new Thickness(0);

            CreateTabButton.Background = Brushes.Transparent;
            CreateTabButton.BorderThickness = new Thickness(0);;
                
            MenuButton.Background = Brushes.Transparent;
            MenuButton.BorderThickness = new Thickness(0);;
                
            var brush = UIHelper.GetBrush("SecondaryTextColor");
            EditableTitlebar.Foreground = brush;
            SearchButton.Foreground = brush;
            CreateTabButton.Foreground = brush;
            MenuButton.Foreground = brush;
        };
        PointerPressed += (_, e) => MoveWindow(e);
    }

    private void MoveWindow(PointerPressedEventArgs e)
    {
        if (VisualRoot is null || DataContext is not MainViewModel vm)
        {
            return;
        }

        var hostWindow = (Window)VisualRoot;
        WindowFunctions.WindowDragAndDoubleClickBehavior(hostWindow, e, vm.PlatformWindowService);
    }
}