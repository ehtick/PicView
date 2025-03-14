using Avalonia.Controls;
using Avalonia.Input;
using PicView.Core.Localization;

namespace PicView.Avalonia.MacOS.Views;

public partial class SingleImageResizeWindow : Window
{
    public SingleImageResizeWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            MinWidth = MaxWidth = Width;
            Height = 500;
            Title = TranslationManager.Translation.ResizeImage + " - PicView";
        };
        KeyDown += (_, e) =>
        {
            if (e.Key is Key.Escape)
            {
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