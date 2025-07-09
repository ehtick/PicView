using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Localization;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class EffectsWindow : Window, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    public EffectsWindow()
    {
        InitializeComponent();
        if (!Settings.Theme.Dark || Settings.Theme.GlassTheme)
        {
            XEffectsView.Background = Brushes.Transparent;
        }
        Loaded += delegate
        {
            MinWidth = MaxWidth = Bounds.Width;
            Title = $"{TranslationManager.Translation.Effects} - PicView";
            
            ClientSizeProperty.Changed.ToObservable()
                .ObserveOn(UIHelper.GetFrameProvider)
                .Subscribe(size => { WindowResizing.HandleWindowResize(this, size); })
                .AddTo(_disposables);
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
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }
    
    public void Dispose()
    {
        Disposable.Dispose(_disposables);
        GC.SuppressFinalize(this);
    }
}