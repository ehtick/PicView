using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PicView.Avalonia.Input;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Localization;

namespace PicView.Avalonia.Win32.Views;

public partial class BatchResizeWindow : Window
{
    public BatchResizeWindow()
    {
        InitializeComponent();
        StartUp();
    }

    private void StartUp()
    {
        if (Settings.Theme.GlassTheme)
        {
            IconBorder.Background = Brushes.Transparent;
            IconBorder.BorderThickness = new Thickness(0);
            MinimizeButton.Background = Brushes.Transparent;
            MinimizeButton.BorderThickness = new Thickness(0);
            CloseButton.Background = Brushes.Transparent;
            CloseButton.BorderThickness = new Thickness(0);
            BorderRectangle.Height = 0;
            TitleText.Background = Brushes.Transparent;

            if (!Application.Current.TryGetResource("SecondaryTextColor",
                    Application.Current.RequestedThemeVariant, out var textColor))
            {
                return;
            }

            if (textColor is not Color color)
            {
                return;
            }

            TitleText.Foreground = new SolidColorBrush(color);
            MinimizeButton.Foreground = new SolidColorBrush(color);
            CloseButton.Foreground = new SolidColorBrush(color);
        }

        Loaded += delegate
        {
            MinWidth = MaxWidth = Width;
            Title = $"{TranslationManager.Translation.BatchResize}  - PicView";
            
            // Keep window position when resizing
            ClientSizeProperty.Changed.Subscribe(size =>
            {
                WindowResizing.HandleWindowResize(this, size);
            });
        };
        KeyDown += (_, e) =>
        {
            if (e.Key is Key.Escape)
            {
                e.Handled = true;
                MainKeyboardShortcuts.IsEscKeyEnabled = false;
                Close();
            }
        };
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is null)
        {
            return;
        }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }

    private void Close(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Minimize(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
}