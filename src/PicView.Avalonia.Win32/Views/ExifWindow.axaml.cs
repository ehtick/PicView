using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.UI;
using PicView.Core.Config;
using PicView.Core.Localization;
using R3;

namespace PicView.Avalonia.Win32.Views;

public partial class ExifWindow : Window, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    public ImageInfoWindowConfig Config = new();
    public ExifWindow()
    {
        InitializeComponent();
        Task.Run(async () =>
        {
            await Config.LoadAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Config.WindowProperties.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    var left = Config.WindowProperties.Left;
                    var top = Config.WindowProperties.Top;
                    if (left.HasValue && top.HasValue)
                    {
                        Position = new PixelPoint(left.Value, top.Value);
                    }
                    var width = Config.WindowProperties.Width ?? Bounds.Width;
                    var height = Config.WindowProperties.Height ?? Bounds.Height;
                    Width = width < MinWidth ? width : MinWidth;
                    Height = height < MinHeight ? height : MinHeight;
                }
            }, DispatcherPriority.Send);
        });
        
        if (Settings.Theme.GlassTheme)
        {
            BorderRectangle.Height = 0;
            
            TopWindowBorder.Background = Brushes.Transparent;
            StarOutlineButtons.Background = Brushes.Transparent;
            RemoveRatingButton.Background = Brushes.Transparent;
            
            CloseButton.Background = Brushes.Transparent;
            CloseButton.BorderThickness = new Thickness(0);
            MinimizeButton.Background = Brushes.Transparent;
            MinimizeButton.BorderThickness = new Thickness(0);
            
            RecycleButton.BorderThickness = new Thickness(0);
            DuplicateButton.BorderThickness = new Thickness(0);
            OptimizeButton.BorderThickness = new Thickness(0);
            OpenWithButton.BorderThickness = new Thickness(0);
            LocateOnDiskButton.BorderThickness = new Thickness(0);
            
            if (!Application.Current.TryGetResource("SecondaryTextColor",
                    Application.Current.RequestedThemeVariant, out var textColor))
            {
                return;
            }

            if (textColor is not Color color)
            {
                return;
            }
            
            MinimizeButton.Foreground = new SolidColorBrush(color);
            CloseButton.Foreground = new SolidColorBrush(color);
            RecycleButton.Foreground = new SolidColorBrush(color);
            DuplicateButton.Foreground = new SolidColorBrush(color);
            OptimizeButton.Foreground = new SolidColorBrush(color);
            OpenWithButton.Foreground = new SolidColorBrush(color);
            LocateOnDiskButton.Foreground = new SolidColorBrush(color);
        }

        if (Settings.Theme.GlassTheme || !Settings.Theme.Dark)
        {
            RecycleButton.Classes.Remove("noBorderHover");
            RecycleButton.Classes.Add("hover");
            
            DuplicateButton.Classes.Remove("noBorderHover");
            DuplicateButton.Classes.Add("hover");
            
            CopyButton.Classes.Remove("noBorderHover");
            CopyButton.Classes.Add("hover");
            
            CopyFileButton.Classes.Remove("noBorderHover");
            CopyFileButton.Classes.Add("hover");
            
            PrintButton.Classes.Remove("noBorderHover");
            PrintButton.Classes.Add("hover");
            
            OptimizeButton.Classes.Remove("noBorderHover");
            OptimizeButton.Classes.Add("hover");
            
            OpenWithButton.Classes.Remove("noBorderHover");
            OpenWithButton.Classes.Add("hover");
            
            LocateOnDiskButton.Classes.Remove("noBorderHover");
            LocateOnDiskButton.Classes.Add("hover");
            
            RemoveRatingButton.Classes.Remove("noBorderHover");
            RemoveRatingButton.Classes.Add("hover");
        }
        GenericWindowHelper.GenericWindowInitialize(this, TranslationManager.Translation.ImageInfo + " - PicView");
        Loaded += delegate
        {
            ClientSizeProperty.Changed.ToObservable()
                .ObserveOn(UIHelper.GetFrameProvider)
                .Debounce(TimeSpan.FromMilliseconds(100))
                .Subscribe(size =>
                {
                    Config.WindowProperties.Width = size.NewValue.Value.Width;
                    Config.WindowProperties.Height = size.NewValue.Value.Height;
                })
                .AddTo(_disposables);
        };
        
        Closing += async delegate
        {
            Hide();
            await Config.SaveAsync();
        };
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }
    
    private void UpdateWindowPosition(object? sender, PointerReleasedEventArgs e)
    {
        if (VisualRoot is null)
        {
            return;
        }

        var hostWindow = (Window)VisualRoot;
        Config.WindowProperties.Left = hostWindow.Position.X;
        Config.WindowProperties.Top = hostWindow.Position.Y;
    }
    
    

    private void Close(object? sender, RoutedEventArgs e) => Close();

    private void Minimize(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    
    public void Dispose()
    {
        Disposable.Dispose(_disposables);
        GC.SuppressFinalize(this);
    }
}