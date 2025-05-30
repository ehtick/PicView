using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using PicView.Core.DebugTools;

namespace PicView.Core.Config;

[JsonSourceGenerationOptions(AllowTrailingCommas = true, WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class SettingsGenerationContext : JsonSerializerContext;

/// <summary>
/// Provides functionality to manage loading, saving, and modifying application settings.
/// </summary>
public static class SettingsManager
{
    /// Gets the file path of the currently loaded settings.
    /// This property represents the path to the settings file that was most recently
    /// loaded into the application. If no settings file has been loaded, this property
    /// will return null.
    /// This value is updated whenever settings are read from a file, and it is used as
    /// the default path for saving settings back to a file. The path is normalized to use
    /// backslashes as directory separators.
    /// This property is read-only and can only be set internally within the `SettingsManager`
    /// class.
    public static string? CurrentSettingsPath { get; private set; }

    /// Gets or sets the current application settings.
    /// This property holds the application's configuration settings encapsulated in an `AppSettings` instance.
    /// It is updated when settings are loaded, either from a file using `LoadSettingsAsync` or by setting default
    /// values with the `SetDefaults` method.
    /// Changes made to this property will affect the behavior and appearance of the application during runtime.
    /// This property is managed internally within the `SettingsManager` class and cannot be set externally.
    public static AppSettings? Settings { get; private set; }
    
    public static async Task<bool> LoadSettingsAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await LoadFromPathAsync(SettingsConfiguration.RoamingSettingsPath).ConfigureAwait(false);
            }

            var path = SettingsConfiguration.UserSettingsPath;
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
                ? SettingsConfiguration.LocalSettingsPath
                : SettingsConfiguration.RoamingSettingsPath;

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

        try
        {
            if (string.IsNullOrWhiteSpace(CurrentSettingsPath))
            {
                return await TrySaveLocal();
            }

            DeleteBadPath();
            await SaveSettingsToPathAsync(CurrentSettingsPath).ConfigureAwait(false);

            return true;

        }
        catch (UnauthorizedAccessException)
        {
            // If unauthorized, try saving to roaming app data
            try
            {
                return await TrySaveRoaming();
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

        async Task<bool> TrySaveRoaming()
        {
            var roamingPath = SettingsConfiguration.RoamingSettingsPath;
            EnsureDirectoryExists(roamingPath);
            await SaveSettingsToPathAsync(roamingPath).ConfigureAwait(false);
            return true;
        }
        
        async Task<bool> TrySaveLocal()
        {
            var localPath = SettingsConfiguration.LocalSettingsPath;
            EnsureDirectoryExists(localPath);
            await SaveSettingsToPathAsync(localPath).ConfigureAwait(false);
            return true;
        }

        // TODO delete this after next release
        void DeleteBadPath()
        {
            if (!File.Exists(SettingsConfiguration.BadLocalSettingsPath))
            {
                return;
            }

            File.Delete(SettingsConfiguration.BadLocalSettingsPath);
            
            var firstDirectory = Path.GetDirectoryName(SettingsConfiguration.BadLocalSettingsPath);
            if (Directory.Exists(firstDirectory))
            {
                Directory.Delete(firstDirectory);
            }
            var secondDirectory = Path.GetDirectoryName(firstDirectory);
            if (Directory.Exists(secondDirectory))
            {
                Directory.Delete(secondDirectory);
            }
            var thirdDirectory = Path.GetDirectoryName(secondDirectory);
            if (Directory.Exists(thirdDirectory))
            {
                Directory.Delete(thirdDirectory);
            }

            CurrentSettingsPath = SettingsConfiguration.LocalSettingsPath;
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
            await SaveSettingsToPathAsync(CurrentSettingsPath).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(SynchronizeSettingsAsync), ex);
        }
    }

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