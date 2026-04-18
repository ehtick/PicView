using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.UI;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.ViewModels;
using R3;
using R3.Avalonia;

namespace PicView.Avalonia.CustomControls;

public class MainWindow : Window, IMainWindow
{
    public CompositeDisposable Disposables { get; set; } = new();
    /// Flag to prevent window state changes while resizing
    public bool IsChangingWindowState { get; set; }
    public BottomBar2? SharedBottomBar { get; set; }
    public UserControl? SharedTitleBar { get; set; }
    public AvaloniaRenderingFrameProvider? FrameProvider { get; set; }

    protected MainWindow()
    {
        FrameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this));
        UIHelper2.SetFrameProvider(FrameProvider);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Resized += WindowSizeChanged;
        
        // Keep window position when resizing
        ClientSizeProperty.Changed.ToObservable()
            .Subscribe(HandleWindowResize, static result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(MainWindow), nameof(HandleWindowResize), result.Exception);
                }
#endif
            })
            .AddTo(Disposables);
            
        UIHelper2.AddDropDownMenu();
        Activated += OnActivated;
    }
    
    private void OnActivated(object? sender, EventArgs e)
    {
        if (Application.Current.DataContext is not CoreViewModel core || Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }
        core.MainWindows.ActiveWindow.Value = DataContext as MainWindowViewModel;
        desktop.MainWindow = this;
    }
    
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        await WindowFunctions2.WindowClosingBehavior(this);
        base.OnClosing(e);
    }
    
    protected override void OnClosed(EventArgs e)
    {
        FrameProvider?.Dispose();
        Resized -= WindowSizeChanged;
        Disposables?.Dispose();
        Loaded -= OnLoaded;
        Activated -= OnActivated;
        SharedBottomBar.Dispose();
        base.OnClosed(e);
    }

    #region Sizing
    
    // Window has been resized
    private void WindowSizeChanged(object? sender, WindowResizedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (e.Reason is WindowResizeReason.User && !IsChangingWindowState)
        {
            if (SharedTitleBar.IsPointerOver && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Traffic light button clicked, don't change window state
                return;
            }

            if (sender is MainWindow { WindowState: WindowState.FullScreen })
            {
                // Don't reset when leaving fullscreen
                return;
            }
            // User manually resized (not maximize or restore), reset to manual window
            Dispatcher.CurrentDispatcher.Post(() => WindowFunctions2.SetManualWindow(vm, this));
            return;
        }

        if (Settings.WindowProperties.AutoFit)
        {
            return;
        }

        WindowResizing2.SetSize(vm, e.Reason);
        SharedBottomBar.ResponsiveNavigationBtnSize();
    }
    
    // Window is being resized
    private void HandleWindowResize(AvaloniaPropertyChangedEventArgs<Size> size)
    {
        if (IsChangingWindowState || WindowState != WindowState.Normal)
        {
            return;
        }

        if (size.NewValue.Value.Width == Bounds.Width && size.NewValue.Value.Height == Bounds.Height)
        {
            return;
        }
        WindowResizing2.HandleWindowResize(this, size);
    }
    
    #endregion
}