using PicView.Avalonia.Gallery;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileSorting;

namespace PicView.Avalonia.Navigation;

/// <summary>
/// Manages file list sorting and updating within the application.
/// </summary>
public static class FileListManager
{
    private static CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Sorts an enumerable collection of file paths based on the specified sort order and platform-specific string comparison.
    /// </summary>
    /// <param name="files">The collection of file paths to sort.</param>
    /// <param name="platformService">Platform-specific service for string comparison.</param>
    /// <returns>A sorted list of file paths.</returns>
    public static List<string> SortIEnumerable(IEnumerable<string> files, IPlatformSpecificService? platformService)
    {
        var sortFilesBy = FileSortHelper.GetSortOrder();

        switch (sortFilesBy)
        {
            default:
            case SortFilesBy.Name: // Alphanumeric sort
                var list = files.ToList();
                if (Settings.Sorting.Ascending)
                {
                    list.Sort(platformService.CompareStrings);
                }
                else
                {
                    list.Sort((x, y) => platformService.CompareStrings(y, x));
                }

                return list;

            case SortFilesBy.FileSize: // Sort by file size
                var fileInfoList = files.Select(f => new FileInfo(f)).ToList();
                var sortedBySize = Settings.Sorting.Ascending
                    ? fileInfoList.OrderBy(f => f.Length)
                    : fileInfoList.OrderByDescending(f => f.Length);
                return sortedBySize.Select(f => f.FullName).ToList();

            case SortFilesBy.Extension: // Sort by file extension
                var sortedByExtension = Settings.Sorting.Ascending
                    ? files.OrderBy(Path.GetExtension)
                    : files.OrderByDescending(Path.GetExtension);
                return sortedByExtension.ToList();

            case SortFilesBy.CreationTime: // Sort by file creation time
                var sortedByCreationTime = Settings.Sorting.Ascending
                    ? files.OrderBy(f => new FileInfo(f).CreationTime)
                    : files.OrderByDescending(f => new FileInfo(f).CreationTime);
                return sortedByCreationTime.ToList();

            case SortFilesBy.LastAccessTime: // Sort by file last access time
                var sortedByLastAccessTime = Settings.Sorting.Ascending
                    ? files.OrderBy(f => new FileInfo(f).LastAccessTime)
                    : files.OrderByDescending(f => new FileInfo(f).LastAccessTime);
                return sortedByLastAccessTime.ToList();

            case SortFilesBy.LastWriteTime: // Sort by file last write time
                var sortedByLastWriteTime = Settings.Sorting.Ascending
                    ? files.OrderBy(f => new FileInfo(f).LastWriteTime)
                    : files.OrderByDescending(f => new FileInfo(f).LastWriteTime);
                return sortedByLastWriteTime.ToList();

            case SortFilesBy.Random: // Sort files randomly
                return files.OrderBy(f => Guid.NewGuid()).ToList();
        }
    }

    /// <summary>
    /// Updates the file list in the view model based on the specified sort order.
    /// </summary>
    /// <param name="platformSpecificService">Platform-specific service for file retrieval and sorting.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <param name="sortFilesBy">The sort order to apply.</param>
    public static async Task UpdateFileList(IPlatformSpecificService? platformSpecificService, MainViewModel vm, SortFilesBy sortFilesBy)
    {
        Settings.Sorting.SortPreference = (int)sortFilesBy;
        if (!NavigationManager.CanNavigate(vm))
        {
            return;
        }

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
        if (!NavigationManager.CanNavigate(vm))
        {
            return;
        }
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
                var files = platformSpecificService.GetFiles(vm.PicViewer.FileInfo);
                if (files is not { Count: > 0 })
                {
                    return false;
                }

                NavigationManager.UpdateFileListAndIndex(files, files.IndexOf(vm.PicViewer.FileInfo.FullName));
                TitleManager.SetTitle(vm);
                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine($"{nameof(UpdateFileList)} exception:\n{e.Message}");
#endif
                return false;
            }

        }, _cancellationTokenSource.Token);
        if (!success)
        {
            return;
        }

        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            await GalleryLoad.ReloadGalleryAsync(vm, vm.PicViewer.FileInfo.DirectoryName);
        }
    }
}