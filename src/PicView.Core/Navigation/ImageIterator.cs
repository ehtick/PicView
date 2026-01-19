using PicView.Core.DebugTools;
using PicView.Core.ImageDecoding;
using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Navigation.Tiff;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Core.Navigation;

public class ImageIterator(IImageCache cache, IThumbnailLoader thumbnailLoader, TabViewModel tab) : IImageIterator
{
    private readonly IImageCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly TabViewModel _tab = tab ?? throw new ArgumentNullException(nameof(tab));

    private readonly IThumbnailLoader _thumbnailLoader =
        thumbnailLoader ?? throw new ArgumentNullException(nameof(thumbnailLoader));

    private System.Timers.Timer? _timer;

    public IReadOnlyList<FileInfo> Files { get; set; } = [];
    private CompositeDisposable? _disposable;

    public int CurrentIndex { get; private set; } = -1;
    public bool IsReversed { get; private set; }

    public void Initialize(IReadOnlyList<FileInfo> files, int initialIndex = 0)
    {
        Files = files ?? [];
        CurrentIndex = Math.Clamp(initialIndex, 0, Math.Max(0, Files.Count - 1));

        _disposable = new CompositeDisposable();
        // Update UI bound values
        Observable.EveryValueChanged(this, i => i.CurrentIndex)
            .Subscribe(i =>
            {
                _tab.NavigationIndex.Value = i;
                UpdateNavigationProperties(i, Files.Count);
            })
            .AddTo(_disposable);
        Observable.EveryValueChanged(Files, i => i.Count)
            .Subscribe(i =>
            {
                _tab.MaxIndex.Value = i;
                UpdateNavigationProperties(CurrentIndex, i);
            })
            .AddTo(_disposable);
    }

    public void UpdateNavigationProperties()
        => UpdateNavigationProperties(CurrentIndex, Files.Count);

    public void UpdateNavigationProperties(int index, int count)
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

        // Handle internal TIFF navigation
        var currentModel = _tab.Model;

        if (tab.Model?.TiffNavigation is { } tiff)
        {
            var isTiffNavigated = ShouldNavigateTiffEntry(currentModel, IsReversed);
            if (isTiffNavigated)
            {
                return;
            }
        }

        CurrentIndex = index;
        var targetFile = Files[index];

        var (status, model) = await TryLoadFromCacheAsync(index, targetFile, ct).ConfigureAwait(false);
        switch (status)
        {
            case CacheStatus.Cancelled:
                Cancel();
                break;
            case CacheStatus.IsInCache when model is not null:
                _tab.Model = model;
                await Update();
                break;
            case CacheStatus.NotInCache:
            case CacheStatus.IsLoadingInCache:
                var loadedModel = await LoadManuallyAsync(index, ct).ConfigureAwait(false);
                if (loadedModel is null)
                {
                    Cancel();
                    return;
                }
                _tab.Model = loadedModel;
                await Update();
                break;
            default:
                Cancel();
                break;
        }
        
        return;

        void Cancel()
        {
            _cache.Preload(_tab.Id, index, IsReversed, Files);
        }

        async ValueTask Update()
        {
            // 2. Load Secondary Image (if Side-by-Side)
            if (Settings.ImageScaling.ShowImageSideBySide)
            {
                var nextIndex = index + 1;
                if (nextIndex < Files.Count)
                {
                    var loadedModel = await LoadManuallyAsync(nextIndex, ct).ConfigureAwait(false);
                    if (loadedModel is null)
                    {
                        Cancel();
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
            
            // Update UI bound vales
            _tab.NavigationIndex.Value = CurrentIndex;
            _tab.MaxIndex.Value = Files.Count;

            // Queue Preloading. Call directly on the current thread; preloader writes to a channel immediately.
            _cache.Preload(_tab.Id, index, IsReversed, Files);
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
        UpdateNavigationProperties();
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
        _disposable?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}
