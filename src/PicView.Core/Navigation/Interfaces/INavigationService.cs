using PicView.Core.FileSorting;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation.Interfaces;

public interface INavigationService
{
    ValueTask LoadFromFileAsync(string source, TabViewModel tab, CancellationTokenSource ct);
    
    ValueTask LoadFromFileAsync(FileInfo fileInfo, TabViewModel tab, CancellationTokenSource ct);

    ValueTask<bool> LoadFromStringAsync(string source, TabViewModel tab, CancellationTokenSource ct);

    ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationTokenSource ct);

    ValueTask NavigateByIncrementsAsync(TabViewModel tab, SkipAmount skipAmount, bool forwards, CancellationTokenSource ct);

    /// <summary>
    /// Sorts the files in the current tab according to the specified sort order,
    /// updates the current index to maintain the current image, and resynchronizes the cache.
    /// </summary>
    ValueTask SortAsync(TabViewModel tab, SortFilesBy sortOrder, CancellationTokenSource ct);

    /// <summary>
    /// Updates the sort direction (ascending/descending) for the current tab,
    /// updates the current index to maintain the current image, and resynchronizes the cache.
    /// </summary>
    ValueTask SortAsync(TabViewModel tab, bool ascending, CancellationTokenSource ct);

    ValueTask<bool> LoadLastFileAsync(TabViewModel tab, CancellationTokenSource ct);
}