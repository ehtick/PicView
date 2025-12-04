using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public interface INavigationService
{
    ValueTask LoadFromStringAsync(string source, TabViewModel tab, CancellationToken ct);
    ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationToken ct);
    ValueTask NavigateToIndexAsync(TabViewModel tab, int index, CancellationToken ct);
    bool CanNavigate(TabViewModel tab);
}