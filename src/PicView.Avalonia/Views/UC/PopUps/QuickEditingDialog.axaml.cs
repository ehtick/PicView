using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Functions;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using R3;

namespace PicView.Avalonia.Views.UC.PopUps;

public partial class QuickEditingDialog : AnimatedPopUp
{
    private readonly CompositeDisposable _subscriptions = new();
    public QuickEditingDialog()
    {
        DataContext = UIHelper.GetMainView.DataContext as MainViewModel;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Ensure we don't double-subscribe if Loaded fires multiple times
        _subscriptions.Clear();
        
        Observable.FromEventHandler<RoutedEventArgs>(h => SingleResizeButton.Click += h,
                h => SingleResizeButton.Click -= h)
            .SubscribeAwait(async (_, _) =>
            {
                _ = AnimatedClosing();
               // await FunctionsMapper.ResizeImage();
            })
            .AddTo(_subscriptions);
        
        Observable.FromEventHandler<RoutedEventArgs>(h => BatchResizeButton.Click += h,
                h => BatchResizeButton.Click -= h)
            .SubscribeAwait(async (_, _) =>
            {
                _ = AnimatedClosing();
                //await FunctionsMapper.BatchResizeWindow();
            })
            .AddTo(_subscriptions);
            
        Observable.FromEventHandler<RoutedEventArgs>(h => ImageInfoButton.Click += h,
                h => ImageInfoButton.Click -= h)
            .SubscribeAwait(async (_, _) =>
            {
                _ = AnimatedClosing();
                // await FunctionsMapper.ImageInfoWindow();
            })
            .AddTo(_subscriptions);
        
        Observable.FromEventHandler<RoutedEventArgs>(h => ImageEffectsButton.Click += h,
                h => ImageEffectsButton.Click -= h)
            .SubscribeAwait(async (_, _) =>
            {
                _ = AnimatedClosing();
                // await FunctionsMapper.EffectsWindow();
            })
            .AddTo(_subscriptions);
        
        Observable.FromEventHandler<RoutedEventArgs>(h => CropButton.Click += h,
                h => CropButton.Click -= h)
            .SubscribeAwait(async (_, _) =>
            {
                _ = AnimatedClosing();
                // await FunctionsMapper.Crop();
            })
            .AddTo(_subscriptions);
        
        Observable.FromEventHandler<RoutedEventArgs>(h => SlideshowButton.Click += h,
                h => SlideshowButton.Click -= h)
            .SubscribeAwait(async (_, _) =>
            {
                await AnimatedClosing();
                // await FunctionsMapper.Slideshow();
            })
            .AddTo(_subscriptions);
                
        Observable.FromEventHandler<RoutedEventArgs>(h => ShowDockedGalleryButton.Click += h,
                h => ShowDockedGalleryButton.Click -= h)
            .SubscribeAwait(async (_, _) =>
            {
                _ = AnimatedClosing();
                // await FunctionsMapper.OpenCloseBottomGallery();
            })
            .AddTo(_subscriptions);
        
        Observable.FromEventHandler<RoutedEventArgs>(h => SideBySideButton.Click += h,
                h => SideBySideButton.Click -= h)
            .SubscribeAwait(async (_, _) =>
            {
                _ = AnimatedClosing();
                // await FunctionsMapper.SideBySide();
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