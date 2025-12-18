using PicView.Core.DebugTools;
using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class ImageIterator(IImageCache cache, IThumbnailLoader thumbnailLoader, TabViewModel tab) : IImageIterator
{
    private readonly IImageCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly TabViewModel _tab = tab ?? throw new ArgumentNullException(nameof(tab));
    private readonly IThumbnailLoader _thumbnailLoader = thumbnailLoader ?? throw new ArgumentNullException(nameof(thumbnailLoader));
    
    public IReadOnlyList<FileInfo> Files { get; set; } = [];

    public int CurrentIndex { get; private set; } = -1;
    public bool IsReversed { get; private set; }

    public void Initialize(IReadOnlyList<FileInfo> files, int initialIndex = 0)
    {
        Files = files ?? [];
        CurrentIndex = Math.Clamp(initialIndex, 0, Math.Max(0, Files.Count - 1));
    }

    public async ValueTask IterateToIndexAsync(int index, CancellationTokenSource ct)
    {
        if (index < 0 || index >= Files.Count)
        {
            return;
        }

        CurrentIndex = index;
        var targetFile = Files[index];

        // 1. Try to get from Cache (Fastest)
        var loadedFromCache = await TryLoadFromCacheAsync(index, targetFile, ct).ConfigureAwait(false);

        // 2. If cache missed or failed, load manually (Slower, prioritized)
        if (!loadedFromCache)
        {
            await LoadManuallyAsync(index, ct).ConfigureAwait(false);
        }

        // 3. Queue Preloading
        // OPTIMIZATION: We call this directly on the current thread.
        // The preloader merely writes to a buffer (Channel) and returns immediately.
        // This eliminates the overhead of scheduling a Task.Run for every keystroke.
        _cache.Preload(_tab.Id, index, IsReversed, Files);
    }
    
    public async ValueTask NavigateByIncrementsAsync(SkipAmount skipAmount, bool forwards, CancellationTokenSource ct)
    {
        var iteration = GetIteration(CurrentIndex, forwards ? NavigateTo.Next : NavigateTo.Previous, _tab.Id, skipAmount);
        await IterateToIndexAsync(iteration, ct).ConfigureAwait(false);
    }

    public void SetCurrentIndex(int index)
    {
        CurrentIndex = index;
    }

    public int GetIteration(int index, NavigateTo navigation, string tabId, SkipAmount skipAmount)
    {
        int next;
        var skip = skipAmount switch
        {
            SkipAmount.One => 1,
            SkipAmount.Two => 2,
            SkipAmount.Ten => 10,
            SkipAmount.Hundred => 100,
            _ => throw new ArgumentOutOfRangeException(nameof(skipAmount), skipAmount, null)
        };
        if (skip is 100)
        {
            if (Files.Count > PreLoaderConfig.MaxCount)
            {
                _cache.Clear(tabId);
            }
        }

        switch (navigation)
        {
            case NavigateTo.Next:
            case NavigateTo.Previous:
                var indexChange = navigation == NavigateTo.Next ? skip : -skip;
                IsReversed = navigation == NavigateTo.Previous;

                if (Settings.UIProperties.Looping)
                {
                    // Calculate new index with looping
                    next = (index + indexChange + Files.Count) % Files.Count;
                }
                else
                {
                    // Calculate new index without looping and ensure bounds
                    var newIndex = index + indexChange;
                    if (newIndex < 0)
                    {
                        return 0;
                    }

                    if (newIndex >= Files.Count)
                    {
                        return Files.Count - 1;
                    }

                    next = newIndex;
                }

                break;

            case NavigateTo.First:
            case NavigateTo.Last:
                if (Files.Count > PreLoaderConfig.MaxCount)
                {
                    _cache.Clear(tabId);
                }

                next = navigation == NavigateTo.First ? 0 : Files.Count - 1;
                break;

            default:
#if DEBUG
                DebugHelper.LogDebug(nameof(ImageIterator), nameof(GetIteration),
                    $"{navigation} is not a valid NavigateTo value.");
#endif
                return -1;
        }

        return next;
    }

    public async ValueTask DisposeAsync()
    {
        await _cache.RemoveOwner(_tab.Id);
        GC.SuppressFinalize(this);
    }

    #region Private Helpers

    private async ValueTask<bool> TryLoadFromCacheAsync(int index, FileInfo file, CancellationTokenSource ct)
    {
        // Check cache first
        if (!_cache.TryGet(file, out var preLoadValue) || preLoadValue is null)
        {
            return false;
        }

        // If it's loading but image isn't ready, show thumbnail first
        if (preLoadValue.IsLoading && preLoadValue.ImageModel.Image is null)
        {
            var thumb = await _thumbnailLoader.GetThumbnailAsync(file).ConfigureAwait(false);

            // Check for cancellation before UI update
            if (ct.IsCancellationRequested || CurrentIndex != index)
            {
                DebugHelper.LogDebug(nameof(ImageIterator), nameof(TryLoadFromCacheAsync), "Cancelled");
                return true;
            }

            _tab.Model.Value = new ImageModel { Image = thumb, FileInfo = file };
            
            DebugHelper.LogDebug(nameof(ImageIterator), nameof(TryLoadFromCacheAsync), "Showing thumbnail");

            // We showed the thumb, but we still need the full load
            return false; // Return false to trigger LoadManuallyAsync for the full image
        }

        // Cache hit is good
        _tab.Model.Value = preLoadValue.ImageModel;
        return true;
    }

    private async ValueTask LoadManuallyAsync(int index, CancellationTokenSource ct)
    {
        var imageModel = await _cache.LoadAsync(_tab.Id, index, Files, ct.Token).ConfigureAwait(false);

        if (!ct.IsCancellationRequested && CurrentIndex == index && imageModel is not null)
        {
            _tab.Model.Value = imageModel;
        }
    }

    #endregion

    #region Not yet made Interface Implementations

    public ValueTask RepeatNavigateAsync(NavigateTo to, TimeSpan repeatInterval, CancellationToken ct)
    {
        return ValueTask.FromException(new NotImplementedException());
    }

    public ValueTask SlimUpdate(int index, object? imageSource)
    {
        return ValueTask.FromException(new NotImplementedException());
    }

    public ValueTask IterateToIndexSlim(int index, bool isReverse, CancellationToken token)
    {
        return ValueTask.FromException(new NotImplementedException());
    }

    public ValueTask ReloadFileListAsync(CancellationToken ct)
    {
        return ValueTask.FromException(new NotImplementedException());
    }

    #endregion
}