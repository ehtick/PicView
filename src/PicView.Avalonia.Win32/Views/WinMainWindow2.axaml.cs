using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.UI;
using PicView.Avalonia.Win32.WindowImpl;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.IPlatform;
using PicView.Core.ViewModels;
using R3;
using R3.Avalonia;

namespace PicView.Avalonia.Win32.Views;

public partial class WinMainWindow2 : Window, IPlatformWindowService
{
    private readonly AvaloniaRenderingFrameProvider? _frameProvider;
    private static WindowInitializer? _windowInitializer;
    public readonly CompositeDisposable Disposables = new();

    public WinMainWindow2()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        var mainWindowViewModel = new MainWindowViewModel(core.Translation, this, core.GallerySettings);
        DataContext = mainWindowViewModel;
        
        // initialize RenderingFrameProvider
        _frameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this)!);
        UIHelper2.SetFrameProvider(_frameProvider);

        InitializeComponent();

        LoadedInitialization();
    }

    private void LoadedInitialization()
    {
        Loaded += delegate
        {
            _windowInitializer = new WindowInitializer();
            if (Application.Current.DataContext is not CoreViewModel || DataContext is not MainWindowViewModel windowViewModel)
            {
                return;
            }

            // Keep window position when resizing
            ClientSizeProperty.Changed.ToObservable()
                .ObserveOn(_frameProvider)
                .Subscribe(size =>
                {
                    if (Win32Window.IsChangingWindowState || WindowState != WindowState.Normal)
                    {
                        return;
                    }

                    WindowResizing2.HandleWindowResize(this, size);
                }, result =>
                {
#if DEBUG
                    if (result is { IsFailure: true, Exception: not null })
                    {
                        DebugHelper.LogDebug(nameof(WinMainWindow2), nameof(ClientSizeProperty),
                            result.Exception);
                    }
#endif
                });
            ScalingChanged += (_, _) =>
            {
                ScreenHelper.UpdateScreenSize(this);
                //WindowResizing.SetSize(windowViewModel);
            };
            PointerExited += (_, _) => { DragAndDropHelper.RemoveDragDropView(); };

            Observable.EveryValueChanged(this, x => x.WindowState, _frameProvider)
                .SubscribeAwait(async (state, _) =>
            {
                switch (state)
                {
                    case WindowState.FullScreen:
                        if (!Settings.WindowProperties.Fullscreen)
                        {
                             await Fullscreen();
                        }

                        break;
                    case WindowState.Maximized:
                        if (!Settings.WindowProperties.Maximized)
                        {
                            await Maximize();
                        }

                        break;
                    case WindowState.Normal:
                        if (Settings.WindowProperties.Fullscreen || Settings.WindowProperties.Maximized)
                        {
                            await Restore();
                        }

                        break;
                }
            }, result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(WinMainWindow2), nameof(WindowState),
                        result.Exception);
                }
#endif
            });

            UIHelper2.AddDropDownMenu();

            // Close tabMenu when clicking outside of it
            PointerPressed += (_, _) =>
            {
                if (windowViewModel.IsEditableTitlebarOpen.Value && !Titlebar.IsPointerOver)
                {
                    Titlebar.EditableTitlebar.CloseTitlebar();
                }

                if (!UIHelper2.GetDropDownMenu.IsPointerOver)
                {
                    windowViewModel.TopTitlebarViewModel.CloseDropDownMenu();
                }
            };
            UIHelper2.GetMainTabControl.TabDetached += MainTabControlOnTabDetached;
            Activated += OnActivated;
        };
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        if (Application.Current.DataContext is not CoreViewModel core || Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }
        core.MainWindows.ActiveWindow.Value = DataContext as MainWindowViewModel;
        desktop.MainWindow = this;
    }

    private void MainTabControlOnTabDetached(object? sender, TabDetachEventArgs e)
    {
        if (e.DetachedItem is not TabViewModel tab)
        {
            return;
        }

        if (DataContext is not MainWindowViewModel parentVm)
        {
            return;
        }

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        // 1. Try to find a target window under the mouse
        WinMainWindow2? targetWindow = null;

        foreach (var window in desktop.Windows)
        {
            if (window == this || window is not WinMainWindow2 macWindow)
            {
                continue;
            }

            var clientPoint = macWindow.PointToClient(e.ScreenPosition);
            if (!new Rect(0, 0, macWindow.ClientSize.Width, macWindow.ClientSize.Height).Contains(clientPoint))
            {
                continue;
            }

            targetWindow = macWindow;
            break;
        }

        // 2. If dropped on an existing window, attach the tab there
        if (targetWindow != null)
        {
            if (targetWindow.DataContext is not MainWindowViewModel targetVm)
            {
                return;
            }

            // Need to properly remove it from the previous location
            parentVm.WindowTabs.RemoveTab(tab);

            // Add to new window (if not already added by drag preview)
            if (!targetVm.WindowTabs.Tabs.Value.Contains(tab))
            {
                targetVm.WindowTabs.Tabs.Value.Add(tab);
            }

            targetVm.WindowTabs.SelectTab(tab);

            // Update context
            tab.ParentWindowContext = targetVm;

            // Refresh bindings
            if (tab.CurrentView.CurrentValue is Control control)
            {
                control.DataContext = tab;
            }

            return;
        }

        // 3. Fallback: Create a new window (Detaching behavior)
        Task.Run(() =>
        {
            MainWindowViewModel? newVm = null;
            Dispatcher.UIThread.Invoke(() =>
            {
                // Create a new window with the detached tab
                var newWindow = new WinMainWindow2
                {
                    Position = new PixelPoint(e.ScreenPosition.X - 100, e.ScreenPosition.Y - 50),
                    Width = Width,
                    Height = Height
                };
                if (Application.Current.DataContext is not CoreViewModel core)
                {
                    return;
                }
                newVm = newWindow.DataContext as MainWindowViewModel;
                core.MainWindows.MainWindows.Add(newVm);
                core.MainWindows.ActiveWindow.Value = newVm;
                StartUpHelper2.StartUpBlank(core, true, false, desktop, newWindow);

                // Fix null DataContext
                if (tab.CurrentView.CurrentValue is Control control)
                {
                    control.DataContext = tab;
                }

                desktop.MainWindow = newWindow;
            }, DispatcherPriority.Send);

            TabNavigationInitializer.InitializeDetachedWindow(parentVm, newVm, tab);
        });
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        await WindowFunctions2.WindowClosingBehavior(this);
        base.OnClosing(e);
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

        if (Settings.WindowProperties.AutoFit)
        {
            return;
        }

        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        //WindowResizing.SetSize(vm);
    }

    protected override void OnClosed(EventArgs e)
    {
        _frameProvider?.Dispose();
        Disposables.Dispose();
        base.OnClosed(e);
    }
    
    #region Window interface implementations
    
    public int CombinedTitleButtonsWidth
    {
        get => (int)(Settings.WindowProperties.Maximized && !Settings.WindowProperties.Fullscreen
            ? OffScreenMargin.Left + OffScreenMargin.Right + field : field);
        set;
    } = 185;
    
    public void ShowAboutWindow() =>
        _windowInitializer?.ShowAboutWindow();

    public async Task ShowImageInfoWindow() =>
        await _windowInitializer?.ShowImageInfoWindow(null);

    public async Task ShowKeybindingsWindow() =>
        await _windowInitializer?.ShowKeybindingsWindow(null);

    public async ValueTask ShowSettingsWindow() =>
        await _windowInitializer.ShowSettingsWindow();

    public void ShowSingleImageResizeWindow() =>
        _windowInitializer?.ShowSingleImageResizeWindow(null);

    public async Task ShowBatchResizeWindow() =>
        await _windowInitializer?.ShowBatchResizeWindow(null);

    public void ShowEffectsWindow() =>
        _windowInitializer?.ShowEffectsWindow(null);

    public void ShowConvertWindow() =>
        _windowInitializer?.ShowConvertWindow(null);

    /// <inheritdoc />
    public async Task Maximize(bool saveSetting = true) =>
        await Win32Window.Maximize(this, null, saveSetting);
    
    /// <inheritdoc />
    public async Task MaximizeRestore(bool saveSetting = true) =>
        await Win32Window.ToggleMaximize(this, null, saveSetting);

    /// <inheritdoc />
    public async Task Fullscreen(bool saveSetting = true) =>
        await Win32Window.Fullscreen(this, null, saveSetting);
    
    /// <inheritdoc />
    public async Task ToggleFullscreen(bool saveSetting = true) =>
        await Win32Window.ToggleFullscreen(this, null, saveSetting);
    
    /// <inheritdoc />
    public async Task Restore() =>
        await Win32Window.Restore(this, null);
    
    public void Minimize() =>
        Win32Window.Minimize(this);

    #endregion
}