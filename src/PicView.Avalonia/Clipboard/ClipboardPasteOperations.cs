using System.Diagnostics;
using Avalonia.Input;
using Avalonia.Threading;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;

namespace PicView.Avalonia.Clipboard;

public static class ClipboardPasteOperations
{
    /// <summary>
    /// Pastes content from the clipboard
    /// </summary>
    /// <param name="vm">The main view model</param>
    public static async Task Paste(MainViewModel vm)
    {
        var clipboard = ClipboardService.GetClipboard();
        if (clipboard == null)
        {
            return;
        }

        try
        {
            // Need to use dispatcher to access clipboard in this instance
            var files = await Dispatcher.UIThread.InvokeAsync(async () => await clipboard.GetDataAsync(DataFormats.Files));
            if (files != null)
            {
                await ClipboardFileOperations.PasteFiles(files, vm);
                return;
            }

            // Try to paste text (URLs, file paths)
            var text = await clipboard.GetTextAsync();
            if (!string.IsNullOrWhiteSpace(text))
            {
                await NavigationManager.LoadPicFromStringAsync(text, vm).ConfigureAwait(false);
                return;
            }

            // Try to paste image data
            await ClipboardImageOperations.PasteClipboardImage(vm);
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"Paste operation failed: {ex.Message}");
#endif
        }
    }
}
