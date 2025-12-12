using PicView.Core.Models;
using PicView.Core.Preloading;

namespace PicView.Core.Navigation.Interfaces;

public interface IImageCache : IAsyncDisposable
{
    Task<ImageModel?> LoadAsync(string ownerId, int index, IReadOnlyList<FileInfo> list,
        CancellationToken ct = default);
    
    bool TryGet(FileInfo f, out PreLoadValue? value);
    bool TryGet(string ownerId, int index, out PreLoadValue? value);
    void Clear();
    bool Contains(ReadOnlySpan<char> span, out PreLoadValue? value);
    bool Add(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse);
    bool TryAdd(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse, out PreLoadValue? value);
    Task PreloadAsync(string ownerId, int currentIndex, bool reversed, IReadOnlyList<FileInfo> files, CancellationToken ct);
    
    /// <summary>
    /// Removes an owner from the cache tracking. 
    /// Should be called when a Tab is closed.
    /// </summary>
    ValueTask RemoveOwner(object owner);
    void RegisterOwner(object owner);

}