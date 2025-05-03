using Avalonia.Controls;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using ReactiveUI;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacMainWindow : Window
{
    public MacMainWindow()
    {
        InitializeComponent();

        Loaded += delegate
        {
            // Keep window position when resizing
            ClientSizeProperty.Changed.Subscribe(size =>
            {
                WindowResizing.HandleWindowResize(this, size);
            });
            if (DataContext is not MainViewModel vm)
            {
                return;
            }
            this.WhenAnyValue(x => x.WindowState).Subscribe(state =>
            {
                switch (state)
                {
                    case WindowState.FullScreen:
                    case WindowState.Maximized:
                        Settings.WindowProperties.Fullscreen = true;
                        vm.IsFullscreen = true;
                        break;
                    case WindowState.Normal:
                        Settings.WindowProperties.Fullscreen = false;
                        vm.IsFullscreen = false;
                        break;
                }
            });
            // Hide macOS buttons when interface is hidden
            vm.WhenAnyValue(x => x.IsTopToolbarShown).Subscribe(shown =>
            {
                SystemDecorations = shown ? SystemDecorations.Full : SystemDecorations.None;
            });
        };
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext == null)
        {
            return;
        }

        if (e is { HeightChanged: false, WidthChanged: false })
        {
            return;
        }
        var vm = (MainViewModel)DataContext;
        WindowResizing.SetSize(vm);
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;
        await WindowFunctions.WindowClosingBehavior(this);
        base.OnClosing(e);
    }
}