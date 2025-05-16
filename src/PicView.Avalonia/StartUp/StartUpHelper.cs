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
using PicView.Core.Gallery;
using PicView.Core.ProcessHandling;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;

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
                else if (Settings.UIProperties.OpenInSameWindow &&
                    ProcessHelper.CheckIfAnotherInstanceIsRunning())
                {
                    HandleMultipleInstances(args);
                }
            }
        }
        
        InitializeSettings(vm);
        
        HandleWindowScalingMode(vm, window);
        
        HandleStartUpMenuOrImage(vm, args);
        window.Show();
        
        HandlePostWindowUpdates(vm, settingsExists, desktop, window);
    }
    
    public static void StartWithoutArguments(MainViewModel vm, bool settingsExists, IClassicDesktopStyleApplicationLifetime desktop,
        Window window, string? arg = null)
    {
        InitializeSettings(vm);
        
        HandleWindowScalingMode(vm, window);
        
        HandleStartUpMenuOrImage(vm, arg);
        window.Show();
        
        HandlePostWindowUpdates(vm, settingsExists, desktop, window);
    }

    public static void HandleWindowScalingMode(MainViewModel vm, Window window)
    {
        ScreenHelper.UpdateScreenSize(window);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (Settings.WindowProperties.Padding <= 0)
            {
                Settings.WindowProperties.Padding = 45;
            }
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

    public static void HandlePostWindowUpdates(MainViewModel vm, bool settingsExists, IClassicDesktopStyleApplicationLifetime desktop, Window window)
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

        HandleThemeUpdates(vm);
        
        UIHelper.SetControls(desktop);

        ValidateGallerySettings(vm, settingsExists);
        
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
        
        HandleWindowControlSettings(vm, desktop);
        SetWindowEventHandlers(window);
        MenuManager.AddMenus();
        FileHistoryManager.Initialize();
        
        if (!Settings.WindowProperties.AutoFit)
        {
            // Need to update the screen size after the window is shown,
            // to avoid rendering error when switching between auto-fit
            ScreenHelper.UpdateScreenSize(window);
        }

        Application.Current.Name = "PicView";
        
        vm.AssociationsViewModel ??= new FileAssociationsViewModel();
        
        if (Settings.UIProperties.OpenInSameWindow)
        {
            // No other instance is running, create named pipe server
            _ = IPC.StartListeningForArguments(vm);
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
            vm.ToggleScrollBarVisibility = ScrollBarVisibility.Disabled;
            vm.IsScrollingEnabled = false;
        }

        if (Settings.WindowProperties.TopMost)
        {
            desktop.MainWindow.Topmost = true;
        }
    }

    public static void HandleStartUpMenuOrImage(MainViewModel vm, string[] args)
    {
        vm.ImageViewer = new ImageViewer();
        
        if (args.Length > 1)
        {
            vm.CurrentView = vm.ImageViewer;
            Task.Run(() => QuickLoad.QuickLoadAsync(vm, args[1]));
        }
        else StartUpMenuOrLastFile(vm);
    }
    
    public static void HandleStartUpMenuOrImage(MainViewModel vm, string? arg = null)
    {
        vm.ImageViewer = new ImageViewer();
        
        if (arg is not null)
        {
            vm.CurrentView = vm.ImageViewer;
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
                ErrorHandling.ShowStartUpMenu(vm);
                if (Settings.WindowProperties.AutoFit)
                {
                    WindowFunctions.CenterWindowOnScreen(false, true);
                }
            }
            else
            {
                vm.CurrentView = vm.ImageViewer;
                Task.Run(() => QuickLoad.QuickLoadAsync(vm, Settings.StartUp.LastFile));
            }
        }
        else
        {
            ErrorHandling.ShowStartUpMenu(vm);
            if (Settings.WindowProperties.AutoFit)
            {
                WindowFunctions.CenterWindowOnScreen();
            }
        }
    }

    private static void HandleNormalWindow(MainViewModel vm, Window window)
    {
        vm.CanResize = true;
        vm.IsAutoFit = false;
        if (Settings.UIProperties.ShowInterface)
        {
            vm.IsTopToolbarShown = true;
            vm.IsBottomToolbarShown = Settings.UIProperties.ShowBottomNavBar;
        }
        WindowFunctions.InitializeWindowSizeAndPosition(window);
    }

    private static void HandleAutoFit(MainViewModel vm, Window window)
    {
        vm.SizeToContent = SizeToContent.WidthAndHeight;
        vm.CanResize = false;
        vm.IsAutoFit = true;
        if (Settings.UIProperties.ShowInterface)
        {
            vm.IsTopToolbarShown = true;
            vm.IsBottomToolbarShown = Settings.UIProperties.ShowBottomNavBar;
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

    private static void ValidateGallerySettings(MainViewModel vm, bool settingsExists)
    {
        vm.GetFullGalleryItemHeight = Settings.Gallery.ExpandedGalleryItemSize;
        vm.GetBottomGalleryItemHeight = Settings.Gallery.BottomGalleryItemSize;
        if (!settingsExists)
        {
            vm.GetBottomGalleryItemHeight = GalleryDefaults.DefaultBottomGalleryHeight;
            vm.GetFullGalleryItemHeight = GalleryDefaults.DefaultFullGalleryHeight;
        }

        // Set default gallery sizes if they are out of range or upgrading from an old version
        if (vm.GetBottomGalleryItemHeight < vm.MinBottomGalleryItemHeight ||
            vm.GetBottomGalleryItemHeight > vm.MaxBottomGalleryItemHeight)
        {
            vm.GetBottomGalleryItemHeight = GalleryDefaults.DefaultBottomGalleryHeight;
        }

        if (vm.GetFullGalleryItemHeight < vm.MinFullGalleryItemHeight ||
            vm.GetFullGalleryItemHeight > vm.MaxFullGalleryItemHeight)
        {
            vm.GetFullGalleryItemHeight = GalleryDefaults.DefaultFullGalleryHeight;
        }

        if (settingsExists)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Settings.Gallery.BottomGalleryStretchMode))
        {
            Settings.Gallery.BottomGalleryStretchMode = "UniformToFill";
        }

        if (string.IsNullOrWhiteSpace(Settings.Gallery.FullGalleryStretchMode))
        {
            Settings.Gallery.FullGalleryStretchMode = "UniformToFill";
        }
    }

    public static void InitializeSettings(MainViewModel vm)
    {    
        // Set corner radius on macOS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            vm.BottomCornerRadius = new CornerRadius(0, 0, 8, 8);
        }
        
        vm.TitlebarHeight = Settings.WindowProperties.Fullscreen
            || !Settings.UIProperties.ShowInterface
            ? 0
            : SizeDefaults.MainTitlebarHeight;
        vm.BottombarHeight = Settings.WindowProperties.Fullscreen
                             || !Settings.UIProperties.ShowInterface
            ? 0
            : SizeDefaults.BottombarHeight;
        vm.GetNavSpeed = Settings.UIProperties.NavSpeed;
        vm.GetSlideshowSpeed = Settings.UIProperties.SlideShowTimer;
        vm.GetZoomSpeed = Settings.Zoom.ZoomSpeed;
        vm.PicViewer.IsShowingSideBySide = Settings.ImageScaling.ShowImageSideBySide;
        vm.IsBottomGalleryShown = Settings.Gallery.IsBottomGalleryShown;
        vm.IsBottomGalleryShownInHiddenUI = Settings.Gallery.ShowBottomGalleryInHiddenUI;
        vm.IsAvoidingZoomingOut  = Settings.Zoom.AvoidZoomingOut;
        vm.IsUIShown  = Settings.UIProperties.ShowInterface;
        vm.IsTopToolbarShown  = Settings.UIProperties.ShowInterface;
        vm.IsBottomToolbarShown   = Settings.UIProperties.ShowBottomNavBar &&
                                    Settings.UIProperties.ShowInterface;
        vm.IsShowingTaskbarProgress  = Settings.UIProperties.IsTaskbarProgressEnabled;
        vm.IsFullscreen  = Settings.WindowProperties.Fullscreen;
        vm.IsTopMost  = Settings.WindowProperties.TopMost;
        vm.IsIncludingSubdirectories = Settings.Sorting.IncludeSubDirectories;
        vm.IsStretched = Settings.ImageScaling.StretchImage;
        vm.IsLooping  = Settings.UIProperties.Looping;
        vm.IsAutoFit  = Settings.WindowProperties.AutoFit;
        vm.IsStayingCentered  = Settings.WindowProperties.KeepCentered;
        vm.IsOpeningInSameWindow  = Settings.UIProperties.OpenInSameWindow;
        vm.IsShowingConfirmationOnEsc  = Settings.UIProperties.ShowConfirmationOnEsc;   
        vm.IsUsingTouchpad  = Settings.Zoom.IsUsingTouchPad;
        vm.IsAscending  = Settings.Sorting.Ascending;
        vm.BackgroundChoice = Settings.UIProperties.BgColorChoice;
        vm.IsConstrainingBackgroundColor = Settings.UIProperties.IsConstrainBackgroundColorEnabled;
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