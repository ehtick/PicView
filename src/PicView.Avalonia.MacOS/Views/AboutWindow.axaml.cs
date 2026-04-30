using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.MacOS.PlatformUpdate;
using PicView.Avalonia.UI;
using PicView.Core.IPlatform;
using PicView.Core.Update;

namespace PicView.Avalonia.MacOS.Views;

public partial class AboutWindow : Window, IPlatformSpecificUpdate
{
    public AboutWindow()
    {
        InitializeComponent();
        if (!Settings.Theme.Dark || Settings.Theme.GlassTheme)
        {
            XAboutView.Background = Brushes.Transparent;
        }
        GenericWindowHelper.AboutWindowInitialize(this);
    }
    
    public async Task HandlePlatformUpdate(UpdateInfo updateInfo, string tempPath)
    {
        await MacUpdateHelper.HandleMacOSUpdate(updateInfo, tempPath);
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }
}