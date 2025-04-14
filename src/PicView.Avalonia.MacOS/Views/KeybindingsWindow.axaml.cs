using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Core.Localization;

namespace PicView.Avalonia.MacOS.Views;

public partial class KeybindingsWindow : Window
{
    public KeybindingsWindow()
    {
        MaxHeight = ScreenHelper.ScreenSize.WorkingAreaHeight;
        InitializeComponent();
        if (!Settings.Theme.Dark || Settings.Theme.GlassTheme)
        {
            WindowBorder.Background = Brushes.Transparent;
            XKeybindingsView.Background = Brushes.Transparent;
        }
        Loaded += (sender, e) =>
        {
            MinWidth = MaxWidth = Bounds.Width;
            Title = $"{TranslationManager.Translation.ApplicationShortcuts} - PicView";
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
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }
}