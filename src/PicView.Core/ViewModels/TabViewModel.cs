using PicView.Core.Extensions;
using PicView.Core.Localization;
using PicView.Core.Models;
using PicView.Core.Navigation;
using PicView.Core.Navigation.Interfaces;
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

    /// Unique identifier for this tab.
    public string Id { get; } = id;
    public bool IsClosing { get; private set; }
    public bool IsSelected { get; set; }
    public BindableReactiveProperty<ImageModel> Model { get; } = new(new ImageModel());
    public BindableReactiveProperty<object?> CurrentView { get; } = new(null);
    /// <inheritdoc cref="Core.Navigation.Interfaces.IImageIterator"/>>
    public IImageIterator? ImageIterator { get; private set; }

    private IFileWatcherService? _fileWatcherService = fileWatcherService;
    
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
        Model
            .Select(model => model.FileInfo) 
            .Subscribe(file => 
            {
                if (file is null)
                {
                    var noImage = TranslationManager.Translation.NoImage;
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
            
        var width = Model.CurrentValue.PixelWidth;
        var height = Model.CurrentValue.PixelHeight;
        var index = ImageIterator.CurrentIndex;
        var windowTitles = ImageTitleFormatter.GenerateTitleStrings(width, height,
            index, Model.CurrentValue.FileInfo, 100, ImageIterator.Files);
        WindowTitle.Value = windowTitles.TitleWithAppName;
        Title.Value = windowTitles.BaseTitle;
        TitleTooltip.Value = windowTitles.FilePathTitle;
    }

    public void Initialize(IImageCache cache, IThumbnailLoader thumbnailLoader, IFileWatcherService? fileWatcherService = null)
    {
        Initialize();
        if (fileWatcherService != null)
        {
            _fileWatcherService = fileWatcherService;
        }
        ImageIterator = new ImageIterator(cache, thumbnailLoader, this);
    }

    public void InitializeImageIterator(IReadOnlyList<FileInfo> files, IImageCache cache, IThumbnailLoader thumbnailLoader, IFileWatcherService? fileWatcherService = null)
    {
        if (fileWatcherService != null)
        {
            _fileWatcherService = fileWatcherService;
        }
        ImageIterator ??= new ImageIterator(cache, thumbnailLoader, this);
        var index = files.FindIndex(x => x.FullName.Equals(Model.Value?.FileInfo.FullName));
        ImageIterator.Initialize(files, index);
        
        var directory = files.Count > 0 ? files[0].DirectoryName : null;
        _fileWatcherService?.Watch(this, directory);
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
        return $"{Id}: {Model.Value?.FileInfo?.FullName}";
    }
    #endif
}
