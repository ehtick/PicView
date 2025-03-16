using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.ImageTransformations;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ImageDecoding;
using PicView.Core.ImageTransformations;

namespace PicView.Avalonia.Views;

public partial class ImageViewer : UserControl
{
    public ImageViewer()
    {
        InitializeComponent();
        TriggerScalingModeUpdate(false);
        AddHandler(PointerWheelChangedEvent, PreviewOnPointerWheelChanged, RoutingStrategies.Tunnel);
        AddHandler(Gestures.PointerTouchPadGestureMagnifyEvent, TouchMagnifyEvent, RoutingStrategies.Bubble);

        Loaded += delegate
        {
            InitializeZoom();
            LostFocus += (_, _) =>
            {
                Zoom.Release();
            };
        };
    }
    
    public void TriggerScalingModeUpdate(bool invalidate)
    {
        var scalingMode = Settings.ImageScaling.IsScalingSetToNearestNeighbor 
            ? BitmapInterpolationMode.LowQuality 
            : BitmapInterpolationMode.HighQuality;
        
        RenderOptions.SetBitmapInterpolationMode(MainImage,scalingMode);
        if (invalidate)
        {
            MainImage.InvalidateVisual();
        }
    }

    private void TouchMagnifyEvent(object? sender, PointerDeltaEventArgs e)
    {
        Zoom.ZoomTo(e.GetPosition(this), e.Delta.X > 0, DataContext as MainViewModel);
    }

    public async Task PreviewOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;
        await Main_OnPointerWheelChanged(e);
    }
    
    private async Task Main_OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (DataContext is not MainViewModel mainViewModel)
            return;

        if (Settings.Zoom.IsUsingTouchPad)
        {
            // Use touch gestures for zooming
            return;
        }
        var ctrl = e.KeyModifiers == KeyModifiers.Control;
        var shift = e.KeyModifiers == KeyModifiers.Shift;
        var reverse = e.Delta.Y < 0;
        
        if (Settings.Zoom.ScrollEnabled)
        {
            if (!shift)
            {
                if (ctrl && !Settings.Zoom.CtrlZoom)
                {
                    await LoadNextPic();
                    return;
                }
                if (ImageScrollViewer.VerticalScrollBarVisibility is ScrollBarVisibility.Visible or ScrollBarVisibility.Auto)
                {
                    if (reverse)
                    {
                        ImageScrollViewer.LineDown();
                    }
                    else
                    {
                        ImageScrollViewer.LineUp();
                    }
                }
                else
                {
                    await LoadNextPic();
                }
                return;
            }
            
        }

        if (Settings.Zoom.CtrlZoom)
        {
            if (ctrl)
            {
                if (reverse)
                {
                    ZoomOut(e);
                }
                else
                {
                    ZoomIn(e);
                }
            }
            else
            {
                await ScrollOrNavigate();
            }
        }
        else
        {
            if (ctrl)
            {
                await ScrollOrNavigate();
            }
            else
            {
                if (reverse)
                {
                    ZoomOut(e);
                }
                else
                {
                    ZoomIn(e);
                }
            }
        }
        return;

        async Task ScrollOrNavigate()
        {
            if (!Settings.Zoom.ScrollEnabled || e.KeyModifiers == KeyModifiers.Shift)
            {
                await LoadNextPic();
            }
            else
            {
                if (ImageScrollViewer.VerticalScrollBarVisibility is ScrollBarVisibility.Visible or ScrollBarVisibility.Auto)
                {
                    if (reverse)
                    {
                        ImageScrollViewer.LineDown();
                    }
                    else
                    {
                        ImageScrollViewer.LineUp();
                    }
                }
                else
                {
                    await LoadNextPic();
                }
            }
        }

        async Task LoadNextPic()
        {
            bool next;
            if (reverse)
            {
                next = Settings.Zoom.HorizontalReverseScroll;
            }
            else
            {
                next = !Settings.Zoom.HorizontalReverseScroll;
            }

            await NavigationManager.Navigate(next, mainViewModel).ConfigureAwait(false);
        }
    }

    #region Zoom

    private void InitializeZoom() => Zoom.InitializeZoom(MainBorder);

    public void ZoomIn(PointerWheelEventArgs e) => Zoom.ZoomIn(e, this, MainImage, DataContext as MainViewModel);

    public void ZoomOut(PointerWheelEventArgs e) => Zoom.ZoomOut(e, this, MainImage, DataContext as MainViewModel);

    public void ZoomIn() => Zoom.ZoomIn(DataContext as MainViewModel);

    public void ZoomOut() => Zoom.ZoomOut( DataContext as MainViewModel);
    
    public void ResetZoom(bool enableAnimations = true) => Zoom.ResetZoom(enableAnimations, DataContext as MainViewModel);
    
    #endregion

    #region Rotation and Flip

    public void Rotate(bool clockWise)
    {
        if (DataContext is not MainViewModel vm)
            return;
        if (MainImage.Source is null)
        {
            return;
        }
        if (RotationHelper.IsValidRotation(vm.RotationAngle))
        {
            var nextAngle = RotationHelper.Rotate(vm.RotationAngle, clockWise);
            vm.RotationAngle = nextAngle switch
            {
                360 => 0,
                -90 => 270,
                _ => nextAngle
            };
        }
        else
        {
            vm.RotationAngle = RotationHelper.NextRotationAngle(vm.RotationAngle, true);
        }

        var rotateTransform = new RotateTransform(vm.RotationAngle);

        if (Dispatcher.UIThread.CheckAccess())
        {
            ImageLayoutTransformControl.LayoutTransform = rotateTransform;
        }
        else
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                ImageLayoutTransformControl.LayoutTransform = rotateTransform;
            });
        }

        WindowResizing.SetSize(vm);
        MainImage.InvalidateVisual();
    }
    
    public void Rotate(double angle)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var rotateTransform = new RotateTransform(angle);
            ImageLayoutTransformControl.LayoutTransform = rotateTransform;
            
            WindowResizing.SetSize(DataContext as MainViewModel);
            MainImage.InvalidateVisual();
        });
    }

    public void Flip(bool animate)
    {
        if (DataContext is not MainViewModel vm)
            return;
        if (MainImage.Source is null)
        {
            return;
        }
        int prevScaleX;
        vm.PicViewer.ScaleX = vm.PicViewer.ScaleX == -1 ? 1 : -1;
        if (vm.PicViewer.ScaleX == 1)
        {
            prevScaleX = 1;
            vm.PicViewer.ScaleX = -1;
            vm.Translation.IsFlipped = vm.Translation.UnFlip;
        }
        else
        {
            prevScaleX = -1;
            vm.PicViewer.ScaleX = 1;
            vm.Translation.IsFlipped = vm.Translation.Flip;
        }
        
        if (animate)
        {
            var flipTransform = new ScaleTransform(prevScaleX, 1)
            {
                Transitions =
                [
                    new DoubleTransition { Property = ScaleTransform.ScaleXProperty, Duration = TimeSpan.FromSeconds(.2) },
                ]
            };
            ImageLayoutTransformControl.RenderTransform = flipTransform;
            flipTransform.ScaleX = vm.PicViewer.ScaleX;
        }
        else
        {
            var flipTransform = new ScaleTransform(vm.PicViewer.ScaleX, 1);
            ImageLayoutTransformControl.RenderTransform = flipTransform;
        }
    }
    
    public void SetTransform(int scaleX, int rotationAngle)
    {
        if (DataContext is not MainViewModel vm)
            return;

        vm.PicViewer.ScaleX = scaleX;
        vm.RotationAngle = rotationAngle;
        var flipTransform = new ScaleTransform(vm.PicViewer.ScaleX, 1);
        ImageLayoutTransformControl.RenderTransform = flipTransform;
        
        var rotateTransform = new RotateTransform(rotationAngle);
        ImageLayoutTransformControl.LayoutTransform = rotateTransform;
        
        if (Zoom.IsZoomed)
        {
            ResetZoom(false);
        }
    }

    public void SetTransform(EXIFHelper.EXIFOrientation? orientation)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set, DispatcherPriority.Send);
        }
        return;

        void Set()
        {
            if (Settings.Zoom.ScrollEnabled)
            {
                ImageScrollViewer.ScrollToHome();
            }

            switch (orientation)
            {
                case null:
                default:
                case EXIFHelper.EXIFOrientation.None:
                case EXIFHelper.EXIFOrientation.Horizontal:
                    ResetZoom();;
                    return;
                case EXIFHelper.EXIFOrientation.MirrorHorizontal:
                    SetTransform(-1, 0);
                    break;
                case EXIFHelper.EXIFOrientation.Rotate180:
                    SetTransform(1, 180);
                    break;
                case EXIFHelper.EXIFOrientation.MirrorVertical:
                    SetTransform(-1, 180);
                    break;
                case EXIFHelper.EXIFOrientation.MirrorHorizontalRotate270Cw:
                    SetTransform(-1, 90); // should be 270, but it's not working
                    break;
                case EXIFHelper.EXIFOrientation.Rotate90Cw:
                    SetTransform(1, 90);
                    break;
                case EXIFHelper.EXIFOrientation.MirrorHorizontalRotate90Cw:
                    SetTransform(-1, 270); // should be 90, but it's not working
                    break;
                case EXIFHelper.EXIFOrientation.Rotated270Cw:
                    SetTransform(1, 270);
                    break;
            }
        }
    }

    #endregion Rotation and Flip

    #region Events

    private void ImageScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        e.Handled = true;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ResetZoom();
        }
    }

    private void MainImage_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }
        if (e.ClickCount == 2)
        {
            ResetZoom();
        }
        else
        {
            Pressed(e);
        }
    }

    private void MainImage_OnPointerMoved(object? sender, PointerEventArgs e) => Zoom.Pan(e, this);

    private void Pressed(PointerEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }
        Zoom.Capture(e);
    }

    private void MainImage_OnPointerReleased(object? sender, PointerReleasedEventArgs e) => Zoom.Release();

    #endregion Events
    
}