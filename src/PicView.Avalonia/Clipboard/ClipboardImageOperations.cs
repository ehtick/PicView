using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using PicView.Avalonia.Animations;
using PicView.Avalonia.Crop;
using PicView.Avalonia.Navigation;
using PicView.Core.DebugTools;
using PicView.Core.Localization;
using PicView.Core.Models;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Clipboard;

/// <summary>
///     Handles clipboard operations related to images
/// </summary>
public static class ClipboardImageOperations
{
    /// <summary>
    ///     Copies the current image to the clipboard
    /// </summary>
    public static async Task CopyImageToClipboard(MainWindowViewModel vm)
    {
        var clipboard = ClipboardService.GetClipboard();
        if (clipboard == null)
        {
            return;
        }
        if (vm.WindowTabs.ActiveTab.CurrentValue.CropService is CropService { IsCropping: true } cropService)
        {
            if (cropService.GetCroppedImage() is Bitmap clipboardBitmap)
            {
                await CopyImageToClipboard(clipboard, clipboardBitmap);
                return;
            }
        }
        if (vm.WindowTabs.ActiveTab.CurrentValue.Image.CurrentValue is not Bitmap bitmap)
        {
            return;
        }
        await CopyImageToClipboard(clipboard, bitmap);
    }
    
    public static async Task CopyImageToClipboard(IClipboard clipboard, Bitmap bitmap)
    {
        _ = AnimationsHelper.CopyAnimation();
        await clipboard.ClearAsync();
        await clipboard.SetBitmapAsync(bitmap);
    }

    /// <summary>
    ///     Copies an image as base64 string to the clipboard
    /// </summary>
    /// <param name="path">Path to the image file</param>
    public static async Task<bool> CopyBase64ToClipboard(string path)
    {
        var clipboard = ClipboardService.GetClipboard();
        if (clipboard == null)
        {
            return false;
        }
        
        var base64 = await GetBase64String(path);
        
        if (string.IsNullOrEmpty(base64))
        {
            return false;
        }
        _ = AnimationsHelper.CopyAnimation();
        
        try
        {
            await clipboard.ClearAsync();
            await clipboard.SetTextAsync(base64);
            return true;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(ClipboardImageOperations), nameof(CopyBase64ToClipboard), ex);
            return false;
        }
    }

    private static async Task<string> GetBase64String(string path)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            return Convert.ToBase64String(await File.ReadAllBytesAsync(path));
        }
        return null; // TODO handle non-image types, such as SVGs
    }

    /// <summary>
    ///     Pastes an image from the clipboard
    /// </summary>
    /// <param name="vm">The main view model</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task PasteClipboardImage(MainWindowViewModel vm)
    {
        var clipboard = ClipboardService.GetClipboard();
        if (clipboard == null)
        {
            return;
        }

        try
        {
            var bitmap = await clipboard.TryGetBitmapAsync();
            if (bitmap is null)
            {
                return;
            }
            UpdateImage.SetSingleImage(vm, bitmap, SingleImageType.Clipboard, TranslationManager.Translation.ClipboardImage);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(ClipboardImageOperations), nameof(PasteClipboardImage), ex);
        }
    }
    
    public static async Task CopyImageToClipboard(Bitmap bitmap)
    {
        var clipboard = ClipboardService.GetClipboard();
        if (clipboard == null)
        {
            return;
        }
        await clipboard.ClearAsync();
        await clipboard.SetBitmapAsync(bitmap);
    }
}