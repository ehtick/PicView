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
public class TabViewModel(string id, Func<string, ValueTask> closeTab) : IAsyncDisposable
{
    private CompositeDisposable? Disposables { get; set; }
    public string Id { get; } = id;
    public bool IsClosing { get; private set; }
    public bool IsSelected { get; set; }
    public BindableReactiveProperty<ImageModel> Model { get; } = new(new ImageModel());
    public BindableReactiveProperty<object?> CurrentView { get; } = new(null);
    public IImageIterator? ImageIterator { get; private set; }
    public CancellationTokenSource NavigationCts { get; private set; } = new();
    
    // Titles
    public BindableReactiveProperty<string>? Title { get; } = new();

    public BindableReactiveProperty<string>? TitleTooltip { get; } = new();
    public BindableReactiveProperty<string>? WindowTitle { get; } = new();
    public BindableReactiveProperty<string> TabTitle { get; } = new(string.Empty);
    public BindableReactiveProperty<string> TabTooltip { get; } = new(string.Empty);

    
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

    public void Initialize(IImageCache cache, IThumbnailLoader thumbnailLoader)
    {
        Initialize();
        ImageIterator = new ImageIterator(cache, thumbnailLoader, this);
    }

    public void InitializeImageIterator(List<FileInfo> files, IImageCache cache, IThumbnailLoader thumbnailLoader)
    {
        ImageIterator ??= new ImageIterator(cache, thumbnailLoader, this);
        var index = files.FindIndex(x => x.FullName.Equals(Model.Value?.FileInfo.FullName));
        ImageIterator.Initialize(files, index);
    }

    public async ValueTask CloseTab()
    {
        IsClosing = true; // Signal it to be removed from the UI
        await closeTab(Id);
    }

    public CancellationTokenSource ResetNavigationCts()
    {
        NavigationCts.Cancel();
        NavigationCts = new CancellationTokenSource();
        return NavigationCts;
    }
    
    public async ValueTask DisposeAsync()
    {
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
