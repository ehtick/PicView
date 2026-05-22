namespace PicView.Core.Navigation.Interfaces;

/// <summary>
/// Defines the contract for navigating a collection of files within a specific context (Tab).
/// </summary>
public interface IImageIterator : IDisposable
{
    public IImageCache Cache { get; }
    public string? CurrentDirectory { get; }

    /// <summary>
    /// The list of files currently being iterated.
    /// </summary>
    IReadOnlyList<FileInfo> Files { get; internal set; }

    /// <summary>
    /// Gets the current position in the file list.
    /// </summary>
    int CurrentIndex { get; }
    
    int SecondaryCurrentIndex { get; }

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
    
    ValueTask NavigateAsync(NavigateTo to, SkipAmount skipAmount, CancellationTokenSource ct);
    
    ValueTask ReloadAsync(CancellationTokenSource ct);
    
    ValueTask ReloadFileListAsync(CancellationTokenSource ct);

    /// <summary>
    /// Moves the iterator to the specified index, triggers image loading, and handles UI updates.
    /// </summary>
    ValueTask IterateToIndexAsync(int index, CancellationTokenSource ct);
    
    ValueTask IterateToIndicesAsync(int index, int secondaryIndex, CancellationTokenSource ct);

    /// <summary>
    /// Checks if the target index is cached before iterating. If not cached, clears cache first.
    /// </summary>
    ValueTask SkipToIndexAsync(int index, CancellationTokenSource ct);

    ValueTask NavigateByIncrementsAsync(SkipAmount skipAmount, bool forwards, CancellationTokenSource ct);

    /// <summary>
    /// Handles continuous navigation (e.g., holding down a key) at a set interval.
    /// </summary>
    ValueTask RepeatNavigateAsync(NavigateTo to, TimeSpan repeatInterval, CancellationToken ct);
    
    /// <summary>
    /// Stops the continuous navigation started by <see cref="RepeatNavigateAsync"/>.
    /// </summary>
    void StopRepeatedNavigation();

    /// <summary>
    /// Updates the forward/backward navigation permissions based on the current index and list bounds.
    /// </summary>
    void UpdateNavigationProperties();
}