using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.MacOS.WindowImpl;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.Views.UC.Menus;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ViewModels;
using R3;
using R3.Avalonia;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacMainWindow2 : Window
{
    private readonly AvaloniaRenderingFrameProvider _frameProvider;

    public MacMainWindow2()
    {
        InitializeComponent();

        _frameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this));
        UIHelper.SetFrameProvider(_frameProvider);

        Loaded += delegate
        {
            // Keep window position when resizing
            ClientSizeProperty.Changed.ToObservable()
                .Subscribe(size =>
                {
                    if (MacOSWindow.IsChangingWindowState || WindowState != WindowState.Normal)
                    {
                        return;
                    }
                    WindowResizing.HandleWindowResize(this, size);
                });
            if (DataContext is not MainViewModel vm)
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
            });
            
            // Hide macOS buttons when interface is hidden
            Observable.EveryValueChanged(vm, x => x.MainWindow.IsTopToolbarShown.CurrentValue, _frameProvider).Subscribe(shown =>
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
            Observable.EveryValueChanged(MainTabControl.Items, x => x.Count).Subscribe(count =>
            {
                vm.Tabs.IsTabPanelVisible.Value = count > 1;
            });
            
            MainTabControl.TabDetached += MainTabControlOnTabDetached;
            MainTabControl.TabCreated += MainTabControlOnTabCreated;
            MainTabControl.SelectionChanged += MainTabControlOnSelectionChanged;

            var tabMenu = new TabMenu
            {
                Name = "TabsMenu",
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(3, 0, 3, 0),
                IsVisible = false,
                HorizontalAlignment = HorizontalAlignment.Right,
                ZIndex = 2
            };
            MainPanel.Children.Add(tabMenu);
            
            // Close tabMenu when clicking outside of it
            PointerPressed += (_, _) =>
            {
                if (!tabMenu.IsPointerOver)
                {
                    vm.MainWindow.IsTabMenuVisible.Value = false;
                }
            };
        };
    }

    private void MainTabControlOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (e.AddedItems[0] is not TabViewModel tab)
        {
            return;
        }

        vm.Tabs.SelectTab(tab);
        tab.UpdateTabTitle();
    }

    private static void MainTabControlOnTabCreated(object? sender, TabCreatedEventArgs e)
    {
        // Only set the StartUpMenu if the View is currently null.
        // This prevents overwriting the view (e.g. an image) when reordering tabs,
        // as reordering triggers the TabCreated event again by recreating containers.
        if (e.CreatedItem is TabViewModel { CurrentView.Value: null } tabViewModel)
        {
            tabViewModel.CurrentView.Value = new StartUpMenu();
        }
    }

    private void MainTabControlOnTabDetached(object? sender, TabDetachEventArgs e)
    {
        if (e.DetachedItem is not TabViewModel tab)
        {
            return;
        }
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        Task.Run(async () =>
        {
            var newVm = new MainViewModel(vm.PlatformService, vm.PlatformWindowService)
            {
                Tabs = new TabOverviewViewModel(tab)
            };

            Dispatcher.UIThread.Invoke(() =>
            {
                // Create a new window with the detached tab
                var newWindow = new MacMainWindow2
                {
                    Position = new PixelPoint(e.ScreenPosition.X - 100, e.ScreenPosition.Y - 50),
                    Width = Width,
                    Height = Height,
                    DataContext = newVm
                };

                if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                {
                    return;
                }

                StartUpHelper2.StartUpBlank(newVm, true, false, desktop, newWindow);
            }, DispatcherPriority.Send);
            
            // Need to properly remove it
            await vm.Tabs.CloseTabAsync(tab);
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