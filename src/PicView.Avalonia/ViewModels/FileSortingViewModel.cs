using PicView.Avalonia.Functions;
using PicView.Core.FileSorting;
using R3;

namespace PicView.Avalonia.ViewModels;

public class FileSortingViewModel : IDisposable
{
    public ReactiveCommand SortFilesByNameCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesByName();
    });
    
    public ReactiveCommand SortFilesByCreationTimeCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesByCreationTime();
    });
    
    public ReactiveCommand<string> SortFilesByLastAccessTimeCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesByLastAccessTime();
    });
    
    public ReactiveCommand<string> SortFilesBySizeCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesBySize();
    });
    
    public ReactiveCommand<string> SortFilesByExtensionCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesByExtension();
    });
    
    public ReactiveCommand<string> SortFilesRandomlyCommand { get; } = new(async (path, _) =>
    {
        await FunctionsMapper.SortFilesRandomly();
    });
    
    public ReactiveCommand<string> SortFilesAscendingCommand { get; } = new(async (path, _) =>
    {
        await FunctionsMapper.SortFilesAscending();
    });
    
    public ReactiveCommand<string> SortFilesDescendingCommand { get; } = new(async (path, _) =>
    {
        await FunctionsMapper.SortFilesDescending();
    });
    

    public BindableReactiveProperty<SortFilesBy> SortOrder { get; } = new();
    
    public BindableReactiveProperty<bool> IsAscending { get; } = new(Settings.Sorting.Ascending);

    public void Dispose()
    {
        Disposable.Dispose();
    }
}