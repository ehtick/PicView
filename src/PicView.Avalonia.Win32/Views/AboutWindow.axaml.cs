using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PicView.Avalonia.UI;
using PicView.Avalonia.Win32.PlatformUpdate;
using PicView.Core.IPlatform;
using PicView.Core.Update;

namespace PicView.Avalonia.Win32.Views;

public partial class AboutWindow : Window, IPlatformSpecificUpdate
{
    public AboutWindow()
    {
        InitializeComponent();

        GenericWindowHelper.AboutWindowInitialize(this);
    }

    public async Task HandlePlatformUpdate(UpdateInfo updateInfo, string tempPath)
    {
        await WinUpdateHelper.HandleWindowsUpdate(updateInfo, tempPath);
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void Close(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Minimize(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
}