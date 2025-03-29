using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.Input;
using PicView.Core.Localization;

namespace PicView.Avalonia.MacOS.Views;

public partial class ExifWindow : Window
{
    public ExifWindow()
    {
        InitializeComponent();
        if (!Settings.Theme.Dark || Settings.Theme.GlassTheme)
        {
            WindowBorder.Background = Brushes.Transparent;
            XExifView.Background = Brushes.Transparent;
        }
        Title = TranslationManager.Translation.ImageInfo + " - PicView";
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
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }
}