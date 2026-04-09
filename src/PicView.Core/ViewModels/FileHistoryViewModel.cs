using ObservableCollections;
using PicView.Core.FileHistory;
using R3;
using ZLinq;

namespace PicView.Core.ViewModels;

public class FileHistoryViewModel
{
    private readonly MainWindowViewModel _mainWindow;

    public ObservableList<FileHistoryEntryViewModel> PinnedEntries { get; } = [];
    public BindableReactiveProperty<bool> HasPinnedEntries { get; } = new(false);
    public ObservableList<FileHistoryEntryViewModel> UnpinnedEntries { get; } = [];
    public BindableReactiveProperty<bool> HasUnpinnedEntries { get; } = new(false);

    public ReactiveCommand ClearHistoryCommand { get; }
    public ReactiveCommand ToggleSortCommand { get; }
    public ReactiveCommand OpenFileHistoryCommand { get; }

    public FileHistoryViewModel(MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;
        ClearHistoryCommand = new ReactiveCommand(ClearHistory);
        ToggleSortCommand = new ReactiveCommand(ToggleSort);
        OpenFileHistoryCommand = new ReactiveCommand(OpenFileHistory);
    }

    private void OpenFileHistory(Unit obj)
    {
        _mainWindow.Mapper.ShowRecentHistoryFile();
    }

    private void ToggleSort(Unit obj)
    {
        FileHistoryManager.IsSortingDescending = !FileHistoryManager.IsSortingDescending;
        UpdateHistory();
    }

    private void ClearHistory(Unit unit)
    {
        FileHistoryManager.Clear();
        UpdateHistory();
    }

    public void UpdateHistory()
    {
        PinnedEntries.Clear();
        UnpinnedEntries.Clear();
        
        if (!Settings.Navigation.IsFileHistoryEnabled)
        {
            return;
        }

        var pinnedEntries = FileHistoryManager.PinnedEntries;
        var unpinnedEntries = FileHistoryManager.UnPinnedEntries;
        
        HasPinnedEntries.Value = pinnedEntries.Any();
        HasUnpinnedEntries.Value = unpinnedEntries.Any();
        
        var currentFilePath = _mainWindow.WindowTabs.ActiveTab.Value?.Model.CurrentValue.FileInfo?.FullName;

        foreach (var entry in pinnedEntries)
        {
            var fileName = Path.GetFileName(entry.Path);
            var pinnedEntry = new FileHistoryEntryViewModel();
            pinnedEntry.Initialize(
                entry.Path, 
                fileName, 
                true, 
                entry.Path == currentFilePath,
                -1, 
                _mainWindow);
            PinnedEntries.Add(pinnedEntry);
        }

        var count = unpinnedEntries.Count();
        if (FileHistoryManager.IsSortingDescending)
        {
            for (var i = 0; i < count; i++)
            {
                var path = unpinnedEntries.ElementAt(i).Path;
                var index = i + 1;
                var fileName = Path.GetFileName(path);
                var unpinnedEntry = new FileHistoryEntryViewModel();
                unpinnedEntry.Initialize(
                    path, 
                    fileName, 
                    false, 
                    path == currentFilePath,
                    index, 
                    _mainWindow);
                UnpinnedEntries.Add(unpinnedEntry);
            }
        }
        else
        {
            for (var i = count - 1; i >= 0; i--)
            {
                var entry = unpinnedEntries.ElementAt(i);
                var index = i + 1;
                var fileName = Path.GetFileName(entry.Path);
                var unpinnedEntry = new FileHistoryEntryViewModel();
                unpinnedEntry.Initialize(
                    entry.Path, 
                    fileName, 
                    false, 
                    entry.Path == currentFilePath,
                    index,
                    _mainWindow);
                UnpinnedEntries.Add(unpinnedEntry);
            }
        }
    }
}
