using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;

namespace PicView.Core.Config;

[JsonSourceGenerationOptions(AllowTrailingCommas = true, WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class SettingsGenerationContext : JsonSerializerContext;

/// <summary>
/// Provides functionality to manage loading, saving, and modifying application settings.
/// </summary>
public static class SettingsManager
{
    public static string? CurrentSettingsPath { get; private set; }

    public static AppSettings? Settings { get; private set; }
    
    public static async Task<bool> LoadSettingsAsync()
    {
        try
        {
            var path = ConfigFileManager.TryGetConfigFilePath(ConfigFileType.UserSettings);
            if (!string.IsNullOrEmpty(path))
            {
                CurrentSettingsPath = path;
                await ReadSettingsFromPathAsync(path).ConfigureAwait(false);
                return true;
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

    
    public static void SetDefaults() => Settings = GetDefaults();

    public static AppSettings GetDefaults()
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

        var settings = new AppSettings
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
        settings.UIProperties.UserLanguage = CultureInfo.CurrentCulture.Name;

        return settings;
    }

    /// <summary>
    ///     Deletes all settings files
    /// </summary>
    public static void DeleteSettingFiles()
    {
        try
        {
            DeleteFileIfExists(SettingsConfiguration.RoamingSettingsPath);
            DeleteFileIfExists(SettingsConfiguration.LocalSettingsPath);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(DeleteSettingFiles), ex);
        }
        
        return;

        void DeleteFileIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
    
    /// <summary>
    /// Reads and deserializes the settings from the specified file path asynchronously.
    /// </summary>
    /// <param name="path">The path to the JSON file containing the settings.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="JsonException">Thrown if deserialization of the settings fails.</exception>
    private static async Task ReadSettingsFromPathAsync(string path)
    {
        var jsonString = await File.ReadAllTextAsync(path).ConfigureAwait(false);

        if (JsonSerializer.Deserialize(
                jsonString, typeof(AppSettings), SettingsGenerationContext.Default) is not AppSettings settings)
        {
            throw new JsonException("Failed to deserialize settings");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            CurrentSettingsPath = path.Replace("/", "\\");
        }
        else
        {
            CurrentSettingsPath = path;
        }
        
        Settings = await UpgradeSettingsIfNeededAsync(settings).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the current application settings to the appropriate file location.
    /// </summary>
    /// <returns>
    /// Whether the settings were successfully saved.
    /// </returns>
    public static async Task<bool> SaveSettingsAsync()
    {
        if (Settings == null)
        {
            return false;
        }

        var saveLocation = await SaveConfigFileAndReturnPathAsync();
        if (string.IsNullOrWhiteSpace(saveLocation))
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(SaveSettingsAsync), "Empty save location");
            return false;
        }

        CurrentSettingsPath = saveLocation;
        return true;
    }


    /// <summary>
    ///     Upgrades settings to the current version if needed
    /// </summary>
    private static async Task<AppSettings> UpgradeSettingsIfNeededAsync(AppSettings settings)
    {
        if (settings.Version >= SettingsConfiguration.CurrentSettingsVersion)
        {
            return settings.WindowProperties is null ? GetDefaults() : settings;
        }

        if (settings.WindowProperties is null)
        {
            return GetDefaults();
        }

        await SynchronizeSettingsAsync(settings).ConfigureAwait(false);
        settings.Version = SettingsConfiguration.CurrentSettingsVersion;

        return settings;
    }

    private static async Task SynchronizeSettingsAsync(AppSettings existingSettings)
    {
        try
        {
            // Copy new property values to existing settings when missing
            MergeObjects(existingSettings, GetDefaults());

            // Save the synchronized settings
            Settings = existingSettings;
            await WriteJsonAsync();
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(SynchronizeSettingsAsync), ex);
        }
    }
    
    private static async Task WriteJsonAsync() =>
        await JsonFileHelper.WriteJsonAsync(CurrentSettingsPath, Settings, typeof(AppSettings), SettingsGenerationContext.Default).ConfigureAwait(false);
    
    private static async Task<string?> SaveConfigFileAndReturnPathAsync() =>
        await ConfigFileManager.SaveConfigFileAndReturnPathAsync(ConfigFileType.UserSettings, CurrentSettingsPath,
            Settings, typeof(AppSettings), SettingsGenerationContext.Default).ConfigureAwait(false);

    private static void MergeObjects<T>(T existing, T defaults) where T : class
    {
        if (existing == null || defaults == null)
        {
            return;
        }

        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanWrite || !property.CanRead)
            {
                continue;
            }

            var existingValue = property.GetValue(existing);
            var defaultValue = property.GetValue(defaults);

            if (existingValue == null && defaultValue != null)
            {
                // If existing is null, use the default value
                property.SetValue(existing, defaultValue);
            }
            else if (existingValue != null && defaultValue != null)
            {
                // If both exist and it's a complex type, merge recursively
                if (!IsComplexType(property.PropertyType))
                {
                    continue;
                }

                var mergeMethod = typeof(SettingsManager)
                    .GetMethod(nameof(MergeObjects), BindingFlags.NonPublic | BindingFlags.Static)
                    ?.MakeGenericMethod(property.PropertyType);

                mergeMethod?.Invoke(null, [existingValue, defaultValue]);
            }
        }
    }

    private static bool IsComplexType(Type type)
    {
        return !type.IsPrimitive &&
               type != typeof(string) &&
               type != typeof(DateTime) &&
               type != typeof(decimal) &&
               type is { IsEnum: false, IsValueType: false };
    }
}