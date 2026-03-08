using PicView.Core.DebugTools;
using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class ImageIterator(IImageCache cache, IThumbnailCache thumbCache, IThumbnailLoader thumbnailLoader, TabViewModel tab) : IImageIterator
{
    private readonly IImageCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IThumbnailCache _thumbCache = thumbCache ?? throw new ArgumentNullException(nameof(thumbCache));
    private readonly TabViewModel _tab = tab ?? throw new ArgumentNullException(nameof(tab));

    private readonly IThumbnailLoader _thumbnailLoader =
        thumbnailLoader ?? throw new ArgumentNullException(nameof(thumbnailLoader));

    private System.Timers.Timer? _timer;

    public IReadOnlyList<FileInfo> Files { get; set; } = [];

    public int CurrentIndex { get; private set; } = -1;
    public bool IsReversed { get; private set; }

    public void Initialize(IReadOnlyList<FileInfo> files, int initialIndex = 0)
    {
        Files = files ?? [];
        CurrentIndex = initialIndex;
        UpdateNavigationProperties();
    }

    public void UpdateNavigationProperties()
        => UpdateNavigationProperties(CurrentIndex, Files.Count);

    private void UpdateNavigationProperties(int index, int count)
    {
        if (count <= 1)
        {
            _tab.CanNavigateForwards.Value = false;
            _tab.CanNavigateBackwards.Value = false;
        }
        else
        {
            var isLooping = Settings.UIProperties.Looping;
            _tab.CanNavigateForwards.Value = isLooping || index < count - 1;
            _tab.CanNavigateBackwards.Value = isLooping || index > 0;
        }
        _tab.NavigationIndex.Value = index;
        _tab.MaxIndex.Value = count;
    }

    /// <summary>
    /// Initiates the <see cref="IterateToIndexAsync"/> in a timer delay, intended for repeated navigation based on the specified direction and interval.
    /// The navigation should continue until key is released, by calling the <see cref="StopRepeatedNavigation"/> or the cancellation token is triggered.
    /// </summary>
    /// <param name="to">The target direction or position for the navigation (e.g., Next, Previous, First, Last).</param>
    /// <param name="repeatInterval">The time interval between each navigation cycle.</param>
    /// <param name="ct">The cancellation token used to stop the navigation process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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

        var iteration = GetIteration(CurrentIndex, to, SkipAmount.One);
        await IterateToIndexAsync(iteration, CancellationTokenSource.CreateLinkedTokenSource(ct));
    }

    public void StopRepeatedNavigation()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    /// <summary>
    /// Navigates to the specified index within the collection of files and manages image caching, TIFF navigation, and updates the model.
    /// Ensures that navigation actions like loading, preloading, or skipping are handled depending on the cache state and cancellation state.
    /// </summary>
    /// <param name="index">The target index of the file to navigate to in the collection.</param>
    /// <param name="ct">The cancellation token source used to cancel the navigation process if needed.</param>
    /// <returns>A task representing the asynchronous operation of navigating to the specified index.</returns>
    public async ValueTask IterateToIndexAsync(int index, CancellationTokenSource ct)
    {
        if (index < 0 || index >= Files.Count)
        {
            return;
        }

        // Handle internal TIFF navigation
        var currentModel = _tab.Model;

        if (tab.Model?.TiffNavigation is not null)
        {
            var isTiffNavigated = ShouldNavigateTiffEntry(currentModel, IsReversed);
            if (isTiffNavigated)
            {
                return;
            }
        }

        CurrentIndex = index;
        var targetFile = Files[CurrentIndex];

        var (status, model) = TryLoadFromCache(CurrentIndex, targetFile, ct);
        switch (status)
        {
            case CacheStatus.Cancelled:
                Preload();
                break;
            case CacheStatus.IsInCache:
                if (model is null)
                {
                    goto case CacheStatus.NotInCache;
                }
                await Update(model);
                break;
            case CacheStatus.IsLoadingInCache:
                var successFullyLoaded = await _cache.WaitForLoadingCompleteAsync(_tab.Id, index).ConfigureAwait(false);
                if (!successFullyLoaded)
                {
                    goto case CacheStatus.NotInCache;
                }
                if (index != CurrentIndex || model is null)
                {
                    // User skipped
                    return;
                }
                await Update(model);
                break;
            case CacheStatus.NotInCache:
                var manuallyLoaded = await LoadManuallyAsync(CurrentIndex, ct).ConfigureAwait(false);
                if (index != CurrentIndex || manuallyLoaded is null)
                {
                    // User skipped
                    return;
                }
                await Update(manuallyLoaded);
                break;

            default: return;
        }
        
        return;

        void Preload()
        {
            _cache.Preload(_tab.Id, CurrentIndex, IsReversed, Files, _tab.GetTabCancellation().Token);
        }

        async ValueTask Update(ImageModel newModel)
        {
            _tab.Model = newModel;
            
            // Load Secondary Image (if Side-by-Side)
            if (Settings.ImageScaling.ShowImageSideBySide)
            {
                var nextIndex = CurrentIndex + 1;
                if (nextIndex < Files.Count)
                {
                    var loadedModel = await LoadManuallyAsync(nextIndex, ct).ConfigureAwait(false);
                    if (loadedModel is null)
                    {
                        Preload();
                        return;
                    }

                    _tab.SecondaryModel = loadedModel;
                }
                else
                {
                    _tab.SecondaryModel = null;
                }
            }
            else
            {
                _tab.SecondaryModel = null;
            }
            
            UpdateNavigationProperties();
            Preload();
        }
    }

    public async ValueTask SkipToIndexAsync(int index, CancellationTokenSource ct)
    {
        if (index < 0 || index >= Files.Count)
        {
            return;
        }

        var file = Files[index];

        if (_cache.TryGet(file, out var preLoadValue) && preLoadValue?.ImageModel?.Image != null)
        {
            await IterateToIndexAsync(index, ct).ConfigureAwait(false);
        }
        else
        {
            _cache.Clear(_tab.Id);
            await IterateToIndexAsync(index, ct).ConfigureAwait(false);
        }
    }

    public async ValueTask NavigateByIncrementsAsync(SkipAmount skipAmount, bool forwards, CancellationTokenSource ct)
    {
        var iteration = GetIteration(CurrentIndex, forwards ? NavigateTo.Next : NavigateTo.Previous, skipAmount);
        await SkipToIndexAsync(iteration, ct).ConfigureAwait(false);
    }

    public void SetCurrentIndex(int index)
    {
        CurrentIndex = index;
        UpdateNavigationProperties();
    }

    public int GetIteration(int index, NavigateTo navigation, SkipAmount skipAmount)
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
    
    private static bool ShouldNavigateTiffEntry(ImageModel model, bool isPrevious)
    {
        if (model.TiffNavigation is null)
        {
            return false;
        }
        if (isPrevious)
        {
            var prev = model.TiffNavigation.CurrentPage - 1;
            if (prev < 0)
            {
                return false;
            }
            model.TiffNavigation.CurrentPage = prev;
        }
        else
        {
            var next = model.TiffNavigation.CurrentPage + 1;
            if (next >= model.TiffNavigation.PageCount)
            {
                return false;
            }
            model.TiffNavigation.CurrentPage = next;
        }

        UpdateImageFromPage(model);
        return true;
    }

    private static void UpdateImageFromPage(ImageModel model)
    {
        if (model.TiffNavigation is { Pages: not null, CurrentPage: >= 0 } && 
            model.TiffNavigation.CurrentPage < model.TiffNavigation.Pages.Length)
        {
            model.Image = model.TiffNavigation.Pages[model.TiffNavigation.CurrentPage];
        }
    }

    private (CacheStatus Status, ImageModel? Model) TryLoadFromCache(int index, FileInfo file,
        CancellationTokenSource ct)
    {
        // Check if the item is in the cache
        if (!_cache.TryGet(file, out var preLoadValue) || preLoadValue is null)
        {
            return LoadThumbnailInternal(index, file, ct, CacheStatus.NotInCache);
        }

        // Show thumb while loading
        if (preLoadValue is { IsLoading: true, ImageModel.Image: null })
        {
            return LoadThumbnailInternal(index, file, ct, CacheStatus.IsLoadingInCache);
        }

        // If we have the full image, show it and return
        return (CacheStatus.IsInCache, preLoadValue.ImageModel);
    }

    /// <summary>
    /// Loads and displays a thumbnail for immediate feedback, then returns the status needed for the next step.
    /// </summary>
    private (CacheStatus Status, ImageModel? Model) LoadThumbnailInternal(int index,
        FileInfo file, CancellationTokenSource ct, CacheStatus statusToReturn)
    {
        if (_thumbCache.TryGet(file.FullName, out var cachedThumb))
        {
            return (statusToReturn, new ImageModel {Image = cachedThumb, FileInfo = file});
        }
        
        var thumb = _thumbnailLoader.GetExifThumbnail(file);

        // Check for cancellation or navigation change before updating UI
        if (ct.IsCancellationRequested || CurrentIndex != index)
        {
            DebugHelper.LogDebug(nameof(ImageIterator), nameof(TryLoadFromCache), "Cancelled");
            return (CacheStatus.Cancelled, null);
        }

        var model = new ImageModel { Image = thumb, FileInfo = file };
        return (statusToReturn, model);
    }

    private async ValueTask<ImageModel?> LoadManuallyAsync(int index, CancellationTokenSource ct) =>
        await _cache.LoadAsync(_tab.Id, index, Files, ct.Token).ConfigureAwait(false);

    #endregion

    #region IDispose

    public void Dispose()
    {
        _timer?.Dispose();
        _cache.RemoveOwner(_tab.Id);
        GC.SuppressFinalize(this);
    }

    #endregion
}
