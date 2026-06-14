using Avalonia.Input.Platform;
using PicView.Avalonia.Animations;
using PicView.Core.DebugTools;

namespace PicView.Avalonia.Clipboard;

/// <summary>
/// Handles clipboard operations related to text
/// </summary>
public static class ClipboardTextOperations
{
    /// <summary>
    /// Copies text to the clipboard
    /// </summary>
    /// <param name="text">The text to copy</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task<bool> CopyTextToClipboard(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }
        
        var clipboard = ClipboardService.GetClipboard();
        if (clipboard == null)
        {
            return false;
        }

        try
        {
            _ = AnimationsHelper.CopyAnimation();
            await clipboard.ClearAsync();
            await clipboard.SetTextAsync(text);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(ClipboardTextOperations), nameof(CopyTextToClipboard), ex);
            return false;
        }
        return true;
    }
}