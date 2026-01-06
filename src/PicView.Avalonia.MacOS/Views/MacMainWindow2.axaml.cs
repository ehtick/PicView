using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.MacOS.WindowImpl;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.IPlatform;
using PicView.Core.ViewModels;
using R3;
using R3.Avalonia;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacMainWindow2 : Window, IPlatformWindowService
{
    private readonly AvaloniaRenderingFrameProvider? _frameProvider;
    private readonly CompositeDisposable _disposables = new();
    private static WindowInitializer? _windowInitializer;

    public MacMainWindow2()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        var mainWindowViewModel = new MainWindowViewModel(core.Translation, this);
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

                    WindowResizing.HandleWindowResize(this, size);
                }).AddTo(_disposables);
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
            }).AddTo(_disposables);
            
            // Hide macOS buttons when interface is hidden
            Observable.EveryValueChanged(vm, x => x.IsTopToolbarShown.CurrentValue, _frameProvider).Subscribe(shown =>
            {
                if (Settings.WindowProperties.Fullscreen)
                {
                    SystemDecorations = SystemDecorations.Full;
                }
                else
                {
                    SystemDecorations = shown ? SystemDecorations.Full : SystemDecorations.None;
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
            if (targetWindow.DataContext is not MainViewModel targetVm)
            {
                return;
            }

            // Need to properly remove it from the previous location
            parentVm.WindowTabs.RemoveTab(tab);

            // Add to new window (if not already added by drag preview)
            if (!targetVm.Tabs.Tabs.Value.Contains(tab))
            {
                targetVm.Tabs.Tabs.Value.Add(tab);
            }

            targetVm.Tabs.SelectTab(tab);

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

            newVm.WindowTabs.Tabs.Value[0] = tab;

            // Initialize the NEW window's tabs with the OLD window's services
            // This ensures both windows share the same memory cache
            if (parentVm.WindowTabs.SharedCache is not { } cache ||
                parentVm.WindowTabs.SharedNavigation is not { } nav ||
                parentVm.WindowTabs.SharedThumbnailLoader is not { } thumb ||
                parentVm.WindowTabs.SharedGallery is not { } gallery ||
                parentVm.WindowTabs.SharedFileWatcher is not { } fileWatcher)
            {
                return;
            }

            if (newVm.WindowTabs.ActiveTab.CurrentValue.ImageIterator?.Files?.Count > 0)
            {
                newVm.WindowTabs.LoadAndInitializeFromPath(newVm.WindowTabs.ActiveTab.CurrentValue.ImageIterator.Files, gallery,
                    nav,
                    cache, thumb, fileWatcher);
            }
            else
            {
                newVm.WindowTabs.LoadAndInitialize(gallery, nav, cache, thumb, fileWatcher);
            }

            // Need to properly remove it from the previous location
            parentVm.WindowTabs.RemoveTab(tab);
            parentVm.WindowTabs.IsTabPanelVisible.Value = parentVm.WindowTabs.Tabs.CurrentValue.Count > 1;
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
        await WindowFunctions.WindowClosingBehavior(this);
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        _frameProvider?.Dispose();
        _disposables.Dispose();
        base.OnClosed(e);
    }
    
    #region Window interface implementations

    public int CombinedTitleButtonsWidth { get; set; } = 165;
    
    public void ShowAboutWindow() =>
        _windowInitializer?.ShowAboutWindow(null);

    public async Task ShowImageInfoWindow() =>
        await _windowInitializer?.ShowImageInfoWindow(null);

    public async Task ShowKeybindingsWindow() =>
        _windowInitializer?.ShowKeybindingsWindow(null);

    public async Task ShowSettingsWindow() =>
        await _windowInitializer?.ShowSettingsWindow(null);

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