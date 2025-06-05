using PicView.Core.Config.ConfigFileManagement;

namespace PicView.Core.Keybindings;

public static class KeybindingFunctions
{
    public static string? CurrentKeybindingsPath { get; private set; }
    
    public static async Task SaveKeyBindingsFile(string json)
    {
        try
        {
            if (string.IsNullOrEmpty(CurrentKeybindingsPath))
            {
                CurrentKeybindingsPath = ConfigFileManager.ResolveDefaultConfigPath(ConfigFileType.KeyBindings);
            }
            await using var writer = new StreamWriter(CurrentKeybindingsPath);
            await writer.WriteAsync(json).ConfigureAwait(false);
        }
        catch (UnauthorizedAccessException)
        {
            await using var writer = new StreamWriter(KeyBindingsConfiguration.RoamingKeybindingsPath);
            await writer.WriteAsync(json).ConfigureAwait(false);
        }
    }

    public static async Task<string?> LoadKeyBindingsFile()
    {
        var path = ConfigFileManager.ResolveDefaultConfigPath(ConfigFileType.KeyBindings);
        if (!File.Exists(path))
        {
            return null;
        }

        var text = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        
        CurrentKeybindingsPath = path;
        return text;
    }
}