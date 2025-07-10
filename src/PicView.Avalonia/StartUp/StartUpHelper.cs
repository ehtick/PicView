using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.Input;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.SettingsManagement;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileAssociations;
using PicView.Core.FileHistory;
using PicView.Core.ProcessHandling;

namespace PicView.Avalonia.StartUp;

public static class StartUpHelper
{
    public static void StartWithArguments(MainViewModel vm, bool settingsExists, IClassicDesktopStyleApplicationLifetime desktop,
        Window window)
    {
        var args = Environment.GetCommandLineArgs();
        if (settingsExists)
        {
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
                else
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        if (Settings.UIProperties.OpenInSameWindow &&
                            ProcessHelper.CheckIfAnotherInstanceIsRunning())
                        {
                            HandleMultipleInstances(args);
                        }
                    }
                }
            }
        }
        
        SettingsUpdater.InitializeSettings(vm);
        
        HandleWindowScalingMode(vm, window);
        
        HandleStartUpMenuOrImage(vm, args);
        window.Show();
        
        HandlePostWindowUpdates(vm, settingsExists, desktop, window);
    }
    
    public static void StartWithoutArguments(MainViewModel vm, bool settingsExists, IClassicDesktopStyleApplicationLifetime desktop,
        Window window, string? arg = null)
    {
        SettingsUpdater.InitializeSettings(vm);
        
        HandleWindowScalingMode(vm, window);
        
        HandleStartUpMenuOrImage(vm, arg);
        window.Show();
        
        HandlePostWindowUpdates(vm, settingsExists, desktop, window);
    }

    private static void HandleWindowScalingMode(MainViewModel vm, Window window)
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
            HandleNormalWindow(vm, window);
        }
    }

    private static void HandlePostWindowUpdates(MainViewModel vm, bool settingsExists, IClassicDesktopStyleApplicationLifetime desktop, Window window)
    {
        ResourceLimits.LimitMemory(new Percentage(90));
        
        Task.Run(() => LanguageUpdater.UpdateLanguageAsync(vm.Translation, vm.PicViewer, settingsExists));
        if (settingsExists)
        {
            Task.Run(() => KeybindingManager.LoadKeybindings(vm.PlatformService));
        }
        else
        {
            Task.Run(() => KeybindingManager.SetDefaultKeybindings(vm.PlatformService));
        }
        
        Task.Run(FileHistoryManager.InitializeAsync);

        HandleThemeUpdates(vm);
        
        UIHelper.SetControls(desktop);
        Task.Run(() =>
        {
            HandleWindowControlSettings(vm, desktop);
            SettingsUpdater.ValidateGallerySettings(vm, settingsExists);
        });
        
        vm.MainWindow.LayoutButtonSubscription();
        
        // Need to delay setting fullscreen or maximized until after the window is shown to select the correct monitor
        if (Settings.WindowProperties.Maximized && !Settings.WindowProperties.Fullscreen)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                vm.PlatformWindowService.Maximize(false);
            }, DispatcherPriority.Normal).Wait();
        }
        else if (Settings.WindowProperties.Fullscreen)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                vm.PlatformWindowService.Fullscreen(false);
            }, DispatcherPriority.Normal).Wait();
        }
        

        SetWindowEventHandlers(window);
        MenuManager.AddMenus();
        
        if (!Settings.WindowProperties.AutoFit)
        {
            // Need to update the screen size after the window is shown,
            // to avoid rendering error when switching between auto-fit
            ScreenHelper.UpdateScreenSize(window);
        }

        Application.Current.Name = "PicView";

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (Settings.UIProperties.OpenInSameWindow && !ProcessHelper.CheckIfAnotherInstanceIsRunning())
            {
                // No other instance is running, create named pipe server
                _ = IPC.StartListeningForArguments(vm);
            }
        }
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
            desktop.MainWindow.Topmost = true;
        }
    }

    private static void HandleStartUpMenuOrImage(MainViewModel vm, string[] args)
    {
        vm.ImageViewer = new ImageViewer();
        
        if (args.Length > 1)
        {
            vm.MainWindow.CurrentView.Value = vm.ImageViewer;
            Task.Run(() => QuickLoad.QuickLoadAsync(vm, args[1]));
        }
        else StartUpMenuOrLastFile(vm);
    }

    private static void HandleStartUpMenuOrImage(MainViewModel vm, string? arg = null)
    {
        vm.ImageViewer = new ImageViewer();
        
        if (arg is not null)
        {
            vm.MainWindow.CurrentView.Value = vm.ImageViewer;
            Task.Run(() => QuickLoad.QuickLoadAsync(vm, arg));
        }
        else StartUpMenuOrLastFile(vm);
    }
    
    private static void StartUpMenuOrLastFile(MainViewModel vm)
    {
        if (Settings.StartUp.OpenLastFile)
        {
            if (string.IsNullOrWhiteSpace(Settings.StartUp.LastFile))
            {
                ShowStartUpMenu();
            }
            else
            {
                vm.MainWindow.CurrentView.Value = vm.ImageViewer;
                Task.Run(() => QuickLoad.QuickLoadAsync(vm, Settings.StartUp.LastFile));
            }
        }
        else
        {
            ShowStartUpMenu();
        }

        return;

        void ShowStartUpMenu()
        {
            // Starting it in Dispatcher with post fixes occurrences where the text is not centered or the text is missing
            Dispatcher.UIThread.Post(() => { ErrorHandling.ShowStartUpMenu(vm); });
            Dispatcher.UIThread.Post(() =>
            {
                if (Settings.WindowProperties.AutoFit)
                {
                    WindowFunctions.CenterWindowOnScreen();
                }
            }, DispatcherPriority.Background);
        }
    }

    private static void HandleNormalWindow(MainViewModel vm, Window window)
    {
        vm.MainWindow.CanResize.Value = true;
        vm.GlobalSettings.IsAutoFit.Value = false;
        if (Settings.UIProperties.ShowInterface)
        {
            vm.MainWindow.IsTopToolbarShown.Value = true;
            vm.MainWindow.IsBottomToolbarShown.Value = Settings.UIProperties.ShowBottomNavBar;
        }
        WindowFunctions.InitializeWindowSizeAndPosition(window);
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
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private static void HandleMultipleInstances(string[] args)
    {
        if (args.Length > 1)
        {
            Task.Run(async () =>
            {
                var retries = 0;
                while (!await IPC.SendArgumentToRunningInstance(args[1]))
                {
                    await Task.Delay(1000);
                    if (++retries > 20)
                    {
                        break;
                    }
                }

                Environment.Exit(0);
            });
        }
    }

    private static void SetWindowEventHandlers(Window w)
    {
        w.KeyDown += async (_, e) => await MainKeyboardShortcuts.MainWindow_KeysDownAsync(e).ConfigureAwait(false);
        w.KeyUp += (_, e) => MainKeyboardShortcuts.MainWindow_KeysUp(e);
        w.PointerPressed += async (_, e) => await MouseShortcuts.MainWindow_PointerPressed(e).ConfigureAwait(false);
        
        w.Deactivated += delegate
        {
            MainKeyboardShortcuts.Reset();
            MainKeyboardShortcuts.ClearKeyDownModifiers();
        };
    }
}