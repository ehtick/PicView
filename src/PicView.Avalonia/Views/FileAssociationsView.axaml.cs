using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PicView.Avalonia.ViewModels;
using ReactiveUI;

namespace PicView.Avalonia.Views;

public partial class FileAssociationsView : UserControl
{
    private readonly List<(CheckBox CheckBox, string SearchText)> _allCheckBoxes = [];
    private CompositeDisposable _disposables = new CompositeDisposable();
        
    public FileAssociationsView()
    {
        InitializeComponent();
            
        FilterBox.TextChanged += FilterBox_TextChanged;
                
        // Clear button functionality
        ClearButton.Click += (s, e) => 
        { 
            FilterBox.Text = string.Empty;
            FilterCheckBoxes(string.Empty);
        };
            
        // Setup binding for the buttons
        SelectAllButton.Click += (s, e) =>
        {
            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            vm.AssociationsViewModel.SelectAllCommand.Execute().Subscribe();
            UpdateCheckBoxesFromViewModel();
        };
            
        UnSelectAllButton.Click += (s, e) =>
        {
            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            vm.AssociationsViewModel.UnselectAllCommand.Execute().Subscribe();
            UpdateCheckBoxesFromViewModel();
        };
            
        DataContextChanged += (s, e) =>
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
                
            // Initialize the collection of checkboxes once the DataContext is set
            InitializeCheckBoxesCollection();
        };
    }
        
    private void InitializeCheckBoxesCollection()
    {
        var container = FileTypesContainer;

        if (DataContext is not MainViewModel vm)
        {
            return;
        }
            
        // Subscribe to changes in the filter text
        vm.AssociationsViewModel.WhenAnyValue(x => x.FilterText)
            .Subscribe(FilterCheckBoxes)
            .DisposeWith(_disposables);
            
        // Create checkboxes for each file type group and item
        foreach (var fileTypeGroup in vm.AssociationsViewModel.FileTypeGroups)
        {
            // Create group header checkbox
            var groupCheckBox = new CheckBox
            {
                Classes = { "altHover", "y" },
                Name = $"{fileTypeGroup.Name.Replace(" ", "")}Group", // Remove spaces for the name
                IsChecked = fileTypeGroup.IsSelected,
            };
                
            var groupTextBlock = new TextBlock
            {
                Classes = { "txt" },
                Text = fileTypeGroup.Name,
                FontFamily = new FontFamily("avares://PicView.Avalonia/Assets/Fonts/Roboto-Bold.ttf#Roboto")
            };
                
            groupCheckBox.Content = groupTextBlock;
                
            // Add to container
            container.Children.Add(groupCheckBox);
                
            // Add to the collection for filtering
            _allCheckBoxes.Add((groupCheckBox, fileTypeGroup.Name));
                
            // Handle group checkbox changes to update all items in the group
            groupCheckBox.IsCheckedChanged += (s, e) =>
            {
                var isChecked = groupCheckBox.IsChecked;
                if (!isChecked.HasValue)
                {
                    return;
                }

                foreach (var fileType in fileTypeGroup.FileTypes)
                {
                    fileType.IsSelected = isChecked.Value;
                }
                UpdateCheckBoxesFromViewModel();
            };
                
            // Create checkboxes for each file type item in the group
            foreach (var fileType in fileTypeGroup.FileTypes)
            {
                var fileCheckBox = new CheckBox
                {
                    Classes = { "altHover", "x" },
                    IsChecked = fileType.IsSelected,
                };
                    
                var fileTextBlock = new TextBlock
                {
                    Classes = { "txt" },
                    Text = $"{fileType.Description} ({fileType.Extension})",
                    Margin = new Thickness(0),
                    Padding = new Thickness(0, 1, 5, 0),
                };
                    
                fileCheckBox.Content = fileTextBlock;
                    
                // Add to container
                container.Children.Add(fileCheckBox);
                    
                // Add to the collection for filtering
                _allCheckBoxes.Add((fileCheckBox, $"{fileType.Description} {fileType.Extension}"));
                    
                // Bind the checkbox to the file type's IsSelected property
                fileCheckBox.IsCheckedChanged += (s, e) =>
                {
                    if (fileCheckBox.IsChecked.HasValue)
                    {
                        fileType.IsSelected = fileCheckBox.IsChecked.Value;
                    }
                };
                    
                // Subscribe to changes in the file type's IsSelected property
                fileType.WhenAnyValue(x => x.IsSelected)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(isSelected =>
                    {
                        fileCheckBox.IsChecked = isSelected;
                    })
                    .DisposeWith(_disposables);
                    
                // Subscribe to changes in the file type's IsVisible property
                fileType.WhenAnyValue(x => x.IsVisible)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(isVisible =>
                    {
                        fileCheckBox.IsVisible = isVisible;
                    })
                    .DisposeWith(_disposables);
            }
        }
            
        // Initial filter
        //FilterCheckBoxes(vm.AssociationsViewModel.FilterText);
    }
        
    private void UpdateCheckBoxesFromViewModel()
    {
        if (DataContext is not MainViewModel vm)
            return;
            
        foreach (var group in vm.AssociationsViewModel.FileTypeGroups)
        {
            // Find the group checkbox
            var groupName = $"{group.Name.Replace(" ", "")}Group";
            var groupCheckBox = FindLogicalDescendant<CheckBox>(groupName);
            if (groupCheckBox == null)
            {
                continue;
            }

            // Check if all children are selected
            var allSelected = true;
            var anySelected = false;
                    
            foreach (var fileType in group.FileTypes)
            {
                if (!fileType.IsSelected)
                    allSelected = false;
                else
                    anySelected = true;
            }
                    
            groupCheckBox.IsChecked = allSelected ? true : (anySelected ? null : false);
        }
    }
        
    private T? FindLogicalDescendant<T>(string name) where T : Control
    {
        foreach (var child in LogicalChildren)
        {
            switch (child)
            {
                case T control when control.Name == name:
                    return control;
                case not null:
                {
                    foreach (var grandchild in child.LogicalChildren)
                    {
                        if (grandchild is T grandControl && grandControl.Name == name)
                            return grandControl;
                    }

                    break;
                }
            }
        }
            
        return null;
    }
        
    private void FilterBox_TextChanged(object? sender, EventArgs e)
    {
        return;
        if (DataContext is MainViewModel vm)
        {
            vm.AssociationsViewModel.FilterText = FilterBox.Text;
        }
    }
        
    private void FilterCheckBoxes(string filterText)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            // Show all checkboxes
            foreach (var (checkBox, _) in _allCheckBoxes)
            {
                checkBox.IsVisible = true;
            }
            return;
        }
            
        filterText = filterText.ToLowerInvariant();
            
        foreach (var (checkBox, searchText) in _allCheckBoxes)
        {
            checkBox.IsVisible = searchText.Contains(filterText, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}