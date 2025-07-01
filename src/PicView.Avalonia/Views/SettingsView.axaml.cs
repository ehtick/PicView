using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Config;
using PicView.Core.Keybindings;
using PicView.Core.Sizing;
using R3;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace PicView.Avalonia.Views;

public partial class SettingsView : UserControl
{
    private static CompositeDisposable? _marginSubscription;
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
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        var settingsVm = vm.SettingsViewModel;
        settingsVm.InitializeNavigation(GoBack, GoForward);
        settingsVm.WindowMargin.Value = Settings.WindowProperties.Margin;
        _marginSubscription = new CompositeDisposable();
        Observable.EveryValueChanged(settingsVm.WindowMargin, x => x.Value)
            .SubscribeAwait(async (x, _) =>
            {
                Settings.WindowProperties.Margin = x;
                if (Settings.WindowProperties.AutoFit)
                {
                    await WindowResizing.SetSizeAsync(vm);
                    WindowFunctions.CenterWindowOnScreen();
                }
            }).AddTo(_marginSubscription);
        SetupUI();
        AttachEventHandlers();
        Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(SettingsConfiguration.CurrentUserSettingsPath))
            {
                _ = SaveSettingsAsync();
            }

            if (string.IsNullOrWhiteSpace(KeybindingFunctions.CurrentKeybindingsPath))
            {
                _ = KeybindingManager.UpdateKeyBindingsFile();
            }
        });
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

    public void GoBack()
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

    public void GoForward()
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

        svm.IsBackButtonEnabled.Value = _backStack.Count > 0;
        svm.IsForwardButtonEnabled.Value = _forwardStack.Count > 0;
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