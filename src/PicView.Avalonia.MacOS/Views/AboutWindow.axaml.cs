using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.MacOS.PlatformUpdate;
using PicView.Avalonia.UI;
using PicView.Avalonia.Update;
using PicView.Avalonia.ViewModels;

namespace PicView.Avalonia.MacOS.Views;

public partial class AboutWindow : Window, IPlatformSpecificUpdate
{
    public AboutWindow()
    {
        var vm = UIHelper.GetMainView.DataContext as MainViewModel;
        vm.AboutView ??= new AboutViewModel(this);
        InitializeComponent();
        if (!Settings.Theme.Dark || Settings.Theme.GlassTheme)
        {
            XAboutView.Background = Brushes.Transparent;
        }
        GenericWindowHelper.AboutWindowInitialize(this);
    }
    
    public async Task HandlePlatofrmUpdate(UpdateInfo updateInfo, string tempPath)
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