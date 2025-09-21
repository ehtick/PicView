using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;

namespace PicView.Avalonia.Win32.Views;

public partial class WinTitleBar : UserControl
{
    public WinTitleBar()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (Settings.Theme.GlassTheme)
            {
                TopWindowBorder.Background = Brushes.Transparent;
                TopWindowBorder.BorderThickness = new Thickness(0);

                LogoBorder.Background = Brushes.Transparent;
                LogoBorder.BorderThickness = new Thickness(0);

                LogoBorder.Background = Brushes.Transparent;
                LogoBorder.BorderThickness = new Thickness(0);

                EditableTitlebar.Background = Brushes.Transparent;
                EditableTitlebar.BorderThickness = new Thickness(0);

                CloseButton.Background = Brushes.Transparent;
                CloseButton.BorderThickness = new Thickness(0);

                MinimizeButton.Background = Brushes.Transparent;
                MinimizeButton.BorderThickness = new Thickness(0);

                RestoreButton.Background = Brushes.Transparent;
                RestoreButton.BorderThickness = new Thickness(0);

                FullscreenButton.Background = Brushes.Transparent;
                FullscreenButton.BorderThickness = new Thickness(0);

                GalleryButton.Background = Brushes.Transparent;
                GalleryButton.BorderThickness = new Thickness(0);

                MenuButton.Background = Brushes.Transparent;
                MenuButton.BorderThickness = new Thickness(0);
                
                var brush = UIHelper.GetBrush("SecondaryTextColor");

                EditableTitlebar.Foreground = brush;
                CloseButton.Foreground = brush;
                MinimizeButton.Foreground = brush;
                RestoreButton.Foreground = brush;
                GalleryButton.Foreground = brush;
                MenuButton.Foreground = brush;
            }

            PointerPressed += (_, e) => MoveWindow(e);
            PointerExited += (_, _) => { DragAndDropHelper.RemoveDragDropView(); };

            MenuButton.Click += (_, _) => { ToggleMenu(); };
            MainMenu.Closed += (_, _) => { CloseMenu(); };
        };
    }

    private void ToggleMenu()
    {
        if (MainMenu.IsOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    private void OpenMenu()
    {
        MainMenu.IsVisible = true;
        MainMenu.Open();
        EditableTitlebar.IsVisible = false;
        GalleryButton.IsVisible = false;
        MenuButton.IsVisible = false;

        FileMenuItem.Open();
    }

    private void CloseMenu()
    {
        MainMenu.Close();
        MainMenu.IsVisible = false;
        EditableTitlebar.IsVisible = true;
        GalleryButton.IsVisible = true;
        MenuButton.IsVisible = true;
    }
    

    private void MoveWindow(PointerPressedEventArgs e)
    {
        if (VisualRoot is null || DataContext is not MainViewModel vm)
        {
            return;
        }

        if (vm.MainWindow.IsEditableTitlebarOpen.Value || MainMenu.IsOpen)
        {
            return;
        }

        WindowFunctions.WindowDragAndDoubleClickBehavior((Window)VisualRoot, e, vm.PlatformWindowService);
    }
}