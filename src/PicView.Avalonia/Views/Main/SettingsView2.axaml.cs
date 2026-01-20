using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PicView.Avalonia.UI;
using PicView.Core.Search;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Views.Main;

public partial class SettingsView2 : UserControl
{
    private const string ClassSearchDim = "searchDim";
    private const string ClassSearchMatch = "searchMatch";

    // Cache for performance
    private readonly List<Control> _searchMatches = [];
    private List<Control>? _allSearchableControls;

    private int _currentMatchIndex = -1;
    private bool _isScrollingProgrammatically;
    private bool _isUpdatingFromSpy;
    private bool _isNavigatingSuggestionsViaKeys;
    private List<KeyValuePair<SettingsCategory, Control>>? _orderedSections;
    private Dictionary<SettingsCategory, Control>? _sectionsMap;

    private IDisposable? _subscription;

    public SettingsView2()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        FilterBox.TextChanged += OnSearchTextChanged;
        FilterBox.KeyDown += OnFilterBoxKeyDown;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        InitializeSections();
        IndexSearchableControls(); // Pre-calculate search targets

        CategoriesListBox.SelectionChanged += OnListBoxSelectionChanged;
        SuggestionsListBox.SelectionChanged += OnSuggestionsSelectionChanged;
        ContentScrollViewer.ScrollChanged += OnScrollChanged;
        KeyDown += OnKeyDown;

        if (DataContext is CoreViewModel core)
        {
            _subscription = core.SettingsViewModel.SelectedCategory.Subscribe(OnViewModelCategoryChanged);
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        CategoriesListBox.SelectionChanged -= OnListBoxSelectionChanged;
        SuggestionsListBox.SelectionChanged -= OnSuggestionsSelectionChanged;
        ContentScrollViewer.ScrollChanged -= OnScrollChanged;
        KeyDown -= OnKeyDown;
        _subscription?.Dispose();

        // Clear caches to free memory if view is destroyed
        _allSearchableControls = null;
        _searchMatches.Clear();
    }

    private void InitializeSections()
    {
        _sectionsMap = new Dictionary<SettingsCategory, Control>();
        var tempOrdered = new List<KeyValuePair<SettingsCategory, Control>>();

        foreach (var category in Enum.GetValues<SettingsCategory>())
        {
            var sectionName = $"{category}Section";
            var control = this.FindControl<Control>(sectionName);
            if (control == null)
            {
                continue;
            }

            _sectionsMap[category] = control;
            tempOrdered.Add(new KeyValuePair<SettingsCategory, Control>(category, control));
        }

        // Assuming Enum order matches visual order, otherwise we sort once here.
        // If your XAML order differs from Enum order, keep the Sort. 
        // If they are the same, you can remove OrderBy to be even faster.
        _orderedSections = tempOrdered.OrderBy(x => x.Value.Bounds.Y).ToList();
    }

    private void IndexSearchableControls()
    {
        // One-time scan of the visual tree to find all controls that can be searched.
        _allSearchableControls = [];

        if (_orderedSections == null)
        {
            return;
        }

        foreach (var (_, sectionControl) in _orderedSections)
        {
            var children = sectionControl.GetVisualDescendants().OfType<Control>();
            foreach (var child in children)
            {
                // Only index controls that have search keywords or tags
                if (child.Tag != null || !string.IsNullOrEmpty(SearchProperties.GetKeywords(child)))
                {
                    _allSearchableControls.Add(child);
                }
            }
        }
    }

    #region Event Handlers

    private void OnFilterBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (FilterBox.Text.Length > 0 && SuggestionsListBox.ItemCount > 0)
        {
            SuggestionsPopup.IsOpen = true;
        }
        
        // Navigate through suggestions if they exist
        if (SuggestionsListBox.ItemCount > 0 && (e.Key == Key.Down || e.Key == Key.Up))
        {
            _isNavigatingSuggestionsViaKeys = true;
            try
            {
                var current = SuggestionsListBox.SelectedIndex;
                var count = SuggestionsListBox.ItemCount;

                if (e.Key == Key.Down)
                {
                    // Move down, or wrap to top if at bottom (or if nothing selected)
                    if (current < count - 1)
                        SuggestionsListBox.SelectedIndex++;
                    else
                        SuggestionsListBox.SelectedIndex = 0;
                }
                else // Key.Up
                {
                    // Move up, or wrap to bottom if at top (or if nothing selected)
                    if (current > 0)
                        SuggestionsListBox.SelectedIndex--;
                    else
                        SuggestionsListBox.SelectedIndex = count - 1;
                }
                
                SuggestionsListBox.ScrollIntoView(SuggestionsListBox.SelectedItem);
            }
            finally
            {
                _isNavigatingSuggestionsViaKeys = false;
            }

            e.Handled = true;
            return;
        }
        
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            
            // If popup is open and has selection, commit it
            if (SuggestionsPopup.IsOpen && SuggestionsListBox.SelectedItem != null)
            {
                CommitSuggestion();
            }
            else
            {
                CycleToNextMatch();
            }
        }
    }
    
    private void OnSuggestionsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Ignore if change was triggered by arrow keys in TextBox
        if (_isNavigatingSuggestionsViaKeys)
        {
            return;
        }
        
        CommitSuggestion();
    }

    private void CommitSuggestion()
    {
        if (SuggestionsListBox.SelectedItem is SettingsSearchItem item &&
            DataContext is CoreViewModel { SettingsViewModel: not null } core)
        {
            // Manually trigger the selection update on the ViewModel
            core.SettingsViewModel.SelectedSuggestion.Value = item;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not CoreViewModel core)
        {
            return;
        }

        var isSearchFocused = FilterBox.IsFocused || FilterBox.IsKeyboardFocusWithin;

        // Handle Escape
        if (e.Key == Key.Escape && isSearchFocused)
        {
            if (FilterBox.Text?.Length > 0)
            {
                FilterBox.Clear();
            }
            else
            {
                ContentScrollViewer.Focus(); // Unfocus search
            }

            e.Handled = true;
            return;
        }

        // Handle Ctrl+F or '?' to focus search
        var isCtrl = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? e.KeyModifiers.HasFlag(KeyModifiers.Meta)
            : e.KeyModifiers.HasFlag(KeyModifiers.Control);

        if ((e.Key == Key.F && isCtrl) || (!isSearchFocused && e.Key == Key.OemQuestion))
        {
            FilterBox.Focus();
            e.Handled = true;
            return;
        }


        if (core.SettingsViewModel.IsOverviewVisible.CurrentValue)
        {
            return;
        }

        // Standard Scroll Navigation
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

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CategoriesListBox.SelectedItem is ListBoxItem { Tag: SettingsCategory category } &&
            DataContext is CoreViewModel core &&
            core.SettingsViewModel.SelectedCategory.Value != category)
        {
            core.SettingsViewModel.SelectedCategory.Value = category;
        }
    }

    private void OnViewModelCategoryChanged(SettingsCategory category)
    {
        // 1. Sync ListBox UI
        var item = CategoriesListBox.Items.OfType<ListBoxItem>()
            .FirstOrDefault(x => x.Tag is SettingsCategory cat && cat == category);

        if (item != null && !ReferenceEquals(CategoriesListBox.SelectedItem, item))
        {
            CategoriesListBox.SelectedItem = item;
        }

        // 2. Scroll to section (unless we are just following the user's scroll)
        if (!_isUpdatingFromSpy)
        {
            ScrollToCategory(category);
        }
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        // Optimization: Exit early if logic is running, if data missing, or if pure UI scrolling
        if (_isScrollingProgrammatically || _orderedSections == null || DataContext is not CoreViewModel core)
        {
            return;
        }

        var offset = ContentScrollViewer.Offset.Y;
        SettingsCategory? bestMatch = null;

        // Optimization: Iterate the pre-ordered list instead of Sorting every frame
        // We look for the last section whose top is above the current scroll offset
        foreach (var kvp in _orderedSections)
        {
            if (kvp.Value.Bounds.Y <= offset + 10) // +10 buffer for better feel
            {
                bestMatch = kvp.Key;
            }
            else
            {
                // Since list is ordered by Y, once we pass the offset, we can stop
                break;
            }
        }

        // Default to General if at top
        if (!bestMatch.HasValue && _orderedSections.Count > 0)
        {
            bestMatch = SettingsCategory.General;
        }

        // Only update ViewModel if value changed to avoid reactive loops
        if (!bestMatch.HasValue || core.SettingsViewModel.SelectedCategory.Value == bestMatch.Value)
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

    #endregion

    #region Search Logic

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var searchText = FilterBox.Text;

        // Auto-switch to list view if searching
        if (DataContext is CoreViewModel core &&
            !string.IsNullOrWhiteSpace(searchText) &&
            core.SettingsViewModel.IsOverviewVisible.Value)
        {
            core.SettingsViewModel.IsOverviewVisible.Value = false;
        }

        PerformSearch(searchText);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            // Post to UI thread to allow layout to update classes/bounds before scrolling
            Dispatcher.UIThread.Post(ScrollToNearestMatch, DispatcherPriority.Input);
        }
    }

    private void PerformSearch(string? query)
    {
        if (_sectionsMap == null)
        {
            return;
        }

        _searchMatches.Clear();
        _currentMatchIndex = -1;

        // Lazy load index if null (edge case)
        if (_allSearchableControls == null)
        {
            IndexSearchableControls();
        }

        var isQueryEmpty = string.IsNullOrWhiteSpace(query);

        if (isQueryEmpty)
        {
            // Fast cleanup path
            if (_allSearchableControls == null)
            {
                return;
            }

            foreach (var child in _allSearchableControls)
            {
                child.Classes.Remove(ClassSearchMatch);
                child.Classes.Remove(ClassSearchDim);
            }

            return;
        }

        // Optimized Search Path
        foreach (var child in _allSearchableControls!)
        {
            var isMatch = IsControlMatch(child, query!);

            if (isMatch)
            {
                child.Classes.Remove(ClassSearchDim);
                child.Classes.Add(ClassSearchMatch);
                _searchMatches.Add(child);
            }
            else
            {
                child.Classes.Remove(ClassSearchMatch);
                child.Classes.Add(ClassSearchDim);
            }
        }
    }

    private static bool IsControlMatch(Control child, string query)
    {
        var keywordsString = SearchProperties.GetKeywords(child);

        // Fallback to Tag if no Attached Property
        if (string.IsNullOrEmpty(keywordsString) && child.Tag is string tag)
        {
            keywordsString = tag;
        }

        if (string.IsNullOrEmpty(keywordsString))
        {
            return false;
        }

        // Simple contains check (case-insensitive)
        return keywordsString.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Scrolling Helpers

    private void CycleToNextMatch()
    {
        if (_searchMatches.Count == 0)
        {
            return;
        }

        _currentMatchIndex = (_currentMatchIndex + 1) % _searchMatches.Count;
        ScrollToControl(_searchMatches[_currentMatchIndex]);
    }

    private void ScrollToNearestMatch()
    {
        if (_searchMatches.Count == 0)
        {
            return;
        }

        var viewportCenter = ContentScrollViewer.Viewport.Height / 2;

        var bestMatch = _searchMatches.MinBy(match =>
        {
            var relativePoint = match.TranslatePoint(new Point(0, 0), ContentScrollViewer);
            if (!relativePoint.HasValue)
            {
                return double.MaxValue;
            }

            var elementCenter = relativePoint.Value.Y + match.Bounds.Height / 2;
            return Math.Abs(elementCenter - viewportCenter);
        });

        if (bestMatch == null)
        {
            return;
        }

        _currentMatchIndex = _searchMatches.IndexOf(bestMatch);
        ScrollToControl(bestMatch);
    }

    private void ScrollToCategory(SettingsCategory category)
    {
        if (_sectionsMap == null || !_sectionsMap.TryGetValue(category, out var section))
        {
            return;
        }

        // Avoid scrolling if the section is practically already at the top (within tolerance)
        if (Math.Abs(ContentScrollViewer.Offset.Y - section.Bounds.Y) < 1.0)
        {
            return;
        }

        PerformProgrammaticScroll(section.Bounds.Y);
    }

    private void ScrollToControl(Control target)
    {
        var relativePoint = target.TranslatePoint(new Point(0, 0), ContentScrollViewer);
        if (!relativePoint.HasValue)
        {
            return;
        }

        var targetOffset = ContentScrollViewer.Offset.Y + relativePoint.Value.Y
                           - ContentScrollViewer.Viewport.Height / 2
                           + target.Bounds.Height / 2;

        PerformProgrammaticScroll(targetOffset, target);
    }

    private void PerformProgrammaticScroll(double offset, Control? targetForCategoryUpdate = null)
    {
        _isScrollingProgrammatically = true;
        try
        {
            ContentScrollViewer.Offset = new Vector(0, offset);

            if (targetForCategoryUpdate != null)
            {
                UpdateCategoryForMatch(targetForCategoryUpdate);
            }
        }
        finally
        {
            // Dispatch reset to ensure we don't catch the ScrollChanged event generated by this action
            Dispatcher.UIThread.Post(() => _isScrollingProgrammatically = false, DispatcherPriority.Input);
        }
    }

    private void UpdateCategoryForMatch(Control target)
    {
        if (_sectionsMap == null || DataContext is not CoreViewModel core)
        {
            return;
        }

        foreach (var (category, section) in _sectionsMap)
        {
            if (section != target && !section.IsVisualAncestorOf(target))
            {
                continue;
            }

            if (core.SettingsViewModel.SelectedCategory.Value == category)
            {
                return;
            }

            _isUpdatingFromSpy = true;
            try
            {
                core.SettingsViewModel.SelectedCategory.Value = category;
            }
            finally
            {
                _isUpdatingFromSpy = false;
            }

            return;
        }
    }

    #endregion
}