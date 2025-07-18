namespace PicView.Core.Config.ConfigFileManagement;

public class ConfigFile(string configFileName)
{
    public const string ConfigFolder = "Ruben2776/PicView/Config";
    
    public string ConfigFileName { get; } = configFileName;
    private string ConfigPath => Path.Combine(ConfigFolder, ConfigFileName);
    
    public string RoamingConfigPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ConfigPath);
    
    public string LocalConfigPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", ConfigFileName);

    public string TryGetCurrentUserConfigPath =>
        File.Exists(RoamingConfigPath) ? RoamingConfigPath :
        File.Exists(LocalConfigPath) ? LocalConfigPath : string.Empty;

    public string? CorrectPath = null;
}