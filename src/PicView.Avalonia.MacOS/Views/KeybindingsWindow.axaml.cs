using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Config;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class KeybindingsWindow : GenericWindow
{

    public KeybindingsWindow(KeybindingWindowConfig config)
    {
        InitializeComponent();
        if (Settings.Theme.GlassTheme)
        {
            WindowBorder.Background = Brushes.Transparent;
            XKeybindingsView.Background = Brushes.Transparent;
        }
        else if (!Settings.Theme.Dark)
        {
            XKeybindingsView.Background = UIHelper.GetMenuBackgroundColor();
        }
        GenericWindowHelper.KeybindingsWindowInitialize(this, config.WindowProperties);
    }
}