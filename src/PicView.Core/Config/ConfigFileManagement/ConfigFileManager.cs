using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.Keybindings;

namespace PicView.Core.Config.ConfigFileManagement;

/// <summary>
/// Provides functionality to manage configuration files, including saving and retrieving paths for various config file types.
/// </summary>
public static class ConfigFileManager
{
    /// <summary>
    /// Saves the configuration file to the specified path and returns the resulting file path.
    /// </summary>
    /// <param name="type">The type of configuration file to be saved (e.g., user settings, file history, key bindings).</param>
    /// <param name="path">The file path where the configuration file should be saved. If null, a default path is resolved based on the configuration type.</param>
    /// <param name="value">The object containing the configuration data to be saved.</param>
    /// <param name="inputType">The type of the configuration object to be serialized.</param>
    /// <param name="context">The JSON serializer context used for serializing the configuration data.</param>
    /// <returns>The file path where the configuration file was successfully saved, or null if the operation failed.</returns>
    /// <remarks>If the initial save location is not writable, it attempts to save to the roaming app data directory.</remarks>
    public static async Task<string?> SaveConfigFileAndReturnPathAsync(ConfigFileType type, string? path, object? value,
        Type inputType, JsonSerializerContext context)
    {
        // If null, try to get the current user file, if exist
        path ??= GetConfigPath(type, ConfigPathKind.CurrentUser);

        try
        {
            if (type == ConfigFileType.UserSettings)
            {
                CleanupOldConfigPath();
            }

            if (!FileHelper.IsPathWritable(path))
            {
                return await TrySaveRoaming();
            }

            await JsonFileHelper.WriteJsonAsync(path, value, inputType, context).ConfigureAwait(false);
            return path;
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
                DebugHelper.LogDebug(nameof(ConfigFileManager), nameof(SaveConfigFileAndReturnPathAsync), ex);
                return null;
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(ConfigFileManager), nameof(SaveConfigFileAndReturnPathAsync), ex);
            return null;
        }

        async Task<string> TrySaveRoaming()
        {
            var roamingPath = GetConfigPath(type, ConfigPathKind.Roaming);

            FileHelper.EnsureDirectoryExists(roamingPath);
            await JsonFileHelper.WriteJsonAsync(roamingPath, value, inputType, context).ConfigureAwait(false);

            return roamingPath;
        }

        // TODO delete this after next release
        void CleanupOldConfigPath()
        {
            if (!File.Exists(SettingsConfiguration.OldLocalSettingsPath))
            {
                return;
            }

            path = SettingsConfiguration.LocalSettingsPath;

            File.Delete(SettingsConfiguration.OldLocalSettingsPath);

            FileHelper.DeleteDirectoryIfExists(Path.GetDirectoryName(SettingsConfiguration.OldLocalSettingsPath));
            FileHelper.DeleteDirectoryIfExists(
                Path.GetDirectoryName(Path.GetDirectoryName(SettingsConfiguration.OldLocalSettingsPath)));
            FileHelper.DeleteDirectoryIfExists(Path.GetDirectoryName(
                Path.GetDirectoryName(Path.GetDirectoryName(SettingsConfiguration.OldLocalSettingsPath))));
        }
    }

    /// <summary>
    /// Resolves the default configuration file path based on the specified configuration file type and the operating system in use.
    /// </summary>
    /// <param name="type">The type of configuration file for which the default path needs to be resolved (e.g., user settings, file history, key bindings).</param>
    /// <returns>The resolved default configuration file path as a string.</returns>
    public static string ResolveDefaultConfigPath(ConfigFileType type)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // On macOS, always use the roaming path. We can't save inside an app bundle.
            return GetConfigPath(type, ConfigPathKind.Roaming);
        }

        var path = GetConfigPath(type, ConfigPathKind.CurrentUser);
        return path.Replace("/", "\\");
    }

    private static string GetConfigPath(ConfigFileType type, ConfigPathKind kind)
    {
        switch (kind)
        {
            case ConfigPathKind.CurrentUser:
                switch (type)
                {
                    case ConfigFileType.FileHistory:
                        return UserPath(ConfigFileType.FileHistory);
                    case ConfigFileType.KeyBindings:
                        return UserPath(ConfigFileType.KeyBindings);
                    case ConfigFileType.UserSettings:
                    default:
                        return UserPath(ConfigFileType.UserSettings);
                }
            case ConfigPathKind.Roaming:
                return type switch
                {
                    ConfigFileType.FileHistory => FileHistoryConfiguration.RoamingFileHistoryPath,
                    ConfigFileType.KeyBindings => KeyBindingsConfiguration.RoamingKeybindingsPath,
                    _ => SettingsConfiguration.RoamingSettingsPath
                };
            case ConfigPathKind.Local:
                return type switch
                {
                    ConfigFileType.FileHistory => FileHistoryConfiguration.LocalFileHistoryPath,
                    ConfigFileType.KeyBindings => KeyBindingsConfiguration.LocalKeybindingsPath,
                    _ => SettingsConfiguration.LocalSettingsPath
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Invalid configuration path kind.");
        }
    }

    private static string UserPath(ConfigFileType type)
    {
        var currentUserPath = type switch
        {
            ConfigFileType.FileHistory => FileHistoryConfiguration.CurrentUserFileHistoryPath,
            ConfigFileType.KeyBindings => KeyBindingsConfiguration.CurrentUserKeybindingsPath,
            _ => SettingsConfiguration.CurrentUserSettingsPath
        };
        if (currentUserPath != string.Empty)
        {
            return currentUserPath;
        }

        var localPath = type switch
        {
            ConfigFileType.FileHistory => FileHistoryConfiguration.LocalFileHistoryPath,
            ConfigFileType.KeyBindings => KeyBindingsConfiguration.LocalKeybindingsPath,
            _ => SettingsConfiguration.LocalSettingsPath
        };
        return FileHelper.IsPathWritable(localPath) ? localPath : GetConfigPath(type, ConfigPathKind.Roaming);
    }
}