using PicView.Core.DebugTools;
using PicView.Core.Extensions;
using PicView.Core.FileHandling;
using PicView.Core.FileHandling.Interfaces;
using PicView.Core.FileSorting;
using PicView.Core.Http;
using PicView.Core.IPlatform;
using PicView.Core.Localization;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class NavigationService : INavigationService
{
    private readonly IArchiveService _archive;
    private readonly IImageCache _cache;
    private readonly IImageLoader _imageLoader;
    private readonly IFileWatcherService _fileWatcherService;
    private readonly IPlatformSpecificService _platformService;
    private readonly ITempFileService _tempFileService;
    private readonly Func<string, string, int> _stringComparer;

    public NavigationService(IImageLoader imageLoader, IArchiveService archive, IImageCache cache,
        IFileWatcherService fileWatcherService, IPlatformSpecificService platformService,
        ITempFileService tempFileService, Func<string, string, int> stringComparer)
    {
        _imageLoader = imageLoader;
        _archive = archive;
        _cache = cache;
        _fileWatcherService = fileWatcherService;
        _platformService = platformService;
        _tempFileService = tempFileService;
        _stringComparer = stringComparer ?? string.CompareOrdinal;
    }
    
    public async ValueTask RepopulateIterator(FileInfo fileInfo, TabViewModel tab, CancellationTokenSource ct, List<FileInfo>? files = null)
    {
        try
        {
            _fileWatcherService.Unwatch(tab);
            _fileWatcherService.Watch(tab, fileInfo.DirectoryName);

            // Show image quickly to make it feel fast
            var model = await _imageLoader.GetImageModelAsync(fileInfo, ct.Token).ConfigureAwait(false);
            tab.Model.Value = model; // Image updated via reactive subscription
            
            tab.ImageIterator.Files = files ?? FileListRetriever.RetrieveFiles(fileInfo, _stringComparer);
            var index = FindIndex(fileInfo, tab);
            tab.ImageIterator.SetCurrentIndex(index);
            
            tab.UpdateTabTitle();
            _cache.Clear(tab.Id);
            _cache.Add(tab.Id, index, new PreLoadValue(model), tab.ImageIterator.Files.Count, false);
            _cache.Preload(tab.Id, index, false, tab.ImageIterator.Files);
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(RepopulateIterator), e);
        }
    }

    public async ValueTask LoadFromFileAsync(string source, TabViewModel tab, CancellationTokenSource ct)
    {
        ArgumentNullException.ThrowIfNull(source);
        await LoadFromFileAsync(new FileInfo(source), tab, ct).ConfigureAwait(false);
    }

    public async ValueTask LoadFromFileAsync(FileInfo fileInfo, TabViewModel tab, CancellationTokenSource ct)
    {
        var iterator = tab.ImageIterator;

        if (iterator.Files is null || iterator.Files.Count == 0)
        {
            await Repopulate();
            return;
        }

        if (iterator.Files.Contains(fileInfo))
        {
            var index = FindIndex(fileInfo, tab);
            await tab.ImageIterator.IterateToIndexAsync(index, ct).ConfigureAwait(false);
        }
        else
        {
            await Repopulate();
        }

        return;

        async ValueTask Repopulate()
        {
            await RepopulateIterator(fileInfo, tab, ct).ConfigureAwait(false);
        }
    }

    public async ValueTask LoadFromStringAsync(string source, TabViewModel tab, CancellationTokenSource ct)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return;
        }

        var check = FileTypeResolver.CheckIfLoadableString(source);
        if (check == null)
        {
            return;
        }

        switch (check.Value.Type)
        {
            case FileTypeResolver.LoadAbleFileType.File:
                await LoadFromFileAsync(check.Value.Data, tab, ct).ConfigureAwait(false);
                return;
            case FileTypeResolver.LoadAbleFileType.Directory:
            {
                var files = await Task.Run(() => FileListRetriever.RetrieveFiles(new FileInfo(check.Value.Data), _stringComparer), ct.Token).ConfigureAwait(false);
                if (files.Count == 0)
                {
                    return;
                }

                var first = files[0];
                await RepopulateIterator(first, tab, ct, files).ConfigureAwait(false);
                return;
            }
            case FileTypeResolver.LoadAbleFileType.Web:
                await LoadFromUrlAsync(check.Value.Data, tab, ct).ConfigureAwait(false);
                return;
            case FileTypeResolver.LoadAbleFileType.Base64:
            case FileTypeResolver.LoadAbleFileType.Zip:
                throw new NotImplementedException();
            default:
                return;
        }
    }

    private async ValueTask LoadFromUrlAsync(string url, TabViewModel tab, CancellationTokenSource ct)
    {
        if (tab.ImageIterator is not null)
        {
            await tab.ImageIterator.DisposeAsync().ConfigureAwait(false);
        }

        _platformService.StopTaskbarProgress();
        var safeFileName = HttpManager.GetSafeFileName(url);
        var destPath = _tempFileService.GetNewTempFilePath(safeFileName);
        
        using var client = new HttpClientDownloadWithProgress(url, destPath);
        client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
        {
            var displayProgress = HttpManager.GetProgressDisplay(totalFileSize, totalBytesDownloaded, progressPercentage);
            var title = $"{safeFileName} {TranslationManager.Translation?.Downloading} {displayProgress}";

            // Update UI properties
            if (tab.TabTitle.Value != title) tab.TabTitle.Value = title;
            if (tab.Title.Value != title) tab.Title.Value = title;
            if (tab.WindowTitle.Value != title) tab.WindowTitle.Value = title;
            if (tab.TitleTooltip.Value != title) tab.TitleTooltip.Value = title;

            if (totalBytesDownloaded.HasValue && totalFileSize.HasValue)
            {
                _platformService.SetTaskbarProgress((ulong)totalBytesDownloaded.Value, (ulong)totalFileSize.Value);
            }
        };

        try
        {
            await client.StartDownloadAsync(ct.Token).ConfigureAwait(false);
            
            _platformService.StopTaskbarProgress();

            if (ct.IsCancellationRequested)
            {
                return;
            }

            var model = await _imageLoader.GetImageModelAsync(new FileInfo(destPath), ct.Token).ConfigureAwait(false);
            tab.Model.Value = model;
            tab.SecondaryModel.Value = null;
            
            // Set titles to filename after successful load
            tab.TabTitle.Value = safeFileName;
            tab.Title.Value = safeFileName;
            tab.WindowTitle.Value = safeFileName;
            tab.TitleTooltip.Value = destPath;

        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(LoadFromUrlAsync), e);
            _platformService.StopTaskbarProgress();
            // Revert or show error state if needed
            tab.TabTitle.Value = TranslationManager.Translation?.ErrorLoadingImage ?? "Error";
        }
    }

    public async ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationTokenSource ct)
    {
        if (!CanNavigate(tab))
        {
            return;
        }

        if (tab.ImageIterator is null)
        {
            return;
        }

        var nextFileIndex = tab.ImageIterator.GetIteration(tab.ImageIterator.CurrentIndex, to, tab.Id, SkipAmount.One);
        await tab.ImageIterator.IterateToIndexAsync(nextFileIndex, ct).ConfigureAwait(false);
    }

    public ValueTask NavigateToIndexAsync(TabViewModel tab, int index, CancellationTokenSource ct)
    {
        return tab.ImageIterator?.IterateToIndexAsync(index, ct) ?? ValueTask.CompletedTask;
    }

    public async ValueTask NavigateByIncrementsAsync(TabViewModel tab, SkipAmount skipAmount, bool forwards, CancellationTokenSource ct)
    {
        var iterator = tab.ImageIterator;
        if (iterator is null)
        {
            return;
        }
        await iterator.NavigateByIncrementsAsync(skipAmount,forwards, ct).ConfigureAwait(false);
    }

    public bool CanNavigate(TabViewModel tab) => tab?.ImageIterator?.Files?.Count > 0;

    public async ValueTask SortAsync(TabViewModel tab, SortFilesBy sortOrder, CancellationTokenSource ct)
    {
        Settings.Sorting.SortPreference = (int)sortOrder;
        await ApplySortAsync(tab, ct).ConfigureAwait(false);
    }

    public async ValueTask SortAsync(TabViewModel tab, bool ascending, CancellationTokenSource ct)
    {
        Settings.Sorting.Ascending = ascending;
        await ApplySortAsync(tab, ct).ConfigureAwait(false);
    }

    private async ValueTask ApplySortAsync(TabViewModel tab, CancellationTokenSource ct)
    {
        if (!CanNavigate(tab))
        {
            return;
        }

        try
        {
            // Get current file to maintain position
            var currentFile = tab.Model.Value?.FileInfo;
            if (currentFile is null)
            {
                return;
            }

            // Retrieve and sort files based on new settings
            var newFiles = await Task.Run(() => FileListRetriever.RetrieveFiles(currentFile, _stringComparer), ct.Token).ConfigureAwait(false);

            if (newFiles.Count == 0)
            {
                return;
            }

            // Update files in iterator
            tab.ImageIterator.Files = newFiles;

            // Find new index of current file
            var newIndex = FindIndex(currentFile, tab);
            tab.ImageIterator.SetCurrentIndex(newIndex);
            
            // Update cache mapping
            _cache.Resynchronize(tab.Id, newFiles);
            
            // Update title
            tab.UpdateTabTitle();
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(ApplySortAsync), e);
        }
    }

    private static int FindIndex(FileInfo fileInfo, TabViewModel tab) =>
        tab.ImageIterator.Files.FindIndex(x =>
            x.FullName.AsSpan().Equals(fileInfo.FullName.AsSpan(), StringComparison.OrdinalIgnoreCase));
}