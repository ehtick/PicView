using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Sizing;

namespace PicView.Avalonia.Win32.WindowImpl;

public static class Win32Window
{
    public static async Task Fullscreen(Window window, MainViewModel vm, bool saveSettings = true)
    {
        // Save window size, so that restoring it will return to the same size and position
        WindowResizing.SaveSize(window);
        
        // Update view model properties
        vm.SizeToContent = SizeToContent.Manual;
        vm.IsFullscreen = true;
        vm.IsMaximized = false;
        vm.CanResize = false;
        
        // Update settings
        Settings.WindowProperties.Fullscreen = true;
        
        // Apply fullscreen state
        await InvokeOnUIThreadAsync(() => window.WindowState = WindowState.FullScreen);

        // Hide interface in fullscreen
        HideInterface(vm);
        
        // Center it, to make sure it is positioned correctly
        CenterWindowOnScreen(window);
        
        WindowResizing.SetSize(vm);
        MenuManager.CloseMenus(vm);
        vm.GalleryWidth = double.NaN;

        if (saveSettings)
        {
            await SaveSettingsAsync().ConfigureAwait(false);
        }
    }
    
    /// <summary>
    /// Maximizes the window
    /// </summary>
    public static async Task Maximize(Window window, MainViewModel vm, bool saveSettings = true)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (Settings.WindowProperties.AutoFit)
            {
                vm.SizeToContent = SizeToContent.Manual;
            }
            else
            {
                // Save window size, so that restoring it will return to the same size and position
                WindowResizing.SaveSize(window);
            }

            window.WindowState = WindowState.Maximized;
            Settings.WindowProperties.Maximized = true;
            WindowResizing.SetSize(vm);
            SetMargin(vm, window);
        });

        vm.IsMaximized = true;
        vm.IsFullscreen = false;
        vm.CanResize = false;
        
        if (saveSettings)
        {
            await SaveSettingsAsync().ConfigureAwait(false);
        }
    }

    public static async Task Restore(Window window, MainViewModel vm, bool saveSettings = true)
    {
        // Update settings
        Settings.WindowProperties.Maximized = false;
        Settings.WindowProperties.Fullscreen = false;
        
        // Update UI state
        SetMargin(vm, window);
        vm.IsMaximized = false;
        vm.IsFullscreen = false;
        
        RestoreInterface(vm);
        
        // Update window state
        await InvokeOnUIThreadAsync(() => window.WindowState = WindowState.Normal);
        
        await ConfigureWindowSizing(vm, window);
        WindowResizing.SetSize(vm);
        
        if (saveSettings)
        {
            await SaveSettingsAsync().ConfigureAwait(false);
        }
    }
    
    public static async Task ToggleFullscreen(Window window, MainViewModel vm, bool saveSettings = true)
    {
        if (Settings.WindowProperties.Fullscreen)
        {
            await Restore(window, vm, saveSettings);
        }
        else
        {
            await Fullscreen(window, vm, saveSettings);
        }
        
        if (saveSettings)
        {
            await SaveSettingsAsync().ConfigureAwait(false);
        }
    }

    public static async Task ToggleMaximize(Window window, MainViewModel vm, bool saveSettings = true)
    {
        if (Settings.WindowProperties.Maximized)
        {
            await Restore(window, vm, saveSettings); 
        }
        else
        {
            await Maximize(window, vm, saveSettings);
        }
        
        if (saveSettings)
        {
            await SaveSettingsAsync().ConfigureAwait(false);
        }
    }


    #region Helpers
    
    /// <summary>
    /// Configures window sizing mode based on settings
    /// </summary>
    private static async Task ConfigureWindowSizing(MainViewModel vm, Window window)
    {
        if (Settings.WindowProperties.AutoFit)
        {
            vm.SizeToContent = SizeToContent.WidthAndHeight;
            vm.CanResize = false;
            vm.IsAutoFit = true;
            WindowFunctions.CenterWindowOnScreen();
            await WindowFunctions.ResizeAndFixRenderingError(vm); // Fixes incorrect render size
        }
        else
        {
            vm.SizeToContent = SizeToContent.Manual;
            vm.CanResize = true;
            WindowFunctions.InitializeWindowSizeAndPosition(window);
        }
    }
    
    /// <summary>
    /// Centers the window on the screen
    /// </summary>
    private static void CenterWindowOnScreen(Window window)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Get the screen that the window is currently on
            var screens = window.Screens;
            var screen = screens.ScreenFromVisual(window);

            if (screen == null)
                return; // No screen found (edge case)

            // Get the scaling factor of the screen (DPI scaling)
            var scalingFactor = screen.Scaling;

            // Get the current screen's bounds (in physical pixels, not adjusted for scaling)
            var screenBounds = screen.Bounds;

            // Calculate the actual bounds in logical units (adjusting for scaling)
            var screenWidth = screenBounds.Width / scalingFactor;
            var screenHeight = screenBounds.Height / scalingFactor;

            // Get the size of the window
            var windowSize = window.ClientSize;

            // Calculate the position to center the window on the screen
            var centeredX = screenBounds.X + (screenWidth - windowSize.Width) / 2;
            var centeredY = screenBounds.Y + (screenHeight - windowSize.Height) / 2;

            // Set the window's new position
            window.Position = new PixelPoint((int)centeredX, (int)centeredY);
        });
    }

    /// <summary>
    /// Restores the interface based on settings
    /// </summary>
    private static void RestoreInterface(MainViewModel vm)
    {
        vm.IsUIShown = Settings.UIProperties.ShowInterface;
        
        if (Settings.UIProperties.ShowInterface)
        {
            vm.IsTopToolbarShown = true;
            vm.TitlebarHeight = SizeDefaults.MainTitlebarHeight;
            
            if (Settings.UIProperties.ShowBottomNavBar)
            {
                vm.IsBottomToolbarShown = true;
                vm.BottombarHeight = SizeDefaults.BottombarHeight;
            }
        }
    }

    /// <summary>
    /// Hides interface elements for fullscreen mode
    /// </summary>
    private static void HideInterface(MainViewModel vm)
    {
        vm.IsTopToolbarShown = false;
        vm.IsBottomToolbarShown = false;
        vm.IsUIShown = false;
    }

    /// <summary>
    /// Sets margin based on window state
    /// </summary>
    private static void SetMargin(MainViewModel vm, Window window)
    {
        if (Settings.WindowProperties.Maximized)
        {
            // Sometimes margin is 0 when it's not supposed to be, so replace with 7. Not sure why.
            var left = window.OffScreenMargin.Left is 0 ? 7 : window.OffScreenMargin.Left;
            var top = window.OffScreenMargin.Top is 0 ? 7 : window.OffScreenMargin.Top;
            var right = window.OffScreenMargin.Right is 0 ? 7 : window.OffScreenMargin.Right;
            var bottom = window.OffScreenMargin.Bottom is 0 ? 7 : window.OffScreenMargin.Bottom;
            vm.TopScreenMargin = new Thickness(left, top, right, 0);
            vm.BottomScreenMargin = new Thickness(left, 0, right, bottom);
        }
        else
        {
            var noThickness = new Thickness(0);
            vm.TopScreenMargin = noThickness;
            vm.BottomScreenMargin = noThickness;
        }
    }
    
    /// <summary>
    /// Invokes an action on the UI thread
    /// </summary>
    private static async Task InvokeOnUIThreadAsync(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
            action();
        else
            await Dispatcher.UIThread.InvokeAsync(action);
    }
    
    #endregion Helpers
}
