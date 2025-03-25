using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PicView.Avalonia.ViewModels;
using PicView.Core.Localization;
using PicView.Core.ViewModels;
using ReactiveUI;

namespace PicView.Avalonia.Views;

public partial class FileAssociationsView : UserControl
{
    private readonly List<(CheckBox CheckBox, string SearchText)> _allCheckBoxes = [];
    private readonly CompositeDisposable _disposables = new();
        
    public FileAssociationsView()
    {
        InitializeComponent();

        AttachedToVisualTree += delegate
        {
            FilterBox.TextChanged += FilterBox_TextChanged;
            
            // Setup binding for the buttons
            SelectAllButton.Click += delegate
            {
                foreach (var checkBox in FileTypesContainer.Children.OfType<CheckBox>())
                {
                    if (checkBox is null)
                    {
                        continue;
                    }

                    if (!checkBox.IsVisible)
                    {
                        continue;
                    }

                    var tag = checkBox.Tag?.ToString();
                    if (tag.StartsWith(".zip") || tag.StartsWith(".rar") || tag.StartsWith(".7z") || tag.StartsWith(".gzip")) 
                    {
                        checkBox.IsChecked = null;
                    }
                    else
                    {
                         checkBox.IsChecked = true;
                    }
                   
                }
            };
            
            UnSelectAllButton.Click += delegate
            {
                var checkBoxes = FileTypesContainer.Children.OfType<CheckBox>();
                var enumerable = checkBoxes as CheckBox[] ?? checkBoxes.ToArray();
                if (enumerable.All(x => x.IsChecked == null && x.IsVisible))
                {
                    foreach (var checkBox in enumerable)
                    {
                        checkBox.IsChecked = false;
                    }
                }
                else
                {
                    foreach (var checkBox in enumerable.Where(x => x.IsVisible))
                    {
                        checkBox.IsChecked = null;
                    }
                }
            };
            
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
        
        vm.AssociationsViewModel ??= new FileAssociationsViewModel();
            
        // Subscribe to changes in the filter text
        vm.AssociationsViewModel.WhenAnyValue(x => x.FilterText)
            .Subscribe(FilterCheckBoxes)
            .DisposeWith(_disposables);
            
        // Create checkboxes for each file type group and item
        foreach (var fileTypeGroup in vm.AssociationsViewModel.FileTypeGroups)
        {
            if (fileTypeGroup.Name is null)
            {
                // If going into this view too fast, sometimes the name is null. This is a workaround
                if (fileTypeGroup.FileTypes.Any(x => x.Extension.StartsWith(".png")))
                {
                    fileTypeGroup.Name = TranslationManager.Translation.Normal!;
                }
                else if (fileTypeGroup.FileTypes.Any(x => x.Extension.StartsWith(".svg")))
                {
                    fileTypeGroup.Name = TranslationManager.GetTranslation("Graphics");
                }
                else if (fileTypeGroup.FileTypes.Any(x => x.Extension.StartsWith(".raw")))
                {
                    fileTypeGroup.Name = TranslationManager.GetTranslation("Raw");
                }
                else if (fileTypeGroup.FileTypes.Any(x => x.Extension.StartsWith(".wpg")))
                {
                    fileTypeGroup.Name = TranslationManager.GetTranslation("Uncommon");
                }
            }
            // Create group header checkbox
            var groupCheckBox = new CheckBox
            {
                Classes = { "altHover", "y" },
                Tag = "group",
                Name = fileTypeGroup.Name.Trim(),
                IsThreeState = true,
                IsChecked = fileTypeGroup.IsSelected
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
            groupCheckBox.IsCheckedChanged += delegate
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
                    Tag = fileType.Extension,
                    IsChecked = fileType.IsSelected,
                    IsThreeState = true
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
                fileCheckBox.IsCheckedChanged += delegate
                {
                    // Update the model - important to handle null state correctly
                    fileType.IsSelected = fileCheckBox.IsChecked;
    
                    // Now update the group checkbox state
                    UpdateGroupCheckboxState(fileTypeGroup);
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
    }
    
    private void UpdateGroupCheckboxState(FileTypeGroup group)
    {
        // Find all checkboxes that are part of this group
        var fileTypeCheckboxes = FileTypesContainer.Children.OfType<CheckBox>()
            .Where(c => c.Tag != null && c.Tag.ToString() != "group" && 
                        c.IsVisible && IsCheckboxInGroup(c, group));
                   
        var allTrue = true;
        var allFalse = true;
        var anyNull = false;
    
        foreach (var cb in fileTypeCheckboxes)
        {
            if (!cb.IsChecked.HasValue || cb.IsChecked == null)
            {
                anyNull = true;
                allTrue = false;
                allFalse = false;
            }
            else if (cb.IsChecked.Value)
            {
                allFalse = false;
            }
            else
            {
                allTrue = false;
            }
        }
    
        // Find the group checkbox
        var groupCheckbox = FileTypesContainer.Children.OfType<CheckBox>()
            .FirstOrDefault(c => c.Tag?.ToString() == "group" && c.Name == group.Name.Trim());
    
        if (groupCheckbox != null)
        {
            // Set state based on children
            if (anyNull)
                groupCheckbox.IsChecked = null;
            else if (allTrue)
                groupCheckbox.IsChecked = true;
            else if (allFalse)
                groupCheckbox.IsChecked = false;
            else
                groupCheckbox.IsChecked = null; // Mixed state
            
            // Update the ViewModel
            group.IsSelected = groupCheckbox.IsChecked;
        }
    }

    private bool IsCheckboxInGroup(CheckBox checkbox, FileTypeGroup group)
    {
        // You can determine this by position in the UI or by extension tag
        var extension = checkbox.Tag?.ToString();
        if (string.IsNullOrEmpty(extension))
            return false;
        
        return group.FileTypes.Any(ft => ft.Extensions.Contains(extension) || 
                                         extension.Contains(ft.Extensions.FirstOrDefault() ?? ""));
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
            bool? allSelected = true;
            bool? anySelected = false;
                    
            foreach (var fileType in group.FileTypes)
            {
                if (!fileType.IsSelected.HasValue)
                    anySelected = null;
                else if (!fileType.IsSelected.Value)
                    allSelected = false;
                else
                    anySelected = true;
            }
            
            // Update the group checkbox
            groupCheckBox.IsChecked = allSelected;
            groupCheckBox.IsThreeState = anySelected == null;
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
        if (DataContext is MainViewModel vm)
        {
            vm.AssociationsViewModel.FilterText = FilterBox.Text;
        }
    }
        
    private void FilterCheckBoxes(string? filterText)
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
            
        foreach (var (checkBox, searchText) in _allCheckBoxes)
        {
            checkBox.IsVisible = searchText.Contains(filterText, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}