using PicView.Core.DebugTools;
using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class ImageIterator : IImageIterator
{
    private readonly IImageCache _cache;
    private readonly IThumbnailLoader _thumbnailLoader;

    private readonly TabViewModel _tab;
    
    private List<FileInfo> _files = [];
    public bool IsReversed { get; private set; }

    public IReadOnlyList<FileInfo> Files => _files;
    public int CurrentIndex { get; private set; } = -1;

    public ImageIterator(IImageCache cache, IThumbnailLoader thumbnailLoader, TabViewModel tab)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _thumbnailLoader = thumbnailLoader ?? throw new ArgumentNullException(nameof(thumbnailLoader));
        _tab = tab ?? throw new ArgumentNullException(nameof(tab));
    }

    public async ValueTask IterateToIndexAsync(int index, CancellationTokenSource ct)
    {
        if (index < 0 || index >= _files.Count)
        {
            return;
        }
        
        CurrentIndex = index;
        var nextFile = _files[index];
        if (_cache.TryGet(_tab.Id, CurrentIndex, out var preLoadValue))
        {
            if (preLoadValue is not null)
            {
                if (preLoadValue.IsLoading && preLoadValue.ImageModel.Image is null)
                {
                    var thumb = await _thumbnailLoader.GetThumbnailAsync(nextFile).ConfigureAwait(false);
                    _tab.CurrentModel.Value = new ImageModel
                    {
                        Image = thumb,
                        FileInfo = nextFile
                    };
                    if (CurrentIndex != index)
                    {
                        await ct.CancelAsync();
                        return;
                    }
                    await LoadManually();
                }
                else
                {
                    _tab.CurrentModel.Value = preLoadValue.ImageModel;
                }
            }
        }
        else
        {
            await LoadManually();
        }

        // Need to explicitly start preloading in a new thread and not wait for it, for performance
        _ = Task.Run(() =>
        {
            _cache.PreloadAsync(_tab.Id, CurrentIndex, IsReversed, _files, ct.Token);
        });
        
        return;

        async Task LoadManually()
        {
            var imageModel = await _cache.LoadAsync(_tab.Id, index, _files, ct.Token).ConfigureAwait(false);
            if (CurrentIndex != index)
            {
                await ct.CancelAsync();
                return;
            }
            if (imageModel is not null)
            {
                _tab.CurrentModel.Value = imageModel;
            }
        }
    }

    public int GetIteration(int index, NavigateTo navigation, bool skip1 = false, bool skip10 = false,
        bool skip100 = false)
    {
        int next;

        // Determine skipAmount based on input flags
        var skipAmount = skip100 ? 100 : skip10 ? 10 : skip1 ? 2 : 1;
        
        if (skip100)
        {
            if (_files.Count > PreLoaderConfig.MaxCount)
            {
                //PreLoader.Clear();
            }
        }

        switch (navigation)
        {
            case NavigateTo.Next:
            case NavigateTo.Previous:
                var indexChange = navigation == NavigateTo.Next ? skipAmount : -skipAmount;
                IsReversed = navigation == NavigateTo.Previous;

                if (Settings.UIProperties.Looping)
                {
                    // Calculate new index with looping
                    next = (index + indexChange + _files.Count) % _files.Count;
                }
                else
                {
                    // Calculate new index without looping and ensure bounds
                    var newIndex = index + indexChange;
                    if (newIndex < 0)
                    {
                        return 0;
                    }

                    if (newIndex >= _files.Count)
                    {
                        return _files.Count - 1;
                    }

                    next = newIndex;
                }

                break;

            case NavigateTo.First:
            case NavigateTo.Last:
                if (_files.Count > PreLoaderConfig.MaxCount)
                {
                    //PreLoader.Clear();
                }

                next = navigation == NavigateTo.First ? 0 : _files.Count - 1;
                break;

            default:
#if DEBUG
                DebugHelper.LogDebug(nameof(ImageIterator), nameof(GetIteration), $"{navigation} is not a valid NavigateTo value.");
#endif
                return -1;
        }

        return next;
    }

    public async ValueTask DisposeAsync()
    {
        _cache.RemoveOwner(_tab.Id);
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
    }
}
