using Avalonia.Controls;
using PicView.Core.Keybindings;

namespace PicView.Avalonia.Views.UC.Buttons;

public partial class SettingsMenuButton : UserControl
{
    public SettingsMenuButton()
    {
        InitializeComponent();
        ToolTip.SetTip(UserSettingsItem, CurrentSettingsPath);
        ToolTip.SetTip(KeybindingsItem, KeybindingFunctions.CurrentKeybindingsPath);
    }
}