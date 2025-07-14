using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacOSTitlebar : UserControl
{
    public MacOSTitlebar()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (Settings.Theme.GlassTheme)
            {
                TopWindowBorder.Background = Brushes.Transparent;

                EditableTitlebar.Background = Brushes.Transparent;
                EditableTitlebar.BorderThickness = new Thickness(0);

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
                    FlipButton.Foreground = new SolidColorBrush(secondaryTextColor);
                    GalleryButton.Foreground = new SolidColorBrush(secondaryTextColor);
                    RotateRightButton.Foreground = new SolidColorBrush(secondaryTextColor);
                }
#if DEBUG
                catch (Exception e)
                {
                    DebugHelper.LogDebug(nameof(MacOSTitlebar), nameof(LoadedEvent), e);
                }
#else
                catch (Exception) { }
#endif
            }

            Observable.EveryValueChanged(this, x => x.RotationContextMenu.IsOpen, UIHelper.GetFrameProvider)
                .Subscribe(_ => { UpdateRotation(); });

            RotateRightButton.PointerPressed += (_, e) => { OpenContextMenu(e); };
            RotateRightButton.Click += (_, e) =>
            {
                if (DataContext is not MainViewModel vm)
                {
                    return;
                }

                vm.MainWindow.IsTopToolbarRotationClicked = Settings.WindowProperties.AutoFit;
            };
            FlipButton.PointerPressed += (_, e) => { OpenContextMenu(e); };
        };
        PointerPressed += (_, e) => MoveWindow(e);
    }


    private void OpenContextMenu(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            return;
        }

        // Context menu doesn't want to be opened normally
        RotationContextMenu.Open();
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

        var hostWindow = (Window)VisualRoot;
        WindowFunctions.WindowDragAndDoubleClickBehavior(hostWindow, e, vm.PlatformWindowService);
    }
}