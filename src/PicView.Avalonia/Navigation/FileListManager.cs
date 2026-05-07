using PicView.Avalonia.Interfaces;
using PicView.Avalonia.ViewModels;
using PicView.Core.DebugTools;
using PicView.Core.FileSorting;

namespace PicView.Avalonia.Navigation;

// TODO: Deprecated, delete

/// <summary>
/// Manages file list sorting and updating within the application.
/// </summary>
public static class FileListManager
{
    private static CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Updates the file list in the view model based on the specified sort order.
    /// </summary>
    /// <param name="platformSpecificService">Platform-specific service for file retrieval and sorting.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <param name="sortFilesBy">The sort order to apply.</param>
    public static async Task UpdateFileList(IPlatformSpecificService? platformSpecificService, MainViewModel vm, SortFilesBy sortFilesBy)
    {
        Settings.Sorting.SortPreference = (int)sortFilesBy;
        // if (!NavigationManager.CanNavigate(vm))
        // {
        //     return;
        // }

        await UpdateFileList(platformSpecificService, vm);
    }

    /// <summary>
    /// Updates the file list in the view model based on the specified sorting direction (ascending/descending).
    /// </summary>
    /// <param name="platformSpecificService">Platform-specific service for file retrieval and sorting.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <param name="ascending">Whether to sort in ascending order (true) or descending order (false).</param>
    public static async Task UpdateFileList(IPlatformSpecificService? platformSpecificService, MainViewModel vm, bool ascending)
    {
        Settings.Sorting.Ascending = ascending;
        // if (!NavigationManager.CanNavigate(vm))
        // {
        //     return;
        // }
        await UpdateFileList(platformSpecificService, vm);
    }

    /// <summary>
    /// Updates the file list in the view model, refreshing the gallery and UI as necessary.
    /// </summary>
    /// <param name="platformSpecificService">Platform-specific service for file retrieval and sorting.</param>
    /// <param name="vm">The main view model instance.</param>
    private static async Task UpdateFileList(IPlatformSpecificService? platformSpecificService, MainViewModel vm)
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        }

        _cancellationTokenSource = new CancellationTokenSource();
        var success = await Task.Run(() =>
        {
            try
            {
                var files = platformSpecificService.GetFiles(vm.PicViewer.FileInfo.CurrentValue);
                if (files is not { Count: > 0 })
                {
                    return false;
                }

                var index = files.FindIndex(info => info.FullName.Equals(vm.PicViewer.FileInfo.CurrentValue.FullName));
                // NavigationManager.UpdateFileListAndIndex(files, index);
                //TitleManager.SetTitle(vm);
                return true;
            }
            catch (Exception e)
            {
                DebugHelper.LogDebug(nameof(FileListManager), nameof(UpdateFileList), e);
                return false;
            }

        }, _cancellationTokenSource.Token);
        if (!success)
        {
            return;
        }

        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            //await GalleryLoad.ReloadGalleryAsync(vm, vm.PicViewer.FileInfo.CurrentValue.DirectoryName);
        }
    }
}