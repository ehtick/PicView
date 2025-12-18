namespace PicView.Core.Navigation.Interfaces;

/// <summary>
/// Defines the contract for navigating a collection of files within a specific context (Tab).
/// <para>
/// This interface manages the file list and the "Current Index" cursor. It determines 
/// logic for looping, skipping (next/previous), and resolving the next target index.
/// </para>
/// </summary>
public interface IImageIterator : IAsyncDisposable
{
    /// <summary>
    /// The list of files currently being iterated.
    /// </summary>
    IReadOnlyList<FileInfo> Files { get; internal set; }

    /// <summary>
    /// Gets the current position in the file list.
    /// </summary>
    int CurrentIndex { get; }

    /// <summary>
    /// Gets a value indicating if the last navigation action was backwards.
    /// </summary>
    bool IsReversed { get; }

    /// <summary>
    /// Manually forces the current index to a specific value.
    /// </summary>
    void SetCurrentIndex(int index);

    /// <summary>
    /// Initializes the iterator with a new list of files and a starting position.
    /// </summary>
    void Initialize(IReadOnlyList<FileInfo> files, int initialIndex = 0);

    /// <summary>
    /// Calculates the next index based on navigation direction and skip modifiers.
    /// </summary>
    /// <returns>The calculated target index.</returns>
    int GetIteration(int index, NavigateTo navigation, string tabId, SkipAmount skipAmount);

    /// <summary>
    /// Moves the iterator to the specified index and triggers the loading of that image.
    /// </summary>
    ValueTask IterateToIndexAsync(int index, CancellationTokenSource ct);

    ValueTask NavigateByIncrementsAsync(SkipAmount skipAmount, bool forwards, CancellationTokenSource ct);

    /// <summary>
    /// Handles continuous navigation (e.g., holding down a key).
    /// </summary>
    ValueTask RepeatNavigateAsync(NavigateTo to, TimeSpan repeatInterval, CancellationToken ct);
    ValueTask ReloadFileListAsync(CancellationToken ct);
}