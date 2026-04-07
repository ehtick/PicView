using PicView.Core.DebugTools;
using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class ImageIterator(IImageCache cache, IThumbnailCache thumbCache, IThumbnailLoader thumbnailLoader, TabViewModel tab) : IImageIterator
{
    #region Dependencies & Properties

    public IImageCache Cache { get; } = cache ?? throw new ArgumentNullException(nameof(cache));
    public string? CurrentDirectory => Files.Count > 0 ? Files[0].DirectoryName : null;

    private readonly IThumbnailCache _thumbCache = thumbCache ?? throw new ArgumentNullException(nameof(thumbCache));
    private readonly TabViewModel _tab = tab ?? throw new ArgumentNullException(nameof(tab));
    private readonly IThumbnailLoader _thumbnailLoader = thumbnailLoader ?? throw new ArgumentNullException(nameof(thumbnailLoader));
    private System.Timers.Timer? _timer;

    public IReadOnlyList<FileInfo> Files { get; set; } = [];
    public int CurrentIndex { get; private set; } = -1;
    public bool IsReversed { get; private set; }

    #endregion

    #region Initialization & Property Updates

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
            if (Settings.ImageScaling.ShowImageSideBySide)
            {
                _tab.CanNavigateForwards.Value = isLooping || index < count - 2;
                _tab.CanNavigateBackwards.Value = isLooping || index > 0;
            }
            else
            {
                _tab.CanNavigateForwards.Value = isLooping || index < count - 1;
                _tab.CanNavigateBackwards.Value = isLooping || index > 0;
            }
        }
        _tab.NavigationIndex.Value = index;
        _tab.MaxIndex.Value = count;
    }

    #endregion

    #region Core Navigation Logic

    public async ValueTask IterateToIndexAsync(int index, CancellationTokenSource ct)
    {
        if (index < 0 || index >= Files.Count) return;

        // Handle internal TIFF navigation
        if (_tab.Model?.CurrentValue.TiffNavigation is not null && ShouldNavigateTiffEntry(_tab.Model.CurrentValue, IsReversed))
        {
            return;
        }

        CurrentIndex = index;
        var targetFile = Files[index];
        if (Cache.TryGet(targetFile, out var preLoadValue))
        {
            if (preLoadValue is { IsLoading: false, ImageModel.Image: not null })
            {
                // Is in cache
                await NavigateNextModelAsync(preLoadValue.ImageModel);
            }
            else
            {
                // Is loading in cache, show thumbnail while loading
                var thumb = _thumbnailLoader.GetExifThumbnail(targetFile);

                // Wait for loading complete
                var successfullyLoaded = await Cache.WaitForLoadingCompleteAsync(_tab.Id, index).ConfigureAwait(false);
                if (successfullyLoaded && index == CurrentIndex && preLoadValue.ImageModel.Image is not null)
                {
                    await NavigateNextModelAsync(preLoadValue.ImageModel);
                }
                else
                {
                    TriggerPreload();
                }
            }
        }
        else
        {
            // Not in cache
            var manuallyLoaded = await Cache.LoadAsync(_tab.Id, index, Files, ct.Token).ConfigureAwait(false);
            if (index == CurrentIndex && manuallyLoaded is not null)
            {
                await NavigateNextModelAsync(manuallyLoaded);
            }
            else
            {
                TriggerPreload();
            }
        }

        return;

        async ValueTask NavigateNextModelAsync(ImageModel model)
        {
            await UpdateModelAsync(model, ct).ConfigureAwait(false);

            // Update the file history
            _tab.FileHistorySubject.OnNext(targetFile.FullName);
        }
    }

    public async ValueTask SkipToIndexAsync(int index, CancellationTokenSource ct)
    {
        if (index < 0 || index >= Files.Count) return;

        var file = Files[index];

        if (!Cache.TryGet(file, out var preLoadValue) || preLoadValue?.ImageModel?.Image == null)
        {
            Cache.Clear(_tab.Id);
        }

        await IterateToIndexAsync(index, ct).ConfigureAwait(false);
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
        var skip = skipAmount switch
        {
            SkipAmount.One => 1,
            SkipAmount.Two => 2,
            SkipAmount.Ten => 10,
            SkipAmount.Hundred => 100,
            _ => throw new ArgumentOutOfRangeException(nameof(skipAmount), skipAmount, null)
        };

        if (Settings.ImageScaling.ShowImageSideBySide && skipAmount == SkipAmount.One)
        {
            skip = 2;
        }

        switch (navigation)
        {
            case NavigateTo.Next:
            case NavigateTo.Previous:
                var indexChange = navigation == NavigateTo.Next ? skip : -skip;
                IsReversed = navigation == NavigateTo.Previous;

                if (Settings.UIProperties.Looping)
                {
                    var loopedIndex = (index + indexChange) % Files.Count;
                    if (loopedIndex < 0)
                    {
                        loopedIndex += Files.Count;
                    }
                    return loopedIndex;
                }

                var newIndex = index + indexChange;

                if (Settings.ImageScaling.ShowImageSideBySide && skipAmount == SkipAmount.One)
                {
                    // Special non-looping clamping logic for side-by-side mode.
                    if (navigation == NavigateTo.Next && newIndex >= Files.Count - 1 && Files.Count > 1)
                    {
                        // Ensure we don't go out of bounds but still show the very last item on the right.
                        newIndex = Files.Count - 2;
                    }
                    else if (navigation == NavigateTo.Previous && newIndex < 0)
                    {
                        // If going backwards skips past the beginning, clamp to 0 
                        // so we cleanly show the first two images.
                        newIndex = 0;
                    }
                }

                return Math.Clamp(newIndex, 0, Files.Count - 1);

            case NavigateTo.First:
                return 0;

            case NavigateTo.Last:
                return Settings.ImageScaling.ShowImageSideBySide && Files.Count > 1 ? Files.Count - 2 : Files.Count - 1;

            default:
#if DEBUG
                DebugHelper.LogDebug(nameof(ImageIterator), nameof(GetIteration), $"{navigation} is not a valid NavigateTo value.");
#endif
                return -1;
        }
    }

    #endregion

    #region Repeated Navigation (Timer)

    public async ValueTask RepeatNavigateAsync(NavigateTo to, TimeSpan repeatInterval, CancellationToken ct)
    {
        if (_timer is null)
        {
            _timer = new System.Timers.Timer { AutoReset = false, Enabled = true };
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

    #endregion

    #region Update model & Loading

    private async ValueTask UpdateModelAsync(ImageModel newModel, CancellationTokenSource ct)
    {
        _tab.Model.Value = newModel;

        // Load Secondary Image (if Side-by-Side is enabled)
        var hasNextImage = CurrentIndex + 1 < Files.Count;
        var loopToStart = !hasNextImage && Settings.UIProperties.Looping && Files.Count > 1;

        if (Settings.ImageScaling.ShowImageSideBySide && (hasNextImage || loopToStart))
        {
            var nextIndex = hasNextImage ? CurrentIndex + 1 : 0;
            var loadedModel = await Cache.LoadAsync(_tab.Id, nextIndex, Files, ct.Token).ConfigureAwait(false);

            if (loadedModel is null)
            {
                TriggerPreload();
                return;
            }

            _tab.SecondaryModel.Value = loadedModel;
        }
        else
        {
            _tab.SecondaryModel.Value = null;
        }

        UpdateNavigationProperties();
        TriggerPreload();
    }

    private void TriggerPreload()
    {
        Cache.Preload(_tab.Id, CurrentIndex, IsReversed, Files, _tab.GetTabCancellation().Token);
    }

    #endregion

    #region TIFF Handling & Helpers

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

    #endregion

    #region IDisposable

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}