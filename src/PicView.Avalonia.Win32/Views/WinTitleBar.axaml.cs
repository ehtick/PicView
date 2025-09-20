using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using R3;
using Observable = R3.Observable;

namespace PicView.Avalonia.Win32.Views;

public partial class WinTitleBar : UserControl
{
    private RotationContextMenu? _rotationContextMenu;
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

                RotateRightButton.Background = Brushes.Transparent;
                RotateRightButton.BorderThickness = new Thickness(0);
                
                var brush = UIHelper.GetBrush("SecondaryTextColor");

                EditableTitlebar.Foreground = brush;
                CloseButton.Foreground = brush;
                MinimizeButton.Foreground = brush;
                RestoreButton.Foreground = brush;
                GalleryButton.Foreground = brush;
                RotateRightButton.Foreground = brush;
            }

            PointerPressed += (_, e) => MoveWindow(e);
            PointerExited += (_, _) => { DragAndDropHelper.RemoveDragDropView(); };

            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            _rotationContextMenu = new RotationContextMenu();
            _rotationContextMenu.UpdateSubscription();
            RotateRightButton.ContextMenu = _rotationContextMenu;
            
            RotateRightButton.PointerPressed += (_, e) => { OpenContextMenu(e); };
        };
    }
    
    private void OpenContextMenu(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            return;
        }

        // Context menu doesn't want to be opened normally
        _rotationContextMenu?.Open();
    }

    private void MoveWindow(PointerPressedEventArgs e)
    {
        if (VisualRoot is null || DataContext is not MainViewModel vm)
        {
            return;
        }

        if (vm.MainWindow.IsEditableTitlebarOpen.Value)
        {
            return;
        }

        WindowFunctions.WindowDragAndDoubleClickBehavior((Window)VisualRoot, e, vm.PlatformWindowService);
    }
}