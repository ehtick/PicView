using PicView.Core.Config;

namespace PicView.Core.Keybindings;

internal static class KeyBindingsConfiguration
{
    internal const string KeybindingsFileName = "keybindings.json";
    internal static string KeybindingsFilePath => Path.Combine(SettingsConfiguration.ConfigFolder, KeybindingsFileName);
    internal static string RoamingKeybindingsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            KeybindingsFilePath);
    
    internal static string LocalKeybindingsPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", KeybindingsFileName);
    
    internal static string CurrentUserKeybindingsPath =>
        File.Exists(RoamingKeybindingsPath) ? RoamingKeybindingsPath :
        File.Exists(LocalKeybindingsPath) ? LocalKeybindingsPath : string.Empty;
}