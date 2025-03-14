using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Core.Localization;

namespace PicView.Avalonia.MacOS.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        if (!Settings.Theme.Dark || Settings.Theme.GlassTheme)
        {
            TitleText.Background = Brushes.Transparent;
            XSettingsView.Background = Brushes.Transparent;
        }
        Loaded += delegate
        {
            MinWidth = MaxWidth = Width;
            Height = 500;
            Title = TranslationManager.Translation.Settings + " - PicView";
        };
        KeyDown += (_, e) =>
        {
            if (e.Key is Key.Escape)
            {
                Close();
            }
        };
        
        Closing += async delegate
        {
            Hide();
            await SaveSettingsAsync();
        };
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }
}