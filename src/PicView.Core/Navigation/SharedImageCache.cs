using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class SharedImageCache : IImageCache
{
    // The storage container
    private readonly EvictingDictionary2<PreLoadValue> _items = new(PreLoaderConfig.MaxCount);
    
    // The worker
    private Preloader2 _preLoader;
    
    private readonly Lock _disposeLock = new();

    public SharedImageCache(Func<FileInfo, ValueTask<ImageModel>> imageLoader)
    {
        _preLoader = new Preloader2(imageLoader, this);
    }

    public void Initialize(string ownerId, int index, IReadOnlyList<FileInfo> list)
    {
        _ = _preLoader.PreloadAsync(ownerId, index, false, list, CancellationToken.None);
    }
    
    public async Task<ImageModel?> LoadAsync(string ownerId, int index, IReadOnlyList<FileInfo> list, CancellationToken ct = default)
    {
        return await _preLoader.AddAsync(ownerId, index, list, false, ct).ConfigureAwait(false);
    }

    public void RegisterOwner(object owner)
    {
        _preLoader.RegisterOwner(owner.ToString());
    }

    public async ValueTask RemoveOwner(object owner)
    {
        var id = owner.ToString();
        _items.DecreaseCapacity(id);
        await _preLoader.CancelOwnerInstanceAsync(id).ConfigureAwait(false);
    }

    public bool TryGet(FileInfo f, out PreLoadValue? value)
    {
        return _items.TryGetValueByPath(f.FullName, out value);
    }
    
    public bool TryGet(string ownerId, int index, out PreLoadValue? value)
    {
        return _items.TryGetValue(ownerId, index, out value);
    }
    
    public bool TryGet(ReadOnlySpan<char> f, out PreLoadValue? value)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        foreach (var kvp in _items)
        {
            var fullName = kvp.Value.ImageModel.FileInfo.FullName;
            value = kvp.Value;
            return fullName.AsSpan().Equals(f, comparison);
        }

        value = null;
        return false;
    }

    public void Clear()
    {
        _items.Clear();
    }

    public bool Contains(ReadOnlySpan<char> span, out PreLoadValue? value)
    {
        return TryGet(span, out value);
    }
    
    public bool Contains(PreLoadValue value)
    {
        throw new NotImplementedException();
    }

    public bool Add(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse)
    {
        return Contains(preLoadValue) || _items.TryAdd(ownerId, index, preLoadValue.ImageModel.FileInfo.FullName, preLoadValue, listCount, isReverse, out _);
    }
    
    public bool TryAdd(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse, out PreLoadValue? value)
    {
        var evicted = _items.TryAdd(ownerId, index, preLoadValue.ImageModel.FileInfo.FullName, preLoadValue, listCount, isReverse, out var evictedValue);
        value = evictedValue;
        return evicted;
    }

    public async Task PreloadAsync(string ownerId, int currentIndex, bool reversed, IReadOnlyList<FileInfo> files, CancellationToken ct)
    {
        await _preLoader.PreloadAsync(ownerId, currentIndex, reversed, files, ct).ConfigureAwait(false);
    }

    internal void DisposeHelper(PreLoadValue? item)
    {
        if (item?.ImageModel?.Image is IDisposable disposable)
        {
            _disposeLock.Enter();
            try
            {
                disposable.Dispose();
            }
            finally
            {
                _disposeLock.Exit();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Not fully written yet
        Clear();
    }

    public void Clear(TabViewModel tab)
    {
        RemoveOwner(tab);
    }
}