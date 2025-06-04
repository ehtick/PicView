using Avalonia.Controls;
using Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Localization;

namespace PicView.Avalonia.MacOS.Views;

public partial class BatchResizeWindow : Window
{
    public BatchResizeWindow()
    {
        InitializeComponent();
        GenericWindowHelper.GenericWindowInitialize(this, TranslationManager.Translation.BatchResize + " - PicView");
        Loaded += delegate
        {
            ClientSizeProperty.Changed.Subscribe(size =>
            {
                Height = 500;
                WindowResizing.HandleWindowResize(this, size);
            });
        };
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }
}