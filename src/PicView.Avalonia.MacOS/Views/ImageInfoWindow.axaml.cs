using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.UI;
using PicView.Core.Config;
using PicView.Core.Localization;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class ImageInfoWindow : Window, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    public readonly ImageInfoWindowConfig Config = new();
    public ImageInfoWindow() 
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
                    var width = Config.WindowProperties.Width ?? 850;
                    var height = Config.WindowProperties.Height ?? 495;
                    Width = width < MinWidth ? MinWidth : width;
                    Height = height < MinHeight ? MinHeight : height;
                }
            });
        });
        if (!Settings.Theme.Dark || Settings.Theme.GlassTheme)
        {
            WindowBorder.Background = Brushes.Transparent;
            XExifView.Background = Brushes.Transparent;
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
            if (VisualRoot is null)
            {
                return;
            }

            var hostWindow = (Window)VisualRoot;
            hostWindow?.Focus();
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
    
    public void Dispose()
    {
        Disposable.Dispose(_disposables);
        GC.SuppressFinalize(this);
    }

    ~ImageInfoWindow()
    {
        Disposable.Dispose(_disposables);
    }
}