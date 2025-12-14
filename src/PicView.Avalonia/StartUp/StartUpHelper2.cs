using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.Functions;
using PicView.Avalonia.Input;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.SettingsManagement;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileAssociations;
using PicView.Core.FileHistory;
using PicView.Core.ProcessHandling;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.StartUp;

public static class StartUpHelper2
{
    public static void StartWithArguments(MainViewModel vm, bool settingsExists,
        IClassicDesktopStyleApplicationLifetime desktop,
        Window window)
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            var arg = args[1];
            if (arg.StartsWith("associate:", StringComparison.OrdinalIgnoreCase))
            {
                // Set file associations and exit
                Task.Run(async () =>
                {
                    try
                    {
                        vm.PlatformService.InitiateFileAssociationService();
                        Debug.WriteLine($"Processing file association argument: {arg}");
                        await FileAssociationProcessor.ProcessFileAssociationArguments(arg);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in file association processing: {ex.Message}");
                    }
                    finally
                    {
                        // Always exit the elevated process after processing associations
                        Environment.Exit(0);
                    }
                });
            }
            else if (arg.Equals("blank:", StringComparison.OrdinalIgnoreCase))
            {
                BlankStartUp();
            }
            else if (Settings.UIProperties.OpenInSameWindow && ProcessHelper.CheckIfAnotherInstanceIsRunning())
            {
                IPC.SendWithArgs(args);
            }
            else
            {
                ImageStartUp(arg);
            }
        }
        else
        {
            RegularStartUp(vm, settingsExists, desktop, window);
        }
            
        return;
        
        void ImageStartUp(string filePath)
        {
            SettingsUpdater.InitializeSettings(vm);

            HandleWindowScalingMode(vm, window);

            HandleStartImage(vm, window, filePath);

            HandlePostWindowUpdates(vm, settingsExists, desktop, window);
        }

        void BlankStartUp()
        {
            SettingsUpdater.InitializeSettings(vm);

            HandleWindowScalingMode(vm, window);

            HandlePostWindowUpdates(vm, settingsExists, desktop, window);
        }
    }
    
    public static void StartUpBlank(MainViewModel vm, bool settingsExists, bool setPos,
        IClassicDesktopStyleApplicationLifetime desktop, Window window)
    {
        SettingsUpdater.InitializeSettings(vm);
        
        HandleWindowScalingMode(vm, window, setPos);

        window.Show();

        Dispatcher.UIThread.Post(() =>
        {
            HandlePostWindowUpdates(vm, settingsExists, desktop, window);
        }, DispatcherPriority.Background);
    }
    
    public static void RegularStartUp(MainViewModel vm, bool settingsExists,
        IClassicDesktopStyleApplicationLifetime desktop,
        Window window)
    {
        SettingsUpdater.InitializeSettings(vm);

        HandleWindowScalingMode(vm, window);

        StartUpMenuOrLastFile(vm, window);

        HandlePostWindowUpdates(vm, settingsExists, desktop, window);
    }
    
    

    private static void HandleWindowScalingMode(MainViewModel vm, Window window, bool setPos = true)
    {
        ScreenHelper.UpdateScreenSize(window);

        if (Settings.WindowProperties.Margin < 0)
        {
            Settings.WindowProperties.Margin = 45;
        }

        if (Settings.WindowProperties.AutoFit)
        {
            HandleAutoFit(vm, window);
        }
        else 
        {
            HandleNormalWindow(vm, window, setPos);
        }
    }

    private static void HandlePostWindowUpdates(MainViewModel vm, bool settingsExists,
        IClassicDesktopStyleApplicationLifetime desktop, Window window)
    {
        SetMemorySettings();

        Task.Run(() => LanguageUpdater.UpdateLanguageAsync(vm.Translation, vm.PicViewer, settingsExists));
        if (settingsExists)
        {
            Task.Run(() => KeybindingManager.LoadKeybindings(vm.PlatformService));
        }
        else
        {
            Task.Run(() =>
            {
                KeybindingManager.SetDefaultKeybindings(vm.PlatformService);
            });
        }

        SetWindowEventHandlers(window);
        HandleThemeUpdates(vm);

        UIHelper.SetControls(desktop);
        Task.Run(() =>
        {
            vm.Tabs ??= new TabOverviewViewModel();
            _ = FileHistoryManager.InitializeAsync();
            HandleWindowControlSettings(vm, desktop);
            SettingsUpdater.ValidateGallerySettings(vm, settingsExists);

            vm.MainWindow.LayoutButtonSubscription(vm);
            vm.Gallery.GalleryItemSizeUpdateSubscription(vm);
        });

        if (!Settings.WindowProperties.AutoFit)
        {
            // Need to update the screen size after the window is shown,
            // to avoid rendering error when switching between auto-fit
            ScreenHelper.UpdateScreenSize(window);
        }

        // Need to delay setting fullscreen or maximized until after the window is shown to select the correct monitor
        if (Settings.WindowProperties.Maximized && !Settings.WindowProperties.Fullscreen)
        {
            Dispatcher.UIThread
                .InvokeAsync(() => { vm.PlatformWindowService.Maximize(false); }, DispatcherPriority.Background);
        }
        else if (Settings.WindowProperties.Fullscreen)
        {
            Dispatcher.UIThread.InvokeAsync(() => { vm.PlatformWindowService.Fullscreen(false); },
                DispatcherPriority.Background);
        }

        if (Settings.UIProperties.ShowHoverNavigationBar)
        {
            UIHelper.AddHoverBar(vm);
        }
        
        TooltipHelper.StartTooltipSubscription(vm);
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Windows needs a named pipe server to open files in the same window
            if (Settings.UIProperties.OpenInSameWindow && !ProcessHelper.CheckIfAnotherInstanceIsRunning())
            {
                _ = IPC.StartListeningForArguments(vm);
            }
        }
        
        Application.Current.Name = "PicView";
    }

    private static void SetMemorySettings()
    {
        ResourceLimits.LimitMemory(new Percentage(80));
        GCSettings.LatencyMode = GCLatencyMode.LowLatency;
    }

    private static void HandleThemeUpdates(MainViewModel vm)
    {
        if (Settings.Theme.GlassTheme)
        {
            GlassThemeHelper.GlassThemeUpdates();
        }

        BackgroundManager.SetBackground(vm);
        ColorManager.UpdateAccentColors(Settings.Theme.ColorTheme);
    }

    private static void HandleWindowControlSettings(MainViewModel vm, IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (Settings.Zoom.ScrollEnabled)
        {
            SettingsUpdater.TurnOnScroll(vm);
        }
        else
        {
            vm.MainWindow.ToggleScrollBarVisibility.Value = ScrollBarVisibility.Disabled;
            vm.GlobalSettings.IsScrollingEnabled.Value = false;
        }

        if (Settings.WindowProperties.TopMost)
        {
            Dispatcher.UIThread.Invoke(() => { desktop.MainWindow.Topmost = true; });
        }
    }

    private static void HandleStartImage(MainViewModel vm, Window window, string arg)
    {
        Task.Run(() => QuickLoad2.QuickLoadAsync(vm, arg, window, false));
    }

    private static void StartUpMenuOrLastFile(MainViewModel vm, Window window)
    {
        if (Settings.StartUp.OpenLastFile)
        {
            if (string.IsNullOrWhiteSpace(Settings.StartUp.LastFile))
            {
                ShowStartUpMenu();
            }
            else
            {
                Task.Run(() => QuickLoad2.QuickLoadAsync(vm, Settings.StartUp.LastFile, window, true));
            }
        }
        else
        {
            ShowStartUpMenu();
        }

        return;

        void ShowStartUpMenu()
        {
            window.Show();
            vm.Tabs.ActiveTab.Value.CurrentView.Value = new StartUpMenu();
            TabNavigationInitializer.Initialize(vm);
        }
    }

    private static void HandleNormalWindow(MainViewModel vm, Window window, bool setPos)
    {
        vm.MainWindow.CanResize.Value = true;
        vm.GlobalSettings.IsAutoFit.Value = false;
        if (Settings.UIProperties.ShowInterface)
        {
            vm.MainWindow.IsTopToolbarShown.Value = true;
            vm.MainWindow.IsBottomToolbarShown.Value = Settings.UIProperties.ShowBottomNavBar;
        }

        if (setPos)
        {
            WindowFunctions.InitializeWindowSizeAndPosition(window);
        }
    }

    private static void HandleAutoFit(MainViewModel vm, Window window)
    {
        vm.MainWindow.SizeToContent.Value = SizeToContent.WidthAndHeight;
        vm.MainWindow.CanResize.Value = false;
        vm.GlobalSettings.IsAutoFit.Value = true;
        if (Settings.UIProperties.ShowInterface)
        {
            vm.MainWindow.IsTopToolbarShown.Value = true;
            vm.MainWindow.IsBottomToolbarShown.Value = Settings.UIProperties.ShowBottomNavBar;
        }

        if (Settings.WindowProperties.Fullscreen || Settings.WindowProperties.Maximized)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
        }
        else
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        
    }

    private static void SetWindowEventHandlers(Window w)
    {
        // Using AddHandler fixes the first keydown event not firing properly
        w.AddHandler(InputElement.KeyDownEvent, MainWindow_KeysDownAsync, RoutingStrategies.Tunnel);
        w.AddHandler(InputElement.KeyUpEvent, MainWindow_KeyUp, RoutingStrategies.Tunnel);
        w.PointerPressed += async (_, e) => await MouseShortcuts.MainWindow_PointerPressed(e).ConfigureAwait(false);

        w.Deactivated += delegate
        {
            MainKeyboardShortcuts.Reset();
            MainKeyboardShortcuts.ClearKeyDownModifiers();
        };
    }

    private static async Task MainWindow_KeysDownAsync(object? sender, KeyEventArgs e)
    {
        await MainKeyboardShortcuts.MainWindow_KeysDownAsync(e).ConfigureAwait(false);
    }

    private static void MainWindow_KeyUp(object? sender, KeyEventArgs e)
    {
        MainKeyboardShortcuts.MainWindow_KeysUp(e);
    }
}