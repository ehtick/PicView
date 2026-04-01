using PicView.Core.FileHistory;
using R3;

namespace PicView.Core.ViewModels;

public class FileHistoryEntryViewModel : IDisposable 
{
    private MainWindowViewModel? _mainWindow;
    public BindableReactiveProperty<string> FilePath { get; } = new();
    public BindableReactiveProperty<string> FileName { get; } = new();
    public BindableReactiveProperty<bool> IsPinned { get; } = new();
    public BindableReactiveProperty<bool> IsCurrentItem { get; } = new(false);
    public BindableReactiveProperty<int> Index { get;  } = new();

    public ReactiveCommand<Unit> OpenCommand { get; } = new();
    public ReactiveCommand<Unit> PinCommand { get; } = new();
    public ReactiveCommand<Unit> UnpinCommand { get; } = new();
    public ReactiveCommand<Unit> RemoveCommand { get; } = new();
    
    public void Initialize(string path, string fileName, bool isPinned, bool isCurrentItem, int index, MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;
        
        FilePath.Value = path;
        FileName.Value = fileName;
        IsPinned.Value = isPinned;
        IsCurrentItem.Value = isCurrentItem;
        Index.Value = index;

        OpenCommand.SubscribeAwait(Open);
        PinCommand.Subscribe(Pin);
        UnpinCommand.Subscribe(Unpin);
        RemoveCommand.Subscribe(Remove);
    }

    private void Pin(Unit unit)
    {
        IsPinned.Value = true;
        FileHistoryManager.Pin(FilePath.CurrentValue);
        _mainWindow.FileHistory.PinnedEntries.Add(this);
        _mainWindow.FileHistory.HasPinnedEntries.Value = _mainWindow.FileHistory.PinnedEntries.Count is 0;
        
        _mainWindow.FileHistory.UnpinnedEntries.Remove(this);
        _mainWindow.FileHistory.HasUnpinnedEntries.Value = _mainWindow.FileHistory.UnpinnedEntries.Count is 0;
    }

    private void Unpin(Unit unit)
    {
        IsPinned.Value = false;
        FileHistoryManager.UnPin(FilePath.CurrentValue);
        _mainWindow.FileHistory.PinnedEntries.Remove(this);
        _mainWindow.FileHistory.HasPinnedEntries.Value = _mainWindow.FileHistory.PinnedEntries.Count is 0;
    }

    private void Remove(Unit unit)
    {
        FileHistoryManager.Remove(FilePath.CurrentValue);
        _mainWindow.FileHistory.PinnedEntries.Remove(this);
        _mainWindow.FileHistory.UnpinnedEntries.Remove(this);
        _mainWindow.FileHistory.HasPinnedEntries.Value = _mainWindow.FileHistory.PinnedEntries.Count is 0;
        _mainWindow.FileHistory.HasUnpinnedEntries.Value = _mainWindow.FileHistory.UnpinnedEntries.Count is 0;
    }

    private async ValueTask Open(Unit unit, CancellationToken ct)
    {
        _mainWindow.TopTitlebarViewModel.DropDownMenu.IsDropDownMenuVisible.Value = false;
        await _mainWindow.WindowTabs.LoadFromStringAsync(FilePath.CurrentValue);
    }
    
    public void Dispose()
    {
        OpenCommand.Dispose();
        PinCommand.Dispose();
        UnpinCommand.Dispose();
        RemoveCommand.Dispose();
        
        FilePath.Dispose();
        FileName.Dispose();
        IsPinned.Dispose();
        IsCurrentItem.Dispose();
        Index.Dispose();
        
        GC.SuppressFinalize(this);
    }
}