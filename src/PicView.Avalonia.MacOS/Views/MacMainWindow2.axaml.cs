using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.MacOS.WindowImpl;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.IPlatform;
using PicView.Core.ViewModels;
using R3;
using R3.Avalonia;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacMainWindow2 : Window, IPlatformWindowService
{
    private readonly AvaloniaRenderingFrameProvider? _frameProvider;
    public readonly CompositeDisposable Disposables = new();
    private static WindowInitializer? _windowInitializer;

    public MacMainWindow2()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        var mainWindowViewModel = new MainWindowViewModel(core.Translation, this, core.GlobalSettings, core.GallerySettings);
        DataContext = mainWindowViewModel;
        InitializeComponent();

        _frameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this));
        UIHelper2.SetFrameProvider(_frameProvider);

        Loaded += delegate
        {
            _windowInitializer = new WindowInitializer();
            // Keep window position when resizing
            ClientSizeProperty.Changed.ToObservable()
                .Subscribe(size =>
                {
                    if (MacOSWindow.IsChangingWindowState || WindowState != WindowState.Normal)
                    {
                        return;
                    }

                    WindowResizing2.HandleWindowResize(this, size);
                }).AddTo(Disposables);
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            Observable.EveryValueChanged(this, x => x.WindowState, _frameProvider)
                .Skip(1)
                .SubscribeAwait(async (state, _) =>
            {
                switch (state)
                {
                    case WindowState.FullScreen:
                        if (!Settings.WindowProperties.Fullscreen)
                        {
                            //await MacOSWindow2.Fullscreen(this, vm);
                        }

                        break;
                    case WindowState.Maximized:
                        if (!Settings.WindowProperties.Maximized && !Settings.WindowProperties.Fullscreen)
                        {
                            //await MacOSWindow2.Maximize(this, vm);
                        }

                        break;
                    case WindowState.Normal:
                        if (Settings.WindowProperties.Maximized || Settings.WindowProperties.Fullscreen)
                        {
                            //await MacOSWindow2.Restore(this, vm);
                        }
                        break;
                }
            }).AddTo(Disposables);
            
            // Hide macOS buttons when interface is hidden
            Observable.EveryValueChanged(vm, x => x.IsTopToolbarShown.CurrentValue, _frameProvider).Subscribe(shown =>
            {
                if (Settings.WindowProperties.Fullscreen)
                {
                    WindowDecorations = WindowDecorations.Full;
                }
                else
                {
                    WindowDecorations = shown ? WindowDecorations.Full : WindowDecorations.None;
                }
            });
            
            UIHelper2.AddDropDownMenu();

            // Close tabMenu when clicking outside of it
            PointerPressed += (_, _) =>
            {
                if (vm.IsEditableTitlebarOpen.Value && !Titlebar.IsPointerOver)
                {
                    Titlebar.EditableTitlebar.CloseTitlebar();
                }
            };
            UIHelper2.GetMainTabControl.TabDetached += MainTabControlOnTabDetached;
            Activated += OnActivated;
        };
    }
    private void OnActivated(object? sender, EventArgs e)
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        core.MainWindows.ActiveWindow.Value = DataContext as MainWindowViewModel;
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
        MacMainWindow2? targetWindow = null;

        foreach (var window in desktop.Windows)
        {
            if (window == this || window is not MacMainWindow2 macWindow)
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
                var newWindow = new MacMainWindow2
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
            }, DispatcherPriority.Send);

            TabNavigationInitializer.InitializeDetachedWindow(parentVm, newVm, tab);
        });
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
        // var vm = (MainViewModel)DataContext;
        // WindowResizing.SetSize(vm);
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        await WindowFunctions2.WindowClosingBehavior(this);
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        _frameProvider?.Dispose();
        Disposables.Dispose();
        base.OnClosed(e);
    }
    
    #region Window interface implementations

    public int CombinedTitleButtonsWidth { get; set; } = 165;
    
    public void ShowAboutWindow() =>
        _windowInitializer?.ShowAboutWindow();

    public async Task ShowImageInfoWindow() =>
        await _windowInitializer?.ShowImageInfoWindow();

    public async Task ShowKeybindingsWindow() =>
        _windowInitializer?.ShowKeybindingsWindow();

    public async ValueTask ShowSettingsWindow()
    {
        if (_windowInitializer is null)
        {
            return;
        }
        await _windowInitializer.ShowSettingsWindow();
    }

    public void ShowSingleImageResizeWindow() =>
        _windowInitializer?.ShowSingleImageResizeWindow();

    public async Task ShowBatchResizeWindow() =>
        await _windowInitializer?.ShowBatchResizeWindow();

    public void ShowEffectsWindow() =>
        _windowInitializer?.ShowEffectsWindow();

    public void ShowConvertWindow() =>
        _windowInitializer?.ShowConvertWindow();
    
    public void ShowPrintWindow(string path)
    {
        var vm = Dispatcher.UIThread.Invoke(() => DataContext as MainWindowViewModel);
        _windowInitializer.ShowPrintPreviewWindow(path, vm);
    }

    /// <inheritdoc />
    public async Task Maximize(bool saveSetting = true) =>
        await MacOSWindow2.Maximize(this, null, saveSetting);
    
    /// <inheritdoc />
    public async Task MaximizeRestore(bool saveSetting = true) =>
        await MacOSWindow2.ToggleMaximize(this, null, saveSetting);

    /// <inheritdoc />
    public async Task Fullscreen(bool saveSetting = true) =>
        await MacOSWindow2.Fullscreen(this, null, saveSetting);
    
    /// <inheritdoc />
    public async Task ToggleFullscreen(bool saveSetting = true) =>
        await MacOSWindow2.ToggleFullscreen(this, null, saveSetting);
    
    /// <inheritdoc />
    public async Task Restore() =>
        await MacOSWindow2.Restore(this, null);
    
    public void Minimize() =>
        MacOSWindow2.Minimize(this);
    
    #endregion
}