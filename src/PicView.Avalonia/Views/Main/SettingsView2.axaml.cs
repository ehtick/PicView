using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PicView.Avalonia.Controllers;

namespace PicView.Avalonia.Views.Main;

public partial class SettingsView2 : UserControl
{
    private SettingsSearchController? _controller;

    public SettingsView2()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _controller = new SettingsSearchController(this);
        _controller.Initialize();

        ContentScrollViewer.ScrollChanged += OnScrollChanged;
        KeyDown += OnKeyDown;
        MainPanel.PointerPressed += MainPanelOnPointerPressed;
        ContentScrollViewer.PointerPressed += MainPanelOnPointerPressed;
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        ContentScrollViewer.ScrollChanged -= OnScrollChanged;
        KeyDown -= OnKeyDown;
        MainPanel.PointerPressed -= MainPanelOnPointerPressed;
        ContentScrollViewer.PointerPressed -= MainPanelOnPointerPressed;

        _controller?.Dispose();
        _controller = null;
    }
    
    private void MainPanelOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _controller?.ResetFilters();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        _controller?.HandleKeyDown(e);
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        _controller?.HandleScrollChanged(ContentScrollViewer.Offset.Y);
    }
}
