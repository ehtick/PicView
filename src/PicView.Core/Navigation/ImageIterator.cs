using PicView.Core.DebugTools;
using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Core.Navigation;

public class ImageIterator(IImageCache cache, IThumbnailLoader thumbnailLoader, TabViewModel tab) : IImageIterator
{
    private readonly IImageCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly TabViewModel _tab = tab ?? throw new ArgumentNullException(nameof(tab));
    private readonly IThumbnailLoader _thumbnailLoader = thumbnailLoader ?? throw new ArgumentNullException(nameof(thumbnailLoader));
    private System.Timers.Timer? _timer;

    public IReadOnlyList<FileInfo> Files { get; set; } = [];

    public int CurrentIndex { get; private set; } = -1;
    public bool IsReversed { get; private set; }

    public void Initialize(IReadOnlyList<FileInfo> files, int initialIndex = 0)
    {
        Files = files ?? [];
        CurrentIndex = Math.Clamp(initialIndex, 0, Math.Max(0, Files.Count - 1));
    }
    
    public async ValueTask RepeatNavigateAsync(NavigateTo to, TimeSpan repeatInterval, CancellationToken ct)
    {
        if (_timer is null)
        {
            _timer = new System.Timers.Timer
            {
                AutoReset = false,
                Enabled = true
            };
        }
        else if (_timer.Enabled)
        {
            return;
        }

        _timer.Interval = repeatInterval.TotalMilliseconds;
        _timer.Start();

        var iteration = GetIteration(CurrentIndex, to, _tab.Id, SkipAmount.One);
        await IterateToIndexAsync(iteration, CancellationTokenSource.CreateLinkedTokenSource(ct));
    }
    
    public void StopRepeatedNavigation()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    public async ValueTask IterateToIndexAsync(int index, CancellationTokenSource ct)
    {
        if (index < 0 || index >= Files.Count)
        {
            return;
        }

        CurrentIndex = index;
        var targetFile = Files[index];

        // 1. Load Primary Image
        await LoadImageToModel(index, targetFile, _tab.Model, ct).ConfigureAwait(false);

        // 2. Load Secondary Image (if Side-by-Side)
        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            var nextIndex = index + 1;
            if (nextIndex < Files.Count)
            {
                 var secondaryFile = Files[nextIndex];
                 await LoadImageToModel(nextIndex, secondaryFile, _tab.SecondaryModel, ct).ConfigureAwait(false);
            }
            else
            {
                _tab.SecondaryModel.Value = null;
            }
        }
        else
        {
            _tab.SecondaryModel.Value = null;
        }

        // Queue Preloading. Call directly on the current thread; preloader writes to a channel immediately.
        _cache.Preload(_tab.Id, index, IsReversed, Files);
    }

    private async ValueTask LoadImageToModel<T>(int index, FileInfo file, BindableReactiveProperty<T> property, CancellationTokenSource ct) where T : class?
    {
        var (status, model) = await TryLoadFromCacheAsync(index, file, ct).ConfigureAwait(false);
        
        if (property.Value != null && model == null && status == CacheStatus.Cancelled)
        {
             return;
        }

        switch (status)
        {
            case CacheStatus.NotInCache:
            case CacheStatus.IsLoadingInCache:
                if (model is T thumbnailModel) 
                {
                     property.Value = thumbnailModel; // Set thumbnail
                }
                var loadedModel = await LoadManuallyAsync(index, ct).ConfigureAwait(false);
                if (loadedModel is T fullModel)
                {
                    property.Value = fullModel;
                }
                break;
            case CacheStatus.IsInCache:
                if (model is T cachedModel)
                {
                    property.Value = cachedModel;
                }
                break;
            case CacheStatus.Cancelled:
            default:
                return;
        }
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
    
    #region Private Helpers

    private async ValueTask<(CacheStatus Status, ImageModel? Model)> TryLoadFromCacheAsync(int index, FileInfo file,
        CancellationTokenSource ct)
    {
        // Check if the item is in the cache
        if (!_cache.TryGet(file, out var preLoadValue) || preLoadValue is null)
        {
            return await LoadThumbnailInternalAsync(index, file, ct, CacheStatus.NotInCache);
        }

        // If we have the full image, show it and return
        if (preLoadValue.IsLoading && preLoadValue.ImageModel.Image is null)
        {
            return await LoadThumbnailInternalAsync(index, file, ct, CacheStatus.IsLoadingInCache);
        }

        return (CacheStatus.IsInCache, preLoadValue.ImageModel);
    }

    /// <summary>
    /// Loads and displays a thumbnail for immediate feedback, then returns the status needed for the next step.
    /// </summary>
    private async ValueTask<(CacheStatus Status, ImageModel? Model)> LoadThumbnailInternalAsync(int index,
        FileInfo file, CancellationTokenSource ct, CacheStatus statusToReturn)
    {
        var thumb = await _thumbnailLoader.GetThumbnailAsync(file).ConfigureAwait(false);

        // Check for cancellation or navigation change before updating UI
        if (ct.IsCancellationRequested || CurrentIndex != index)
        {
            DebugHelper.LogDebug(nameof(ImageIterator), nameof(TryLoadFromCacheAsync), "Cancelled");
            return (CacheStatus.Cancelled, null);
        }

        var model = new ImageModel { Image = thumb, FileInfo = file };

        if (statusToReturn == CacheStatus.IsLoadingInCache)
        {
            DebugHelper.LogDebug(nameof(ImageIterator), nameof(TryLoadFromCacheAsync),
                "Showing thumbnail (waiting for load)");
        }

        return (statusToReturn, model);
    }

    private async ValueTask<ImageModel?> LoadManuallyAsync(int index, CancellationTokenSource ct)
    {
        var imageModel = await _cache.LoadAsync(_tab.Id, index, Files, ct.Token).ConfigureAwait(false);

        if (!ct.IsCancellationRequested && CurrentIndex == index && imageModel is not null)
        {
            return imageModel;
        }

        return null;
    }

    #endregion

    #region IDispose

    public async ValueTask DisposeAsync()
    {
        _timer?.Dispose();
        await _cache.RemoveOwner(_tab.Id);
        GC.SuppressFinalize(this);
    }

    #endregion
}