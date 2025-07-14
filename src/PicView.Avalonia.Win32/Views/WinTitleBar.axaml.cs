using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using R3;
using Observable = R3.Observable;

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

                FlipButton.Background = Brushes.Transparent;
                FlipButton.BorderThickness = new Thickness(0);

                GalleryButton.Background = Brushes.Transparent;
                GalleryButton.BorderThickness = new Thickness(0);

                RotateRightButton.Background = Brushes.Transparent;
                RotateRightButton.BorderThickness = new Thickness(0);

                if (!Application.Current.TryGetResource("SecondaryTextColor", Application.Current.RequestedThemeVariant,
                        out var color))
                {
                    return;
                }

                if (color is not Color secondaryTextColor)
                {
                    return;
                }

                try
                {
                    EditableTitlebar.Foreground = new SolidColorBrush(secondaryTextColor);
                    CloseButton.Foreground = new SolidColorBrush(secondaryTextColor);
                    MinimizeButton.Foreground = new SolidColorBrush(secondaryTextColor);
                    RestoreButton.Foreground = new SolidColorBrush(secondaryTextColor);
                    FlipButton.Foreground = new SolidColorBrush(secondaryTextColor);
                    GalleryButton.Foreground = new SolidColorBrush(secondaryTextColor);
                    RotateRightButton.Foreground = new SolidColorBrush(secondaryTextColor);
                }
#if DEBUG
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
#else
                catch (Exception) { }
#endif
            }

            PointerPressed += (_, e) => MoveWindow(e);
            PointerExited += (_, _) => { DragAndDropHelper.RemoveDragDropView(); };

            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            Observable.EveryValueChanged(this, x => x.RotationContextMenu.IsOpen, UIHelper.GetFrameProvider)
                .Subscribe(_ => { UpdateRotation(); });

            RotateRightButton.PointerPressed += (_, e) => { OpenContextMenu(e); };
            RotateRightButton.Click += (_, e) =>
            {
                vm.MainWindow.IsTopToolbarRotationClicked = Settings.WindowProperties.AutoFit;
            };
            FlipButton.PointerPressed += (_, e) => { OpenContextMenu(e); };
        };
    }

    private bool OpenContextMenu(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            return false;
        }

        // Context menu doesn't want to be opened normally
        RotationContextMenu.Open();
        return true;
    }

    private void UpdateRotation()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        Rotation0Item.IsChecked = false;
        Rotation90Item.IsChecked = false;
        Rotation180Item.IsChecked = false;
        Rotation270Item.IsChecked = false;
        switch (vm.GlobalSettings.RotationAngle.CurrentValue)
        {
            case 0:
                Rotation0Item.IsChecked = true;
                break;
            case 90:
                Rotation90Item.IsChecked = true;
                break;
            case 180:
                Rotation180Item.IsChecked = true;
                break;
            case 270:
                Rotation270Item.IsChecked = true;
                break;
        }
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