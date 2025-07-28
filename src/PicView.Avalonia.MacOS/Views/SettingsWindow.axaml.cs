using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Config;
using PicView.Core.FileAssociations;
using PicView.Core.Localization;
using PicView.Core.MacOS.FileAssociation;
using PicView.Core.Sizing;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsWindowConfig _config;
    
    public SettingsWindow(SettingsWindowConfig config)
    {
        _config = config;
        InitializeComponent();
        Task.Run(async () =>
        {
            await _config.LoadAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_config.WindowProperties.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    var left = _config.WindowProperties.Left;
                    var top = _config.WindowProperties.Top;
                    if (left.HasValue && top.HasValue)
                    {
                        Position = new PixelPoint(left.Value, top.Value);
                    }
                }
            });
        });
        MinHeight = ScreenHelper.ScreenSize.WorkingAreaHeight switch
        {
            < 650 => 600,
            >= 650 => 700,
            _ => SizeDefaults.WindowMinSize
        };
        if (!Settings.Theme.Dark || Settings.Theme.GlassTheme)
        {
            TitleText.Background = Brushes.Transparent;
            SettingsView.Background = Brushes.Transparent;
            SettingsButton.Background = Brushes.Transparent;
        }
        Loaded += delegate
        {
            MinWidth = MaxWidth = Bounds.Width;
            Height = 500;
            Title = TranslationManager.Translation.Settings + " - PicView";

            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            PositionChanged += (_, _) => UpdateWindowPosition();
            
            GoForwardButton.Command = vm.SettingsViewModel.GoForwardCommand;
            GoBackButton.Command = vm.SettingsViewModel.GoBackCommand;
        };
        KeyDown += (_, e) =>
        {
            if (e.Key is Key.Escape)
            {
                e.Handled = true;
                MainKeyboardShortcuts.IsEscKeyEnabled = false;
                Close();
            }
        };
        
        Closing += async delegate
        {
            Hide();
            if (VisualRoot is null)
            {
                return;
            }

            var hostWindow = (Window)VisualRoot;
            hostWindow?.Focus();
            await _config.SaveAsync();
            await SaveSettingsAsync();
        };
        
        InitializeFileAssociationManager();
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
    
    private void UpdateWindowPosition()
    {
        _config.WindowProperties.Left = Position.X;
        _config.WindowProperties.Top = Position.Y;
    }
    
    private static void InitializeFileAssociationManager()
    {
        var iIFileAssociationService = new MacFileAssociationService();
        FileAssociationManager.Initialize(iIFileAssociationService);
    }
}