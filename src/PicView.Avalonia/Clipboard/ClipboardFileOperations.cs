using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using PicView.Avalonia.Animations;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.UI;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.Localization;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Clipboard;

/// <summary>
/// Handles clipboard operations related to files (MVVM refactor)
/// </summary>
public static class ClipboardFileOperations
{
    /// <summary>
    /// Duplicates the specified file, either the current file or another one specified by path.
    /// If the current file is being duplicated, the view model will navigate to the duplicated file.
    /// </summary>
    /// <param name="path">Path to the file to duplicate, or null to duplicate the current file.</param>
    /// <param name="vm">The main window view model</param>
    public static async Task Duplicate(string? path, MainWindowViewModel vm)
    {
        var currentFile = vm.WindowTabs.ActiveTab.CurrentValue.Model.FileInfo?.FullName;
        
        // If path is null/empty, we assume we want to duplicate the current file
        var targetPath = string.IsNullOrWhiteSpace(path) ? currentFile : path;

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return;
        }
        
        try
        {
            vm.IsLoadingIndicatorShown.Value = true;
            
            // If we are duplicating the currently viewing file, we want to perform navigation to the new file
            if (targetPath == currentFile)
            {
                await DuplicateCurrentFile(vm);
            }
            else
            {
                await DuplicateFile(targetPath);
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(ClipboardFileOperations), nameof(Duplicate), ex);
            TooltipHelper.ShowTooltipMessage(TranslationManager.Translation?.UnexpectedError);
        }
        finally
        {
            vm.IsLoadingIndicatorShown.Value = false;
        }
    }

    /// <summary>
    /// Duplicates the current file and navigates to it
    /// </summary>
    /// <param name="vm">The main window view model</param>
    private static async Task DuplicateCurrentFile(MainWindowViewModel vm)
    {
        var activeTab = vm.WindowTabs.ActiveTab.CurrentValue;
        
        if (activeTab.ImageIterator is null || vm.WindowTabs.SharedNavigation is null)
        {
            return;
        }

        try
        {
            var currentPath = activeTab.Model.FileInfo?.FullName;
            if (string.IsNullOrWhiteSpace(currentPath)) return;

            var duplicatedPath =
                await FileHelper.DuplicateAndReturnFileNameAsync(currentPath);

            if (string.IsNullOrWhiteSpace(duplicatedPath) || !File.Exists(duplicatedPath))
            {
                return;
            }

            var loadTask = vm.WindowTabs.SharedNavigation.LoadFromFileAsync(duplicatedPath, activeTab, activeTab.GetTabCancellation());
            var animTask = AnimationsHelper.CopyAnimation();

            await Task.WhenAll(animTask, loadTask.AsTask());
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(ClipboardFileOperations), nameof(DuplicateCurrentFile), ex);
        }
    }
    
    /// <summary>
    /// Duplicates the specified file and plays a copy animation when done. The original file is not navigated away from.
    /// </summary>
    /// <param name="path">Path to the file to duplicate</param>
    private static async Task DuplicateFile(string path)
    {
        var duplicatedPath = await FileHelper.DuplicateAndReturnFileNameAsync(path);
        if (!string.IsNullOrWhiteSpace(duplicatedPath))
        {
            await AnimationsHelper.CopyAnimation();
        }
    }

    /// <summary>
    /// Copies a file to the clipboard
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="visual">Optional visual to locate clipboard</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task CopyFileToClipboard(string? filePath, Visual? visual = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var clipboard = ClipboardService.GetClipboard(visual);
        if (clipboard == null)
        {
            return;
        }
        
        // We need a StorageProvider to get the IStorageFile.
        // If we have a visual, we can try to get TopLevel -> StorageProvider
        IStorageProvider? storageProvider = null;
        if (visual != null)
        {
             var topLevel = TopLevel.GetTopLevel(visual);
             storageProvider = topLevel?.StorageProvider;
        }
        
        // Fallback to Application lifetime if visual didn't work
        if (storageProvider == null && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            storageProvider = desktop.MainWindow?.StorageProvider;
        }
        
        if (storageProvider == null)
        {
            return;
        }
        
        var animTask = AnimationsHelper.CopyAnimation();
        var storageFile = await storageProvider.TryGetFileFromPathAsync(Path.GetFullPath(filePath));
        
        if (storageFile != null)
        {
             var fileTask = clipboard.SetFileAsync(storageFile);
             await Task.WhenAll(animTask, fileTask);
        }
    }
    
    /// <summary>
    /// Copies text (file path) to the clipboard
    /// </summary>
    public static async Task<bool> CopyFilePath(string? text, Visual? visual = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }
        
        var clipboard = ClipboardService.GetClipboard(visual);
        if (clipboard == null)
        {
            return false;
        }

        return await ClipboardService.ExecuteClipboardOperation(async () =>
        {
            await clipboard.SetTextAsync(text);
            return true;
        }, showAnimation: true);
    }

    /// <summary>
    /// Cuts a file to the clipboard (copy + mark for deletion on paste)
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="platformService">Platform specific service for cutting files</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static Task<bool> CutFile(string filePath, Core.IPlatform.IPlatformSpecificService platformService)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.FromResult(false);
        }

        return ClipboardService.ExecuteClipboardOperation(
            () => Task.Run(() => platformService.CutFile(filePath))
        );
    }

    public static async ValueTask PasteFiles(object files, MainWindowViewModel vm)
    {
        try
        {
            switch (files)
            {
                case IEnumerable<IStorageItem> items:
                    await ProcessStorageItems(items.ToArray(), vm);
                    break;
                case IStorageItem singleFile:
                {
                    await vm.WindowTabs.LoadFromFileAsync(singleFile.Path.AbsolutePath).ConfigureAwait(false);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(ClipboardFileOperations), nameof(PasteFiles), ex);
        }
    }
    
    private static async ValueTask ProcessStorageItems(IStorageItem[] storageItems, MainWindowViewModel vm)
    {
        if (storageItems.Length == 0)
        {
            return;
        }

        // Load the first file
        await vm.WindowTabs.LoadFromFileAsync(storageItems[0].Path.AbsolutePath).ConfigureAwait(false);

        // Open consecutive files in a new process
        foreach (var file in storageItems.Skip(1))
        {
            var tab = await vm.WindowTabs.CreateNewTabFromFileAsync(file.Path.AbsolutePath);
            TabNavigationInitializer.InitializeNewTab(tab, vm);
            file.Dispose();
        }
    }
}
