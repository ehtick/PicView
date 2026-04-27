using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.Extensions;
using PicView.Core.Localization;
using R3;

namespace PicView.Avalonia.Win32.Views;

public partial class EffectsWindow : Window, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    public EffectsWindow()
    {
        InitializeComponent();
        
        if (Settings.Theme.GlassTheme)
        {
            IconBorder.Background = Brushes.Transparent;
            IconBorder.BorderThickness = new Thickness(0);
            MinimizeButton.Background = Brushes.Transparent;
            MinimizeButton.BorderThickness = new Thickness(0);
            CloseButton.Background = Brushes.Transparent;
            CloseButton.BorderThickness = new Thickness(0);
            BorderRectangle.Height = 0;
            TitleText.Background = Brushes.Transparent;
            TitleBarPanel.Background = Brushes.Transparent;
            
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
        
        GenericWindowHelper.GenericWindowInitialize(this, StringExtensions.CombineWithPlusAppName(TranslationManager.Translation.Effects));
        Loaded += delegate
        {
            ClientSizeProperty.Changed.ToObservable()
                .ObserveOn(UIHelper.GetFrameProvider)
                .Subscribe(size =>
                {
                    WindowResizing.HandleWindowResize(this, size);
                }, DebugHelper.LogError(nameof(EffectsWindow), nameof(WindowResizing.HandleWindowResize)))
                .AddTo(_disposables);
            ClearEffectsItem.Click += delegate
            {
                EffectsView?.RemoveEffects();
            };
        };
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void Close(object? sender, RoutedEventArgs e) => Close();

    private void Minimize(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    
    public void Dispose()
    {
        Disposable.Dispose(_disposables);
        GC.SuppressFinalize(this);
    }
}