using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileAssociations;
using PicView.Core.Localization;
using PicView.Core.ViewModels;
using R3;
using Observable = R3.Observable;

namespace PicView.Avalonia.Views;

public partial class FileAssociationsView : UserControl
{
    private readonly List<(CheckBox CheckBox, string SearchText)> _allCheckBoxes = [];
    private readonly CompositeDisposable _disposables = new();
        
    public FileAssociationsView()
    {
        InitializeComponent();
        
        FileTypesScrollViewer.Height = ScreenHelper.ScreenSize.WorkingAreaHeight switch
        {
            > 500 and <= 650 => 340,
            >= 650 and <= 700 => 440,
            >= 700 => 500,
            _ => 240
        };

        Loaded += delegate
        {
            InitializeCheckBoxesCollection();

            KeyDown += (_, e) =>
            {
                var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
                if (e.Key == Key.F && ctrl)
                {
                    FilterBox.Focus();
                }
            };
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
        Observable.EveryValueChanged(vm.AssociationsViewModel, x => x.FilterText.Value, UIHelper.GetFrameProvider)
            .Subscribe(FilterCheckBoxes)
            .AddTo(_disposables);
            
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
                    fileTypeGroup.Name = TranslationManager.Translation.Graphics!;
                }
                else if (fileTypeGroup.FileTypes.Any(x => x.Extension.StartsWith(".raw")))
                {
                    fileTypeGroup.Name = TranslationManager.Translation.RawCamera!;
                }
                else if (fileTypeGroup.FileTypes.Any(x => x.Extension.StartsWith(".wpg")))
                {
                    fileTypeGroup.Name = TranslationManager.Translation.Uncommon!;
                }
                else if (fileTypeGroup.FileTypes.Any(x => x.Extension.StartsWith(".zip")))
                {
                    fileTypeGroup.Name = TranslationManager.Translation.Archives!;
                }
            }
            // Create group header checkbox
            var groupCheckBox = new CheckBox
            {
                Classes = { "altHover", "y", "changeColor" },
                Tag = "group",
                Name = fileTypeGroup.Name,
                IsThreeState = true,
                IsChecked = fileTypeGroup.IsSelected.CurrentValue
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
            groupCheckBox.Click += delegate
            {
                var isChecked = groupCheckBox.IsChecked;

                foreach (var fileType in fileTypeGroup.FileTypes)
                {
                    fileType.IsSelected.Value = isChecked;
                }
                UpdateCheckBoxesFromViewModel();
            };
                
            // Create checkboxes for each file type item in the group
            foreach (var fileType in fileTypeGroup.FileTypes)
            {
                var fileCheckBox = new CheckBox
                {
                    Classes = { "altHover", "x", "changeColor" },
                    Tag = fileType.Extension,
                    IsChecked = fileType.IsSelected.CurrentValue,
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
                    fileType.IsSelected.Value = fileCheckBox.IsChecked;
    
                    // Now update the group checkbox state
                    UpdateGroupCheckboxState(fileTypeGroup);
                };
                    
                // Subscribe to changes in the file type's IsSelected property
                Observable.EveryValueChanged(fileType, x => x.IsSelected, UIHelper.GetFrameProvider)
                    .Subscribe( isSelected =>
                    {
                        fileCheckBox.IsChecked = isSelected.CurrentValue;
                    })
                    .AddTo(_disposables);
                    
                // Subscribe to changes in the file type's IsVisible property
                Observable.EveryValueChanged(fileType, x => x.IsVisible, UIHelper.GetFrameProvider)
                    .Subscribe(isVisible =>
                    {
                        fileCheckBox.IsVisible = isVisible.CurrentValue;
                    })
                    .AddTo(_disposables);
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

        if (groupCheckbox == null)
        {
            return;
        }

        if (anyNull)
            groupCheckbox.IsChecked = null;
        else if (allTrue)
            groupCheckbox.IsChecked = true;
        else if (allFalse)
            groupCheckbox.IsChecked = false;
        else
            groupCheckbox.IsChecked = null;
            
        // Update the ViewModel
        group.IsSelected.Value = groupCheckbox.IsChecked;
    }

    private static bool IsCheckboxInGroup(CheckBox checkbox, FileTypeGroup group)
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
                if (!fileType.IsSelected.CurrentValue.HasValue)
                    anySelected = null;
                else if (!fileType.IsSelected.CurrentValue.Value)
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