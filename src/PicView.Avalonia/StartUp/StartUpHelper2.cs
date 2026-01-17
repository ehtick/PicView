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
using PicView.Core.Localization;
using PicView.Core.ProcessHandling;
using PicView.Core.ViewModels;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.StartUp;

public static class StartUpHelper2
{
    public static void StartWithArguments(CoreViewModel vm, bool settingsExists,
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
            SettingsUpdater2.InitializeSettings(vm.MainWindows.ActiveWindow.CurrentValue);

            HandleWindowScalingMode(vm, window);

            HandleStartImage(vm, window, filePath);

            HandlePostWindowUpdates(vm, settingsExists, desktop, window);
        }

        void BlankStartUp()
        {
            SettingsUpdater2.InitializeSettings(vm.MainWindows.ActiveWindow.CurrentValue);

            HandleWindowScalingMode(vm, window);

            HandlePostWindowUpdates(vm, settingsExists, desktop, window);
        }
    }

    
    public static void StartUpBlank(CoreViewModel vm, bool settingsExists, bool setPos,
        IClassicDesktopStyleApplicationLifetime desktop, Window window)
    {
        SettingsUpdater2.InitializeSettings(vm.MainWindows.ActiveWindow.CurrentValue);
        
        HandleWindowScalingMode(vm, window, setPos);

        window.Show();

        HandlePostWindowUpdates(vm, settingsExists, desktop, window);
    }
    
    public static void RegularStartUp(CoreViewModel vm, bool settingsExists,
        IClassicDesktopStyleApplicationLifetime desktop,
        Window window)
    {
        TranslationManager.Init();
        SettingsUpdater2.InitializeSettings(vm.MainWindows.ActiveWindow.CurrentValue);

        HandleWindowScalingMode(vm, window);

        StartUpMenuOrLastFile(vm, window);
        window.Show();

        HandlePostWindowUpdates(vm, settingsExists, desktop, window);
    }
    
    

    private static void HandleWindowScalingMode(CoreViewModel vm, Window window, bool setPos = true)
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

    private static void HandlePostWindowUpdates(CoreViewModel vm, bool settingsExists,
        IClassicDesktopStyleApplicationLifetime desktop, Window window)
    {
        SetMemorySettings();

        Task.Run(() => LanguageUpdater2.UpdateLanguageAsync(vm.Translation, settingsExists));
        if (settingsExists)
        {
            Task.Run(async () =>
            {
               await KeybindingManager2.LoadKeybindings(vm.PlatformService);
               vm.MainWindows.ActiveWindow.Value.Mapper =
                   new FunctionsMapper2(vm.MainWindows.ActiveWindow.CurrentValue);
            });
        }
        else
        {
            Task.Run(() =>
            {
                KeybindingManager2.SetDefaultKeybindings(vm.PlatformService);
                vm.MainWindows.ActiveWindow.Value.Mapper =
                    new FunctionsMapper2(vm.MainWindows.ActiveWindow.CurrentValue);
            });
        }

        SetWindowEventHandlers(window);
        HandleThemeUpdates(vm);

        UIHelper2.SetControls(window);
        Task.Run(() =>
        {
   //         vm.Tabs.SetParentContext(vm);
            _ = FileHistoryManager.InitializeAsync();
            HandleWindowControlSettings(vm, desktop);
     //       SettingsUpdater.ValidateGallerySettings(vm, settingsExists);

            // vm.MainWindow.LayoutButtonSubscription(vm);
            // vm.Gallery.GalleryItemSizeUpdateSubscription(vm);
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
            // Dispatcher.UIThread
            //     .InvokeAsync(() => { window Maximize(false); }, DispatcherPriority.Background);
        }
        else if (Settings.WindowProperties.Fullscreen)
        {
            // Dispatcher.UIThread.InvokeAsync(() => { vm.PlatformWindowService.Fullscreen(false); },
            //     DispatcherPriority.Background);
        }

        if (Settings.UIProperties.ShowHoverNavigationBar)
        {
            vm.MainWindows.ActiveWindow.CurrentValue.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverbarVisible
                .Value = !Settings.UIProperties.ShowBottomNavBar;
        }
        
 //       TooltipHelper.StartTooltipSubscription(vm);
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Windows needs a named pipe server to open files in the same window
            if (Settings.UIProperties.OpenInSameWindow && !ProcessHelper.CheckIfAnotherInstanceIsRunning())
            {
  //              _ = IPC.StartListeningForArguments(vm);
            }
        }
        
        Application.Current.Name = "PicView";
    }

    private static void SetMemorySettings()
    {
        ResourceLimits.LimitMemory(new Percentage(80));
        GCSettings.LatencyMode = GCLatencyMode.LowLatency;
    }

    private static void HandleThemeUpdates(CoreViewModel vm)
    {
        if (Settings.Theme.GlassTheme)
        {
            GlassThemeHelper.GlassThemeUpdates();
        }

  //      BackgroundManager.SetBackground(vm);
        ColorManager.UpdateAccentColors(Settings.Theme.ColorTheme);
    }

    private static void HandleWindowControlSettings(CoreViewModel vm, IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (Settings.Zoom.ScrollEnabled)
        {
            //SettingsUpdater.TurnOnScroll(vm);
        }
        else
        {
      //      vm.MainWindow.ToggleScrollBarVisibility.Value = ScrollBarVisibility.Disabled;
            vm.GlobalSettings.IsScrollingEnabled.Value = false;
        }

        if (Settings.WindowProperties.TopMost)
        {
            Dispatcher.UIThread.Invoke(() => { desktop.MainWindow.Topmost = true; });
        }
    }

    private static void HandleStartImage(CoreViewModel vm, Window window, string arg)
    {
        Task.Run(() => QuickLoad2.QuickLoadAsync(vm, arg, window, false));
    }

    private static void StartUpMenuOrLastFile(CoreViewModel vm, Window window)
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
            var startUpMenu = new StartUpMenu
            {
                Buttons =
                {
                    DataContext = vm
                }
            };
           // vm.Tabs.ActiveTab.Value.CurrentView.Value = startUpMenu;
            TabNavigationInitializer.Initialize(vm);
        }
    }

    private static void HandleNormalWindow(CoreViewModel vm, Window window, bool setPos)
    {
      //  vm.MainWindow.CanResize.Value = true;
        vm.GlobalSettings.IsAutoFit.Value = false;
        if (Settings.UIProperties.ShowInterface)
        {
         //   vm.MainWindow.IsTopToolbarShown.Value = true;
        //    vm.MainWindow.IsBottomToolbarShown.Value = Settings.UIProperties.ShowBottomNavBar;
        }

        if (setPos)
        {
            WindowFunctions.InitializeWindowSizeAndPosition(window);
        }
    }

    private static void HandleAutoFit(CoreViewModel vm, Window window)
    {
    //    vm.MainWindow.SizeToContent.Value = SizeToContent.WidthAndHeight;
   //    vm.MainWindow.CanResize.Value = false;
        vm.GlobalSettings.IsAutoFit.Value = true;
        if (Settings.UIProperties.ShowInterface)
        {
  //          vm.MainWindow.IsTopToolbarShown.Value = true;
    //        vm.MainWindow.IsBottomToolbarShown.Value = Settings.UIProperties.ShowBottomNavBar;
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
        w.PointerPressed += async (_, e) => await MouseShortcuts2.MainWindow_PointerPressed(e).ConfigureAwait(false);

        w.Deactivated += delegate
        {
            MainKeyboardShortcuts.Reset();
            MainKeyboardShortcuts.ClearKeyDownModifiers();
        };
    }

    private static async ValueTask MainWindow_KeysDownAsync(object? sender, KeyEventArgs e)
    {
        // Extract the ViewModel from the window that received the key press
        var vm = (sender as Control)?.DataContext as MainWindowViewModel;
        await MainKeyboardShortcuts2.MainWindow_KeysDownAsync(e, vm).ConfigureAwait(false);
    }

    private static void MainWindow_KeyUp(object? sender, KeyEventArgs e)
    {
        // Extract the ViewModel from the window that received the key press
        var vm = (sender as Control)?.DataContext as MainWindowViewModel;
        MainKeyboardShortcuts2.MainWindow_KeysUp(e, vm);
    }
}