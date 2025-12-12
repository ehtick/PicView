using System.Collections.ObjectModel;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;
using PicView.Core.Navigation;
using PicView.Core.Navigation.Interfaces;
using R3;

namespace PicView.Core.ViewModels;

public class NavigationViewModel
{
    public TitleViewModel TitleViewModel { get; } = new();
    public BindableReactiveProperty<ObservableCollection<TabViewModel>>? Tabs { get; } = new([]);
    public BindableReactiveProperty<int> ActiveTabIndex { get; } = new(0);
    public BindableReactiveProperty<TabViewModel> ActiveTab { get; }
    
    /// <summary>
    /// The tab panel should only be visible when there are more than one tab. It should not be possible to close the last tab.
    /// </summary>
    public BindableReactiveProperty<bool> IsTabPanelVisible { get; } = new();
    
    private INavigationService? _sharedNavigation;
    private IImageCache? _sharedCache;
    private IGalleryService? _sharedGallery;
    private IThumbnailLoader? _sharedThumbnailLoader;

    public NavigationViewModel()
    {
        ActiveTab = new BindableReactiveProperty<TabViewModel>(CreateInitialTab());
        ActiveTab.Value.IsSelected = true;
    }

    /// <summary>
    /// Initialize the navigation service. Must be called before any other methods.
    /// </summary>
    /// <remarks>This is separated from constructor to improve initial startup time</remarks>
    /// <param name="gallery">The Gallery service shared between tabs in the same directory,
    /// to reduce application memory and switch between tabs, if the selected gallery item is within another tab</param>
    /// <param name="navigationService">The navigation service responsible for navigating within the tabs</param>
    /// <param name="cache">The bitmap cache shared between tabs to reduce application memory usage</param>
    /// <param name="thumbnailLoader">The thumbnail loader to use for preloading images</param>
    public void Initialize(IGalleryService gallery, INavigationService navigationService, IImageCache cache, IThumbnailLoader thumbnailLoader)
    {
        _sharedCache = cache;
        _sharedGallery = gallery;
        _sharedNavigation = navigationService;
        _sharedThumbnailLoader = thumbnailLoader;
    }
    
    /// <summary>
    /// Creates the initial tab and adds it to the tabs collection. Used to show the initial image in the application,
    /// before the Initialize has been called.
    /// </summary>
    /// <returns></returns>
    public TabViewModel CreateInitialTab()
    {
        var tab = CreateTabInternal();
        tab.IsSelected = true;
        return tab;
    }


    private TabViewModel CreateTabInternal()
    {
        var id = Guid.NewGuid().ToString("N");
        var tab = new TabViewModel(id, CloseTabAsync);
        Tabs.Value.Add(tab);
        return tab;
    }

    public void CreateTab()
    {
        var tab = CreateTabInternal();
        if (_sharedCache != null && _sharedThumbnailLoader != null)
        {
            tab.Initialize(_sharedCache, _sharedThumbnailLoader, TitleViewModel);
        }
        ActiveTab.Value = tab;
        ActiveTabIndex.Value = Tabs.Value.IndexOf(tab);
    }
    
    private void SelectTab(TabViewModel tab)
    {
        ActiveTab.Value = tab;
        ActiveTabIndex.Value = Tabs.Value.IndexOf(tab);
        ActiveTab.Value.IsSelected = true;
    }


    public async ValueTask CloseTabAsync(TabViewModel tab)
    {
        if (Tabs.Value.Count <= 0)
        {
#if DEBUG
            DebugHelper.LogDebug(nameof(NavigationViewModel), nameof(CloseTabAsync), new Exception("There should always be at least one tab open."));
#endif
            return;
        }
        var wasActive = ReferenceEquals(tab, ActiveTab.Value);
        Tabs.Value.Remove(tab);
        IsTabPanelVisible.Value = Tabs.Value.Count > 1;
        if (tab is not null)
        {
            await tab.DisposeAsync();
        }

        if (wasActive)
        {
            // pick nearest tab
            var newIndex = Math.Clamp(ActiveTabIndex.Value, 0, Tabs.Value.Count - 1);
            if (Tabs.Value.Count > 0)
            {
                SelectTab(Tabs.Value[newIndex]);
            }
        }
    }
    
    public async ValueTask CloseTabAsync(string tabId)
    {
        var tab = Tabs.Value.FirstOrDefault(t => t.Id == tabId);
        if (tab is not null)
        {
            await CloseTabAsync(tab);
        }
    }

    public void Dispose()
    {
        Tabs.Value.Clear();
    }
    
    public void LoadAndInitializeFromPath(List<FileInfo> files, IGalleryService gallery, INavigationService navigationService, IImageCache cache, IThumbnailLoader thumbnailLoader)
    {
        Initialize(gallery, navigationService, cache, thumbnailLoader);
        ActiveTab.Value.InitializeImageIterator(files, cache, thumbnailLoader);
        _sharedCache.PreloadAsync(ActiveTab.Value.Id, ActiveTab.Value.ImageIterator.CurrentIndex, false, files, CancellationToken.None);
    }

    #region Navigation

    public bool CanNavigate(TabViewModel tab) =>
        tab.CanNavigate();
    public bool CanNavigate() =>
        ActiveTab.Value.CanNavigate();

    public async ValueTask NextFile() =>
        await NextFileCore(NavigateTo.Next).ConfigureAwait(false);

    public async ValueTask PrevFile() =>
        await NextFileCore(NavigateTo.Previous).ConfigureAwait(false);
    
    public async ValueTask FirstFile() =>
        await NextFileCore(NavigateTo.First).ConfigureAwait(false);

    public async ValueTask LastFile() =>
        await NextFileCore(NavigateTo.Last).ConfigureAwait(false);

    private async ValueTask NextFileCore(NavigateTo navigateTo)
    {
        var tab = ActiveTab.Value;

        if (!CanNavigate(tab) || _sharedNavigation is null)
        {
            return;
        }
        var ct = tab.ResetNavigationCts();
        await _sharedNavigation.NavigateAsync(tab, navigateTo, ct).ConfigureAwait(false);
    }


    public async ValueTask LoadFromStringAsync(string source)
    {
        if (_sharedNavigation is null)
        {
            return;
        }
        var tab = ActiveTab.Value;
        var ct = tab.ResetNavigationCts();
        
        await _sharedNavigation.LoadFromStringAsync(source, tab, ct)
            .ConfigureAwait(false);
    }
    public async ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationTokenSource ct)
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
        await _sharedNavigation.NavigateToIndexAsync(ActiveTab.Value, index, ActiveTab.Value.NavigationCts).ConfigureAwait(false);
    }

    #endregion
}
