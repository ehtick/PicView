using System.Collections.ObjectModel;
using PicView.Core.FileSorting;
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
    
    public INavigationService? SharedNavigation { get; private set; }
    public IImageCache? SharedCache { get; private set; }
    public IThumbnailLoader? SharedThumbnailLoader { get; private set; }
    public IFileWatcherService? SharedFileWatcher { get; private set; }

    // Needed for correct context in multi-window scenarios
    private object? _parentVm;

    public TabOverviewViewModel()
    {
        ActiveTab = new BindableReactiveProperty<TabViewModel>(CreateInitialTab());
        ActiveTab.Value.IsSelected = true;
    }

    /// <summary>
    /// Initialize the navigation service. Must be called before any other methods.
    /// </summary>
    /// <remarks>This is separated from constructor to improve initial startup time</remarks>
    /// <param name="navigationService">The navigation service responsible for navigating within the tabs</param>
    /// <param name="cache">The bitmap cache shared between tabs to reduce application memory usage</param>
    /// <param name="thumbnailLoader">The thumbnail loader to use for preloading images</param>
    /// <param name="fileWatcherService">The service that watches for file changes in the directory</param>
    private void Initialize(INavigationService navigationService, IImageCache cache, IThumbnailLoader thumbnailLoader, IFileWatcherService fileWatcherService)
    {
        SharedCache = cache;
        SharedNavigation = navigationService;
        SharedThumbnailLoader = thumbnailLoader;
        SharedFileWatcher = fileWatcherService;
        SharedCache.RegisterOwner(ActiveTab.Value.Id);
    }
    
    /// <inheritdoc cref="Initialize(INavigationService, IImageCache, IThumbnailLoader, IFileWatcherService)"/>>
    public void LoadAndInitializeFromPath(IReadOnlyList<FileInfo> files, INavigationService navigationService, IImageCache cache, IThumbnailLoader thumbnailLoader, IFileWatcherService fileWatcherService)
    {
        Initialize(navigationService, cache, thumbnailLoader, fileWatcherService);
        ActiveTab.Value.InitializeImageIterator(files, cache, thumbnailLoader, fileWatcherService);
        SharedCache.Preload(ActiveTab.Value.Id, ActiveTab.Value.ImageIterator.CurrentIndex, false, files);
        CanActiveTabNavigate.Value = files.Count > 1;
    }
    /// <inheritdoc cref="Initialize(INavigationService, IImageCache, IThumbnailLoader, IFileWatcherService)"/>>
    public void LoadAndInitialize(INavigationService navigationService, IImageCache cache, IThumbnailLoader thumbnailLoader, IFileWatcherService fileWatcherService)
    {
        Initialize(navigationService, cache, thumbnailLoader, fileWatcherService);
        ActiveTab.Value.InitializeImageIterator([], cache, thumbnailLoader, fileWatcherService);
        ActiveTab.Value.Initialize(cache, thumbnailLoader, fileWatcherService);
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
    
    private TabViewModel CreateTabInternal(FileInfo? file = null)
    {
        var id = Guid.NewGuid().ToString("N");
        var tab = new TabViewModel(id, CloseTabAsync, SharedFileWatcher);
        tab.ParentWindowContext = _parentVm;
        if (file is not null)
        {
            tab.Model.FileInfo = file;
        }
        Tabs.Value.Add(tab);
        return tab;
    }

    public TabViewModel CreateTab(FileInfo? file = null)
    {
        var tab = CreateTabInternal(file);
        if (SharedCache != null && SharedThumbnailLoader != null)
        {
            tab.Initialize(SharedCache, SharedThumbnailLoader, SharedFileWatcher);
        }
        SharedCache.RegisterOwner(tab.Id);
        SelectTab(tab);
        IsTabPanelVisible.Value = Tabs.CurrentValue.Count > 1;
        return tab;
    }

    public async ValueTask CreateNewTabFromFileAsync(string filePath)
        => await CreateNewTabFromFileAsync(new FileInfo(filePath));
    public async ValueTask CreateNewTabFromFileAsync(FileInfo file)
    {
        var tab = CreateTab(file);
        await SharedNavigation.LoadFromFileAsync(file, tab, tab.GetTabCancellation())
            .ConfigureAwait(false);
    }
    
    public async ValueTask CreateNewTabFromStringAsync(string source)
    {
        var tab = CreateTab();
        await SharedNavigation.LoadFromStringAsync(source, tab, tab.GetTabCancellation())
            .ConfigureAwait(false);
    }
    
    public void SetParentContext(object parent)
    {
        _parentVm = parent;
        // Update existing tabs if any
        foreach(var tab in Tabs.Value)
        {
            tab.ParentWindowContext = parent;
        }
    }
    
    public void SelectTab(TabViewModel tab)
    {
        ActiveTab.Value = tab;
        
        // If the tab is detached, IndexOf will be -1. 
        // This effectively "deselects" the tab in the Main Window's TabControl, which is correct behavior.
        ActiveTabIndex.Value = Tabs.Value.IndexOf(tab);
        
        ActiveTab.Value.IsSelected = true;
        CanActiveTabNavigate.Value = ActiveTab.Value.ImageIterator?.Files?.Count > 1;
    }
    
    public async ValueTask CloseTabAsync()
    {
        var tab = ActiveTab.Value;
        await CloseTabAsync(tab);
    }
    
    public async ValueTask CloseTabAsync(string tabId)
    {
        var tab = Tabs.Value.FirstOrDefault(t => t.Id == tabId);
        if (tab is not null)
        {
            await CloseTabAsync(tab);
        }
    }


    public async ValueTask CloseTabAsync(TabViewModel tab)
    {
        // 1. Guard Clause: Prevent closing if this is the last tab.
        if (Tabs.Value.Count <= 1)
        {
            return;
        }

        var wasActive = ReferenceEquals(tab, ActiveTab.Value);
    
        // 2. Capture the index BEFORE removing the item
        var indexToRemove = Tabs.Value.IndexOf(tab);
        Tabs.Value.Remove(tab);

        IsTabPanelVisible.Value = Tabs.Value.Count > 1;

        if (wasActive && Tabs.Value.Count > 0)
        {
            // 3. Calculate the new index. 
            // We subtract 1 to go "behind" (left). 
            // We use Math.Max(0, ...) to ensure we don't go below zero if the first tab was closed.
            var targetIndex = Math.Max(0, indexToRemove - 1);
        
            // 4. Clamp ensures we don't exceed the new count
            var newIndex = Math.Clamp(targetIndex, 0, Tabs.Value.Count - 1);
            SelectTab(Tabs.Value[newIndex]);
        }

        if (SharedCache is not null)
        {
            await SharedCache.RemoveOwner(tab.Id);
        }
    
        if (tab is not null)
        {
            await tab.DisposeAsync();
        }
    }

    // Used when detaching the tab
    public void RemoveTab(TabViewModel tab)
    {
        var wasActive = ReferenceEquals(tab, ActiveTab.Value);
    
        // Capture index
        var indexToRemove = Tabs.Value.IndexOf(tab);
    
        Tabs.Value.Remove(tab);

        if (!wasActive || Tabs.Value.Count <= 0)
        {
            return;
        }

        // Target the previous tab
        var targetIndex = Math.Max(0, indexToRemove - 1);
        var newIndex = Math.Clamp(targetIndex, 0, Tabs.Value.Count - 1);
        
        SelectTab(Tabs.Value[newIndex]);
    }

    #region Navigation

    public async ValueTask NextFile() =>
        await NextFileCore(NavigateTo.Next).ConfigureAwait(false);

    public async ValueTask PrevFile() =>
        await NextFileCore(NavigateTo.Previous).ConfigureAwait(false);

    public async ValueTask NavigateDirectionalAsync(bool isKeyHeldDown, NavigateTo direction)
    {
        var tab = ActiveTab.Value;
        if (!CanActiveTabNavigate.Value || SharedNavigation is null || tab.ImageIterator is null)
        {
            return;
        }

        if (isKeyHeldDown)
        {
            var ct = tab.GetTabCancellation();
            await tab.ImageIterator.RepeatNavigateAsync(direction,
                TimeSpan.FromSeconds(Settings.UIProperties.NavSpeed), ct.Token).ConfigureAwait(false);
        }
        else
        {
            await NextFileCore(direction).ConfigureAwait(false);
        }
    }

    public void StopRepeatedNavigation()
    {
        var tab = ActiveTab.Value;
        tab.ImageIterator?.StopRepeatedNavigation();
    }
    
    public async ValueTask FirstFile() =>
        await NextFileCore(NavigateTo.First).ConfigureAwait(false);

    public async ValueTask LastFile() =>
        await NextFileCore(NavigateTo.Last).ConfigureAwait(false);
    
    public async ValueTask Next10() =>
        await IncrementsCore(SkipAmount.Ten, true).ConfigureAwait(false);
    
    public async ValueTask Next100() =>
        await IncrementsCore(SkipAmount.Hundred, true).ConfigureAwait(false);
    
    public async ValueTask Prev10() =>
        await IncrementsCore(SkipAmount.Ten, false).ConfigureAwait(false);
    
    public async ValueTask Prev100() =>
        await IncrementsCore(SkipAmount.Hundred, false).ConfigureAwait(false);

    private async ValueTask NextFileCore(NavigateTo navigateTo)
    {
        var tab = ActiveTab.Value;

        if (!CanActiveTabNavigate.Value || SharedNavigation is null)
        {
            return;
        }
        var ct = tab.GetTabCancellation();
        await SharedNavigation.NavigateAsync(tab, navigateTo, ct).ConfigureAwait(false);
    }
    
    private async ValueTask IncrementsCore(SkipAmount skipAmount, bool forwards)
    {
        var tab = ActiveTab.Value;
        
        if (SharedNavigation is null || tab.ImageIterator is null)
        {
            return;
        }
        var ct = tab.GetTabCancellation();
        await SharedNavigation.NavigateByIncrementsAsync(tab, skipAmount, forwards, ct).ConfigureAwait(false);
    }


    public async ValueTask LoadFromStringAsync(string source, TabViewModel? senderTab = null)
    {
        if (SharedNavigation is null)
        {
            return;
        }
        var tab = senderTab ?? ActiveTab.Value;
        var ct = tab.GetTabCancellation();
        
        await SharedNavigation.LoadFromStringAsync(source, tab, ct)
            .ConfigureAwait(false);
        ActiveTab.Value = tab;
        CanActiveTabNavigate.Value = tab.ImageIterator.Files.Count > 1;
    }
    
    public async ValueTask LoadFromFileAsync(FileInfo file, TabViewModel? senderTab = null)
    {
        if (SharedNavigation is null)
        {
            return;
        }
        var tab = senderTab ?? ActiveTab.Value;
        var ct = tab.GetTabCancellation();
        await SharedNavigation.LoadFromFileAsync(file, tab, ct).ConfigureAwait(false);
        CanActiveTabNavigate.Value = tab.ImageIterator?.Files?.Count > 1;
    }
    
    public async ValueTask LoadFromFileAsync(string file, TabViewModel? senderTab = null)
    {
        await LoadFromFileAsync(new FileInfo(file), senderTab).ConfigureAwait(false);
    }
    
    public async ValueTask NavigateToIndexAsync(int index, TabViewModel? senderTab = null)
    {
        if (SharedNavigation is null)
        {
            return;
        }
        var tab = senderTab ?? ActiveTab.Value;
        var ct = tab.GetTabCancellation();
        await SharedNavigation.NavigateToIndexAsync(tab, index, ct).ConfigureAwait(false);
        CanActiveTabNavigate.Value = tab.ImageIterator?.Files?.Count > 1;
    }
    #endregion

    #region Sort

    public async ValueTask SortAsync(SortFilesBy sortOrder)
    {
        if (SharedNavigation is null)
        {
            return;
        }
        var tab = ActiveTab.Value;
        var ct = tab.GetTabCancellation();
        await SharedNavigation.SortAsync(tab, sortOrder, ct).ConfigureAwait(false);
    }
    
    public async ValueTask SortAsync(bool ascending)
    {
        if (SharedNavigation is null)
        {
            return;
        }
        var tab = ActiveTab.Value;
        var ct = tab.GetTabCancellation();
        await SharedNavigation.SortAsync(tab, ascending, ct).ConfigureAwait(false);
    }
    

    #endregion
}
