using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.UI;
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
        GenericWindowHelper.GenericWindowInitialize(this, TranslationManager.Translation.ImageInfo + " - PicView");
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }
}