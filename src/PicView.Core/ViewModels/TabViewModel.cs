using PicView.Core.Extensions;
using PicView.Core.Gallery;
using PicView.Core.Localization;
using PicView.Core.Models;
using PicView.Core.Navigation;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.Titles;
using R3;

namespace PicView.Core.ViewModels;

/// <summary>
/// Represents the state, data, and context for a single navigation tab.
/// <para>
/// This class acts as the holder for the tab's specific <see cref="ImageIterator"/>, the current 
/// <see cref="ImageModel"/>, and the visual properties (Title, Tooltip). It manages the 
/// lifecycle of resources specific to this tab instance.
/// </para>
/// </summary>
public class TabViewModel(string id, Func<string, ValueTask> closeTab, IFileWatcherService? fileWatcherService = null) : IAsyncDisposable
{
    /// The MainViewModel that currently "owns" this tab
    public object? ParentWindowContext { get; set; }
    private CompositeDisposable? Disposables { get; set; }

    public HoverbarViewModel Hoverbar { get; } = new();
    
    public GalleryViewModel Gallery { get; } = new();

    /// Unique identifier for this tab.
    public string Id { get; } = id;
    public bool IsClosing { get; private set; }
    public bool IsSelected { get; set; }
    public BindableReactiveProperty<object?> Image { get; } = new();
    public BindableReactiveProperty<object?> ImageType { get; } = new();
    public BindableReactiveProperty<object?> FileInfo { get; } = new();
    public BindableReactiveProperty<object?> SecondaryImage { get; } = new();
    public BindableReactiveProperty<object?> SecondaryFileInfo { get; } = new();
    public ImageModel Model { get; set; } = new();
    public ImageModel? SecondaryModel { get; set; }
    public BindableReactiveProperty<object?> CurrentView { get; } = new(null);
    /// <inheritdoc cref="Core.Navigation.Interfaces.IImageIterator"/>>
    public IImageIterator? ImageIterator { get; private set; }
    public IThumbnailCache? ThumbnailCache { get; private set; }

    private readonly GalleryLoaderService _galleryLoader = new();
    private IFileWatcherService? _fileWatcherService = fileWatcherService;
    
    public BindableReactiveProperty<int> NavigationIndex { get; } = new(0);
    public BindableReactiveProperty<int> MaxIndex { get; } = new(0);
    public BindableReactiveProperty<bool> CanNavigateForwards { get; } = new();
    public BindableReactiveProperty<bool> CanNavigateBackwards { get; } = new();
    
    /// <summary>
    /// Should be used when changing directory or closing the tab
    /// </summary>
    private CancellationTokenSource NavigationCts { get; set; } = new();
    
    /// <summary>
    /// The main title displayed in the window title bar.
    /// </summary>
    public BindableReactiveProperty<string>? Title { get; } = new();
    /// <summary>
    /// The tooltip displayed when hovering over the title.
    /// </summary>
    public BindableReactiveProperty<string>? TitleTooltip { get; } = new();
    /// <summary>
    /// The title displayed in the taskbar or task manager.
    /// </summary>
    public BindableReactiveProperty<string>? WindowTitle { get; } = new();
    /// <summary>
    /// The title displayed in the tab.
    /// </summary>
    public BindableReactiveProperty<string> TabTitle { get; } = new(string.Empty);
    /// <summary>
    /// The tooltip displayed when hovering over the tab.
    /// </summary>
    public BindableReactiveProperty<string> TabTooltip { get; } = new(string.Empty);

    /// <summary>
    /// Initializes the TabViewModel instance by setting up necessary disposables
    /// and subscribing to model changes. If the instance is already initialized,
    /// this method returns without performing any further action.
    /// </summary>
    public void Initialize()
    {
        if (Disposables is null)
        {
            Disposables = new CompositeDisposable();
        }
        else
        {
            // Already initialized
            return;
        }

        ModelSubscription();
    }

    private void ModelSubscription()
    {
        Observable.EveryValueChanged(this, tab => tab.Model.FileInfo)
            .Subscribe(file =>
            {
                // Trigger file changes to UI
                FileInfo.Value = file;
                
                // Update title to reflect file changes
                if (file is null)
                {
                    var noImage = TranslationManager.Translation?.NoImage;
                    if (string.IsNullOrEmpty(noImage))
                    {
                        return;
                    }

                    TabTitle.Value = noImage;
                    TabTooltip.Value = noImage;
                    return;
                }

                TabTitle.Value = file.Name;
                TabTooltip.Value = file.FullName;
                UpdateTabTitle();
            })
            .AddTo(Disposables);
        Observable.EveryValueChanged(this, tab => tab.Model.Image)
            .Subscribe(image =>
            {
                // Trigger image change to UI
                Image.Value = image;
                
                // Update tiff title if appropriate (there are no file changes in this instance
                if (Model.TiffNavigation is null)
                {
                    return;
                }

                UpdateTabTitle();
            })
            .AddTo(Disposables);
    }

    /// <summary>
    /// Updates the window title and tab title based on the current image model.
    /// </summary>
    public void UpdateTabTitle()
    {
        if (ImageIterator?.Files is null || !IsSelected)
        {
            return;
        }
            
        var width = Model.PixelWidth;
        var height = Model.PixelHeight;
        var index = ImageIterator.CurrentIndex;
        var windowTitles = GetTitles();
        WindowTitle.Value = windowTitles.TitleWithAppName;
        Title.Value = windowTitles.BaseTitle;
        TitleTooltip.Value = windowTitles.FilePathTitle;
        return;
        
        WindowTitles GetTitles()
        {
            if (Model.TiffNavigation is { } tiff)
            {
                return ImageTitleFormatter.GenerateTitleStrings(width, height,
                    index, Model.FileInfo, 100, ImageIterator.Files, tiff.CurrentPage, tiff.PageCount);
            }

            return ImageTitleFormatter.GenerateTitleStrings(width, height,
                index, Model.FileInfo, 100, ImageIterator.Files);
        }
    }

    public void Initialize(IImageCache cache, IThumbnailLoader thumbnailLoader, IFileWatcherService? fileWatcherService = null, IThumbnailCache? thumbnailCache = null)
    {
        Initialize();
        if (fileWatcherService != null)
        {
            _fileWatcherService = fileWatcherService;
        }
        if (thumbnailCache != null)
        {
            ThumbnailCache = thumbnailCache;
        }
        ImageIterator = new ImageIterator(cache, thumbnailLoader, this);
    }

    public void InitializeImageIterator(IReadOnlyList<FileInfo> files, IImageCache cache, IThumbnailLoader thumbnailLoader, IFileWatcherService? fileWatcherService = null, IThumbnailCache? thumbnailCache = null)
    {
        if (fileWatcherService != null)
        {
            _fileWatcherService = fileWatcherService;
        }
        if (thumbnailCache != null)
        {
            ThumbnailCache = thumbnailCache;
            ThumbnailCache.RemoveOwner(Id);
        }
        ImageIterator ??= new ImageIterator(cache, thumbnailLoader, this);
        var index = files.FindIndex(x => x.FullName.Equals(Model?.FileInfo.FullName));
        ImageIterator.Initialize(files, index);

        if (index > -1 && index < files.Count)
        {
            cache.TryAdd(Id, index, new PreLoadValue(Model), files.Count, false, out _);
        }
        
        var directory = files.Count > 0 ? files[0].DirectoryName : null;
        _fileWatcherService?.Watch(this, directory);
        
        if (ThumbnailCache != null)
        { 
            _ = GalleryLoaderService.LoadGalleryAsync(this, files, thumbnailLoader, ThumbnailCache, GetTabCancellation().Token);
        }
    }
    
    public async ValueTask Next()
    {
        if (!CanNavigateForwards.CurrentValue)
        {
            return;
        }
        var index = ImageIterator.CurrentIndex;
        var next = ImageIterator.GetIteration(index, NavigateTo.Next, Id, SkipAmount.One);
        await ImageIterator.IterateToIndexAsync(next, NavigationCts).ConfigureAwait(false);
    }

    public async ValueTask Prev()
    {
        if (!CanNavigateBackwards.CurrentValue)
        {
            return;
        }
        var index = ImageIterator.CurrentIndex;
        var prev = ImageIterator.GetIteration(index, NavigateTo.Previous, Id, SkipAmount.One);
        await ImageIterator.IterateToIndexAsync(prev, NavigationCts).ConfigureAwait(false);
    }

    public async ValueTask CloseTab()
    {
        IsClosing = true; // Signal it to be removed from the UI
        await closeTab(Id);
        await DisposeAsync();
    }

    public CancellationTokenSource GetTabCancellation()
    {
        if (NavigationCts.IsCancellationRequested)
        {
            NavigationCts = new CancellationTokenSource();
        }
        return NavigationCts;
    }
    
    public async ValueTask DisposeAsync()
    {
        _fileWatcherService?.Unwatch(this);
        ThumbnailCache?.RemoveOwner(Id);
        if (ImageIterator is not null)
        {
            await ImageIterator.DisposeAsync();
        }
        Disposables?.Dispose();
        NavigationCts.Dispose();
        
        GC.SuppressFinalize(this);
    }

    #if DEBUG
    public override string ToString()
    {
        return $"{Id}: {Model?.FileInfo?.FullName}";
    }
    #endif
}
