using System.Collections.ObjectModel;
using PicView.Core.Navigation;
using R3;

namespace PicView.Core.ViewModels;

public class NavigationViewModel : IDisposable
{
    public BindableReactiveProperty<ObservableCollection<TabViewModel>>? Tabs { get; } = new([]);
    public BindableReactiveProperty<int> ActiveTabIndex { get; } = new(0);
    
    public BindableReactiveProperty<bool> IsTabPanelVisible { get; } = new();

    private IImageIteratorFactory? _iteratorFactory;
    private INavigationService? _navService;
    private IImageCache? _sharedCache;

    public NavigationViewModel()
    {
        CreateInitialTab();
    }

    public void Initialize(IImageIteratorFactory iteratorFactory, INavigationService navService, IImageCache cache)
    {
        _iteratorFactory = iteratorFactory;
        _navService = navService;
        _sharedCache = cache;

        // Create dummy tabs for testing
        for (var i = 0; i < 3; i++)
        {
            CreateTab();
        }
    }
    
    public void CreateInitialTab()
    {
        //var picModel = new PicViewerModel();
        //var iterator = _iteratorFactory?.Create(file ?? new FileInfo(Environment.CurrentDirectory));

        //var tab = new TabViewModel(picModel, iterator);
        var randomFileName = Path.GetRandomFileName();
        var id = Guid.NewGuid().ToString("N");
        var tabViewModel = new TabViewModel(null, null, id, CloseTab)
        {
            TabTitle = randomFileName,
            TabTooltip = randomFileName
        };
        Tabs.Value.Add(tabViewModel);
        ActiveTabIndex.Value = Tabs.Value.Count - 1;
    }

    public void CreateTab()
    {
        var randomFileName = Path.GetRandomFileName();
        var id = Guid.NewGuid().ToString("N");
        var tabViewModel = new TabViewModel(null, null, id, CloseTab)
        {
            TabTitle = randomFileName,
            TabTooltip = randomFileName
        };
        Tabs.Value.Add(tabViewModel);
        ActiveTabIndex.Value = Tabs.Value.Count - 1;
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
}
