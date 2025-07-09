using Avalonia.Controls;
using Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Localization;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class BatchResizeWindow : Window, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    public BatchResizeWindow()
    {
        InitializeComponent();
        GenericWindowHelper.GenericWindowInitialize(this, TranslationManager.Translation.BatchResize + " - PicView");
        Loaded += delegate
        {
            ClientSizeProperty.Changed.ToObservable()
                .ObserveOn(UIHelper.GetFrameProvider)
                .Subscribe(size =>
                {
                    Height = 500;
                    WindowResizing.HandleWindowResize(this, size);
                })
                .AddTo(_disposables);
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