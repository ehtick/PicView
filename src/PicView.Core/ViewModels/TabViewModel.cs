using PicView.Core.Models;
using PicView.Core.Navigation;
using R3;

namespace PicView.Core.ViewModels;

public class TabViewModel(string id, Func<string, ValueTask> closeTab) : IAsyncDisposable
{
    private CompositeDisposable? Disposables { get; set; }
    public string Id { get; init; } = id;
    public bool IsClosing { get; private set; }
    public bool IsSelected { get; set; } = false;
    public ImageModel IModel { get; set; } = new();
    public IImageIterator? ImageIterator { get; set; }
    public CancellationTokenSource NavigationCts { get; private set; } = new();

    public bool CanNavigate()
    {
        return ImageIterator is { Files.Count: > 0 };
    }
    
    public BindableReactiveProperty<string> TabTitle { get; } = new(string.Empty);
    public BindableReactiveProperty<string> TabTooltip { get; } = new(string.Empty);
    public BindableReactiveProperty<object?> ImageSource { get; } = new(null);

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
        Observable.EveryValueChanged(IModel, model => model.FileInfo).Subscribe(file => 
        {
            if (file is null)
            {
                return;
            }

            TabTitle.Value = file.Name;
            TabTooltip.Value = file.FullName;
        })
        .AddTo(Disposables);
        
        Observable.EveryValueChanged(IModel, model => model.Image).Subscribe(img => 
        {
            ImageSource.Value = img;
        })
        .AddTo(Disposables);
    }


    public async ValueTask CloseTab()
    {
        IsClosing = true; // Signal it to be removed from the UI
        await closeTab(Id);
    }

    public CancellationTokenSource ResetNavigationCts()
    {
        NavigationCts.Cancel();
        NavigationCts.Dispose();
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
        Disposables.Dispose();
        NavigationCts.Dispose();
        
        GC.SuppressFinalize(this);
    }
}