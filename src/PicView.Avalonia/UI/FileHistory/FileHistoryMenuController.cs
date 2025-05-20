using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileHistory;

namespace PicView.Avalonia.UI.FileHistory;

/// <summary>
///     Controls the file history menu functionality
/// </summary>
public class FileHistoryMenuController
{
    private readonly Button _clearButton;
    private readonly Panel _menuContainer;
    private readonly IconButton _sortButton;
    private readonly MainViewModel _viewModel;

    public FileHistoryMenuController(Panel menuContainer, IconButton sortButton, Button clearButton, Button historyFileButton,
        MainViewModel viewModel)
    {
        _menuContainer = menuContainer;
        _sortButton = sortButton;
        _clearButton = clearButton;
        _viewModel = viewModel;

        // Initialize sort button icon
        UpdateSortButtonIcon();

        // Setup event handlers
        _sortButton.Click += OnHistorySortButtonClicked;
        _clearButton.Click += OnHistoryClearButtonClicked;
        
        ToolTip.SetTip(historyFileButton, FileHistoryManager.CurrentFileHistoryFile);
    }

    /// <summary>
    ///     Updates the file history menu items when the context menu is opened
    /// </summary>
    public void UpdateFileHistoryMenu()
    {
        if (FileHistoryManager.Count <= 0)
        {
            _menuContainer.Children.Clear();
            return;
        }

        var fileHistoryBuilder = new FileHistoryMenuBuilder(
            _menuContainer,
            _viewModel,
            FileHistoryManager.IsSortingDescending);

        fileHistoryBuilder.BuildMenu();
    }

    /// <summary>
    ///     Updates the sort button icon based on the current sort direction
    /// </summary>
    private void UpdateSortButtonIcon()
    {
        if (FileHistoryManager.IsSortingDescending)
        {
            if (Application.Current.TryGetResource("SortDescImage",
                    Application.Current.RequestedThemeVariant, out var sortDescImage))
            {
                _sortButton.Icon = sortDescImage as DrawingImage;
            }
        }
        else
        {
            if (Application.Current.TryGetResource("SortAscImage",
                    Application.Current.RequestedThemeVariant, out var sortAscImage))
            {
                _sortButton.Icon = sortAscImage as DrawingImage;
            }
        }
    }

    private void OnHistorySortButtonClicked(object? sender, RoutedEventArgs e)
    {
        FileHistoryManager.IsSortingDescending = !FileHistoryManager.IsSortingDescending;
        UpdateSortButtonIcon();
        UpdateFileHistoryMenu();
    }

    private void OnHistoryClearButtonClicked(object? sender, RoutedEventArgs e)
    {
        FileHistoryManager.Clear();
        _menuContainer.Children.Clear();
    }
}