using System.Collections.ObjectModel;
using PicView.Core.Gallery;
using PicView.Core.Navigation;
using R3;

namespace PicView.Core.ViewModels;

public class NavigationViewModel
{
    public BindableReactiveProperty<ObservableCollection<TabViewModel>>? Tabs { get; } = new([]);
    public BindableReactiveProperty<int> ActiveTabIndex { get; } = new(0);
    public BindableReactiveProperty<TabViewModel> ActiveTab { get; }
    public BindableReactiveProperty<bool> IsTabPanelVisible { get; } = new();
    
    private INavigationService? _sharedNavigation;
    private IImageCache? _sharedCache;
    private IGalleryService? _sharedGallery;

    public NavigationViewModel()
    {
        ActiveTab = new BindableReactiveProperty<TabViewModel>(CreateInitialTab());
    }

    public void Initialize(IGalleryService gallery, INavigationService navigationService, IImageCache cache)
    {
        _sharedCache = cache;
        _sharedGallery = gallery;
        _sharedNavigation = navigationService;
    }
    
    public TabViewModel CreateInitialTab()
    {
        var id = Guid.NewGuid().ToString("N");
        var tabViewModel = new TabViewModel(id, CloseTab);
        Tabs.Value.Add(tabViewModel);
        return tabViewModel;
    }

    public void CreateTab()
    {
        var id = Guid.NewGuid().ToString("N");
        var tabViewModel = new TabViewModel(id, CloseTab);
        Tabs.Value.Add(tabViewModel);
        IsTabPanelVisible.Value = Tabs.Value.Count > 1;
    }

    public async ValueTask CloseTab(TabViewModel tab)
    {
        Tabs.Value.Remove(tab);
        IsTabPanelVisible.Value = Tabs.Value.Count > 1;
        if (tab is not null)
        {
            await tab.DisposeAsync();
        }
    }
    
    public async ValueTask CloseTab(string tabId)
    {
        var tab = Tabs.Value.FirstOrDefault(t => t.Id == tabId);
        if (tab is not null)
        {
            await CloseTab(tab);
        }
    }

    public void Dispose()
    {
        Tabs.Value.Clear();
    }

    #region Navigation

    public bool CanNavigate(TabViewModel tab) =>
        tab.CanNavigate();

    public async ValueTask NextFile() =>
        await NextFileCore(NavigateTo.Next);

    public async ValueTask PrevFile() =>
        await NextFileCore(NavigateTo.Previous);
    
    public async ValueTask FirstFile() =>
        await NextFileCore(NavigateTo.First);
    public async ValueTask LastFile() =>
        await NextFileCore(NavigateTo.Last);

    private async ValueTask NextFileCore(NavigateTo navigateTo)
    {
        if (!CanNavigate(ActiveTab.Value) || _sharedNavigation is null)
        {
            return;
        }
        await NavigateAsync(ActiveTab.Value, navigateTo, ActiveTab.Value.NavigationCts.Token).ConfigureAwait(false);
    }

    public async ValueTask LoadFromStringAsync(string source)
    {
        if (_sharedNavigation is null)
        {
            return;
        }
        await _sharedNavigation.LoadFromStringAsync(source, ActiveTab.Value, ActiveTab.Value.NavigationCts.Token).ConfigureAwait(false);
    }
    public async ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationToken ct)
    {
        if (_sharedNavigation is null || tab.ImageIterator is null)
        {
            return;
        }
        await _sharedNavigation.NavigateAsync(tab, to, ct).ConfigureAwait(false);
    }
    public async ValueTask NavigateToIndexAsync(int index)
    {
        if (!CanNavigate(ActiveTab.Value) || _sharedNavigation is null)
        {
            return;
        }
        await _sharedNavigation.NavigateToIndexAsync(ActiveTab.Value, index, ActiveTab.Value.NavigationCts.Token).ConfigureAwait(false);
    }
    

    #endregion
}
