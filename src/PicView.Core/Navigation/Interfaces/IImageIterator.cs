namespace PicView.Core.Navigation.Interfaces;

public interface IImageIterator : IAsyncDisposable
{
    IReadOnlyList<FileInfo> Files { get; }
    int CurrentIndex { get; }
    bool IsReversed { get; }
    void Initialize(List<FileInfo> files, int initialIndex = 0);
    int GetIteration(int index, NavigateTo navigation, bool skip1=false, bool skip10=false, bool skip100=false);
    ValueTask IterateToIndexAsync(int index, CancellationTokenSource ct);
    ValueTask RepeatNavigateAsync(NavigateTo to, TimeSpan repeatInterval, CancellationToken ct);  // held-key repeat
    ValueTask SlimUpdate(int index, object? imageSource);
    ValueTask IterateToIndexSlim(int index, bool isReverse, CancellationToken token);
    ValueTask ReloadFileListAsync(CancellationToken ct);
}