using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class NavigationService : INavigationService
{
    private readonly IArchiveService _archive;
    private readonly IImageLoader _imageLoader;
    private readonly IPreloader _preloader;

    public NavigationService(IImageLoader imageLoader, IArchiveService archive, IPreloader preloader)
    {
        _imageLoader = imageLoader;
        _archive = archive;
        _preloader = preloader;
    }

    public async ValueTask LoadFromStringAsync(string source, TabViewModel tab, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationToken ct)
    {
        if (!CanNavigate(tab))
        {
            return;
        }

        var next = tab.ImageIterator.GetIteration(tab.ImageIterator.CurrentIndex, to);
        await tab.ImageIterator.IterateToIndexAsync(next, ct);
    }

    public ValueTask NavigateToIndexAsync(TabViewModel tab, int index, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public bool CanNavigate(TabViewModel tab)
    {
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}