using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using R3;

namespace PicView.Avalonia.ViewModels;

// TODO: Move this to Core by using interfaces
public class NavigationViewModel : IDisposable
{
    // Reload
    public ReactiveCommand ReloadCommand { get; } = new(async (_, _) =>
    {
        await ErrorHandling.ReloadAsync(UIHelper.GetMainView.DataContext as MainViewModel).ConfigureAwait(false);
    });
    
    // Next
    public ReactiveCommand NextCommand { get; } = new(async (_, _) =>
    {
        await NavigationManager.Iterate(next: true).ConfigureAwait(false);
    });
    
    public ReactiveCommand NextButtonCommand { get; } = new(async (_, _) =>
    {
        await UIHelper.NextButtonNavigation();
    });
    
    public ReactiveCommand NextArrowButtonCommand { get; } = new(async (_, _) =>
    {
        await UIHelper.NextArrowButtonNavigation();
    });
    
    public ReactiveCommand NextFolderCommand { get; } = new(async (_, _) =>
    {
        await NavigationManager.NavigateBetweenDirectories(next: true).ConfigureAwait(false);
    });
    
    // Prev
    public ReactiveCommand PreviousCommand { get; } = new(async (_, _) =>
    {
        await NavigationManager.Iterate(next: false).ConfigureAwait(false);
    });
    public ReactiveCommand PreviousButtonCommand { get; } = new(async (_, _) =>
    {
        await UIHelper.PreviousButtonNavigation();
    });
    public ReactiveCommand PreviousArrowButtonCommand { get; } = new(async (_, _) =>
    {
        await UIHelper.PreviousArrowButtonNavigation();
    });

    public ReactiveCommand PreviousFolderCommand { get; } = new(async (_, _) =>
    {
        await NavigationManager.NavigateBetweenDirectories(next: false).ConfigureAwait(false);
    });
    
    // Skip
    public ReactiveCommand FirstCommand { get; } = new(async (_, _) =>
    {
        await NavigationManager.NavigateFirstOrLast(last: false).ConfigureAwait(false);
    });
    public ReactiveCommand LastCommand { get; } = new(async (_, _) =>
    {
        await NavigationManager.NavigateFirstOrLast(last: true).ConfigureAwait(false);
    });
    public ReactiveCommand Skip10Command { get; } = new(async (_, _) =>
    {
        await NavigationManager.NavigateIncrements(next: true, true, false).ConfigureAwait(false);
    });
    public ReactiveCommand Skip100Command { get; } = new(async (_, _) =>
    {
        await NavigationManager.NavigateIncrements(next: true, false, true).ConfigureAwait(false);
    });
    public ReactiveCommand Prev10Command { get; } = new(async (_, _) =>
    {
        await NavigationManager.NavigateIncrements(next: false, true, false).ConfigureAwait(false);
    });

    public ReactiveCommand Prev100Command { get; } = new(async (_, _) =>
    {
        await NavigationManager.NavigateIncrements(next: false, false, true).ConfigureAwait(false);
    });
    
    public void Dispose()
    {
        Disposable.Dispose(ReloadCommand,
            NextCommand,
            NextButtonCommand,
            NextArrowButtonCommand,
            NextFolderCommand,
            PreviousCommand,
            PreviousButtonCommand,
            PreviousArrowButtonCommand,
            PreviousFolderCommand,
            FirstCommand,
            LastCommand,
            Skip10Command,
            Prev10Command,
            Skip100Command,
            Prev100Command);
    }
}