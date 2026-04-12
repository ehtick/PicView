using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.MacOS.WindowImpl;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.UI;
using PicView.Core.DebugTools;
using PicView.Core.IPlatform;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacMainWindow2 : MainWindow, IPlatformWindowService
{
    private static WindowInitializer? _windowInitializer;

    public MacMainWindow2()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        var mainWindowViewModel = new MainWindowViewModel(core.Translation, this, core.GlobalSettings, core.GallerySettings);
        DataContext = mainWindowViewModel;
        core.MainWindows.MainWindows.Add(mainWindowViewModel);
        core.MainWindows.ActiveWindow.Value = mainWindowViewModel;
        InitializeComponent();
        
        SharedBottomBar = BottomBar;
        SharedTitleBar = Titlebar;

        Loaded += delegate
        {
            _windowInitializer = new WindowInitializer();

            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            
            // Subscribe to window state changes to handle when user changes state outside the UI
            Observable.EveryValueChanged(this, x => x.WindowState, FrameProvider)
                .Skip(1)
                .SubscribeAwait(async (state, _) =>
            {
                switch (state)
                {
                    case WindowState.FullScreen:
                        if (!Settings.WindowProperties.Fullscreen)
                        {
                            await MacOSWindow2.Fullscreen(this, vm);
                        }

                        break;
                    case WindowState.Maximized:
                        if (!Settings.WindowProperties.Maximized && !Settings.WindowProperties.Fullscreen)
                        {
                            await MacOSWindow2.Maximize(this, vm);
                        }

                        break;
                    case WindowState.Normal:
                        if (Settings.WindowProperties.Maximized || Settings.WindowProperties.Fullscreen)
                        {
                            await MacOSWindow2.Restore(this, vm);
                        }
                        break;
                }
            }, static result =>
                {
#if DEBUG
                    if (result is { IsFailure: true, Exception: not null })
                    {
                        DebugHelper.LogDebug(nameof(MacMainWindow2), nameof(vm.IsTopToolbarShown), result.Exception);
                    }
#endif
                })
                .AddTo(Disposables);
            
            // Hide macOS traffic lights buttons when interface is hidden
            Observable.EveryValueChanged(vm, x => x.IsTopToolbarShown.CurrentValue, FrameProvider).Subscribe(shown =>
            {
                if (Settings.WindowProperties.Fullscreen)
                {
                    WindowDecorations = WindowDecorations.Full;
                }
                else
                {
                    WindowDecorations = shown ? WindowDecorations.Full : WindowDecorations.None;
                }
            }, static result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(MacMainWindow2), nameof(vm.IsTopToolbarShown), result.Exception);
                }
#endif
            })
            .AddTo(Disposables);

            // Close tabMenu when clicking outside of it
            PointerPressed += (_, _) =>
            {
                if (vm.IsEditableTitlebarOpen.Value && !Titlebar.IsPointerOver)
                {
                    Titlebar.EditableTitlebar.CloseTitlebar();
                }
                
                if (!UIHelper2.GetDropDownMenu.IsPointerOver)
                {
                    vm.TopTitlebarViewModel.CloseDropDownMenu();
                }
                
            };
            UIHelper2.GetMainTabControl.TabDetached += MainTabControlOnTabDetached;
            
        };
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
                StartUpHelper2.StartUpBlank(core, true, desktop, newWindow);


                // Fix null DataContext
                if (tab.CurrentView.CurrentValue is Control control)
                {
                    control.DataContext = tab;
                }
            }, DispatcherPriority.Send);

            TabNavigationInitializer.InitializeDetachedWindow(parentVm, newVm, tab);
        });
    }
    
    #region Window interface implementations

    public int CombinedTitleButtonsWidth { get; set; } = 165;
    
    public void ShowAboutWindow() =>
        _windowInitializer?.ShowAboutWindow();

    public async Task ShowImageInfoWindow() =>
        await _windowInitializer?.ShowImageInfoWindow(DataContext as MainWindowViewModel);

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
    
    public async Task ShowPrintWindow(string path)
    {
        var vm = Dispatcher.UIThread.Invoke(() => DataContext as MainWindowViewModel);
        await _windowInitializer.ShowPrintPreviewWindow(path, vm);
    }

    /// <inheritdoc />
    public async Task Maximize(bool saveSetting = true) =>
        await MacOSWindow2.Maximize(this, DataContext as MainWindowViewModel, saveSetting);
    
    /// <inheritdoc />
    public async Task MaximizeRestore(bool saveSetting = true) =>
        await MacOSWindow2.ToggleMaximize(this, DataContext as MainWindowViewModel, saveSetting);

    /// <inheritdoc />
    public async Task Fullscreen(bool saveSetting = true) =>
        await MacOSWindow2.Fullscreen(this, DataContext as MainWindowViewModel, saveSetting);
    
    /// <inheritdoc />
    public async Task ToggleFullscreen(bool saveSetting = true) =>
        await MacOSWindow2.ToggleFullscreen(this, DataContext as MainWindowViewModel, saveSetting);
    
    /// <inheritdoc />
    public async Task Restore() =>
        await MacOSWindow2.Restore(this, DataContext as MainWindowViewModel);
    
    public void Minimize() =>
        MacOSWindow2.Minimize(this);
    
    #endregion
}