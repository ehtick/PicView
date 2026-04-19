using PicView.Core.DebugTools;
using PicView.Core.Extensions;
using PicView.Core.Localization;
using PicView.Core.Models;
using PicView.Core.Navigation;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.Titles;
using PicView.Core.Generators;
using PicView.Core.Sizing;
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
public class TabViewModel(Action<uint> closeTab, IFileWatcherService? fileWatcherService = null) : IDisposable
{
    /// The CoreViewModel that currently "owns" this tab
    public object? ParentWindowContext { get; set; }
    
    /// Unique identifier for this tab.
    public uint Id { get; } = TabIDGenerator.GetNextId();

    public double TabHeight => SizeDefaults.TabHeight;
    
    public CompositeDisposable Disposables { get; } = new();
    public bool IsInitialized { get; set; }
    public HoverbarViewModel Hoverbar { get; } = new();
    public GalleryViewModel Gallery { get; } = new();

    public bool IsClosing { get; private set; }
    public bool IsSelected { get; set; }
    public BindableReactiveProperty<ImageModel> Model { get; set; } = new();
    public BindableReactiveProperty<ImageModel?> SecondaryModel { get; set; } = new();
    public BindableReactiveProperty<object?> CurrentView { get; } = new(null);
    /// <inheritdoc cref="Core.Navigation.Interfaces.IImageIterator"/>>
    public IImageIterator? ImageIterator { get; private set; }
    public IThumbnailCache? ThumbnailCache { get; private set; }

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

    public BindableReactiveProperty<double> RotationAngle { get; } = new(0);
    public BindableReactiveProperty<double> ScaleX { get; } = new(1);

    /// <summary>
    /// Updates the window title and tab title based on the current image model.
    /// </summary>
    public void UpdateTabTitle()
    {
        if (!IsSelected)
        {
            return;
        }

        if (ImageIterator?.Files?.Count <= 0)
        {
            SetNewTabTitle();
            return;
        }
            
        var width = Model.CurrentValue.PixelWidth;
        var height = Model.CurrentValue.PixelHeight;
        var index = ImageIterator.CurrentIndex;
        var windowTitles = GetTitles();
        WindowTitle.Value = windowTitles.TitleWithAppName;
        Title.Value = windowTitles.BaseTitle;
        TitleTooltip.Value = windowTitles.FilePathTitle;
        if (Settings.ImageScaling.ShowImageSideBySide && SecondaryModel.CurrentValue is not null)
        {
            TabTitle.Value = StringExtensions.Combine(Model.CurrentValue.FileInfo.Name, SecondaryModel.CurrentValue.FileInfo.Name);
            TabTooltip.Value = StringExtensions.Combine(Model.CurrentValue.FileInfo.FullName, SecondaryModel.CurrentValue.FileInfo.FullName);
        }
        else
        {
            TabTitle.Value = Model.CurrentValue.FileInfo.Name;
            TabTooltip.Value = Model.CurrentValue.FileInfo.FullName;
        }
        
        return;
        
        WindowTitles GetTitles()
        {
            if (Model.CurrentValue.TiffNavigation is { } tiff)
            {
                return ImageTitleFormatter.GenerateTiffTitleStrings(width, height,
                    index, Model.CurrentValue.FileInfo, 100, ImageIterator.Files, tiff.CurrentPage, tiff.PageCount);
            }

            if (!Settings.ImageScaling.ShowImageSideBySide || SecondaryModel.CurrentValue is null)
            {
                return ImageTitleFormatter.GenerateTitleStrings(width, height,
                    index, Model.CurrentValue.FileInfo, 100, ImageIterator.Files);
            }

            var firstInfo = new ImageTitleInfo(width, height, index, Model.CurrentValue.FileInfo, 100);
            var secondInfo = new ImageTitleInfo(SecondaryModel.CurrentValue.PixelWidth, SecondaryModel.CurrentValue.PixelHeight, index + 1, SecondaryModel.CurrentValue.FileInfo, 100);
            return ImageTitleFormatter.GenerateTitleForSideBySide(firstInfo, secondInfo, ImageIterator.CurrentIndex, ImageIterator.SecondaryCurrentIndex, ImageIterator.Files);
        }
    }
    
    public void SetNewTabTitle()
    {
        var title = TranslationManager.Translation.NoImage;
        if (string.IsNullOrEmpty(title))
        {
            return;
        }
        WindowTitle.Value = title + " - PicView";
        Title.Value = title;
        TitleTooltip.Value = title;
        TabTitle.Value = title;
     }

    public void Initialize(IImageCache cache, IThumbnailCache thumbCache, IThumbnailLoader thumbnailLoader, IFileWatcherService? fileWatcherService = null, IThumbnailCache? thumbnailCache = null)
    {
        if (fileWatcherService != null)
        {
            _fileWatcherService = fileWatcherService;
        }
        if (thumbnailCache != null)
        {
            ThumbnailCache = thumbnailCache;
        }
        ImageIterator = new ImageIterator(cache, thumbCache, thumbnailLoader, this);
    }

    public void InitializeImageIterator(IReadOnlyList<FileInfo> files, IImageCache cache, IThumbnailCache thumbCache,  IThumbnailLoader thumbnailLoader, IFileWatcherService? fileWatcherService = null, IThumbnailCache? thumbnailCache = null)
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
        ImageIterator ??= new ImageIterator(cache, thumbCache, thumbnailLoader, this);
        if (Model?.CurrentValue?.FileInfo is null)
        {
#if DEBUG
            DebugHelper.LogDebug(nameof(TabViewModel), nameof(InitializeImageIterator), $"Model.FileInfo is null for tab {Id}");
#endif
            return;
        }
        var index = files.FindIndex(x => x.FullName.Equals(Model.CurrentValue.FileInfo.FullName));
        ImageIterator.Initialize(files, index);

        if (index > -1 && index < files.Count)
        {
            cache.TryAdd(Id, index, new PreLoadValue(Model.CurrentValue), files.Count, false, out _);
        }
        
        var directory = files.Count > 0 ? files[0].DirectoryName : null;
        _fileWatcherService?.Watch(this, directory);
    }

    public async Task Next()
    {
        if (!CanNavigateForwards.CurrentValue)
        {
            return;
        }
        await ImageIterator.NavigateAsync(NavigateTo.Next, SkipAmount.One, NavigationCts).ConfigureAwait(false);
    }

    public async Task Prev()
    {
        if (!CanNavigateBackwards.CurrentValue)
        {
            return;
        }
        await ImageIterator.NavigateAsync(NavigateTo.Previous, SkipAmount.One, NavigationCts).ConfigureAwait(false);
    }

    public void CloseTab()
    {
        IsClosing = true; // Signal it to be removed from the UI
        closeTab(Id);
        Dispose();
    }

    public CancellationTokenSource GetTabCancellation()
    {
        if (!NavigationCts.IsCancellationRequested)
        {
            return NavigationCts;
        }

        var oldCts = NavigationCts;
        NavigationCts = new CancellationTokenSource();
        oldCts.Dispose();
        return NavigationCts;
    }
    
    public void Dispose()
    {
        _fileWatcherService?.Unwatch(this);
        ThumbnailCache?.RemoveOwner(Id);

        ImageIterator?.Dispose();

        NavigationCts.Dispose();
        Disposables.Dispose();
        
        GC.SuppressFinalize(this);
    }

}
