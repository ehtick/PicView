using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using PicView.Avalonia.Animations;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileHandling;
using PicView.Core.Localization;
using PicView.Core.ProcessHandling;

namespace PicView.Avalonia.Clipboard;

/// <summary>
/// Helper class for clipboard operations
/// </summary>
public static class ClipboardHelper
{
    /// <summary>
    /// Duplicates the current file and navigates to it
    /// </summary>
    /// <param name="vm">The main view model</param>
    public static async Task DuplicateCurrentFile(MainViewModel vm)
    {
        if (!NavigationManager.CanNavigate(vm))
        {
            return;
        }

        vm.IsLoading = true;
        var oldPath = vm.FileInfo.FullName;
        var duplicatedPath = await FileHelper.DuplicateAndReturnFileNameAsync(oldPath, vm.FileInfo);

        if (string.IsNullOrWhiteSpace(duplicatedPath) || !File.Exists(duplicatedPath))
        {
            return;
        }

        await Task.WhenAll(AnimationsHelper.CopyAnimation(), NavigationManager.LoadPicFromFile(duplicatedPath, vm));
    }
    
    /// <summary>
    /// Duplicates the specified file and plays a copy animation when done. The original file is not navigated away from.
    /// </summary>
    /// <param name="path">Path to the file to duplicate</param>
    public static async Task DuplicateFile(string path)
    {
        var duplicatedPath = await FileHelper.DuplicateAndReturnFileNameAsync(path);
        if (!string.IsNullOrWhiteSpace(duplicatedPath))
        {
            await AnimationsHelper.CopyAnimation();
        }
    }
    
    /// <summary>
    /// Duplicates the specified file, either the current file or another one specified by path.
    /// If the current file is being duplicated, the view model will navigate to the duplicated file.
    /// </summary>
    /// <param name="path">Path to the file to duplicate, or null to duplicate the current file.</param>
    /// <param name="vm">The main view model</param>
    public static async Task Duplicate(string path, MainViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }
        try
        {
            vm.IsLoading = true;
            
            if (path == vm.FileInfo?.FullName)
            {
                await DuplicateCurrentFile(vm);
            }
            else
            {
                await DuplicateFile(path);
            }
        }
        catch (Exception e)
        {
#if DEBUG
            Debug.WriteLine($"{nameof(ClipboardHelper)} {nameof(Duplicate)} {e.StackTrace}");
#endif
            await TooltipHelper.ShowTooltipMessageAsync(TranslationHelper.Translation.UnexpectedError);
        }
        finally
        {
            vm.IsLoading = false;
        }
    }

    /// <summary>
    /// Copies text to the clipboard
    /// </summary>
    /// <param name="text">The text to copy</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task<bool> CopyTextToClipboard(string text)
    {
        var clipboard = GetClipboard();
        if (clipboard == null || string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        try
        {
            await Task.WhenAll(clipboard.SetTextAsync(text), AnimationsHelper.CopyAnimation());
            return true;
        }
        catch (Exception e)
        {
#if DEBUG
            Debug.WriteLine($"{nameof(ClipboardHelper)} {nameof(CopyTextToClipboard)} {e.StackTrace}");
#endif
            await TooltipHelper.ShowTooltipMessageAsync(TranslationHelper.Translation.UnexpectedError);
            return false;
        }
    }

    /// <summary>
    /// Copies a file to the clipboard
    /// </summary>
    /// <param name="file">Path to the file</param>
    /// <param name="vm">The main view model</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task<bool> CopyFileToClipboard(string? file, MainViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            return false;
        }

        try
        {
            var success = await Task.Run(() => vm.PlatformService.CopyFile(file));
            if (success)
            {
                await AnimationsHelper.CopyAnimation();
            }
            return success;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Copies the current image to the clipboard
    /// </summary>
    /// <param name="vm">The main view model</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task<bool> CopyImageToClipboard(MainViewModel vm)
    {
        var clipboard = GetClipboard();
        if (clipboard == null || vm.ImageSource is not Bitmap bitmap)
        {
            return false;
        }

        try
        {
            await clipboard.ClearAsync();

            // Handle for Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await Task.WhenAll(vm.PlatformService.CopyImageToClipboard(bitmap), AnimationsHelper.CopyAnimation());
                return false;
            }

            using var ms = new MemoryStream();
            bitmap.Save(ms);

            var dataObject = new DataObject();
            dataObject.Set("image/png", ms.ToArray());
            await Task.WhenAll(clipboard.SetDataObjectAsync(dataObject), AnimationsHelper.CopyAnimation());
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Copies an image as base64 string to the clipboard
    /// </summary>
    /// <param name="path">Optional path to the image file</param>
    /// <param name="vm">The main view model</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task<bool> CopyBase64ToClipboard(string path, MainViewModel vm)
    {
        var clipboard = GetClipboard();
        if (clipboard == null)
        {
            return false;
        }

        try
        {
            string base64;
            if (string.IsNullOrWhiteSpace(path))
            {
                switch (vm.ImageType)
                {
                    case ImageType.AnimatedGif:
                    case ImageType.AnimatedWebp:
                        throw new ArgumentOutOfRangeException(nameof(vm.ImageType), "Animated images are not supported");
                    case ImageType.Bitmap:
                        if (vm.ImageSource is not Bitmap bitmap)
                        {
                            return false;
                        }

                        using (var stream = new MemoryStream())
                        {
                            bitmap.Save(stream, quality: 100);
                            base64 = Convert.ToBase64String(stream.ToArray());
                        }
                        break;
                    case ImageType.Svg:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(vm.ImageType), $"Unsupported image type: {vm.ImageType}");
                }
            }
            else
            {
                base64 = Convert.ToBase64String(await File.ReadAllBytesAsync(path));
            }

            if (string.IsNullOrEmpty(base64))
            {
                return false;
            }

            await Task.WhenAll(clipboard.SetTextAsync(base64), AnimationsHelper.CopyAnimation());
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Cuts a file to the clipboard (copy + mark for deletion on paste)
    /// </summary>
    /// <param name="path">Path to the file</param>
    /// <param name="vm">The main view model</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task<bool> CutFile(string path, MainViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            var success = await Task.Run(() => vm.PlatformService.CutFile(path));
            if (success)
            {
                await AnimationsHelper.CopyAnimation();
            }
            return success;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Pastes content from the clipboard
    /// </summary>
    /// <param name="vm">The main view model</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task Paste(MainViewModel vm)
    {
        var clipboard = GetClipboard();
        if (clipboard == null)
        {
            return;
        }

        try
        {
            // Try to paste files first
            var files = await clipboard.GetDataAsync(DataFormats.Files);
            if (files != null)
            {
                await PasteFiles(files, vm);
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
            await PasteClipboardImage(vm, clipboard);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Paste operation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Pastes an image from the clipboard
    /// </summary>
    /// <param name="vm">The main view model</param>
    /// <param name="clipboard">The clipboard instance</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task PasteClipboardImage(MainViewModel vm, IClipboard clipboard)
    {
        var name = TranslationHelper.Translation.ClipboardImage;
        var imageType = ImageType.Bitmap;

        // List of formats to try
        string[] formats = new[]
        {
            "PNG", "image/jpeg", "image/png", "image/bmp", "BMP",
            "JPG", "JPEG", "image/tiff", "GIF", "image/gif"
        };

        foreach (var format in formats)
        {
            var bitmap = await GetBitmapFromBytes(format);
            if (bitmap != null)
            {
                await UpdateImage.SetSingleImageAsync(bitmap, imageType, name, vm);
                return;
            }
        }

        // Windows-specific clipboard handling
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var bitmap = await vm.PlatformService.GetImageFromClipboard();
            if (bitmap != null)
            {
                await UpdateImage.SetSingleImageAsync(bitmap, imageType, name, vm);
            }
        }

        async Task<Bitmap?> GetBitmapFromBytes(string format)
        {
            try
            {
                var data = await clipboard.GetDataAsync(format);
                if (data is byte[] dataBytes)
                {
                    using var memoryStream = new MemoryStream(dataBytes);
                    return new Bitmap(memoryStream);
                }
            }
            catch (Exception)
            {
                // Ignore format errors and try next format
            }
            return null;
        }
    }

    /// <summary>
    /// Handles pasting files from the clipboard
    /// </summary>
    private static async Task PasteFiles(object files, MainViewModel vm)
    {
        if (files is IEnumerable<IStorageItem> items)
        {
            var storageItems = items.ToArray();
            if (storageItems.Length > 0)
            {
                // Load the first file
                var firstFile = storageItems[0];
                var firstPath = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? firstFile.Path.AbsolutePath
                    : firstFile.Path.LocalPath;

                await NavigationManager.LoadPicFromStringAsync(firstPath, vm).ConfigureAwait(false);

                // Open consecutive files in a new process
                foreach (var file in storageItems.Skip(1))
                {
                    var path = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                        ? file.Path.AbsolutePath
                        : file.Path.LocalPath;

                    ProcessHelper.StartNewProcess(path);
                }
            }
        }
        else if (files is IStorageItem singleFile)
        {
            var path = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? singleFile.Path.AbsolutePath
                : singleFile.Path.LocalPath;

            await NavigationManager.LoadPicFromStringAsync(path, vm).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the clipboard instance from the current application
    /// </summary>
    /// <returns>The clipboard instance or null if not available</returns>
    private static IClipboard? GetClipboard()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow.Clipboard;
        }
        return null;
    }
}