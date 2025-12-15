using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

/// <summary>
/// Acts as the central station for acquiring and managing cached <see cref="ImageModel"/> resources.
/// <para>
/// This class coordinates between the storage container (<see cref="EvictingDictionary2{TValue}"/>) 
/// and the background worker (<see cref="Preloader2"/>) to ensure images are loaded, retrieved, 
/// and evicted efficiently across multiple tab owners.
/// </para>
/// </summary>
public class SharedImageCache : IImageCache
{
    // The storage container
    private readonly EvictingDictionary2<PreLoadValue> _items = new(PreLoaderConfig.MaxCount);
    
    // The worker
    private readonly Preloader2 _preLoader;
    
    private readonly Lock _disposeLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SharedImageCache"/> class.
    /// </summary>
    /// <param name="imageLoader">The function used to load an ImageModel from a FileInfo.</param>
    public SharedImageCache(Func<FileInfo, ValueTask<ImageModel>> imageLoader)
    {
        _preLoader = new Preloader2(imageLoader, this);
    }
    
    public async Task<ImageModel?> LoadAsync(string ownerId, int index, IReadOnlyList<FileInfo> list, CancellationToken ct = default)
    {
        return await _preLoader.AddAsync(ownerId, index, list, false, ct).ConfigureAwait(false);
    }

    public void RegisterOwner(string ownerId)
    {
        _items.ExpandCapacity(ownerId); 
        _preLoader.RegisterOwner(ownerId);
    }

    public async ValueTask RemoveOwner(string ownerId)
    {
        _items.DecreaseCapacity(ownerId);
        await _preLoader.CancelOwnerInstanceAsync(ownerId).ConfigureAwait(false);
    }

    public bool TryGet(FileInfo f, out PreLoadValue? value) =>
        _items.TryGetValueByPath(f.FullName, out value);

    public bool TryGet(string ownerId, int index, out PreLoadValue? value) =>
        _items.TryGetValue(ownerId, index, out value);

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

    public void Clear() =>
        _items.Clear();

    public void Clear(string ownerId) =>
        _items.Clear(ownerId);

    public bool Contains(ReadOnlySpan<char> span, out PreLoadValue? value) =>
        TryGet(span, out value);

    public bool Contains(PreLoadValue value) =>
        TryGet(value.ImageModel.FileInfo.FullName, out var preLoadValue) && preLoadValue == value;

    public bool Add(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse) =>
        Contains(preLoadValue) || _items.TryAdd(ownerId, index, preLoadValue.ImageModel.FileInfo.FullName, preLoadValue, listCount, isReverse, out _);

    public bool TryAdd(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse, out PreLoadValue? value)
    {
        var evicted = _items.TryAdd(ownerId, index, preLoadValue.ImageModel.FileInfo.FullName, preLoadValue, listCount, isReverse, out var evictedValue);
        value = evictedValue;
        return evicted;
    }

    public async Task PreloadAsync(string ownerId, int currentIndex, bool reversed, IReadOnlyList<FileInfo> files, CancellationToken ct) 
        => await _preLoader.PreloadAsync(ownerId, currentIndex, reversed, files, ct).ConfigureAwait(false);
    
    public async ValueTask Clear(TabViewModel tab) =>
        await RemoveOwner(tab.Id);

    internal void DisposeHelper(PreLoadValue? item)
    {
        if (item?.ImageModel?.Image is not IDisposable disposable)
        {
            return;
        }

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