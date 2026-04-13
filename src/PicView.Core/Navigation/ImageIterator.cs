using PicView.Core.DebugTools;
using PicView.Core.FileHistory;
using PicView.Core.ImageDecoding;
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
    public int SecondaryCurrentIndex { get; private set; } = -1;
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
    
    public async ValueTask NavigateAsync(NavigateTo navigateTo, SkipAmount skipAmount, CancellationTokenSource ct)
    {
        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            var (currentIndex, secondaryIndex, isReversed) = IterationHelper.GetIterations(CurrentIndex, Files.Count, navigateTo, skipAmount);
            IsReversed = isReversed;
            await IterateToIndicesAsync(currentIndex, secondaryIndex, ct).ConfigureAwait(false);
        }
        else
        {
            var (iteration, isReversed) = IterationHelper.GetIteration(CurrentIndex, Files.Count, navigateTo, skipAmount);
            IsReversed = isReversed;
            await IterateToIndexAsync(iteration, ct).ConfigureAwait(false);
        }
    }

    public async ValueTask IterateToIndexAsync(int index, CancellationTokenSource ct)
    {
        if (index < 0 || index >= Files.Count)
        {
            return;
        }

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
                UpdateModel(preLoadValue.ImageModel);
            }
            else
            {
                // Is loading in cache, show thumbnail while loading
                var thumb = _thumbnailLoader.GetExifThumbnail(targetFile);
                _tab.Model.Value = new ImageModel
                {
                    Image = thumb,
                    FileInfo = targetFile,
                    ImageType = ImageType.Bitmap
                };

                // Wait for loading complete
                var successfullyLoaded = await Cache.WaitForLoadingCompleteAsync(_tab.Id, index).ConfigureAwait(false);
                if (successfullyLoaded && index == CurrentIndex && preLoadValue.ImageModel.Image is not null)
                {
                    UpdateModel(preLoadValue.ImageModel);
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
                UpdateModel(manuallyLoaded);
            }
            else
            {
                TriggerPreload();
            }
        }
    }
    
    public async ValueTask IterateToIndicesAsync(int index, int secondaryIndex, CancellationTokenSource ct)
    {
        if (index < 0 || index >= Files.Count)
        {
            return;
        }

        // Handle internal TIFF navigation
        if (_tab.Model?.CurrentValue.TiffNavigation is not null && ShouldNavigateTiffEntry(_tab.Model.CurrentValue, IsReversed))
        {
            return;
        }

        CurrentIndex = index;
        SecondaryCurrentIndex = secondaryIndex;
        var targetFile = Files[index];
        var secondaryFile = Files[secondaryIndex];
        ImageModel firstModel, secondModel;
        if (Cache.TryGet(targetFile, out var preLoadValue))
        {
            if (preLoadValue is { IsLoading: false, ImageModel.Image: not null })
            {
                // Is in cache
                firstModel = preLoadValue.ImageModel;
            }
            else
            {
                // Wait for loading complete
                var successfullyLoaded = await Cache.WaitForLoadingCompleteAsync(_tab.Id, index).ConfigureAwait(false);
                if (successfullyLoaded && index == CurrentIndex && preLoadValue.ImageModel.Image is not null)
                {
                    firstModel = preLoadValue.ImageModel;
                }
                else
                {
                    TriggerPreload();
                    return;
                }
            }
        }
        else
        {
            // Not in cache
            var manuallyLoaded = await Cache.LoadAsync(_tab.Id, index, Files, ct.Token).ConfigureAwait(false);
            if (index == CurrentIndex && manuallyLoaded is not null)
            {
                firstModel = manuallyLoaded;
            }
            else
            {
                TriggerPreload();
                return;
            }
        }
        
        if (Cache.TryGet(secondaryFile, out var secondaryPreLoadValue))
        {
            if (secondaryPreLoadValue is { IsLoading: false, ImageModel.Image: not null })
            {
                // Is in cache
                secondModel = secondaryPreLoadValue.ImageModel;
            }
            else
            {
                // Wait for loading complete
                var successfullyLoaded = await Cache.WaitForLoadingCompleteAsync(_tab.Id, secondaryIndex).ConfigureAwait(false);
                if (successfullyLoaded && secondaryIndex == CurrentIndex && secondaryPreLoadValue.ImageModel.Image is not null)
                {
                    secondModel = secondaryPreLoadValue.ImageModel;
                }
                else
                {
                    TriggerPreload();
                    return;
                }
            }
        }
        else
        {
            // Not in cache
            var manuallyLoaded = await Cache.LoadAsync(_tab.Id, secondaryIndex, Files, ct.Token).ConfigureAwait(false);
            if (secondaryIndex == CurrentIndex && manuallyLoaded is not null)
            {
                secondModel = manuallyLoaded;
            }
            else
            {
                TriggerPreload();
                return;
            }
        }

        _tab.SecondaryModel.Value = secondModel;
        _tab.Model.Value = firstModel;
        UpdateNavigationProperties();
        TriggerPreload();
        
        FileHistoryManager.Add(firstModel.FileInfo.FullName);
        FileHistoryManager.Add(secondModel.FileInfo.FullName);
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
        var (iteration, isReversed) = IterationHelper.GetIteration(CurrentIndex, Files.Count, forwards ? NavigateTo.Next : NavigateTo.Previous, skipAmount);
        IsReversed = isReversed;
        await SkipToIndexAsync(iteration, ct).ConfigureAwait(false);
    }

    public void SetCurrentIndex(int index)
    {
        CurrentIndex = index;
        UpdateNavigationProperties();
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

        var (iteration, isReversed) = IterationHelper.GetIteration(CurrentIndex, Files.Count, to, SkipAmount.One);
        IsReversed = isReversed;
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

    private void UpdateModel(ImageModel newModel)
    {
        _tab.Model.Value = newModel;
        UpdateNavigationProperties();
        TriggerPreload();
        
        // Update the file history
        FileHistoryManager.Add(newModel.FileInfo.FullName);
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