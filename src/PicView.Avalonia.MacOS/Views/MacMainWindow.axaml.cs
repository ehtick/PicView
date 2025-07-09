using Avalonia.Controls;
using PicView.Avalonia.MacOS.WindowImpl;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using R3;
using R3.Avalonia;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacMainWindow : Window
{
    private readonly AvaloniaRenderingFrameProvider _frameProvider;

    public MacMainWindow()
    {
        InitializeComponent();

        _frameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this));
        UIHelper.SetFrameProvider(_frameProvider);

        Loaded += delegate
        {
            // Keep window position when resizing
            ClientSizeProperty.Changed.ToObservable()
                .Subscribe(size => { WindowResizing.HandleWindowResize(this, size); });
            if (DataContext is not MainViewModel vm)
            {
                return;
            }
            Observable.EveryValueChanged(this, x => x.WindowState, _frameProvider).SubscribeAwait(async (state, _) =>
            {
                switch (state)
                {
                    case WindowState.FullScreen:
                        if (!Settings.WindowProperties.Fullscreen)
                        {
                            await MacOSWindow.Fullscreen(this, vm);
                        }

                        break;
                    case WindowState.Maximized:
                        if (!Settings.WindowProperties.Maximized)
                        {
                            await MacOSWindow.Maximize(this, vm);
                        }

                        break;
                    case WindowState.Normal:
                        if (Settings.WindowProperties.Maximized || Settings.WindowProperties.Fullscreen)
                        {
                            await MacOSWindow.Restore(this, vm);
                        }
                        break;
                }
            });
            
            // Hide macOS buttons when interface is hidden
            Observable.EveryValueChanged(vm, x => x.MainWindow.IsTopToolbarShown.CurrentValue, _frameProvider).Subscribe(shown =>
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

    protected override void OnClosed(EventArgs e)
    {
        _frameProvider?.Dispose();
    }
}