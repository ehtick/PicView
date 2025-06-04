using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.UI;

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
        GenericWindowHelper.KeybindingsWindowInitialize(this);
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }
}