using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.Sizing;
using ReactiveUI;

namespace PicView.Avalonia.Views;

public partial class SettingsView : UserControl
{
    private readonly Stack<TabItem?> _backStack = new();
    private readonly Stack<TabItem?> _forwardStack = new();
    private TabItem? _currentTab;
    
    public SettingsView()
    {
        InitializeComponent();
        Loaded += delegate
        {
            Height = MainTabControl.MaxHeight = ScreenHelper.GetWindowMaxHeight() - SizeDefaults.TopBorderHeight;
            if (!Settings.Theme.Dark)
            {
                MainTabControl.Background = Brushes.Transparent;
            }
            MainTabControl.SelectionChanged += TabSelectionChanged;
            PointerPressed += OnMouseButtonDown;

            if (DataContext is not MainViewModel vm)
            {
                return;
            }
            
            vm.SettingsViewModel.GoBackCommand = ReactiveCommand.Create(
                GoBack, 
                vm.SettingsViewModel.WhenAnyValue(x => x.IsBackButtonEnabled)
            );
            
            vm.SettingsViewModel.GoForwardCommand = ReactiveCommand.Create(
                GoForward, 
                vm.SettingsViewModel.WhenAnyValue(x => x.IsForwardButtonEnabled)
            );
        };
    }

    private void TabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not TabControl tabControl)
        {
            return;
        }

        if (tabControl.SelectedItem is not TabItem tabItem)
        {
            return;
        }
        
        if (_currentTab == tabItem)
        {
            return;
        }

        OnTabSelected(tabItem);
    }

    public void OnTabSelected(TabItem? selectedTab)
    {
        if (_currentTab != null)
        {
            _backStack.Push(_currentTab);
            // Clear forward stack when a new tab is selected directly
            _forwardStack.Clear();
        }
    
        _currentTab = selectedTab;
        SelectTab(_currentTab);
        UpdateNavigationButtons();
    }
    
    public void GoBack()
    {
        if (_backStack.Count <= 0)
        {
            return;
        }

        _forwardStack.Push(_currentTab);
        _currentTab = _backStack.Pop();
        SelectTab(_currentTab);
        UpdateNavigationButtons();
    }

    public void GoForward()
    {
        if (_forwardStack.Count <= 0)
        {
            return;
        }

        _backStack.Push(_currentTab);
        _currentTab = _forwardStack.Pop();
        SelectTab(_currentTab);
        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }
        
        vm.SettingsViewModel.IsBackButtonEnabled = _backStack.Count > 0;
        vm.SettingsViewModel.IsForwardButtonEnabled = _forwardStack.Count > 0;
    }

    private void SelectTab(TabItem? tab)
    {
        MainTabControl.SelectedItem = tab;
    }

    public void OnMouseButtonDown(object? sender, PointerPressedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }
        var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
        var prop = e.GetCurrentPoint(topLevel).Properties;
        if (prop.IsXButton1Pressed)  // Back button
        {
            GoBack();
        }
        else if (prop.IsXButton2Pressed)  // Forward button
        {
            GoForward();
        }
    }
}