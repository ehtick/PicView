using Avalonia;
using Avalonia.Media;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Config;
using R3;

namespace PicView.Avalonia.Win32.Views;

public partial class KeybindingsWindow : GenericWindow, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private readonly KeybindingWindowConfig _config;

    public KeybindingsWindow(KeybindingWindowConfig config)
    {
        _config = config;
        InitializeComponent();
        if (Settings.Theme.GlassTheme)
        {
            TopWindowBorder.Background = Brushes.Transparent;
            TopWindowBorder.BorderThickness = new Thickness(0);
            
            CloseButton.Background = Brushes.Transparent;
            CloseButton.BorderThickness = new Thickness(0);
            MinimizeButton.Background = Brushes.Transparent;
            MinimizeButton.BorderThickness = new Thickness(0);
            TitleText.Background = Brushes.Transparent;
            
            if (!Application.Current.TryGetResource("SecondaryTextColor",
                    Application.Current.RequestedThemeVariant, out var textColor))
            {
                return;
            }

            if (textColor is not Color color)
            {
                return;
            }
            
            TitleText.Foreground = new SolidColorBrush(color);
            MinimizeButton.Foreground = new SolidColorBrush(color);
            CloseButton.Foreground = new SolidColorBrush(color);
        }
        else if (!Settings.Theme.Dark)
        {
            KeybindingsView.Background = UIHelper.GetMenuBackgroundColor();
        }
        GenericWindowHelper.KeybindingsWindowInitialize(this);

        ClientSizeProperty.Changed.ToObservable()
            .ObserveOn(UIHelper.GetFrameProvider)
            .Subscribe(UpdateWindowSize)
            .AddTo(_disposables);
        PositionChanged += (_, _) => UpdateWindowPosition();
    }

    private void UpdateWindowSize(AvaloniaPropertyChangedEventArgs<Size> size)
        => WindowFunctions.SetWindowSize(this, size, _config.WindowProperties);

    private void UpdateWindowPosition()
    {
        _config.WindowProperties.Left = Position.X;
        _config.WindowProperties.Top = Position.Y;
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}