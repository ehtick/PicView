using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ImageMagick;
using PicView.Avalonia.ImageTransformations;
using PicView.Avalonia.ImageTransformations.Rotation;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Core.Exif;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Views.UC;


public partial class ImageViewer2 : UserControl
{
    private RotationTransformer? _imageTransformer;
    private CompositeDisposable? _disposables;
    
    public ImageViewer2()
    {
        InitializeComponent();
        ImageControlHelper.TriggerScalingModeUpdate(MainImage, true);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        InitializeImageTransformer();
        AddHandler(PointerWheelChangedEvent, PreviewOnPointerWheelChanged, RoutingStrategies.Tunnel);
        AddHandler(Gestures.PointerTouchPadGestureMagnifyEvent, TouchMagnifyEvent, RoutingStrategies.Bubble);
        AddHandler(Gestures.PinchEvent, TouchMagnifyEvent, RoutingStrategies.Bubble);
    }

    public void TriggerScalingModeUpdate(bool invalidate) =>
        ImageControlHelper.TriggerScalingModeUpdate(MainImage, invalidate);

    private void TouchMagnifyEvent(object? sender, PointerDeltaEventArgs e) =>
        ZoomPanControl.ZoomWithPointerWheelCore(e.Delta.Y > 0, e.GetPosition(this));

    private async ValueTask PreviewOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is Control control)
        {
            if (control.GetVisualRoot() is Window { DataContext: MainWindowViewModel vm })
            {
                await MouseShortcuts2.HandlePointerWheelChanged(
                    e, 
                    vm, 
                    ImageScrollViewer,
                    async args => await Dispatcher.UIThread.InvokeAsync(() => ZoomIn(args)),
                    async args => await Dispatcher.UIThread.InvokeAsync(() => ZoomOut(args)));
            }
        }
    }
        

    private void InitializeImageTransformer()
    {
        if (_imageTransformer is not null)
        {
            return;
        }
        // _imageTransformer = new RotationTransformer(
        //     ImageLayoutTransformControl,
        //     MainImage,
        //     () => DataContext,
        //     () =>
        //     {
        //         ZoomPanControl.ResetZoomSlim();
        //     });
        ZoomPanControl.Initialize(DataContext);
        MainPanel.Children.Add(ZoomPanControl.ZoomPreviewer);

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
                    _ = TooltipHelper.ShowTooltipMessageContinuallyAsync($"{zoomLevel}%", true,
                        TimeSpan.FromSeconds(1));
                }
            }).AddTo(_disposables);
            
        
        Debug.Assert(Settings.ImageScaling is not null);
        Observable.EveryValueChanged(Settings.ImageScaling, s => s.ShowImageSideBySide)
            .SubscribeAwait(async (isSideBySide, c) =>
            {
                if (isSideBySide)
                {
                    SecondaryImage.IsVisible = true;
                    if (SecondaryImage.Source is null)
                    {
                        var ct = CancellationTokenSource.CreateLinkedTokenSource(c, tab.GetTabCancellation().Token);
                        await tab.ImageIterator.IterateToIndexAsync(tab.ImageIterator.CurrentIndex, ct);   
                    }
                }
                else
                {
                    SecondaryImage.IsVisible = false;
                }
            }).AddTo(_disposables);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        RemoveHandler(PointerWheelChangedEvent, PreviewOnPointerWheelChanged);
        RemoveHandler(Gestures.PointerTouchPadGestureMagnifyEvent, TouchMagnifyEvent);
        RemoveHandler(Gestures.PinchEvent, TouchMagnifyEvent);
        _disposables.Dispose();
    }

    #region Zoom
    /// <inheritdoc cref="Zoom.ZoomIn(MainViewModel)"/>
    private void ZoomIn(PointerWheelEventArgs e) =>
        ZoomPanControl.ZoomWithPointerWheel(e);

    /// <inheritdoc cref="Zoom.ZoomOut(MainViewModel)"/>
    private void ZoomOut(PointerWheelEventArgs e) =>
        ZoomPanControl.ZoomWithPointerWheel(e);

    /// <inheritdoc cref="Zoom.ZoomIn(MainViewModel)"/>
    public void ZoomIn() =>
        ZoomPanControl.ZoomIn();

    /// <inheritdoc cref="Zoom.ZoomOut(MainViewModel)"/>
    public void ZoomOut() =>
        ZoomPanControl.ZoomOut();

    /// <inheritdoc cref="Zoom.ResetZoom(bool, MainViewModel)"/>
    public void ResetZoom(bool enableAnimations = true) =>
        ZoomPanControl.ResetZoom(enableAnimations);
    
    #endregion

    #region Image Transformation
    public void Rotate(bool clockWise) => _imageTransformer?.Rotate(clockWise);
    public void Rotate(double angle) => _imageTransformer?.Rotate(angle);
    public void Flip(bool animate) => _imageTransformer?.Flip(animate);

    public void SetTransform(ExifOrientation? orientation, MagickFormat? format, bool reset = true)
    {
        if (_imageTransformer is null)
        {
            InitializeImageTransformer();
        }

        _imageTransformer.SetTransform(orientation, format, reset);
    }
        
    #endregion
}
