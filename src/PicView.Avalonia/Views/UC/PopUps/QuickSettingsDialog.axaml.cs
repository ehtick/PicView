using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Functions;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using R3;

namespace PicView.Avalonia.Views.UC.PopUps;

public partial class QuickSettingsDialog : AnimatedPopUp
{
    private readonly CompositeDisposable _subscriptions = new();
    public QuickSettingsDialog()
    {
        DataContext = UIHelper.GetMainView.DataContext as MainViewModel;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Ensure we don't double-subscribe if Loaded fires multiple times
        _subscriptions.Clear();

        Observable.FromEventHandler<RoutedEventArgs>(h => AllSettingsButton.Click += h,
                h => AllSettingsButton.Click -= h)
            .SubscribeAwait(async (_, _) =>
            {
                _ = AnimatedClosing();
                await FunctionsMapper.SettingsWindow();
            })
            .AddTo(_subscriptions);
        
        Observable.FromEventHandler<RoutedEventArgs>(h => AboutButton.Click += h,
                h => AboutButton.Click -= h)
            .SubscribeAwait(async (_, _) =>
            {
                _ = AnimatedClosing();
                await FunctionsMapper.AboutWindow();
            })
            .AddTo(_subscriptions);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Loaded -= OnLoaded;
        // Dispose current subscriptions but keep the CompositeDisposable reusable
        _subscriptions.Clear();
    }
}