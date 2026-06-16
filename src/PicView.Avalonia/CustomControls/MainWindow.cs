using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.UI;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;
using R3;
using R3.Avalonia;

namespace PicView.Avalonia.CustomControls;

public class MainWindow : Window, IMainWindow
{
    public CompositeDisposable Disposables { get; set; } = new();
    /// Flag to prevent window state changes while resizing
    public bool IsChangingWindowState { get; set; }
    public BottomBar? SharedBottomBar { get; set; }
    public MainTitleBar? SharedTitleBar { get; set; }
    public AvaloniaRenderingFrameProvider? FrameProvider { get; set; }

    protected MainWindow()
    {
        FrameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this)!);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Resized += WindowSizeChanged;
        
        // Set initial layout size and visibility, so that the UI is responsive at first startup also
        SetLayoutSizeAndVisibility(Bounds.Width);
        
        // Keep window position when resizing
        Debug.Assert(FrameProvider != null, nameof(FrameProvider) + " != null");
        ClientSizeProperty.Changed.ToObservable()
            .SubscribeOn(FrameProvider)
            .Subscribe(HandleWindowResize, DebugHelper.LogError(nameof(MainWindow), nameof(HandleWindowResize)))
            .AddTo(Disposables);
            
        UIHelper.AddDropDownMenu();
        Activated += OnActivated;
        
        ScalingChanged += OnScalingChanged;
        
        PointerExited += (_, _) => { DragAndDropManager.RemoveDragDropView(); };
        
        Deactivated += OnDeactivated;
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        core.SharedCache.ForceDisposalQueue();
    }

    private void OnScalingChanged(object? sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel windowViewModel)
        {
            return;
        }
        ScreenHelper.UpdateScreenSize(this);
        WindowResizing.SetSize(windowViewModel, WindowResizeReason.DpiChange);
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        if (Application.Current!.DataContext is not CoreViewModel core || Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }
        core.MainWindows.ActiveWindow.Value = DataContext as MainWindowViewModel;
        desktop.MainWindow = this;
    }
    
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        try
        {
            await WindowFunctions.WindowClosingBehavior(this);
            base.OnClosing(e);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(MainWindow), nameof(OnClosing), ex);
            Environment.Exit(1);
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        FrameProvider?.Dispose();
        Resized -= WindowSizeChanged;
        Disposables?.Dispose();
        Loaded -= OnLoaded;
        Activated -= OnActivated;
        Deactivated -= OnDeactivated;
        SharedBottomBar?.Dispose();
        base.OnClosed(e);
    }

    #region Sizing

    private void SetLayoutSizeAndVisibility(double width)
    {
        SharedBottomBar.ResponsiveNavigationBtnSize();
        SharedTitleBar.SharedDropDownMenuButton.IsVisible = width > SizeDefaults.MainTitleDropDownBtnBp;
        SharedTitleBar.SharedSearchButton.IsVisible = width > SizeDefaults.MainTitleSearchBtnBp;
    }
    
    // Window has been resized
    private void WindowSizeChanged(object? sender, WindowResizedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm || SharedBottomBar is null || SharedTitleBar is null)
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
            Dispatcher.CurrentDispatcher.Post(() => WindowFunctions.SetManualWindow(vm, this));
        }

        WindowResizing.SetSize(vm, e.Reason);
        SetLayoutSizeAndVisibility(e.ClientSize.Width);
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
        WindowResizing.HandleWindowResize(this, size);
    }
    
    #endregion
}