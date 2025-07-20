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
    private readonly ImageInfoWindowConfig _config;
    public ImageInfoWindow(ImageInfoWindowConfig config)
    {
        _config = config;
        InitializeComponent();
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
                    _config.WindowProperties.Width = size.NewValue.Value.Width;
                    _config.WindowProperties.Height = size.NewValue.Value.Height;
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
            await _config.SaveAsync();
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
        _config.WindowProperties.Left = hostWindow.Position.X;
        _config.WindowProperties.Top = hostWindow.Position.Y;
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