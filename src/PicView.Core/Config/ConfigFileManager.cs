using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.Keybindings;

namespace PicView.Core.Config;

public enum ConfigFileType
{
    UserSettings,
    FileHistory,
    KeyBindings
}

public static class ConfigFileManager
{
    public static async Task<string?> SaveConfigFileAndReturnPathAsync(ConfigFileType type, string? path, object? value, Type inputType, JsonSerializerContext context)
    {
        path ??= type switch
        {
            // If null, try to get the current user file, if exist
            ConfigFileType.UserSettings => SettingsConfiguration.CurrentUserSettingsPath,
            ConfigFileType.FileHistory => FileHistoryConfiguration.CurrentUserFileHistoryPath,
            ConfigFileType.KeyBindings => KeyBindingsConfiguration.CurrentUserKeybindingsPath,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return await TrySaveLocal();
            }

            CleanupOldConfigPath();
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
            var roamingPath = type switch
            {
                ConfigFileType.UserSettings => SettingsConfiguration.RoamingSettingsPath,
                ConfigFileType.FileHistory => FileHistoryConfiguration.RoamingFileHistoryPath,
                ConfigFileType.KeyBindings => KeyBindingsConfiguration.RoamingKeybindingsPath,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            
            FileHelper.EnsureDirectoryExists(roamingPath);
            await JsonFileHelper.WriteJsonAsync(roamingPath, value, inputType, context).ConfigureAwait(false);
            
            return roamingPath;
        }
        
        async Task<string> TrySaveLocal()
        {
            var localPath = type switch
            {
                ConfigFileType.UserSettings => SettingsConfiguration.LocalSettingsPath,
                ConfigFileType.FileHistory => FileHistoryConfiguration.LocalFileHistoryPath,
                ConfigFileType.KeyBindings => KeyBindingsConfiguration.LocalKeybindingsPath,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            if (type is ConfigFileType.KeyBindings)
            {
                File.Create(localPath);
            }
            else
            {
                await JsonFileHelper.WriteJsonAsync(localPath, value, inputType, context).ConfigureAwait(false);
            }

            
            return localPath;
        }

        // TODO delete this after next release
        void CleanupOldConfigPath()
        {
            if (!File.Exists(SettingsConfiguration.OldLocalSettingsPath))
            {
                return;
            }

            File.Delete(SettingsConfiguration.OldLocalSettingsPath);
            
            var firstDirectory = Path.GetDirectoryName(SettingsConfiguration.OldLocalSettingsPath);
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

            path = SettingsConfiguration.LocalSettingsPath;
        }
    }

    public static string TryGetConfigFilePath(ConfigFileType type)
    {
        // On macOS, always use the roaming path. We can't save inside an app bundle.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return type switch
            {
                // If null, try to get the current user file, if exist
                ConfigFileType.UserSettings => SettingsConfiguration.RoamingSettingsPath,
                ConfigFileType.FileHistory => FileHistoryConfiguration.RoamingFileHistoryPath,
                ConfigFileType.KeyBindings => KeyBindingsConfiguration.RoamingKeybindingsPath,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        var path = type switch
        {
            // If null, try to get the current user file, if exist
            ConfigFileType.UserSettings => SettingsConfiguration.CurrentUserSettingsPath,
            ConfigFileType.FileHistory => FileHistoryConfiguration.CurrentUserFileHistoryPath,
            ConfigFileType.KeyBindings => KeyBindingsConfiguration.CurrentUserKeybindingsPath,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        return path.Replace("/", "\\");
    }
}