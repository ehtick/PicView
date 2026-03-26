using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using PicView.Avalonia.ImageTransformations;
using PicView.Avalonia.ImageTransformations.Rotation;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Core.Config;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Views.UC;


public partial class ImageViewer2 : UserControl
{
    private RotationTransformer2? _imageTransformer;
    private CompositeDisposable? _disposables;
    private MainWindowViewModel? _mainWindowViewModel;
    
    public ImageViewer2()
    {
        InitializeComponent();
        ImageControlHelper.TriggerScalingModeUpdate(MainImage, true);
        Loaded += OnLoaded;
        if (Application.Current.DataContext is CoreViewModel core)
        {
            _mainWindowViewModel = core.MainWindows.ActiveWindow.CurrentValue;
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        InitializeImageTransformer();
        AddHandler(PointerWheelChangedEvent, PreviewOnPointerWheelChanged, RoutingStrategies.Tunnel);
        AddHandler(PointerTouchPadGestureMagnifyEvent, TouchMagnifyEvent, RoutingStrategies.Bubble);
        AddHandler(PinchEvent, TouchMagnifyEvent, RoutingStrategies.Bubble);
    }

    public void TriggerScalingModeUpdate(bool invalidate) =>
        ImageControlHelper.TriggerScalingModeUpdate(MainImage, invalidate);

    private void TouchMagnifyEvent(object? sender, PointerDeltaEventArgs e) =>
        ZoomPanControl.ZoomWithPointerWheelCore(e.Delta.Y > 0, e.GetPosition(this));

    public async ValueTask PreviewOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (GalleryView.IsPointerOver)
        {
            return;
        }
        
        if (sender is not Control control)
        {
            return;
        }
        
        if (_mainWindowViewModel is null)
        {
            return;
        }
        await MouseShortcuts2.HandlePointerWheelChanged(
            e,
            _mainWindowViewModel, 
            ImageScrollViewer,
            async args => await Dispatcher.UIThread.InvokeAsync(() => ZoomIn(args)),
            async args => await Dispatcher.UIThread.InvokeAsync(() => ZoomOut(args)));
    }
        

    private void InitializeImageTransformer()
    {
        if (_imageTransformer is not null)
        {
            return;
        }

        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        _imageTransformer = new RotationTransformer2(
            MainTransform,
            MainImage,
            core.MainWindows.ActiveWindow.CurrentValue,
            () =>
            {
                ZoomPanControl.ResetZoomSlim();
            });
        ZoomPanControl.Initialize(ZoomPreview);

        if (DataContext is not TabViewModel tab)
        {
            return;
        }

        _disposables = new CompositeDisposable();
        Observable.EveryValueChanged(ZoomPanControl, zoom => zoom.ZoomLevel)
            .Skip(1)
            .Subscribe(zoomLevel =>
            {
                TitleManager.SetTabTitle(tab, zoomLevel);
                if (Settings.Zoom.IsShowingZoomPercentagePopup)
                {
                    _ = TooltipHelper2.ShowTooltipMessageContinuallyAsync($"{Math.Floor(zoomLevel)}%", true,
                        TimeSpan.FromSeconds(1));
                }

                ZoomPreview.Margin = HoverBar.Opacity > 0 ? new Thickness(0,0,25,(HoverBar.Bounds.Height / 2) + 25) : new Thickness(0, 0, 25, 25);
            }).AddTo(_disposables);
            
        
        Debug.Assert(Settings.ImageScaling is not null);
        Observable.EveryValueChanged(Settings.ImageScaling, s => s.ShowImageSideBySide)
            .SubscribeAwait(async (isSideBySide, c) =>
            {
                if (isSideBySide)
                {
                    if (SecondaryImage.Source is null)
                    {
                        var ct = CancellationTokenSource.CreateLinkedTokenSource(c, tab.GetTabCancellation().Token);
                        await tab.ImageIterator.IterateToIndexAsync(tab.ImageIterator.CurrentIndex, ct);   
                    }
                }
            }).AddTo(_disposables);
        
        // Correspond to change when index clicked on track
        Observable.FromEvent<EventHandler<int>, int>(
                handler => (sender, index) => handler(index),
                handler => HoverBar.ProgressBar.ClickedOnTrack += handler,
                handler => HoverBar.ProgressBar.ClickedOnTrack -= handler)
            .SubscribeAwait(async (x, _) =>
            {
                await tab.ImageIterator.SkipToIndexAsync(x, tab.GetTabCancellation()).ConfigureAwait(false);
            }, AwaitOperation.Drop)
            .AddTo(_disposables);
        // Correspond to change when index dragged on track
        // wait for a 25ms pause in changes (debounce), and then emit the last value.
        Observable.FromEvent<EventHandler<int>, int>(
                handler => (sender, index) => handler(index),
                handler => HoverBar.ProgressBar.DraggedOnTrack += handler,
                handler => HoverBar.ProgressBar.DraggedOnTrack -= handler)
            .Debounce(TimeSpan.FromMilliseconds(25)) // Debounce handles rapid events during drag
            .SubscribeAwait(async (x, _) =>
            {
                await tab.ImageIterator.SkipToIndexAsync(x, tab.GetTabCancellation()).ConfigureAwait(false);
            }, AwaitOperation.Drop)
            .AddTo(_disposables);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        RemoveHandler(PointerWheelChangedEvent, PreviewOnPointerWheelChanged);
        RemoveHandler(PointerTouchPadGestureMagnifyEvent, TouchMagnifyEvent);
        RemoveHandler(PinchEvent, TouchMagnifyEvent);
        _disposables?.Dispose();
    }

    #region Zoom
    /// <inheritdoc cref="Zoom.ZoomIn(ViewModels.MainViewModel)"/>
    private void ZoomIn(PointerWheelEventArgs e) =>
        ZoomPanControl.ZoomWithPointerWheel(e);

    /// <inheritdoc cref="Zoom.ZoomOut(ViewModels.MainViewModel)"/>
    private void ZoomOut(PointerWheelEventArgs e) =>
        ZoomPanControl.ZoomWithPointerWheel(e);

    /// <inheritdoc cref="Zoom.ZoomIn(ViewModels.MainViewModel)"/>
    public void ZoomIn() =>
        ZoomPanControl.ZoomIn();

    /// <inheritdoc cref="Zoom.ZoomOut(ViewModels.MainViewModel)"/>
    public void ZoomOut() =>
        ZoomPanControl.ZoomOut();

    /// <inheritdoc cref="Zoom.ResetZoom(bool, ViewModels.MainViewModel)"/>
    public void ResetZoom(bool enableAnimations = true) =>
        ZoomPanControl.ResetZoom(enableAnimations);
    
    #endregion

    #region Image Transformation
    public void Rotate(bool clockWise) => _imageTransformer?.Rotate(clockWise);
    public void Rotate(double angle) => _imageTransformer?.Rotate(angle);
    public void Flip(bool animate) => _imageTransformer?.Flip(animate);
        
    #endregion
}
