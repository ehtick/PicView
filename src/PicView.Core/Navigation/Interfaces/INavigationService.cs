using PicView.Core.FileSorting;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation.Interfaces;

public interface INavigationService
{
    ValueTask RepopulateIterator(FileInfo fileInfo, TabViewModel tab, CancellationTokenSource ct, List<FileInfo>? files = null);
    
    ValueTask LoadFromFileAsync(string source, TabViewModel tab, CancellationTokenSource ct);
    
    ValueTask LoadFromFileAsync(FileInfo fileInfo, TabViewModel tab, CancellationTokenSource ct);

    ValueTask LoadFromStringAsync(string source, TabViewModel tab, CancellationTokenSource ct);

    ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationTokenSource ct);

    ValueTask NavigateToIndexAsync(TabViewModel tab, int index, CancellationTokenSource ct);

    ValueTask NavigateByIncrementsAsync(TabViewModel tab, SkipAmount skipAmount, bool forwards, CancellationTokenSource ct);
    
    bool CanNavigate(TabViewModel tab);

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
}