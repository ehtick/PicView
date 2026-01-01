using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Win32.WindowImpl;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Localization;
using PicView.Core.ViewModels;
using R3;
using R3.Avalonia;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Win32.Views;

public partial class WinMainWindow2 : Window
{
    private readonly CompositeDisposable _disposables = new();
    private readonly AvaloniaRenderingFrameProvider _frameProvider;
    private MainWindowViewModel _mainWindowViewModel;

    public WinMainWindow2()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        _mainWindowViewModel = new MainWindowViewModel(core.Translation);
        DataContext = _mainWindowViewModel;
        
        // initialize RenderingFrameProvider
        _frameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this)!);
        UIHelper2.SetFrameProvider(_frameProvider);

        Initialization();
    }

    // public WinMainWindow2(bool mainWindowAlreadyExists)
    // {
    //     if (Application.Current.DataContext is not CoreViewModel core)
    //     {
    //         return;
    //     }
    //     _mainWindowViewModel = new MainWindowViewModel(core.Translation);
    //     DataContext = _mainWindowViewModel;
    //     
    //     if (mainWindowAlreadyExists)
    //     {
    //         // initialize RenderingFrameProvider
    //         _frameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this)!);
    //         UIHelper.SetFrameProvider(_frameProvider);
    //
    //         if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
    //         {
    //             return;
    //         }
    //         
    //         ThemeManager.DetermineTheme(Application.Current, true);
    //         StartUpHelper2.StartUpBlank(Application.Current.DataContext as CoreViewModel, true, false, desktop, this);
    //
    //         Initialization();
    //         return;
    //     }
    //
    //     var settingsExists = LoadSettings();
    //
    //     // initialize RenderingFrameProvider
    //     _frameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this)!);
    //     UIHelper.SetFrameProvider(_frameProvider);
    //
    //     Initialization();
    //     WindowInitialization(settingsExists);
    // }

    private void WindowInitialization(bool settingsExists)
    {
        TranslationManager.Init();

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        ThemeManager.DetermineTheme(Application.Current, settingsExists);
        // StartUpHelper2.StartWithArguments(_vm, settingsExists, desktop, this);
        // _windowInitializer = new WindowInitializer();
    }

    private void Initialization()
    {
        InitializeComponent();

        LoadedInitialization();
    }

    private void LoadedInitialization()
    {
        Loaded += delegate
        {
            if (Application.Current.DataContext is not CoreViewModel vm)
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

                    WindowResizing.HandleWindowResize(this, size);
                });
            ScalingChanged += (_, _) =>
            {
                ScreenHelper.UpdateScreenSize(this);
                WindowResizing.SetSize(DataContext as MainViewModel);
            };
            PointerExited += (_, _) => { DragAndDropHelper.RemoveDragDropView(); };

            Observable.EveryValueChanged(this, x => x.WindowState, _frameProvider).Subscribe(state =>
            {
                switch (state)
                {
                    case WindowState.FullScreen:
                        if (!Settings.WindowProperties.Fullscreen)
                        {
                            vm.PlatformWindowService.Fullscreen();
                        }

                        break;
                    case WindowState.Maximized:
                        if (!Settings.WindowProperties.Maximized)
                        {
                            vm.PlatformWindowService.Maximize();
                        }

                        break;
                    case WindowState.Normal:
                        if (Settings.WindowProperties.Fullscreen || Settings.WindowProperties.Maximized)
                        {
                            vm.PlatformWindowService.Restore();
                        }

                        break;
                }
            });

            UIHelper2.AddDropDownMenu();

            // Close tabMenu when clicking outside of it
            PointerPressed += (_, _) =>
            {
                if (vm.MainWindows.ActiveWindow.CurrentValue.IsEditableTitlebarOpen.Value && !Titlebar.IsPointerOver)
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

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        await WindowFunctions.WindowClosingBehavior(this);
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

        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        WindowResizing.SetSize(vm);
    }

    protected override void OnClosed(EventArgs e)
    {
        _frameProvider?.Dispose();
        _disposables.Dispose();
        base.OnClosed(e);
    }
}