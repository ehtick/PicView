using PicView.Core.ViewModels;

namespace PicView.Core.Navigation.Interfaces;

public interface INavigationService
{
    ValueTask LoadFromStringAsync(string source, TabViewModel tab, CancellationTokenSource ct);
    ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationTokenSource ct);
    ValueTask NavigateToIndexAsync(TabViewModel tab, int index, CancellationTokenSource ct);
    bool CanNavigate(TabViewModel tab);
}