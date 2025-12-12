using PicView.Core.Models;
using PicView.Core.Navigation;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Titles;
using R3;

namespace PicView.Core.ViewModels;

public class TabViewModel(string id, Func<string, ValueTask> closeTab) : IAsyncDisposable
{
    private CompositeDisposable? Disposables { get; set; }
    public string Id { get; } = id;
    public bool IsClosing { get; private set; }
    public bool IsSelected { get; set; }
    public BindableReactiveProperty<ImageModel?> CurrentModel { get; } = new(null);
    public BindableReactiveProperty<object?> CurrentView { get; } = new(null);
    public IImageIterator? ImageIterator { get; private set; }
    public CancellationTokenSource NavigationCts { get; private set; } = new();

    public bool CanNavigate()
    {
        return ImageIterator is { Files.Count: > 0 };
    }
    
    public BindableReactiveProperty<string> TabTitle { get; } = new(string.Empty);
    public BindableReactiveProperty<string> TabTooltip { get; } = new(string.Empty);
    
    public void Initialize(TitleViewModel titleViewModel)
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

        CurrentModel
            .Select(model => model?.FileInfo) 
            .Subscribe(file => 
            {
                if (file is null)
                {
                    return;
                }
                TabTitle.Value = file.Name;
                TabTooltip.Value = file.FullName;

                UpdateTabTitle(titleViewModel);
            })
            .AddTo(Disposables);
    }
    
    public void UpdateTabTitle(TitleViewModel titleViewModel)
    {
        if (ImageIterator?.Files is null || !IsSelected)
        {
            return;
        }
            
        var width = CurrentModel.CurrentValue.PixelWidth;
        var height = CurrentModel.CurrentValue.PixelHeight;
        var index = ImageIterator.CurrentIndex;
        var windowTitles = ImageTitleFormatter.GenerateTitleStrings(width, height,
            index, CurrentModel.CurrentValue.FileInfo, 100, ImageIterator.Files);
        titleViewModel.WindowTitle.Value = windowTitles.BaseTitle;
        titleViewModel.WindowTitleTooltip.Value = windowTitles.FilePathTitle;
    }

    public void Initialize(IImageCache cache, IThumbnailLoader thumbnailLoader, TitleViewModel titleViewModel)
    {
        Initialize(titleViewModel);
        ImageIterator = new ImageIterator(cache, thumbnailLoader, this);
    }

    public void InitializeImageIterator(List<FileInfo> files, IImageCache cache, IThumbnailLoader thumbnailLoader)
    {
        ImageIterator ??= new ImageIterator(cache, thumbnailLoader, this);
        var index = files.FindIndex(x => x.FullName.Equals(CurrentModel.Value?.FileInfo.FullName));
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

    public void Dispose()
    {
        _ = DisposeAsync().AsTask();
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

    public override string ToString()
    {
        return Id;
    }
}
