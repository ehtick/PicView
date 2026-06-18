using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Core.ArchiveHandling;
using PicView.Core.Config;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.IPlatform;
using PicView.Core.Models;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace PicView.Avalonia.WindowBehavior;

public static class WindowFunctions
{
    public static async Task WindowClosingBehavior()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        await WindowClosingBehavior(desktop.MainWindow);
    }

    public static async Task WindowClosingBehavior(Window window)
    {
        WindowResizing.SaveSize(window);

        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        if (window.DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }
        
        window.Hide();
        
        string? lastFile;

        if (!string.IsNullOrEmpty(ArchiveExtraction.LastOpenedArchive))
        {
            lastFile = ArchiveExtraction.LastOpenedArchive;
        }
        else if (viewModel.WindowTabs.ActiveTab.CurrentValue.SingleImageType is SingleImageType.Url && viewModel.WindowTabs.ActiveTab.CurrentValue.SourceURL is not null)
        {
            lastFile = viewModel.WindowTabs.ActiveTab.CurrentValue.SourceURL;
        }
        else
        {
            lastFile = viewModel.WindowTabs.ActiveTab.CurrentValue?.Model?.FileInfo?.FullName ?? FileHistoryManager.GetLastEntry() ?? null;
        }


        if (lastFile is not null)
        {
            Settings.StartUp.LastFile = lastFile;
        }

        try
        {
            await SaveSettingsAsync();
            await KeybindingManager.UpdateKeyBindingsFile();
            TempFileManager.Cleanup();
            await FileHistoryManager.SaveToFileAsync();
            ArchiveExtraction.Cleanup();
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(WindowFunctions), nameof(WindowClosingBehavior), e);
        }
        core.MainWindows.MainWindows.Remove(viewModel);

        if (core.SettingsViewModel?.SettingsWindowConfig is not null)
        {
            await core.SettingsViewModel.SettingsWindowConfig.SaveAsync();
        }

        if (core.MainWindows.MainWindows.Count <= 0)
        {
            // No mainWindow, close it manually to not have it running in the background
            Environment.Exit(0);
        }
    }

    #region Window State

    /// <summary>
    /// Restores the interface based on settings
    /// </summary>
    public static void RestoreInterface(MainWindowViewModel vm)
    {
        vm.IsUIShown.Value = Settings.UIProperties.ShowInterface;

        if (!Settings.UIProperties.ShowInterface)
        {
            return;
        }

        vm.IsTopToolbarShown.Value = true;
        vm.TitlebarHeight.Value = SizeDefaults.MainTitlebarHeight;

        if (!Settings.UIProperties.ShowBottomNavBar)
        {
            return;
        }

        vm.IsBottomToolbarShown.Value = true;
        vm.BottombarHeight.Value = SizeDefaults.BottombarHeight;
    }

    public static void ShowMinimizedWindow(Window window)
    {
        window.BringIntoView();
        window.WindowState = WindowState.Normal;
        window.Activate();
        window.Focus();
    }

    public static async Task ToggleTopMost(MainWindowViewModel vm)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        var shouldBeTopMost = !Settings.WindowProperties.TopMost;
        desktop.MainWindow.Topmost = shouldBeTopMost;
        Settings.WindowProperties.TopMost = shouldBeTopMost;
        vm.IsTopMost.Value  = shouldBeTopMost;

        await SaveSettingsAsync().ConfigureAwait(false);
    }

    public static async Task ToggleAutoFit(MainWindowViewModel vm, Window window)
    {
        if (Settings.WindowProperties.AutoFit)
        {
            SetManualWindow(vm, window);
        }
        else
        {
            SetAutoFit(vm, window);
        }
        WindowResizing.SetSize(vm, WindowResizeReason.Application);
        await SaveSettingsAsync().ConfigureAwait(false);
    }

    public static void SetAutoFit(MainWindowViewModel vm, Window window, bool center = true)
    {
        window.SizeToContent = SizeToContent.WidthAndHeight;
        Settings.WindowProperties.AutoFit = true;
        vm.IsAutoFit.Value = true;

        if (center)
        {
            // Fix unpleasant window placement
            CenterWindowOnScreen();
        }
    }
    
    public static void SetManualWindow(MainWindowViewModel vm)
    {
        if (Application.Current.ApplicationLifetime  is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }
        SetManualWindow(vm, desktop.MainWindow);
    }

    public static void SetManualWindow(MainWindowViewModel vm, Window window)
    {
        vm.WindowMaxWidth.Value = vm.WindowMaxHeight.Value = double.NaN;
        window.SizeToContent = SizeToContent.Manual;
        Settings.WindowProperties.AutoFit = false;
        vm.IsAutoFit.Value = false;
    }

    #endregion

    #region Window Size and Position

    public static void CenterWindowOnScreen(Window window)
    {
        CenterWindowOnScreen(horizontal: true, top: false, window: window);
    }

    public static void CenterWindowOnScreen(bool horizontal = true, bool top = false, Window? window = null)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            window ??= desktop.MainWindow;

            ScreenHelper.UpdateScreenSize(window);
            var screen = ScreenHelper.ScreenSize;

            // Get the size of the window
            var windowSize = window.ClientSize;

            var x = screen.X;
            var y = screen.Y;

            // Calculate the position to center the window on the screen
            var centeredX = x + (screen.WorkingAreaWidth - windowSize.Width) / 2;
            var centeredY = y + (top ? 0 : (screen.WorkingAreaHeight - windowSize.Height) / 2);

            // Set the window's new position
            window.Position = horizontal
                ? new PixelPoint((int)centeredX, (int)centeredY)
                : new PixelPoint(window.Position.X, (int)centeredY);
        });
    }

    public static void InitializeWindowSizeAndPosition(Window window)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            window.Position = new PixelPoint((int)Settings.WindowProperties.Left,
                (int)Settings.WindowProperties.Top);
            window.Width = Settings.WindowProperties.Width;
            window.Height = Settings.WindowProperties.Height;
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                window.Position = new PixelPoint((int)Settings.WindowProperties.Left,
                    (int)Settings.WindowProperties.Top);
                window.Width = Settings.WindowProperties.Width;
                window.Height = Settings.WindowProperties.Height;
            });
        }
    }

    public static void InitializeWindowPosition(Window window, IWindowProperties properties)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set);
        }

        return;

        void Set()
        {
            if (properties is { Left: not null, Top: not null })
            {
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Position = new PixelPoint(properties.Left.GetValueOrDefault(),
                    properties.Top.GetValueOrDefault());
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
    }

    public static void InitializeWindowSizeAndPosition(Window window, IWindowProperties properties)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set);
        }

        return;

        void Set()
        {
            if (properties.Maximized)
            {
                window.WindowState = WindowState.Maximized;
            }
            else if (properties is { Left: not null, Top: not null })
            {
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Position = new PixelPoint(properties.Left.GetValueOrDefault(),
                    properties.Top.GetValueOrDefault());
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (properties is not { Width: not null, Height: not null })
            {
                return;
            }

            if (properties.Width > window.MinWidth && properties.Width < window.MaxWidth &&
                properties.Height > window.MinHeight && properties.Height < window.MaxHeight)
            {
                window.Width = properties.Width.Value;
                window.Height = properties.Height.Value;
            }
            else
            {
                DebugHelper.LogDebug(nameof(WindowFunctions), nameof(InitializeWindowSizeAndPosition),
                    "Invalid width and height values");
            }
        }
    }

    public static void SetWindowSize(Window window, AvaloniaPropertyChangedEventArgs<Size> size,
        IWindowProperties properties)
    {
        if (!size.NewValue.HasValue)
        {
            return;
        }

        if (size.NewValue.Value == size.OldValue.Value)
        {
            return;
        }

        if (size.NewValue.Value.Width < window.MinWidth)
        {
            return;
        }

        if (size.NewValue.Value.Height < window.MinHeight)
        {
            return;
        }

        properties.Width = window.Bounds.Width;
        properties.Height = window.Bounds.Height;
    }

    #endregion

    #region Window Drag and Behavior

    public static void WindowDragAndDoubleClickBehavior(Window window, PointerPressedEventArgs e,
        IPlatformWindowService platformWindowService)
    {
        var currentScreen = ScreenHelper.ScreenSize;

        var screen = window.Screens.ScreenFromVisual(window);
        if (screen == null)
        {
            return;
        }

        if (e.ClickCount == 2 && e.GetCurrentPoint(window).Properties.IsLeftButtonPressed)
        {
            platformWindowService.MaximizeRestore();
            return;
        }

        window.BeginMoveDrag(e);

        if (screen.WorkingArea.Width == currentScreen.WorkingAreaWidth &&
            screen.WorkingArea.Height == currentScreen.WorkingAreaHeight && screen.Scaling == currentScreen.Scaling)
        {
            return;
        }

        ScreenHelper.UpdateScreenSize(window);
        WindowResizing.SetSize(window.DataContext as MainWindowViewModel, WindowResizeReason.DpiChange);
    }

    public static void WindowDragBehavior(Window window, PointerPressedEventArgs e)
    {
        var currentScreen = ScreenHelper.ScreenSize;
        window.BeginMoveDrag(e);
        var screen = window.Screens.ScreenFromVisual(window);
        if (screen == null)
        {
            return;
        }

        if (screen.WorkingArea.Width == currentScreen.WorkingAreaWidth &&
            screen.WorkingArea.Height == currentScreen.WorkingAreaHeight && screen.Scaling == currentScreen.Scaling)
        {
            return;
        }

        ScreenHelper.UpdateScreenSize(window);
        WindowResizing.SetSize(window.DataContext as MainWindowViewModel, WindowResizeReason.DpiChange);
    }

    #endregion
}