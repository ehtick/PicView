using ObservableCollections;
using PicView.Core.FileHistory;
using R3;
using ZLinq;

namespace PicView.Core.ViewModels;

public class FileHistoryViewModel
{
    private readonly CoreViewModel _core;

    public ObservableList<FileHistoryEntryViewModel> PinnedEntries { get; } = [];
    public BindableReactiveProperty<bool> HasPinnedEntries { get; } = new(false);
    public ObservableList<FileHistoryEntryViewModel> Entries { get; } = [];

    public ReactiveCommand ClearHistoryCommand { get; }
    public ReactiveCommand ToggleSortCommand { get; }
    public ReactiveCommand OpenFileHistoryCommand { get; }

    public FileHistoryViewModel(CoreViewModel core)
    {
        _core = core;
        ClearHistoryCommand = new ReactiveCommand(ClearHistory);
        ToggleSortCommand = new ReactiveCommand(ToggleSort);
        OpenFileHistoryCommand = new ReactiveCommand(OpenFileHistory);
    }

    private void OpenFileHistory(Unit obj)
    {
        _core.MainWindows.ActiveWindow.CurrentValue.Mapper.ShowRecentHistoryFile();
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
        Entries.Clear();
        PinnedEntries.Clear();
        
        if (!Settings.Navigation.IsFileHistoryEnabled)
        {
            return;
        }

        var pinnedEntries = FileHistoryManager.PinnedEntries;
        var entries = FileHistoryManager.AllEntries;
        
        HasPinnedEntries.Value = pinnedEntries.Any();
        
        var currentFilePath = _core.MainWindows.ActiveWindow.CurrentValue.WindowTabs.ActiveTab.Value?.Model.CurrentValue.FileInfo?.FullName;

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
                _core);
            PinnedEntries.Add(pinnedEntry);
        }

        var count = entries.Count;
        if (FileHistoryManager.IsSortingDescending)
        {
            for (var i = 0; i < count; i++)
            {
                var path = entries.ElementAt(i).Path;
                var index = i + 1;
                var fileName = Path.GetFileName(path);
                var entry = new FileHistoryEntryViewModel();
                entry.Initialize(
                    path, 
                    fileName, 
                    false, 
                    path == currentFilePath,
                    index, 
                    _core);
                Entries.Add(entry);
            }
        }
        else
        {
            for (var i = count - 1; i >= 0; i--)
            {
                var entry = entries.ElementAt(i);
                var index = i + 1;
                var fileName = Path.GetFileName(entry.Path);
                var unpinnedEntry = new FileHistoryEntryViewModel();
                unpinnedEntry.Initialize(
                    entry.Path, 
                    fileName, 
                    false, 
                    entry.Path == currentFilePath,
                    index,
                    _core);
                Entries.Add(unpinnedEntry);
            }
        }
    }
}
