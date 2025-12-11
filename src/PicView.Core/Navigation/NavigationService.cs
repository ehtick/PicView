using PicView.Core.Navigation.Interfaces;
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

    public async ValueTask LoadFromStringAsync(string source, TabViewModel tab, CancellationTokenSource ct)
    {
        throw new NotImplementedException();
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
        if (tab.ImageIterator is null)
        {
            return ValueTask.CompletedTask;
        }

        return tab.ImageIterator.IterateToIndexAsync(index, ct);
    }

    public bool CanNavigate(TabViewModel tab)
    {
        return tab?.ImageIterator?.Files?.Count > 0;
    }

    public async ValueTask DisposeAsync()
    {
        // Dispose dependencies if needed, or leave it to DI container
    }
}