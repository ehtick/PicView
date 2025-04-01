using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using PicView.Core.FileAssociations;
using ReactiveUI;

namespace PicView.Core.ViewModels;

/// <summary>
/// View model for managing file associations in PicView.
/// Handles the binding between UI checkboxes and file type data.
/// </summary>
public class FileAssociationsViewModel : ReactiveObject
{
    private readonly ReadOnlyObservableCollection<FileTypeGroup> _fileTypeGroups;
    private readonly SourceList<FileTypeGroup> _fileTypeGroupsList = new();

    /// <summary>
    /// Gets the read-only collection of file type groups that are available for association.
    /// </summary>
    public ReadOnlyObservableCollection<FileTypeGroup> FileTypeGroups => _fileTypeGroups;

    /// <summary>
    /// Gets or sets the filter text used to search and filter file type groups and items.
    /// </summary>
    public string? FilterText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the view model is currently processing an operation.
    /// Used to disable UI interaction during long-running tasks.
    /// </summary>
    public bool IsProcessing
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Gets or sets the opacity value for the UI, used to visually indicate processing state.
    /// </summary>
    public double Opacity
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 1.0;

    /// <summary>
    /// Command to apply the selected file associations.
    /// </summary>
    public ReactiveCommand<Unit, bool> ApplyCommand { get; }

    /// <summary>
    /// Command to clear the current filter text.
    /// </summary>
    public ReactiveCommand<Unit, string> ClearFilterCommand { get; }

    /// <summary>
    /// Command to unassociate all file types from the application.
    /// </summary>
    public ReactiveCommand<Unit, Unit> UnassociateCommand { get; }

    /// <summary>
    /// Command to reset file type selections to their default state.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }

    /// <summary>
    /// Command to select all visible file types.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SelectAllCommand { get; }

    /// <summary>
    /// Command to unselect all visible file types.
    /// </summary>
    public ReactiveCommand<Unit, Unit> UnselectAllCommand { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FileAssociationsViewModel"/> class.
    /// Sets up file type groups, commands, and filtering behavior.
    /// </summary>
    public FileAssociationsViewModel()
    {
        // Create file type groups and populate with data
        InitializeFileTypes();
        
        // Setup the filtering
        var filter = this.WhenAnyValue(x => x.FilterText)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Select(BuildFilter);
            
        _fileTypeGroupsList.Connect()
            .AutoRefresh()
            .Filter(filter)
            .Bind(out _fileTypeGroups)
            .Subscribe();

        // CanExecute for commands
        var canExecute = this.WhenAnyValue(x => x.IsProcessing)
            .Select(processing => !processing);
            
        // Initialize commands with error handling
        ApplyCommand = ReactiveCommand.CreateFromTask(
            ApplyFileAssociations, 
            canExecute);
            
        // Handle errors from the Apply command
        ApplyCommand.ThrownExceptions
            .Subscribe(ex => 
            {
                IsProcessing = false;
#if DEBUG
                Debug.WriteLine($"Error in ApplyCommand: {ex}");
#endif
            });
        
        UnassociateCommand = ReactiveCommand.CreateFromTask(
            UnassociateFileAssociations, 
            canExecute);
        
        UnassociateCommand.ThrownExceptions
            .Subscribe(ex => 
            {
                IsProcessing = false;
                Debug.WriteLine($"Error in UnassociateCommand: {ex}");
            });
            
        ClearFilterCommand = ReactiveCommand.Create(() => FilterText = string.Empty);
        
        ResetCommand = ReactiveCommand.Create(ResetFileTypesToDefault, canExecute);
        
        ResetCommand.ThrownExceptions
            .Subscribe(ex =>
            {
                Debug.WriteLine($"Error in ResetCommand: {ex}");
            });
        
        SelectAllCommand = ReactiveCommand.Create(SelectAllFileTypes, canExecute);
        
        SelectAllCommand.ThrownExceptions
            .Subscribe(ex =>
            {
                Debug.WriteLine($"Error in SelectAllCommand: {ex}");
            });
        
        UnselectAllCommand = ReactiveCommand.Create(UnselectAllFileTypes, canExecute);
        
        UnselectAllCommand.ThrownExceptions
            .Subscribe(ex =>
            {
                Debug.WriteLine($"Error in UnselectAllCommand: {ex}");
            });
        
        this.WhenAnyValue(x => x.IsProcessing).Subscribe(isProcessing =>
        {
            Opacity = isProcessing ? 0.3 : 1.0;
        });
    }

    #region Initialize and filtering
    
    /// <summary>
    /// Initializes file type groups by loading default file types from <see cref="FileTypeHelper"/>.
    /// </summary>
    private void InitializeFileTypes()
    {
        var groups = FileTypeHelper.GetFileTypes();
        
        _fileTypeGroupsList.Edit(list =>
        {
            list.Clear();
            list.AddRange(groups);
        });
    }
    
    /// <summary>
    /// Builds a filter function for file type groups based on the provided filter text.
    /// </summary>
    /// <param name="filter">The filter text to search for in file type descriptions and extensions.</param>
    /// <returns>A function that determines if a file type group should be visible based on the filter.</returns>
    private Func<FileTypeGroup, bool> BuildFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            // Reset all items to visible when filter is empty
            foreach (var group in _fileTypeGroupsList.Items)
            {
                foreach (var item in group.FileTypes)
                {
                    item.IsVisible = true;
                }
            }
            return _ => true;
        }
        
        return group => {
            // Update visibility of items based on filter
            var anyVisible = false;
            foreach (var item in group.FileTypes)
            {
                item.IsVisible = item.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) || 
                                 item.Extension.Contains(filter, StringComparison.OrdinalIgnoreCase);
                if (item.IsVisible)
                    anyVisible = true;
            }
        
            // Only show groups that have at least one visible item
            return anyVisible;
        };
    }
    
    #endregion
    
    #region Selection
    
    /// <summary>
    /// Updates all selection states to ensure changes are properly reflected in the UI.
    /// This method forces property notifications to be sent for all selection states.
    /// </summary>
    private void UpdateSelection()
    {
        // Force property notifications to ensure all changes are processed
        foreach (var group in FileTypeGroups)
        {
            group.IsSelected = group.IsSelected;
            foreach (var fileType in group.FileTypes)
            {
                fileType.IsSelected = fileType.IsSelected;
            }
        }
    }
    
    /// <summary>
    /// Resets all file type selections to their default state as defined by <see cref="FileTypeHelper.GetFileTypes"/>.
    /// </summary>
    /// <remarks>
    /// This method uses snapshots of collections to avoid enumeration modification exceptions.
    /// </remarks>
    private void ResetFileTypesToDefault()
    {
        // Get fresh default file types
        var defaultGroups = FileTypeHelper.GetFileTypes();
        
        // Use snapshot to get current groups to avoid enumeration issues
        var currentGroups = FileTypeGroups.ToArray();
        
        foreach (var group in currentGroups)
        {
            var defaultGroup = defaultGroups.FirstOrDefault(g => g.Name == group.Name);
            if (defaultGroup == null)
                continue;
                
            // Update the group selection state
            group.IsSelected = defaultGroup.IsSelected;
            
            // Create snapshot of file types to avoid enumeration issues
            var fileTypes = group.FileTypes.ToArray();
            foreach (var fileType in fileTypes)
            {
                var defaultType = defaultGroup.FileTypes.FirstOrDefault(dt => 
                    dt.Description == fileType.Description);
                
                if (defaultType != null)
                {
                    fileType.IsSelected = defaultType.IsSelected;
                }
            }
        }
    }

    /// <summary>
    /// Unselects all file types by setting their selection state to false.
    /// Used before unassociating all file types from the application.
    /// </summary>
    /// <remarks>
    /// This method uses snapshots of collections to avoid enumeration modification exceptions.
    /// </remarks>
    private void UnselectFileTypes()
    {
        // Make a copy of the current groups to avoid enumeration issues
        var currentGroups = FileTypeGroups.ToArray();
    
        // Update selection states to false for all items
        foreach (var group in currentGroups)
        {
            group.IsSelected = false;
            
            // Use snapshot of file types to avoid enumeration issues
            var fileTypes = group.FileTypes.ToArray();
            foreach (var fileType in fileTypes)
            {
                fileType.IsSelected = false;
            }
        }
    }
    
    /// <summary>
    /// Selects all visible file types except for archive types which are handled specially.
    /// </summary>
    /// <remarks>
    /// This method uses snapshots of collections to avoid enumeration modification exceptions.
    /// Archive types (.zip, .rar, etc.) are not automatically selected to avoid unwanted associations.
    /// </remarks>
    private void SelectAllFileTypes()
    {
        // Make a copy of the current groups to avoid enumeration issues
        var currentGroups = FileTypeGroups.ToArray();
    
        // Update selection states to true for all items
        foreach (var group in currentGroups)
        {
            // Use snapshot of file types to avoid enumeration issues
            var fileTypes = group.FileTypes.ToArray();
            foreach (var fileType in fileTypes)
            {
                if (!fileType.IsVisible)
                {
                    // We don't want to select hidden items
                    continue;
                }
                // Only set archive types explicitly
                if (fileType.Extension.StartsWith(".zip") ||
                    fileType.Extension.StartsWith(".rar") ||
                    fileType.Extension.StartsWith(".7z") ||
                    fileType.Extension.StartsWith(".gzip")) 
                {
                    continue;
                }
                fileType.IsSelected = true;
            }
        }
    }

    /// <summary>
    /// Toggles selection state of all visible file types between indeterminate and unselected.
    /// If the number of indeterminate checkboxes equals or is greater than the number of non-indeterminate ones,
    /// all visible checkboxes will be set to unchecked. Otherwise, all will be set to indeterminate.
    /// </summary>
    /// <remarks>
    /// This method uses snapshots of collections to avoid enumeration modification exceptions.
    /// </remarks>
    private void UnselectAllFileTypes()
    {
        // Make a copy of the current groups to avoid enumeration issues
        var currentGroups = FileTypeGroups.ToArray();
    
        // Count the total number of visible checkboxes and indeterminate ones
        var totalVisible = 0;
        var indeterminateCount = 0;

        foreach (var group in currentGroups)
        {
            foreach (var fileType in group.FileTypes.Where(ft => ft.IsVisible))
            {
                totalVisible++;
                if (fileType.IsSelected == null)
                {
                    indeterminateCount++;
                }
            }
        }

        // Determine which state to set based on the counts
        // If indeterminate count is equal to or greater than non-indeterminate count, 
        // set all to unchecked, otherwise set all to indeterminate
        var setToUnchecked = indeterminateCount >= totalVisible - indeterminateCount;

        // Apply the chosen state to all visible checkboxes
        foreach (var group in currentGroups)
        {
            foreach (var fileType in group.FileTypes.Where(ft => ft.IsVisible))
            {
                fileType.IsSelected = setToUnchecked ? false : null;
            }
        }
    }
    
    #endregion

    #region Associations
    
    /// <summary>
    /// Applies the current file type associations based on selection states.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, with a boolean result indicating success.</returns>
    /// <remarks>
    /// This method sets <see cref="IsProcessing"/> to true during execution, which disables UI interaction.
    /// </remarks>
    private async Task<bool> ApplyFileAssociations() => await SetFileAssociations(false);
    
    /// <summary>
    /// Unassociates all file types from the application by setting all selection states to false
    /// and then applying the associations.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method sets <see cref="IsProcessing"/> to true during execution, which disables UI interaction.
    /// </remarks>
    private async Task UnassociateFileAssociations() => await SetFileAssociations(true);
    
    private async Task<bool> SetFileAssociations(bool unassociate)
    {
        try
        {
            IsProcessing = true;

            return await Task.Run(async () =>
            {
                if (unassociate)
                {
                    UnselectFileTypes();
                }
                else
                {
                    UpdateSelection();
                }
                
                return await FileAssociationProcessor.SetFileAssociations(FileTypeGroups);
            });
        }
        finally
        {
            IsProcessing = false;
        }
    }
    
    #endregion
}