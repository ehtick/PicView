namespace PicView.Core.Config;

public static class SettingsConfiguration
{
    public const double CurrentSettingsVersion = 1.5;
    
    public const string ConfigFolder = "Ruben2776/PicView/Config";
    public const string ConfigFileName = "UserSettings.json";
    private static string ConfigPath => Path.Combine(ConfigFolder, ConfigFileName);
    
    public static string RoamingSettingsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ConfigPath);
    
    // TODO delete this after next release
    public static string BadLocalSettingsPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigPath);
    
    public static string LocalSettingsPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", ConfigFileName);

    public static string UserSettingsPath =>
        File.Exists(RoamingSettingsPath) ? RoamingSettingsPath :
        File.Exists(LocalSettingsPath) ? LocalSettingsPath : 
        File.Exists(BadLocalSettingsPath) ? BadLocalSettingsPath : string.Empty;
}