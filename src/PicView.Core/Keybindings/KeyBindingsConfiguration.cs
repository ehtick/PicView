using PicView.Core.Config;

namespace PicView.Core.Keybindings;

public static class KeyBindingsConfiguration
{
    public const string KeybindingsFileName = "keybindings.json";
    public static string KeybindingsFilePath => Path.Combine(SettingsConfiguration.ConfigFolder, KeybindingsFileName);
    public static string RoamingKeybindingsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            KeybindingsFilePath);
    
    public static string LocalKeybindingsPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", KeybindingsFileName);
    
    public static string CurrentUserKeybindingsPath =>
        File.Exists(RoamingKeybindingsPath) ? RoamingKeybindingsPath :
        File.Exists(LocalKeybindingsPath) ? LocalKeybindingsPath : string.Empty;
}