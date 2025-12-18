using System.Collections.Concurrent;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.FileSorting;
using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class FileWatcherService : IFileWatcherService, IDisposable
{
    private readonly Func<string, string, int> _stringComparer;
    private readonly IImageCache _cache;
    
    // Maps Directory Path -> (Watcher, Subscribers)
    private readonly ConcurrentDictionary<string, (FileSystemWatcher Watcher, List<WeakReference<TabViewModel>> Subscribers)> _watchers = new();
    private readonly Lock _lock = new();

    public FileWatcherService(Func<string, string, int> stringComparer, IImageCache cache)
    {
        _stringComparer = stringComparer ?? throw new ArgumentNullException(nameof(stringComparer));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public void Watch(TabViewModel tab, string? directory = null)
    {
        if (string.IsNullOrEmpty(directory))
        {
            var fileInfo = tab.Model.Value?.FileInfo;
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

            // Create new watcher
            var watcher = new FileSystemWatcher(directory)
            {
                EnableRaisingEvents = true,
                Filter = "*.*",
                IncludeSubdirectories = Settings.Sorting.IncludeSubDirectories,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite 
            };
            
            // Wire up events
            watcher.Created += (_, e) => OnFileCreated(directory, e);
            watcher.Deleted += (_, e) => OnFileDeleted(directory, e);
            watcher.Renamed += (_, e) => OnFileRenamed(directory, e);

            _watchers[directory] = (watcher, [new WeakReference<TabViewModel>(tab)]);
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
                 var (watcher, subscribers) = kvp.Value;
                 
                 // Remove the tab
                 subscribers.RemoveAll(wr => !wr.TryGetTarget(out var t) || ReferenceEquals(t, tab));
                 
                 if (subscribers.Count == 0)
                 {
                     watcher.Dispose();
                     keysToRemove.Add(kvp.Key);
                 }
             }

             foreach (var key in keysToRemove)
             {
                 _watchers.TryRemove(key, out _);
             }
        }
    }

    private void OnFileCreated(string directory, FileSystemEventArgs e)
    {
        if (!e.FullPath.IsSupported()) return;

        HandleUpdate(directory, (tab, files) =>
        {
            // 1. Update Iterator Logic
            tab.ImageIterator.Files = files;
            
            // 2. Find new index (Current file shouldn't change, but index might)
            var currentFile = tab.Model.Value?.FileInfo;
            if (currentFile != null)
            {
                var newIndex = files.FindIndex(x => x.FullName.Equals(currentFile.FullName, StringComparison.OrdinalIgnoreCase));
                if (newIndex >= 0)
                {
                    tab.ImageIterator.SetCurrentIndex(newIndex);
                }
            }
            
            // 3. Resync Cache
            _cache.Resynchronize(tab.Id, files);
            
            // 4. Update Title
            tab.UpdateTabTitle();
            
            // TODO: Gallery Add
        });
    }

    private void OnFileDeleted(string directory, FileSystemEventArgs e)
    {
        if (!e.FullPath.IsSupported()) return;
        
        HandleUpdate(directory, (tab, files) =>
        {
            var oldIndex = tab.ImageIterator.CurrentIndex;
            var currentFile = tab.Model.Value?.FileInfo;
            var wasCurrentFileDeleted = currentFile?.FullName.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase) ?? false;

            tab.ImageIterator.Files = files;

            if (wasCurrentFileDeleted)
            {
                if (files.Count == 0)
                {
                    // No files left
                    // TODO: Handle empty state?
                }
                else
                {
                    // Navigate to appropriate neighbor
                    // Simple logic: Stay at same index (next file) or go back one if we were at end
                    var targetIndex = Math.Clamp(oldIndex, 0, files.Count - 1);
                    if (targetIndex >= 0)
                    {
                        // We must fire navigation to load the new image
                        // Use Fire and Forget for the async void event handler context
                        _ = tab.ImageIterator.IterateToIndexAsync(targetIndex, tab.GetTabCancellation()); 
                    }
                }
            }
            else
            {
                // Just update index of current file
                if (currentFile != null)
                {
                    var newIndex = files.FindIndex(x => x.FullName.Equals(currentFile.FullName, StringComparison.OrdinalIgnoreCase));
                    if (newIndex >= 0)
                    {
                        tab.ImageIterator.SetCurrentIndex(newIndex);
                    }
                }
            }

            _cache.Resynchronize(tab.Id, files);
            tab.UpdateTabTitle();
            
            // TODO: Gallery Remove
        });
    }

    private void OnFileRenamed(string directory, RenamedEventArgs e)
    {
         if (!e.FullPath.IsSupported()) return;

         HandleUpdate(directory, (tab, files) =>
         {
             var currentFile = tab.Model.Value?.FileInfo;
             var wasCurrentFileRenamed = currentFile?.FullName.Equals(e.OldFullPath, StringComparison.OrdinalIgnoreCase) ?? false;
             
             tab.ImageIterator.Files = files;
             
             if (wasCurrentFileRenamed)
             {
                 var newFileInfo = new FileInfo(e.FullPath);
                 // Update the Model directly so UI reflects change immediately
                 // Note: We might want to construct a new ImageModel or just update FileInfo
                 // Given ImageModel is immutable-ish on FileInfo usually, let's see.
                 // TabViewModel.Model is BindableReactiveProperty<ImageModel>.
                 // We should probably update it to reflect the new path, but keep the image.
                 
                 var currentModel = tab.Model.Value;
                 if (currentModel != null)
                 {
                     // Create new model with updated file info but same image
                     var newModel = new ImageModel
                     {
                         FileInfo = newFileInfo,
                         Image = currentModel.Image,
                         Orientation = currentModel.Orientation,
                         ImageType = currentModel.ImageType
                         // Add other properties if needed
                     };
                     tab.Model.Value = newModel;
                 }
             }

             // Recalculate index
             var fileToCheck = wasCurrentFileRenamed ? new FileInfo(e.FullPath) : currentFile;
             
             if (fileToCheck != null)
             {
                 var newIndex = files.FindIndex(x => x.FullName.Equals(fileToCheck.FullName, StringComparison.OrdinalIgnoreCase));
                 if (newIndex >= 0)
                 {
                     tab.ImageIterator.SetCurrentIndex(newIndex);
                 }
             }
             
             _cache.Resynchronize(tab.Id, files);
             tab.UpdateTabTitle();
             
             // TODO: Gallery Rename
         });
    }

    private void HandleUpdate(string directory, Action<TabViewModel, List<FileInfo>> updateAction)
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

        if (targets.Count == 0) return;

        // Perform IO once
        // Note: This runs on the thread pool from the FileSystemWatcher event
        // We might want to debounce this if many events come in fast
        try
        {
            // We use the first tab's current file info (directory) to re-fetch
            // But we already know the directory from the key.
            // However, FileListRetriever needs a FileInfo to determine directory? 
            // FileListRetriever.RetrieveFiles takes FileInfo. 
            // We can construct a dummy FileInfo for the directory or use one of the tabs files.
            
            var files = FileListRetriever.RetrieveFiles(new FileInfo(directory), _stringComparer);
            
            foreach (var tab in targets)
            {
                // Dispatch to UI thread if necessary? 
                // PicView uses R3, BindableReactiveProperties usually handle synchronization context if set?
                // But ImageIterator modification is internal state.
                // However, tab.Model.Value assignment triggers subscribers which might update UI.
                // Avalonia requires UI updates on UI thread.
                // We should likely dispatch this.
                // Since I don't have direct access to Dispatcher here easily (Core project), 
                // I rely on the fact that R3 properties might need handling, 
                // OR ViewModels dispatch. 
                // But wait, PicView.Core shouldn't depend on Avalonia.
                // Use SynchronizationContext? 
                
                // For now, I will execute it here. If UI thread issues arise, we need a Dispatcher service.
                // Assuming R3 or the View layer handles marshaling or we need to act carefully.
                // The old ImageIterator used `Dispatcher.UIThread.InvokeAsync`.
                
                // We'll execute the action.
                try
                {
                     updateAction(tab, files);
                }
                catch (Exception ex)
                {
                    DebugHelper.LogDebug(nameof(FileWatcherService), "UpdateAction", ex);
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(FileWatcherService), nameof(HandleUpdate), ex);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var (_, (watcher, _)) in _watchers)
            {
                watcher.Dispose();
            }
            _watchers.Clear();
        }
        GC.SuppressFinalize(this);
    }
}
