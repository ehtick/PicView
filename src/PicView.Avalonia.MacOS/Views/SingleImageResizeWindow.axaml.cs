using Avalonia.Controls;
using Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Core.Localization;

namespace PicView.Avalonia.MacOS.Views;

public partial class SingleImageResizeWindow : Window
{
    public SingleImageResizeWindow()
    {
        InitializeComponent();
        GenericWindowHelper.GenericWindowInitialize(this, TranslationManager.Translation.ResizeImage + " - PicView");
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }
}