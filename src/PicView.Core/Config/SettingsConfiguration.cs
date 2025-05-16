namespace PicView.Core.Config;

public static class SettingsConfiguration
{
    public const double CurrentSettingsVersion = 1.5;
    
    public const string ConfigFileName = "UserSettings.json";
    public const string LocalConfigFilePath = "Config/" + ConfigFileName;
    public const string RoamingConfigFolder = "Ruben2776/PicView/Config";
    public const string RoamingConfigPath = RoamingConfigFolder + "/" + ConfigFileName;
    
    public const string HistoryFileName = "FileHistory.json";
    public const string LocalHistoryFilePath = "Config/" + HistoryFileName;
    public const string RoamingFileHistoryPath = RoamingConfigFolder + "/" + HistoryFileName;
}