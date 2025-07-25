using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
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
                .Subscribe(UpdateWindowSize)
                .AddTo(_disposables);
            PositionChanged += (_, __) => UpdateWindowPosition();
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
    
    private void UpdateWindowPosition()
    {
        if (VisualRoot is null)
        {
            return;
        }

        var hostWindow = (Window)VisualRoot;
        _config.WindowProperties.Left = hostWindow.Position.X;
        _config.WindowProperties.Top = hostWindow.Position.Y;
    }
    
    private void UpdateWindowSize(AvaloniaPropertyChangedEventArgs<Size> size)
        => WindowFunctions.SetWindowSize(this, size, _config.WindowProperties);
    
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