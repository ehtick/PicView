using PicView.Core.MacOS.AppleScripts;

namespace PicView.Avalonia.MacOS.WindowImpl;

public  static class DockSizeHelper
{
    private const string Script = """

                                  set dockSize to do shell script "defaults read com.apple.dock tilesize"
                                  set dockHidden to do shell script "defaults read com.apple.dock autohide"
                                  return dockSize & "," & dockHidden

                                  """;
    
    public static async Task SetDockSizeAsync()
    {
        var result = await GetDockSizeAsync();
        Settings.WindowProperties.Padding = result;
    }
    
    public static async Task<double> GetDockSizeAsync()
    {
        var result = await AppleScriptManager.ExecuteAppleScriptWithResultAsync(Script);
        if (!string.IsNullOrWhiteSpace(result))
        {
            var parts = result.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out var dockSize) && int.TryParse(parts[1], out var hidden))
            {
                if (hidden == 1)
                {
                    // ReSharper disable once PossibleLossOfFraction
                    return dockSize / 2;
                }

                return dockSize;
            }
        }
        return -1;
    }
}
