using System.Collections.ObjectModel;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;
using PicView.Core.Navigation;
using PicView.Core.Navigation.Interfaces;
using R3;

namespace PicView.Core.ViewModels;

/// <summary>
/// The central hub that orchestrates the lifecycle and interaction of all application tabs.
/// <para>
/// This ViewModel manages the collection of <see cref="TabViewModel"/>s, handles the creation and 
/// closing of tabs, and initializes the shared services (Gallery, Navigation, Cache) that 
/// are injected into individual tabs.
/// </para>
/// </summary>
public class TabOverviewViewModel
{
    public BindableReactiveProperty<ObservableCollection<TabViewModel>> Tabs { get; } = new([]);
    public BindableReactiveProperty<int> ActiveTabIndex { get; } = new(0);
    public BindableReactiveProperty<TabViewModel> ActiveTab { get; }
    public BindableReactiveProperty<bool> CanActiveTabNavigate { get; } = new();
    
    /// <summary>
    /// The tab panel should only be visible when there are more than one tab. It should not be possible to close the last tab.
    /// </summary>
    public BindableReactiveProperty<bool> IsTabPanelVisible { get; } = new();
    
    private INavigationService? _sharedNavigation;
    private IImageCache? _sharedCache;
    private IGalleryService? _sharedGallery;
    private IThumbnailLoader? _sharedThumbnailLoader;

    public TabOverviewViewModel()
    {
        ActiveTab = new BindableReactiveProperty<TabViewModel>(CreateInitialTab());
        ActiveTab.Value.IsSelected = true;
    }
    
    public TabOverviewViewModel(TabViewModel initialTab)
    {
        Tabs.Value.Add(initialTab);
        ActiveTab = new BindableReactiveProperty<TabViewModel>(initialTab);
        ActiveTab.Value.IsSelected = true;
        ActiveTabIndex.Value = 0;
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
        _sharedCache.RegisterOwner(ActiveTab.Value);
    }
    
    public void LoadAndInitializeFromPath(List<FileInfo> files, IGalleryService gallery, INavigationService navigationService, IImageCache cache, IThumbnailLoader thumbnailLoader)
    {
        Initialize(gallery, navigationService, cache, thumbnailLoader);
        ActiveTab.Value.InitializeImageIterator(files, cache, thumbnailLoader);
        _sharedCache.PreloadAsync(ActiveTab.Value.Id, ActiveTab.Value.ImageIterator.CurrentIndex, false, files, CancellationToken.None);
        CanActiveTabNavigate.Value = files.Count > 1;
    }
    
    public void LoadAndInitialize(IGalleryService gallery, INavigationService navigationService, IImageCache cache, IThumbnailLoader thumbnailLoader)
    {
        Initialize(gallery, navigationService, cache, thumbnailLoader);
        ActiveTab.Value.Initialize(cache, thumbnailLoader);
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
            tab.Initialize(_sharedCache, _sharedThumbnailLoader);
        }
        _sharedCache.RegisterOwner(tab);
        SelectTab(tab);
    }
    
    public void SelectTab(TabViewModel tab)
    {
        ActiveTab.Value = tab;
        
        // If the tab is floating, IndexOf will be -1. 
        // This effectively "deselects" the tab in the Main Window's TabControl, which is correct behavior.
        ActiveTabIndex.Value = Tabs.Value.IndexOf(tab);
        
        ActiveTab.Value.IsSelected = true;
        CanActiveTabNavigate.Value = ActiveTab.Value.ImageIterator?.Files?.Count > 1;
    }


    public async ValueTask CloseTabAsync(TabViewModel tab)
    {
        // ... (Check for minimum tabs if necessary, usually enforced on main tabs) ...

        var wasActive = ReferenceEquals(tab, ActiveTab.Value);
        
        // 2. Try removing from both collections
        var removedFromMain = Tabs.Value.Remove(tab);

        IsTabPanelVisible.Value = Tabs.Value.Count > 1;

        if (wasActive)
        {
            // If it was a main tab, select the nearest main tab
            if (removedFromMain && Tabs.Value.Count > 0)
            {
                var newIndex = Math.Clamp(ActiveTabIndex.Value, 0, Tabs.Value.Count - 1);
                SelectTab(Tabs.Value[newIndex]);
            }
            // If it was a floating tab, we generally don't automatically focus the main window
            // unless we want that specific behavior.
        }

        if (_sharedCache is not null)
        {
            await _sharedCache.RemoveOwner(tab.Id);
        }
     
        if (tab is not null)
        {
            await tab.DisposeAsync();
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

    #region Navigation

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

        if (!CanActiveTabNavigate.Value || _sharedNavigation is null)
        {
            return;
        }
        var ct = tab.ResetNavigationCts();
        await _sharedNavigation.NavigateAsync(tab, navigateTo, ct).ConfigureAwait(false);
    }


    public async ValueTask LoadFromStringAsync(string source, TabViewModel? senderTab = null)
    {
        if (_sharedNavigation is null)
        {
            return;
        }
        var tab = senderTab ?? ActiveTab.Value;
        var ct = tab.ResetNavigationCts();
        
        await _sharedNavigation.LoadFromStringAsync(source, tab, ct)
            .ConfigureAwait(false);
        CanActiveTabNavigate.Value = tab.ImageIterator?.Files?.Count > 1;
    }
    
    public async ValueTask LoadFromFileAsync(FileInfo file, TabViewModel? senderTab = null)
    {
        if (_sharedNavigation is null)
        {
            return;
        }
        var tab = senderTab ?? ActiveTab.Value;
        var ct = tab.ResetNavigationCts();
        await _sharedNavigation.LoadFromFileAsync(file, tab, ct).ConfigureAwait(false);
        CanActiveTabNavigate.Value = tab.ImageIterator?.Files?.Count > 1;
    }
    
    public async ValueTask LoadFromFileAsync(string file, TabViewModel? senderTab = null)
    {
        await LoadFromFileAsync(new FileInfo(file), senderTab).ConfigureAwait(false);
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
        if (!CanActiveTabNavigate.Value || _sharedNavigation is null)
        {
            return;
        }
        await _sharedNavigation.NavigateToIndexAsync(ActiveTab.Value, index, ActiveTab.Value.NavigationCts).ConfigureAwait(false);
    }

    #endregion
}
