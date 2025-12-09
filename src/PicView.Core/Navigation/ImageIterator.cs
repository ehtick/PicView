using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class ImageIterator(IImageCache cache, IThumbnailLoader thumbnailLoader, TabViewModel tab) : IImageIterator
{
    private readonly IImageCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IThumbnailLoader _thumbnailLoader = thumbnailLoader ?? throw new ArgumentNullException(nameof(thumbnailLoader));

    private readonly TabViewModel _tab = tab ?? throw new ArgumentNullException(nameof(tab));

    // Configuration for preloading window
    private readonly int _positiveIterations = PreLoaderConfig.PositiveIterations;
    private readonly int _negativeIterations = PreLoaderConfig.NegativeIterations;
    
    private List<FileInfo> _files = [];

    public IReadOnlyList<FileInfo> Files => _files;
    public int CurrentIndex { get; private set; } = -1;

    public async ValueTask IterateToIndexAsync(int index, CancellationToken ct)
    {
        if (index < 0 || index >= _files.Count)
        {
            return;
        }

        // Get the current image to ensure it's loaded (User is waiting for this)
        var currentFile = _files[index];
        var preLoadValue = _cache.GetOrScheduleLoad(currentFile, ct);
        
        if (preLoadValue.IsLoading && preLoadValue.ImageModel.Image is null)
        {
            // Try to load thumbnail if full image is not ready
            var thumb = await _thumbnailLoader.GetThumbnailAsync(currentFile).ConfigureAwait(false);
            
            // Check if full image loaded while we were getting the thumbnail
            if (preLoadValue.IsLoading && preLoadValue.ImageModel.Image is null && thumb is not null)
            {
                 // Show thumbnail temporarily
                 var tempModel = new Models.ImageModel
                 {
                     FileInfo = currentFile,
                     Image = thumb,
                 };
                 _tab.CurrentModel.Value = tempModel;
            }
            
            // Wait for full load
            await preLoadValue.WaitForLoadingCompleteAsync().WaitAsync(ct).ConfigureAwait(false);
        }

        // Now update with the final loaded image
        _tab.CurrentModel.Value = preLoadValue.ImageModel;
        
        CurrentIndex = index;

        // Update background priorities
        UpdateCachePriorities();
    }

    // UI-Agnostic "GetIteration" logic
    public int GetIteration(int index, NavigateTo navigation, bool skip1 = false, bool skip10 = false,
        bool skip100 = false)
    {
        if (_files.Count == 0)
        {
            return -1;
        }

        var skipAmount = skip100 ? 100 : skip10 ? 10 : skip1 ? 2 : 1;
        int next;

        switch (navigation)
        {
            case NavigateTo.Next:
            case NavigateTo.Previous:
                var change = navigation == NavigateTo.Next ? skipAmount : -skipAmount;
                // Assuming looping is handled by caller or config?
                // The original code checked Settings.UIProperties.Looping.
                // We should inject this or assume true/false.
                // For Core, let's implement standard looping logic or make it configurable.
                // Let's assume looping for now as it's common.
                next = (index + change) % _files.Count;
                if (next < 0)
                {
                    next += _files.Count;
                }

                break;

            case NavigateTo.First:
                next = 0;
                break;
            case NavigateTo.Last:
                next = _files.Count - 1;
                break;
            default:
                return index;
        }

        return next;
    }

    public ValueTask DisposeAsync()
    {
        _cache.RemoveOwner(_tab);
        return ValueTask.CompletedTask;
    }

    // Implementing interface stubs
    public ValueTask RepeatNavigateAsync(NavigateTo to, TimeSpan repeatInterval, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public ValueTask SlimUpdate(int index, object? imageSource)
    {
        throw new NotImplementedException();
    }

    public ValueTask IterateToIndexSlim(int index, bool isReverse, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ValueTask PreloadAsync()
    {
        UpdateCachePriorities();
        return ValueTask.CompletedTask;
    }

    public ValueTask ReloadFileListAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public void Initialize(List<FileInfo> files, int initialIndex = 0)
    {
        _files = files ?? [];
        CurrentIndex = initialIndex;

        if (CurrentIndex < 0 && _files.Count > 0)
        {
            CurrentIndex = 0;
        }

        if (CurrentIndex >= _files.Count)
        {
            CurrentIndex = _files.Count - 1;
        }

        UpdateCachePriorities();
    }

    private void UpdateCachePriorities()
    {
        if (_files.Count == 0)
        {
            return;
        }

        // Calculate window around current index
        // Priority list: [Current, Next, Prev, Next+1, Prev+1, ...]

        var priorities = new List<string> { _files[CurrentIndex].FullName };

        for (var i = 1; i <= Math.Max(_positiveIterations, _negativeIterations); i++)
        {
            if (i <= _positiveIterations)
            {
                var nextIndex = (CurrentIndex + i) % _files.Count;
                priorities.Add(_files[nextIndex].FullName);
            }

            if (i > _negativeIterations)
            {
                continue;
            }

            var prevIndex = (CurrentIndex - i + _files.Count) % _files.Count;
            priorities.Add(_files[prevIndex].FullName);
        }

        _cache.UpdatePriorities(_tab, priorities);
    }
}