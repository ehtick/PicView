using PicView.Core.DebugTools;
using PicView.Core.FileSorting;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class NavigationService : INavigationService
{
    private readonly IArchiveService _archive;
    private readonly IImageCache _cache;
    private readonly IImageLoader _imageLoader;

    public NavigationService(IImageLoader imageLoader, IArchiveService archive, IImageCache cache)
    {
        _imageLoader = imageLoader;
        _archive = archive;
        _cache = cache;
    }
    
    public async ValueTask RepopulateIterator(FileInfo fileInfo, TabViewModel tab, CancellationTokenSource ct)
    {
        try
        {
            // Show image quickly to make it feel fast
            var model = await _imageLoader.GetImageModelAsync(fileInfo, ct.Token).ConfigureAwait(false);
            tab.Model.Value = model; // Image updated via reactive subscription
            
            tab.ImageIterator.Files = FileListRetriever.RetrieveFiles(fileInfo, CompareStrings);
            var index = FindIndex(fileInfo, tab);
            tab.ImageIterator.SetCurrentIndex(index);
            tab.UpdateTabTitle();
            _cache.Clear();
            _cache.Add(tab.Id, index, new PreLoadValue(model), tab.ImageIterator.Files.Count, false);
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(RepopulateIterator), e);
        }

        return;

        int CompareStrings(string str1, string str2)
        {
            // TODO: Integrate platform service file sorting
            return string.CompareOrdinal(str1, str2);
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
        }
        else if (iterator.Files.Contains(fileInfo))
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
        await LoadFromFileAsync(source, tab, ct).ConfigureAwait(false); // TODO: Implement
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

        var nextFileIndex = tab.ImageIterator.GetIteration(tab.ImageIterator.CurrentIndex, to);
        await tab.ImageIterator.IterateToIndexAsync(nextFileIndex, ct).ConfigureAwait(false);
    }

    public ValueTask NavigateToIndexAsync(TabViewModel tab, int index, CancellationTokenSource ct)
    {
        return tab.ImageIterator?.IterateToIndexAsync(index, ct) ?? ValueTask.CompletedTask;
    }

    public bool CanNavigate(TabViewModel tab) => tab?.ImageIterator?.Files?.Count > 0;

    private static int FindIndex(FileInfo fileInfo, TabViewModel tab) =>
        tab.ImageIterator.Files.FindIndex(x =>
            x.FullName.AsSpan().Equals(fileInfo.FullName.AsSpan(), StringComparison.OrdinalIgnoreCase));
}