using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Config;
using PicView.Core.Localization;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class ImageInfoWindow2 : Window, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private readonly ImageInfoWindowConfig _config;
    public ImageInfoWindow2(MainWindowViewModel viewModel)
    {
        Debug.Assert(viewModel.InfoWindow.ImageInfoWindowConfig != null);
        _config = viewModel.InfoWindow.ImageInfoWindowConfig;
        DataContext = viewModel;
        InitializeComponent();
        if (Settings.Theme.GlassTheme)
        {
            WindowBorder.Background = Brushes.Transparent;
        }
        else if (!Settings.Theme.Dark)
        {
            XExifView.Background = UIHelper.GetMenuBackgroundColor();
        }
        GenericWindowHelper.GenericWindowInitialize(this, TranslationManager.Translation.ImageInfo + " - PicView");
        Loaded += delegate
        {
            ClientSizeProperty.Changed.ToObservable()
                .ObserveOn(UIHelper.GetFrameProvider)
                .Debounce(TimeSpan.FromMilliseconds(10))
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
            var topLevel = GetTopLevel(VisualRoot);

            if (topLevel is Window hostWindow)
            {
                hostWindow.Focus();
            }
            await _config.SaveAsync();
        };
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        
        if (topLevel is Window hostWindow)
        {
            hostWindow.BeginMoveDrag(e);
        }
    }
    
    private void UpdateWindowPosition()
    {
        _config.WindowProperties.Left = Position.X;
        _config.WindowProperties.Top = Position.Y;
    }
    
    private void UpdateWindowSize(AvaloniaPropertyChangedEventArgs<Size> size)
        => WindowFunctions2.SetWindowSize(this, size, _config.WindowProperties);
    
    public void Dispose()
    {
        Disposable.Dispose(_disposables);
        GC.SuppressFinalize(this);
    }

    ~ImageInfoWindow2()
    {
        Disposable.Dispose(_disposables);
    }
}