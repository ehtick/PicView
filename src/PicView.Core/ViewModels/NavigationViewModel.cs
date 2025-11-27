using System.Collections.ObjectModel;
using PicView.Core.Navigation;
using R3;

namespace PicView.Core.ViewModels;

public class NavigationViewModel : IDisposable
{
    public ObservableCollection<TabViewModel> Tabs { get; } = [];
    public BindableReactiveProperty<int> ActiveTabIndex { get; } = new(0);

    private readonly IImageIteratorFactory _iteratorFactory;
    private readonly INavigationService _navService;
    private readonly IImageCache _sharedCache;

    public NavigationViewModel(IImageIteratorFactory iteratorFactory, INavigationService navService, IImageCache cache)
    {
        _iteratorFactory = iteratorFactory;
        _navService = navService;
        _sharedCache = cache;
    }

    public TabViewModel CreateTab(FileInfo? file = null)
    {
        var picModel = new PicViewerModel();
        var iterator = _iteratorFactory.Create(file ?? new FileInfo(Environment.CurrentDirectory));

        var tab = new TabViewModel(picModel, iterator);
        Tabs.Add(tab);
        ActiveTabIndex.Value = Tabs.Count - 1;
        return tab;
    }

    public void CloseTab(TabViewModel tab)
    {
        tab.Dispose();
        Tabs.Remove(tab);
        if (Tabs.Count == 0)
        {
            // maybe create an empty tab
        }
    }

    public void Dispose()
    {
        foreach (var t in Tabs) t.Dispose();
        Tabs.Clear();
    }
}
