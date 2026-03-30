using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using PicView.Avalonia.Crop;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Functions;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Core.DebugTools;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Input;

/// <summary>
/// Handles keyboard shortcuts and tracks key modifier states for the new architecture.
/// </summary>
public static class MainKeyboardShortcuts2
{
    /// <summary>
    /// Indicates whether a key is held down.
    /// </summary>
    public static bool IsKeyHeldDown { get; private set; }

    /// <summary>
    /// The current modifiers being pressed.
    /// </summary>
    public static KeyModifiers CurrentModifiers { get; private set; }

    /// <summary>
    /// Stores the current key gesture, including the key and its modifiers.
    /// </summary>
    public static KeyGesture? CurrentKeys { get; private set; }

    /// <summary>
    /// Gets or sets whether keyboard shortcuts are enabled.
    /// </summary>
    public static bool IsKeysEnabled { get; set; } = true;
    
    public static bool IsEscKeyEnabled { get; set; } = true;

    public static bool CtrlDown => (CurrentModifiers & KeyModifiers.Control) == KeyModifiers.Control;
    public static bool AltOrOptionDown => (CurrentModifiers & KeyModifiers.Alt) == KeyModifiers.Alt;
    public static bool ShiftDown => (CurrentModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;
    public static bool CommandDown => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && 
                                     (CurrentModifiers & KeyModifiers.Meta) == KeyModifiers.Meta;

    private static int _keyRepeatCount;
    private const int KeyRepeatThreshold = 1;

    /// <summary>
    /// Processes the KeyDown event for the main window.
    /// </summary>
    /// <param name="e">The key event arguments.</param>
    /// <param name="mainWindowViewModel">The ViewModel of the active MainWindow.</param>
    public static async ValueTask MainWindow_KeysDownAsync(KeyEventArgs e, Core.ViewModels.MainWindowViewModel? mainWindowViewModel)
    {
        if (KeybindingManager2.CustomShortcuts is null || !IsKeysEnabled)
        {
            return;
        }

        UpdateModifierState(e.Key, true);
        
#if DEBUG
        // Handle special debug keys first
        if (HandleDebugKeys(e.Key))
        {
            return;
        }
#endif

        // If it's a modifier key only, nothing more to do
        if (IsModifierKey(e.Key))
        {
            return;
        }

        // Create key gesture from current state
        CurrentKeys = new KeyGesture(e.Key, CurrentModifiers);

        // Track key repeat for held down state
        _keyRepeatCount++;
        IsKeyHeldDown = _keyRepeatCount > KeyRepeatThreshold;

        // Handle special cases before processing shortcuts
        if (await HandleSpecialCases(e, mainWindowViewModel))
        {
            return;
        }
        
        if (mainWindowViewModel is null)
        {
            return;
        }

        // Handle registered shortcuts
        await ExecuteShortcutIfRegistered(mainWindowViewModel);
    }

    /// <summary>
    /// Processes the KeyUp event for the main window.
    /// </summary>
    /// <param name="e">The key event arguments.</param>
    public static void MainWindow_KeysUp(KeyEventArgs e, Core.ViewModels.MainWindowViewModel? mainWindowViewModel)
    {
        mainWindowViewModel?.Mapper?.StopRepeatedNavigation();
        UpdateModifierState(e.Key, false);
        Reset();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (e.Key is Key.LeftAlt or Key.RightAlt)
            {
                mainWindowViewModel.TopTitlebarViewModel.ToggleMenu();
            }
        }
    }

    /// <summary>
    /// Updates the state of a modifier key.
    /// </summary>
    /// <param name="key">The key that changed state.</param>
    /// <param name="isDown">Whether the key is being pressed down.</param>
    private static void UpdateModifierState(Key key, bool isDown)
    {
        CurrentModifiers = key switch
        {
            Key.LeftShift or Key.RightShift => isDown
                ? CurrentModifiers | KeyModifiers.Shift
                : CurrentModifiers & ~KeyModifiers.Shift,
            Key.LeftCtrl or Key.RightCtrl => isDown
                ? CurrentModifiers | KeyModifiers.Control
                : CurrentModifiers & ~KeyModifiers.Control,
            Key.LeftAlt or Key.RightAlt => isDown
                ? CurrentModifiers | KeyModifiers.Alt
                : CurrentModifiers & ~KeyModifiers.Alt,
            Key.LWin or Key.RWin when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => isDown
                ? CurrentModifiers | KeyModifiers.Meta
                : CurrentModifiers & ~KeyModifiers.Meta,
            _ => CurrentModifiers
        };
    }

    /// <summary>
    /// Checks if a key is a modifier key.
    /// </summary>
    private static bool IsModifierKey(Key key) => key switch
    {
        Key.LeftShift or Key.RightShift or
        Key.LeftCtrl or Key.RightCtrl or
        Key.LeftAlt or Key.RightAlt or
        Key.LWin or Key.RWin => true,
        _ => false
    };

    /// <summary>
    /// Handles debug-specific key commands.
    /// </summary>
    /// <returns>True if the key was handled as a debug key.</returns>
    private static bool HandleDebugKeys(Key key)
    {
#if DEBUG
        switch (key)
        {
            case Key.F12: // Show Avalonia DevTools in DEBUG mode
                return true;
            // Removed F9 and F7 as they were accessing static FunctionsMapper
        }
#endif
        return false;
    }

    /// <summary>
    /// Handles special cases like cropping, dialog handling, and escape key.
    /// </summary>
    /// <returns>True if the key event was handled by a special case handler.</returns>
    private static async ValueTask<bool> HandleSpecialCases(KeyEventArgs e, Core.ViewModels.MainWindowViewModel? vm)
    {
        if (vm is null) return false;

        // TODO: Re-implement cropping logic with new architecture if needed
        // For now, we stub this out or need to access the relevant View/ViewModel
        /*
        // Handle cropping mode
        if (CropFunctions.IsCropping)
        {
            if (vm.MainWindow.CurrentView.CurrentValue is CropControl cropControl )
            {
                await cropControl.KeyDownHandler(null, e);
            }
            return true;
        }
        */

        // Don't interrupt navigating main menu with keyboard
        if (vm.TopTitlebarViewModel.IsMainMenuVisible.CurrentValue)
        {
            return true;
        }

        // Handle open dialog
        if (DialogManager.IsDialogOpen)
        {
            UIHelper.GetMainView.MainGrid.Children
                .OfType<AnimatedPopUp>()
                .FirstOrDefault()
                ?.KeyDownHandler(null, e);
            return true;
        }
        
        // Handle escape key
        if (e.Key == Key.Escape)
        {
            if (vm.IsEditableTitlebarOpen.CurrentValue)
            {
                return true;
            }
            
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { Windows.Count: > 1 } desktop)
            {
                 // Check if the current window (associated with vm) is one of the secondary windows
                 // This logic might need refinement to identify WHICH window is closing
                 // For now, mirroring legacy behavior of checking count
                desktop.Windows[^1].Close();
                IsKeyHeldDown = true; // If closing the last window, make sure not to call Close()
                return true;
            }

            if (Slideshow.IsRunning)
            {
                // Slideshow.StopSlideshow(vm); // Needs refactor
                return true;
            }

            if (!IsKeyHeldDown && IsEscKeyEnabled)
            {
                if (vm.Mapper != null)
                {
                    await vm.Mapper.Close();
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Executes the registered shortcut action for the current key combination.
    /// </summary>
    private static async ValueTask ExecuteShortcutIfRegistered(Core.ViewModels.MainWindowViewModel vm)
    {
        if (CurrentKeys is not null)
        {
            var functionName = KeybindingManager2.GetFunctionName(CurrentKeys);
            if (!string.IsNullOrEmpty(functionName))
            {
                var action = vm.Mapper?.GetFunctionByName(functionName);
                if (action is not null)
                {
                    await action.Invoke().ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Resets the keyboard state tracking.
    /// </summary>
    public static void Reset()
    {
        IsKeyHeldDown = false;
        IsEscKeyEnabled = true;
        CurrentKeys = null;
        _keyRepeatCount = 0;
    }

    /// <summary>
    /// Clears the states of all modifier keys.
    /// </summary>
    public static void ClearKeyDownModifiers()
    {
        CurrentModifiers = KeyModifiers.None;
    }
}
