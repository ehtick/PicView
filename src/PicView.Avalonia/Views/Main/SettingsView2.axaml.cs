using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PicView.Avalonia.UI;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Views.Main;

public partial class SettingsView2 : UserControl
{
    private const string SearchDim = "searchDim";
    private const string SearchMatch = "searchMatch";
    private int _currentMatchIndex = -1;

    private bool _isScrollingProgrammatically;
    private bool _isUpdatingFromSpy;
    private readonly List<Control> _searchMatches = [];
    private Dictionary<SettingsCategory, Control>? _sections;
    private IDisposable? _subscription;

    public SettingsView2()
    {
        InitializeComponent();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            //FileAssociationsTabItem.IsEnabled = false;
        }

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        FilterBox.TextChanged += OnSearchTextChanged;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        InitializeSectionsMap();
        CategoriesListBox.SelectionChanged += OnListBoxSelectionChanged;
        ContentScrollViewer.ScrollChanged += OnScrollChanged;

        KeyDown += OnKeyDown;
        // Add this line:
        FilterBox.KeyDown += OnFilterBoxKeyDown;

        if (DataContext is not CoreViewModel core)
        {
            return;
        }

        _subscription = core.SettingsViewModel.SelectedCategory.Subscribe(OnViewModelCategoryChanged);
    }

    private void OnFilterBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            CycleToNextMatch();
        }
    }

    private void CycleToNextMatch()
    {
        if (_searchMatches.Count == 0)
        {
            return;
        }

        // Increment index and wrap around
        _currentMatchIndex++;
        if (_currentMatchIndex >= _searchMatches.Count)
        {
            _currentMatchIndex = 0;
        }

        var match = _searchMatches[_currentMatchIndex];
        ScrollToControl(match);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not CoreViewModel core)
        {
            return;
        }

        var isSearchFocused = FilterBox.IsFocused || FilterBox.IsKeyboardFocusWithin;

        if (e.Key is Key.Escape)
        {
            if (isSearchFocused)
            {
                if (FilterBox.Text.Length <= 0)
                {
                    // Switch away focus
                    ContentScrollViewer.Focus();
                }
                else
                {
                    FilterBox.Clear();
                }

                e.Handled = true;
                return;
            }
        }

        var isCtrl = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? e.KeyModifiers is KeyModifiers.Meta
            : e.KeyModifiers is KeyModifiers.Control;

        if ((e.Key is Key.F && isCtrl) || (!isSearchFocused && e.Key is Key.OemQuestion))
        {
            FilterBox.Focus();
            e.Handled = true;
            return;
        }

        if (core.SettingsViewModel.IsOverviewVisible.CurrentValue)
        {
            // TODO: Use arrow keys to navigate overview categories
            return;
        }

        switch (e.Key)
        {
            case Key.Down:
            case Key.PageDown:
                ContentScrollViewer.LineDown();
                break;
            case Key.Up:
            case Key.PageUp:
                ContentScrollViewer.LineUp();
                break;
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var searchText = FilterBox.Text;

        // Logic: 
        // Always keep the main view visible. 
        // Just run the styling pass.

        // Ensure we are in the "Split View" mode (where settings are visible), 
        // because searching in the "Overview" (grid of big buttons) might be confusing
        // if we are highlighting individual controls inside the categories.
        if (DataContext is CoreViewModel core && !string.IsNullOrWhiteSpace(searchText))
        {
            // Force switch to Split View so we can see the controls we are searching
            if (core.SettingsViewModel.IsOverviewVisible.Value)
            {
                core.SettingsViewModel.IsOverviewVisible.Value = false;
            }
        }

        PerformSearch(searchText);

        // 2. Scroll to the best match
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            // We invoke this on the next UI tick to ensure Layout/Bounds are updated 
            // if the view just switched from Overview to SplitView
            Dispatcher.UIThread.Post(ScrollToNearestMatch, DispatcherPriority.Input);
        }
    }

    private void PerformSearch(string query)
    {
        if (_sections == null)
        {
            return;
        }

        // Reset State
        _searchMatches.Clear();
        _currentMatchIndex = -1;

        // 1. CLEAR: If query is empty, remove all search classes
        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var child in _sections.Values
                         .Select(sectionControl => sectionControl.GetVisualDescendants().OfType<Border>())
                         .SelectMany(allControls => allControls))
            {
                child.Classes.Remove(SearchMatch);
                child.Classes.Remove(SearchDim);
            }

            return;
        }

        // 2. SEARCH: Loop through all controls in Order
        // We use Enum.GetValues to ensure we iterate top-to-bottom
        foreach (var category in Enum.GetValues<SettingsCategory>())
        {
            if (!_sections.TryGetValue(category, out var sectionControl))
            {
                continue;
            }

            var children = sectionControl.GetVisualDescendants().OfType<Control>();

            foreach (var child in children)
            {
                // Skip controls that aren't marked for search
                if (child.Tag == null && SearchProperties.GetKeywords(child) == null)
                {
                    continue;
                }

                // Determine if it matches
                var isMatch = false;

                // Get keywords (Attached Property) or Tag
                var keywordsString = SearchProperties.GetKeywords(child);
                if (string.IsNullOrEmpty(keywordsString) && child.Tag is string tag)
                {
                    keywordsString = tag;
                }

                if (!string.IsNullOrEmpty(keywordsString))
                {
                    var keywords = keywordsString.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
                    isMatch = keywords.Any(k => k.Contains(query, StringComparison.OrdinalIgnoreCase));
                }

                // 3. APPLY CLASSES AND STORE MATCH
                if (isMatch)
                {
                    child.Classes.Remove(SearchDim);
                    child.Classes.Add(SearchMatch);
                    _searchMatches.Add(child);
                }
                else
                {
                    child.Classes.Remove(SearchMatch);
                    child.Classes.Add(SearchDim);
                }
            }
        }
    }

    private void ScrollToNearestMatch()
    {
        if (_searchMatches.Count == 0)
        {
            return;
        }

        Control? bestMatch = null;
        var bestDistanceToCenter = double.MaxValue;
        var viewportCenter = ContentScrollViewer.Viewport.Height / 2;

        // Find the match closest to the center of the viewport
        foreach (var match in _searchMatches)
        {
            var relativePoint = match.TranslatePoint(new Point(0, 0), ContentScrollViewer);
            if (!relativePoint.HasValue)
            {
                continue;
            }

            // Calculate distance from the element's center to the viewport's center
            var elementCenter = relativePoint.Value.Y + match.Bounds.Height / 2;
            var distance = Math.Abs(elementCenter - viewportCenter);

            if (distance < bestDistanceToCenter)
            {
                bestDistanceToCenter = distance;
                bestMatch = match;
            }
        }

        if (bestMatch == null)
        {
            return;
        }

        _currentMatchIndex = _searchMatches.IndexOf(bestMatch);
        ScrollToControl(bestMatch);
    }

    private void ScrollToControl(Control target)
    {
        var relativePoint = target.TranslatePoint(new Point(0, 0), ContentScrollViewer);
        if (!relativePoint.HasValue)
        {
            return;
        }

        var currentOffset = ContentScrollViewer.Offset.Y;
        var relativeY = relativePoint.Value.Y;
        var viewportHalfHeight = ContentScrollViewer.Viewport.Height / 2;
        var elementHalfHeight = target.Bounds.Height / 2;
    
        var targetOffset = currentOffset + relativeY - viewportHalfHeight + elementHalfHeight;

        _isScrollingProgrammatically = true;
        try
        {
            ContentScrollViewer.Offset = new Vector(0, targetOffset);

            UpdateCategoryForMatch(target);
        }
        finally
        {
            Dispatcher.UIThread.Post(() => _isScrollingProgrammatically = false, DispatcherPriority.Input);
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        FilterBox.KeyDown -= OnFilterBoxKeyDown;
        CategoriesListBox.SelectionChanged -= OnListBoxSelectionChanged;
        ContentScrollViewer.ScrollChanged -= OnScrollChanged;
        _subscription?.Dispose();
    }
    
    private void UpdateCategoryForMatch(Control target)
    {
        if (_sections == null || DataContext is not CoreViewModel core)
        {
            return;
        }

        foreach (var kvp in _sections)
        {
            var category = kvp.Key;
            var section = kvp.Value;

            // Check if the section contains the target control
            if (section == target || section.IsVisualAncestorOf(target))
            {
                // Only update if the category is actually different
                if (core.SettingsViewModel.SelectedCategory.Value != category)
                {
                    // Set this flag to prevent the ViewModel change from triggering 
                    // a scroll back to the top of the category (loop prevention)
                    _isUpdatingFromSpy = true;
                    try
                    {
                        core.SettingsViewModel.SelectedCategory.Value = category;
                    }
                    finally
                    {
                        _isUpdatingFromSpy = false;
                    }
                }
                return;
            }
        }
    }

    private void InitializeSectionsMap()
    {
        _sections = new Dictionary<SettingsCategory, Control>();
        foreach (var category in Enum.GetValues<SettingsCategory>())
        {
            var sectionName = category + "Section";
            var control = this.FindControl<Control>(sectionName);
            if (control != null)
            {
                _sections[category] = control;
            }
        }
    }

    private void OnViewModelCategoryChanged(SettingsCategory category)
    {
        // Update ListBox selection
        var item = CategoriesListBox.Items.OfType<ListBoxItem>()
            .FirstOrDefault(x => x.Tag is SettingsCategory cat && cat == category);

        if (!ReferenceEquals(CategoriesListBox.SelectedItem, item))
        {
            CategoriesListBox.SelectedItem = item;
        }

        if (!_isUpdatingFromSpy)
        {
            // Scroll to section
            ScrollToCategory(category);
        }
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CategoriesListBox.SelectedItem is not ListBoxItem { Tag: SettingsCategory category })
        {
            return;
        }

        if (DataContext is not CoreViewModel core)
        {
            return;
        }

        if (core.SettingsViewModel != null && core.SettingsViewModel.SelectedCategory.Value != category)
        {
            core.SettingsViewModel.SelectedCategory.Value = category;
        }
    }

    private void ScrollToCategory(SettingsCategory category)
    {
        if (_sections == null || !_sections.TryGetValue(category, out var section))
        {
            return;
        }

        _isScrollingProgrammatically = true;
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (section.Bounds.Y != 0 || section == _sections[SettingsCategory.General])
                {
                    ContentScrollViewer.Offset = new Vector(0, section.Bounds.Y);
                }
            }
            finally
            {
                // Delay flag reset slightly to ensure scroll events from this action are ignored
                Dispatcher.UIThread.Post(() => _isScrollingProgrammatically = false, DispatcherPriority.Input);
            }
        }, DispatcherPriority.Loaded);
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_isScrollingProgrammatically)
        {
            return;
        }

        if (_sections == null)
        {
            return;
        }

        var offset = ContentScrollViewer.Offset.Y;

        SettingsCategory? bestMatch = null;

        // Sort sections by their visual Y position.
        // This ensures we iterate from top to bottom, so the "last match" 
        // is always the deepest section that satisfies the condition.
        var sortedSections = _sections.OrderBy(x => x.Value.Bounds.Y);

        foreach (var kvp in sortedSections)
        {
            if (kvp.Value.Bounds.Y <= offset)
            {
                bestMatch = kvp.Key;
            }
        }

        // If no section is above offset (e.g. at very top), default to General
        if (!bestMatch.HasValue && _sections.Count > 0)
        {
            bestMatch = SettingsCategory.General;
        }

        if (DataContext is not CoreViewModel core)
        {
            return;
        }

        if (!bestMatch.HasValue || core?.SettingsViewModel == null ||
            core.SettingsViewModel.SelectedCategory.Value == bestMatch.Value)
        {
            return;
        }

        _isUpdatingFromSpy = true;
        try
        {
            core.SettingsViewModel.SelectedCategory.Value = bestMatch.Value;
        }
        finally
        {
            _isUpdatingFromSpy = false;
        }
    }
}