using Avalonia.Controls;
using Avalonia.Input;
using PicView.Avalonia.Input;
using PicView.Core.Localization;

namespace PicView.Avalonia.UI;

public static class GenericWindowHelper
{
    public static void AboutWindowInitialize(Window window)
    {
        if (Settings.UIProperties.UserLanguage.StartsWith("ja"))
        {
            // Japanese already contains PicView in translation
            GenericWindowInitialize(window, TranslationManager.Translation.About);
        }
        else
        {
            GenericWindowInitialize(window,$"{TranslationManager.Translation.About}  - PicView");
        }
    }
    
    public static void KeybindingsWindowInitialize(Window window)
    {
        window.Loaded += delegate
        {
            window.MinWidth = window.MaxWidth = window.Width;
            window.Title = $"{TranslationManager.Translation.ApplicationShortcuts}  - PicView";
        };
        window.KeyUp += (_, e) =>
        {
            if (e.Key is not Key.Escape)
            {
                return;
            }

            if (!MainKeyboardShortcuts.IsEscKeyEnabled)
            {
                return;
            }
            e.Handled = true;
            MainKeyboardShortcuts.IsEscKeyEnabled = false;
            window.Close();
        };
    }
    
    public static void GenericWindowInitialize(Window window, string title)
    {
        window.Loaded += delegate
        {
            if (!double.IsNaN(window.Width) && !double.IsInfinity(window.Width))
            {
                window.MinWidth = window.MaxWidth = window.Width;
            }
            
            window.Title = title;
        };
        window.KeyDown += (_, e) =>
        {
            if (e.Key is not Key.Escape)
            {
                return;
            }

            e.Handled = true;
            MainKeyboardShortcuts.IsEscKeyEnabled = false;
            window.Close();
        };
    }
}