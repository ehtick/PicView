using PicView.Core.Models;
using PicView.Core.Navigation;
using R3;

namespace PicView.Core.ViewModels;

public class TabViewModel(string id, Func<string, ValueTask> closeTab) : IAsyncDisposable
{
    private CompositeDisposable Disposables { get; } = new();
    public string Id { get; init; } = id;
    public bool IsClosing { get; private set; }
    public bool IsSelected { get; set; } = false;
    public ImageModel IModel { get; set; } = new();
    public IImageIterator? ImageIterator { get; set; }
    public CancellationTokenSource NavigationCts { get; private set; } = new();

    public bool CanNavigate()
    {
        if (ImageIterator is null)
        {
            return false;
        }

        return false;
    }

    
    // Used to bind the tab content to the UI. I.E, the image viewer or start-up menu.
    public BindableReactiveProperty<object?> TabContent { get; set; } = new();
    
    public BindableReactiveProperty<string> TabTitle { get; } = new(string.Empty);
    public BindableReactiveProperty<string> TabTooltip { get; } = new(string.Empty);
    public BindableReactiveProperty<object?> ImageSource { get; } = new(null);

    public void Initialize()
    {
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

    public void CancelNavigation()
    {
        NavigationCts.Cancel();
        NavigationCts.Dispose();
        NavigationCts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        ImageIterator?.DisposeAsync().AsTask().Start();
        Disposables.Dispose();
    }
    public async ValueTask DisposeAsync()
    {
        if (ImageIterator is not null)
        {
            await ImageIterator.DisposeAsync();
        }
        Disposables.Dispose();
    }
}