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

    /// <summary>
    /// Gets the file path to the currently loaded settings file, if available.
    /// </summary>
    /// <remarks>
    /// This property reflects the full path to the settings file currently in use by the application.
    /// It is updated when settings are loaded via <see cref="SettingsManager.LoadSettingsAsync"/>
    /// or saved using <see cref="SettingsManager.SaveSettingsAsync"/>. If no settings file is loaded,
    /// the value will be null. This path can be used to reference the specific configuration file
    /// being utilized or modified.
    /// </remarks>
    public static string? CurrentSettingsPath { get; private set; }

    /// <summary>
    /// Gets or sets the current application settings instance, which stores all configurable
    /// values for the application's behavior, appearance, and functionality.
    /// </summary>
    /// <remarks>
    /// This property can be used to retrieve or assign settings related to application preferences,
    /// such as UI, theme, sorting, scaling, and startup configurations. The associated settings
    /// are loaded via <see cref="SettingsManager.LoadSettingsAsync"/> or set to defaults using
    /// <see cref="SettingsManager.SetDefaults"/>. Changes to the settings can be saved using
    /// <see cref="SettingsManager.SaveSettingsAsync"/>.
    /// </remarks>
    public static AppSettings? Settings { get; private set; }

    /// <summary>
    /// Loads application settings asynchronously from a file or initializes them to default if loading fails.
    /// </summary>
    /// <returns>
    /// A boolean indicating whether the settings were successfully loaded.
    /// </returns>
    /// <exception cref="JsonException">Thrown if deserialization of the settings file fails.</exception>
    public static async Task<bool> LoadSettingsAsync()
    {
        try
        {
            var path = ConfigFileManager.TryGetConfigFilePath(ConfigFileType.UserSettings);
            if (!string.IsNullOrEmpty(path))
            {
                CurrentSettingsPath = path;
                var jsonString = await File.ReadAllTextAsync(path).ConfigureAwait(false);

                if (JsonSerializer.Deserialize(
                        jsonString, typeof(AppSettings), SettingsGenerationContext.Default) is not AppSettings settings)
                {
                    throw new JsonException("Failed to deserialize settings");
                }

                Settings = await UpgradeSettingsIfNeededAsync(settings).ConfigureAwait(false);
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

    /// <summary>
    /// Synchronizes the settings with defaults, adding missing properties and saving.
    /// </summary>
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

    /// <summary>
    /// Sets the application's settings to their default values.
    /// </summary>
    public static void SetDefaults() => Settings = GetDefaults();

    /// <summary>
    /// Initializes and returns an instance of the default application settings.
    /// </summary>
    /// <returns>
    /// An <see cref="AppSettings"/> object populated with default values.
    /// </returns>
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
    /// Resets the application's settings to their default values and removes any existing settings file
    /// to start with a clean configuration.
    /// </summary>
    /// <exception cref="IOException">Thrown if an error occurs while attempting to delete the existing settings file.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if access to the settings file is denied during deletion.</exception>
    /// <remarks>
    /// This method ensures that the default settings are applied and deletes the user settings file if it exists,
    /// allowing the application configuration to be completely reset.
    /// </remarks>
    public static void ResetDefaults()
    {
        try
        {
            DeleteFileIfExists(SettingsConfiguration.CurrentUserSettingsPath);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(SettingsManager), nameof(ResetDefaults), ex);
        }
        finally
        {
            SetDefaults();
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

    #region Helpers

    private static async Task WriteJsonAsync() =>
        await JsonFileHelper
            .WriteJsonAsync(CurrentSettingsPath, Settings, typeof(AppSettings), SettingsGenerationContext.Default)
            .ConfigureAwait(false);

    private static async Task<string?> SaveConfigFileAndReturnPathAsync() =>
        await ConfigFileManager.SaveConfigFileAndReturnPathAsync(ConfigFileType.UserSettings, CurrentSettingsPath,
            Settings, typeof(AppSettings), SettingsGenerationContext.Default).ConfigureAwait(false);

    /// <summary>
    /// Recursively merges properties from defaults into existing settings.
    /// </summary>
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

    #endregion
}