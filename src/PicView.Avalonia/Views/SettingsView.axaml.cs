using System.Runtime.InteropServices;
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            FileAssociationsTabItem.IsEnabled = false;
        }
        Loaded += OnLoaded;
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void OnLoaded(object? sender, EventArgs e)
    {
        CloseItem.Click += (_, _) => (VisualRoot as Window)?.Close();
        SetupUI();
        AttachEventHandlers();
        SetupCommands();
    }

    private void SetupUI()
    {
        Height = MainTabControl.MaxHeight = ScreenHelper.GetWindowMaxHeight() - SizeDefaults.TopBorderHeight;
        if (!Settings.Theme.Dark)
        {
            MainTabControl.Background = Brushes.Transparent;
        }
    }

    private void AttachEventHandlers()
    {
        MainTabControl.SelectionChanged += OnTabSelectionChanged;
        PointerPressed += OnPointerPressed;
    }

    private void SetupCommands()
    {
        if (ViewModel is not { SettingsViewModel: { } svm })
        {
            return;
        }

        svm.GoBackCommand = ReactiveCommand.Create(GoBack, svm.WhenAnyValue(x => x.IsBackButtonEnabled));
        svm.GoForwardCommand = ReactiveCommand.Create(GoForward, svm.WhenAnyValue(x => x.IsForwardButtonEnabled));
    }

    private void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (MainTabControl.SelectedItem is TabItem selectedTab && _currentTab != selectedTab)
        {
            HandleTabSelection(selectedTab);
        }
    }

    private void HandleTabSelection(TabItem selectedTab)
    {
        if (_currentTab != null)
        {
            _backStack.Push(_currentTab);
            _forwardStack.Clear(); // Clear forward history
        }

        _currentTab = selectedTab;
        SelectTab(_currentTab);
        UpdateNavigationButtons();
    }

    private void GoBack()
    {
        if (!_backStack.TryPop(out var previousTab))
        {
            return;
        }

        _forwardStack.Push(_currentTab);
        _currentTab = previousTab;
        SelectTab(_currentTab);
        UpdateNavigationButtons();
    }

    private void GoForward()
    {
        if (!_forwardStack.TryPop(out var nextTab))
        {
            return;
        }

        _backStack.Push(_currentTab);
        _currentTab = nextTab;
        SelectTab(_currentTab);
        UpdateNavigationButtons();
    }

    private void SelectTab(TabItem? tab)
    {
        MainTabControl.SelectedItem = tab;
    }

    private void UpdateNavigationButtons()
    {
        if (ViewModel?.SettingsViewModel is not { } svm)
        {
            return;
        }

        svm.IsBackButtonEnabled = _backStack.Count > 0;
        svm.IsForwardButtonEnabled = _forwardStack.Count > 0;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
        var properties = e.GetCurrentPoint(topLevel).Properties;

        if (properties.IsXButton1Pressed)
        {
            GoBack();
        }

        if (properties.IsXButton2Pressed)
        {
            GoForward();
        }

        if (properties.IsRightButtonPressed)
        {
            ContextMenu.Open();
        }
    }
}