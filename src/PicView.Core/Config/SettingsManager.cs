using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using PicView.Core.DebugTools;

namespace PicView.Core.Config;

[JsonSourceGenerationOptions(AllowTrailingCommas = true, WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class SettingsGenerationContext : JsonSerializerContext;

public static class SettingsManager
{
    public static string? CurrentSettingsPath { get; private set; }

    public static AppSettings? Settings { get; private set; }

    /// <summary>
    ///     Asynchronously loads the user settings. Loads defaults if not found
    /// </summary>
    /// <returns>True if settings exists</returns>
    public static async Task<bool> LoadSettingsAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await LoadFromPathAsync(GetRoamingSettingsPath()).ConfigureAwait(false);
            }

            var path = GetUserSettingsPath();
            if (!string.IsNullOrEmpty(path))
            {
                return await LoadFromPathAsync(path).ConfigureAwait(false);
            }

            SetDefaults();
            return false;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(LoadSettingsAsync), ex);
            SetDefaults();
            return false;
        }
    }

    /// <summary>
    ///     Determines the path to the user settings file
    /// </summary>
    /// <returns>Path to the user settings file, or empty string if not found</returns>
    public static string GetUserSettingsPath()
    {
        var roamingPath = GetRoamingSettingsPath();
        return File.Exists(roamingPath) ? roamingPath : GetLocalSettingsPath();
    }

    /// <summary>
    ///     Gets the path to the roaming settings file
    /// </summary>
    private static string GetRoamingSettingsPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            SettingsConfiguration.RoamingConfigPath);

    /// <summary>
    ///     Gets the path to the local settings file
    /// </summary>
    private static string GetLocalSettingsPath() =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsConfiguration.LocalConfigFilePath);

    /// <summary>
    ///     Sets default settings values
    /// </summary>
    public static void SetDefaults()
    {
        UIProperties uiProperties;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            uiProperties = new UIProperties
            {
                IsTaskbarProgressEnabled = false,
                OpenInSameWindow = true
            };
        }
        else
        {
            uiProperties = new UIProperties();
        }

        Settings = new AppSettings
        {
            UIProperties = uiProperties,
            Gallery = new Gallery(),
            ImageScaling = new ImageScaling(),
            Sorting = new Sorting(),
            Theme = new Theme(),
            WindowProperties = new WindowProperties(),
            Zoom = new Zoom(),
            StartUp = new StartUp(),
            Version = SettingsConfiguration.CurrentSettingsVersion
        };

        // Get the default culture from the OS
        Settings.UIProperties.UserLanguage = CultureInfo.CurrentCulture.Name;
    }

    /// <summary>
    ///     Deletes all settings files
    /// </summary>
    public static void DeleteSettingFiles()
    {
        try
        {
            DeleteFileIfExists(GetRoamingSettingsPath());
            DeleteFileIfExists(GetLocalSettingsPath());
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(DeleteSettingFiles), ex);
        }
    }

    /// <summary>
    ///     Deletes a file if it exists
    /// </summary>
    private static void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    ///     Loads settings from the specified path
    /// </summary>
    private static async Task<bool> LoadFromPathAsync(string path)
    {
        try
        {
            await ReadSettingsFromPathAsync(path).ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            // If primary path fails, try the alternative path
            var alternativePath = Path.GetDirectoryName(path)?.Contains("ApplicationData") == true
                ? GetLocalSettingsPath()
                : GetRoamingSettingsPath();

            if (File.Exists(alternativePath))
            {
                try
                {
                    await ReadSettingsFromPathAsync(alternativePath).ConfigureAwait(false);
                    return true;
                }
                catch (Exception)
                {
                    SetDefaults();
                }
            }
            else
            {
                SetDefaults();
            }

            return false;
        }
    }

    /// <summary>
    ///     Reads settings from the specified path and upgrades them if necessary
    /// </summary>
    private static async Task ReadSettingsFromPathAsync(string path)
    {
        var jsonString = await File.ReadAllTextAsync(path).ConfigureAwait(false);

        if (JsonSerializer.Deserialize(
                jsonString, typeof(AppSettings), SettingsGenerationContext.Default) is not AppSettings settings)
        {
            throw new JsonException("Failed to deserialize settings");
        }

        Settings = await UpgradeSettingsIfNeededAsync(settings).ConfigureAwait(false);
        CurrentSettingsPath = path.Replace("/", "\\");
    }

    /// <summary>
    ///     Saves settings to disk
    /// </summary>
    public static async Task<bool> SaveSettingsAsync()
    {
        if (Settings == null)
        {
            return false;
        }

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                await SaveSettingsToPathAsync(GetRoamingSettingsPath()).ConfigureAwait(false);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(CurrentSettingsPath))
            {
                await SaveSettingsToPathAsync(CurrentSettingsPath).ConfigureAwait(false);
            }
            else
            {
                await SaveSettingsToPathAsync(GetUserSettingsPath()).ConfigureAwait(false);
            }

            return true;
        }
        catch (UnauthorizedAccessException)
        {
            // If unauthorized, try saving to roaming app data
            try
            {
                var roamingPath = GetRoamingSettingsPath();
                EnsureDirectoryExists(roamingPath);
                await SaveSettingsToPathAsync(roamingPath).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                DebugHelper.LogDebug(nameof(SettingsManager), nameof(SaveSettingsAsync), ex);
                return false;
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(SaveSettingsAsync), ex);
            return false;
        }
    }

    /// <summary>
    ///     Ensures that the directory for a file path exists
    /// </summary>
    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    ///     Saves settings to the specified path
    /// </summary>
    private static async Task SaveSettingsToPathAsync(string path)
    {
        if (Settings == null)
        {
            return;
        }

        var json = JsonSerializer.Serialize(Settings, typeof(AppSettings), SettingsGenerationContext.Default);
        await File.WriteAllTextAsync(path, json).ConfigureAwait(false);
    }

    /// <summary>
    ///     Upgrades settings to the current version if needed
    /// </summary>
    private static async Task<AppSettings> UpgradeSettingsIfNeededAsync(AppSettings settings)
    {
        if (settings.Version >= SettingsConfiguration.CurrentSettingsVersion)
        {
            return settings;
        }

        await SynchronizeSettingsAsync(settings).ConfigureAwait(false);
        settings.Version = SettingsConfiguration.CurrentSettingsVersion;

        return settings;
    }

    /// <summary>
    ///     Synchronizes settings between different versions
    /// </summary>
    private static async Task SynchronizeSettingsAsync(AppSettings newSettings)
    {
        try
        {
            var localPath = GetLocalSettingsPath();
            if (!File.Exists(localPath))
            {
                return;
            }

            var jsonString = await File.ReadAllTextAsync(localPath).ConfigureAwait(false);

            if (JsonSerializer.Deserialize(
                    jsonString, typeof(AppSettings),
                    SettingsGenerationContext.Default) is not AppSettings existingSettings)
            {
                return;
            }

            // Copy new property values to existing settings when missing
            MergeSettings(existingSettings, newSettings);

            // Save the synchronized settings
            Settings = existingSettings;
            await SaveSettingsToPathAsync(localPath).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(SynchronizeSettingsAsync), ex);
        }
    }

    /// <summary>
    ///     Merges settings by copying properties from newSettings to existingSettings
    ///     where the property is missing or null in existingSettings
    /// </summary>
    private static void MergeSettings(AppSettings existingSettings, AppSettings newSettings)
    {
        existingSettings.UIProperties ??= newSettings.UIProperties;
        existingSettings.Gallery ??= newSettings.Gallery;
        existingSettings.Theme ??= newSettings.Theme;
        existingSettings.Sorting ??= newSettings.Sorting;
        existingSettings.ImageScaling ??= newSettings.ImageScaling;
        existingSettings.WindowProperties ??= newSettings.WindowProperties;
        existingSettings.Zoom ??= newSettings.Zoom;
        existingSettings.StartUp ??= newSettings.StartUp;

        // Fallback for any properties missing in older versions
        foreach (var property in typeof(AppSettings).GetProperties())
        {
            var existingValue = property.GetValue(existingSettings);
            if (existingValue != null)
            {
                continue;
            }

            var newValue = property.GetValue(newSettings);
            property.SetValue(existingSettings, newValue);
        }
    }
}