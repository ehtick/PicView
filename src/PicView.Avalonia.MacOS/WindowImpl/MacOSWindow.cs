using Avalonia.Controls;
using PicView.Avalonia.MacOS.Views;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;

namespace PicView.Avalonia.MacOS.WindowImpl;

public static class MacOSWindow
{
    public static async Task ToggleFullscreen(MacMainWindow? window, MainViewModel? vm, bool saveSettings)
    {
        if (Settings.WindowProperties.Fullscreen)
        {
            await Restore(window, vm, saveSettings);
        }
        else
        {
            await Fullscreen(window, vm, saveSettings);
        }
    }
    
    public static async Task ToggleMaximize(MacMainWindow? window, MainViewModel? vm, bool saveSettings = true)
    {
        if (Settings.WindowProperties.Maximized)
        {
            await Restore(window, vm, saveSettings); 
        }
        else
        {
            await Maximize(window, vm, saveSettings);
        }
    }

    public static async Task Restore(MacMainWindow? window, MainViewModel? vm, bool saveSettings = true)
    {
        // Update settings
        Settings.WindowProperties.Maximized = false;
        Settings.WindowProperties.Fullscreen = false;
        
        window.WindowState = WindowState.Normal;
        window.Topmost = Settings.WindowProperties.TopMost;

        if (Settings.UIProperties.ShowInterface)
        {
            vm.IsTopToolbarShown = true;
            vm.IsBottomToolbarShown = Settings.UIProperties.ShowBottomNavBar;
            vm.IsUIShown = true;
        }
        else
        {
            vm.IsTopToolbarShown = false;
            vm.IsUIShown = false;
        }

        if (Settings.WindowProperties.AutoFit)
        {
            vm.SizeToContent = SizeToContent.WidthAndHeight;
            vm.CanResize = false;
            await WindowResizing.SetSizeAsync(vm);
        }
        else
        {
            vm.SizeToContent = SizeToContent.Manual;
            vm.CanResize = true;
            WindowFunctions.InitializeWindowSizeAndPosition(window);
        }
        
        if (saveSettings)
        {
            await SaveSettingsAsync().ConfigureAwait(false);
        }
    }

    public static async Task Fullscreen(MacMainWindow? window, MainViewModel? vm, bool saveSettings = true)
    {
        Settings.WindowProperties.Maximized = false;
        Settings.WindowProperties.Fullscreen = true;
        
        window.WindowState = WindowState.FullScreen;
        
        vm.IsTopToolbarShown = false;
        vm.IsBottomToolbarShown = false;
        
        vm.IsFullscreen = true;
        vm.IsMaximized = false;
        vm.CanResize = false;
        await WindowResizing.SetSizeAsync(vm);
        
        if (saveSettings)
        {
            await SaveSettingsAsync().ConfigureAwait(false);
        }
    }

    public static async Task Maximize(MacMainWindow? window, MainViewModel? vm, bool saveSettings = true)
    {
        var isAutoFit = Settings.WindowProperties.AutoFit;
        Settings.WindowProperties.AutoFit = false;
        
        if (!isAutoFit)
        {
            // Save window size, so that restoring it will return to the same size and position
            WindowResizing.SaveSize(window);
        }
        
        // Update settings
        Settings.WindowProperties.Maximized = true;
        Settings.WindowProperties.Fullscreen = false;
        
        vm.SizeToContent = SizeToContent.Manual;
        vm.IsMaximized = true;
        vm.IsFullscreen = false;
        vm.CanResize = false;
        
        window.WindowState = WindowState.Maximized;
        

        await WindowResizing.SetSizeAsync(vm);
        
        if (isAutoFit)
        {
            Settings.WindowProperties.AutoFit = true;
        }
        
        if (saveSettings)
        {
            await SaveSettingsAsync().ConfigureAwait(false);
        }
    }
}