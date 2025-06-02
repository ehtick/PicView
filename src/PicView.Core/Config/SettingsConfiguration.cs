namespace PicView.Core.Config;

public static class SettingsConfiguration
{
    public const double CurrentSettingsVersion = 1.6;
    
    public const string ConfigFolder = "Ruben2776/PicView/Config";
    public const string ConfigFileName = "UserSettings.json";
    private static string ConfigPath => Path.Combine(ConfigFolder, ConfigFileName);
    
    public static string RoamingSettingsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ConfigPath);
    
    // TODO delete this after next release
    public static string OldLocalSettingsPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigPath);
    
    public static string LocalSettingsPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", ConfigFileName);

    public static string CurrentUserSettingsPath =>
        File.Exists(RoamingSettingsPath) ? RoamingSettingsPath :
        File.Exists(LocalSettingsPath) ? LocalSettingsPath : 
        File.Exists(OldLocalSettingsPath) ? OldLocalSettingsPath : string.Empty;
}