using System.Collections.Concurrent;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.FileSorting;
using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Core.Navigation;

public class FileWatcherService : IFileWatcherService, IDisposable
{
    private readonly IImageCache _cache;
    private readonly IThumbnailCache? _thumbnailCache;
    private readonly Lock _lock = new();
    private readonly Func<string, string, int> _stringComparer;

    // Maps Directory Path -> (Watcher, Subscribers)
    private readonly
        ConcurrentDictionary<string, (FileSystemWatcher Watcher, IDisposable Subscription,
            List<WeakReference<TabViewModel>> Subscribers)> _watchers = new();

    public FileWatcherService(Func<string, string, int> stringComparer, IImageCache cache, IThumbnailCache? thumbnailCache = null)
    {
        _stringComparer = stringComparer ?? throw new ArgumentNullException(nameof(stringComparer));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _thumbnailCache = thumbnailCache;
    }

    public void Watch(TabViewModel tab, string? directory = null)
    {
        if (string.IsNullOrEmpty(directory))
        {
            var fileInfo = tab.Model?.FileInfo;
            if (fileInfo is null || string.IsNullOrEmpty(fileInfo.DirectoryName))
            {
                return;
            }

            directory = fileInfo.DirectoryName;
        }

        lock (_lock)
        {
            // If we are already watching this directory, just add the subscriber
            if (_watchers.TryGetValue(directory, out var entry))
            {
                // Remove dead references first
                entry.Subscribers.RemoveAll(wr => !wr.TryGetTarget(out _));

                // Add if not exists
                if (!entry.Subscribers.Any(wr => wr.TryGetTarget(out var t) && ReferenceEquals(t, tab)))
                {
                    entry.Subscribers.Add(new WeakReference<TabViewModel>(tab));
                }

                return;
            }

            var watcher = new FileSystemWatcher(directory)
            {
                EnableRaisingEvents = true,
                Filter = "*.*",
                IncludeSubdirectories = Settings.Sorting.IncludeSubDirectories,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            // We use Observable.FromEvent to bridge standard .NET events to R3

            var created = Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (s, e) => h(e),
                h => watcher.Created += h,
                h => watcher.Created -= h
            );

            var deleted = Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (s, e) => h(e),
                h => watcher.Deleted += h,
                h => watcher.Deleted -= h
            );

            var renamed = Observable.FromEvent<RenamedEventHandler, RenamedEventArgs>(
                h => (s, e) => h(e),
                h => watcher.Renamed += h,
                h => watcher.Renamed -= h
            );

            // AwaitOperation.Sequential ensures we don't process two file events for the same folder at the exact same time, 
            // which protects the Integrity of the 'files' list and the CurrentIndex.

            var fileCreatedSub = created.SubscribeAwait(async (e, ct) =>
                await OnFileCreatedAsync(directory, e, ct));

            var fileDeletedSub = deleted.SubscribeAwait(async (e, ct) =>
                await OnFileDeletedAsync(directory, e, ct));

            var fileRenamedSub = renamed.SubscribeAwait(async (e, ct) =>
                await OnFileRenamedAsync(directory, e, ct));

            // Combine disposables
            var subscription = Disposable.Combine(fileCreatedSub, fileDeletedSub, fileRenamedSub);

            _watchers[directory] = (watcher, subscription, [new WeakReference<TabViewModel>(tab)]);
        }
    }

    public void Unwatch(TabViewModel tab)
    {
        lock (_lock)
        {
            // iterate all watchers to find the tab
            var keysToRemove = new List<string>();

            foreach (var kvp in _watchers)
            {
                var (watcher, subscription, subscribers) = kvp.Value;

                // Remove the tab
                subscribers.RemoveAll(wr => !wr.TryGetTarget(out var t) || ReferenceEquals(t, tab));

                if (subscribers.Count != 0)
                {
                    continue;
                }

                // Dispose R3 subscription AND Watcher
                subscription.Dispose();
                watcher.Dispose();
                keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
            {
                _watchers.TryRemove(key, out _);
            }
        }
    }

    private async ValueTask OnFileCreatedAsync(string directory, FileSystemEventArgs e, CancellationToken ct)
    {
        if (!e.FullPath.IsSupported())
        {
            return;
        }

        await HandleUpdateAsync(directory, (tab, files) =>
        {
            tab.ImageIterator.Files = files;

            var currentFile = tab.Model?.FileInfo;
            if (currentFile != null)
            {
                var newIndex = files.FindIndex(x =>
                    x.FullName.AsSpan().Equals(currentFile.FullName.AsSpan(), StringComparison.OrdinalIgnoreCase));
                if (newIndex >= 0)
                {
                    tab.ImageIterator.SetCurrentIndex(newIndex);
                }
            }

            _cache.Resynchronize(tab.Id, files);
            tab.UpdateTabTitle();

            return ValueTask.CompletedTask;
        });
    }

    private async ValueTask OnFileDeletedAsync(string directory, FileSystemEventArgs e, CancellationToken ct)
    {
        if (!e.FullPath.IsSupported())
        {
            return;
        }

        _thumbnailCache?.Remove(e.FullPath);

        await HandleUpdateAsync(directory, async (tab, files) =>
        {
            var oldIndex = tab.ImageIterator.CurrentIndex;
            var currentFile = tab.Model?.FileInfo;
            var wasCurrentFileDeleted =
                currentFile?.FullName.AsSpan().Equals(e.FullPath.AsSpan(), StringComparison.OrdinalIgnoreCase) ?? false;

            tab.ImageIterator.Files = files;

            if (wasCurrentFileDeleted)
            {
                if (files.Count == 0)
                {
                    // TODO: Switch current view to a StartUpMenu
                }
                else
                {
                    var targetIndex = Math.Clamp(oldIndex, 0, files.Count - 1);
                    await tab.ImageIterator.IterateToIndexAsync(targetIndex, tab.GetTabCancellation());
                }
            }
            else
            {
                if (currentFile != null)
                {
                    var newIndex = files.FindIndex(x =>
                        x.FullName.AsSpan().Equals(currentFile.FullName.AsSpan(), StringComparison.OrdinalIgnoreCase));
                    if (newIndex >= 0)
                    {
                        tab.ImageIterator.SetCurrentIndex(newIndex);
                    }
                }
            }

            _cache.Resynchronize(tab.Id, files);
            tab.UpdateTabTitle();
        });
    }

    private async ValueTask OnFileRenamedAsync(string directory, RenamedEventArgs e, CancellationToken ct)
    {
        if (!e.FullPath.IsSupported())
        {
            return;
        }

        _thumbnailCache?.Remove(e.OldFullPath);

        await HandleUpdateAsync(directory, (tab, files) =>
        {
            var currentFile = tab.Model?.FileInfo;
            var wasCurrentFileRenamed = currentFile?.FullName.AsSpan()
                .Equals(e.OldFullPath.AsSpan(), StringComparison.OrdinalIgnoreCase) ?? false;

            tab.ImageIterator.Files = files;

            if (wasCurrentFileRenamed)
            {
                var newFileInfo = new FileInfo(e.FullPath);
                var currentModel = tab.Model;
                if (currentModel != null)
                {
                    var newModel = new ImageModel
                    {
                        FileInfo = newFileInfo,
                        Image = currentModel.Image,
                        Orientation = currentModel.Orientation,
                        ImageType = currentModel.ImageType
                    };
                    tab.Model = newModel;
                }
            }

            var fileToCheck = wasCurrentFileRenamed ? new FileInfo(e.FullPath) : currentFile;

            if (fileToCheck != null)
            {
                var newIndex = files.FindIndex(x =>
                    x.FullName.Equals(fileToCheck.FullName, StringComparison.OrdinalIgnoreCase));
                if (newIndex >= 0)
                {
                    tab.ImageIterator.SetCurrentIndex(newIndex);
                }
            }

            _cache.Resynchronize(tab.Id, files);
            tab.UpdateTabTitle();

            return ValueTask.CompletedTask;
        });
    }

    private async ValueTask HandleUpdateAsync(string directory,
        Func<TabViewModel, List<FileInfo>, ValueTask> updateAction)
    {
        List<TabViewModel> targets = [];
        lock (_lock)
        {
            if (_watchers.TryGetValue(directory, out var entry))
            {
                foreach (var wr in entry.Subscribers)
                {
                    if (wr.TryGetTarget(out var tab))
                    {
                        targets.Add(tab);
                    }
                }
            }
        }

        if (targets.Count == 0)
        {
            return;
        }

        try
        {
            // Perform IO to get fresh list
            var files = FileListRetriever.RetrieveFiles(new FileInfo(directory), _stringComparer);

            foreach (var tab in targets)
            {
                try
                {
                    await updateAction(tab, files);
                }
                catch (Exception ex)
                {
                    DebugHelper.LogDebug(nameof(FileWatcherService), nameof(updateAction), ex);
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(FileWatcherService), nameof(HandleUpdateAsync), ex);
        }
    }

    #region IDispose

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var (_, (watcher, subscription, _)) in _watchers)
            {
                subscription.Dispose();
                watcher.Dispose();
            }

            _watchers.Clear();
        }

        GC.SuppressFinalize(this);
    }

    #endregion
}