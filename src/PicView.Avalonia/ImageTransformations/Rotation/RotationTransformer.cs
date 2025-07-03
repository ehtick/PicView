using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ImageDecoding;
using PicView.Core.ImageTransformations;

namespace PicView.Avalonia.ImageTransformations.Rotation;

public class RotationTransformer(
    LayoutTransformControl imageLayoutTransformControl,
    PicBox mainImage,
    Func<object?> getDataContext,
    Action resetZoom)
{
    public void Rotate(bool clockWise)
    {
        if (getDataContext() is not MainViewModel vm || mainImage.Source is null)
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

        SetImageLayoutTransform(new RotateTransform(vm.RotationAngle));
        WindowResizing.SetSize(vm);
        mainImage.InvalidateVisual();
    }

    public void Rotate(double angle)
    {
        SetImageLayoutTransform(new RotateTransform(angle));
        WindowResizing.SetSize(getDataContext() as MainViewModel);
        mainImage.InvalidateVisual();
    }

    private void SetImageLayoutTransform(RotateTransform rotateTransform)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            imageLayoutTransformControl.LayoutTransform = rotateTransform;
        }
        else
        {
            Dispatcher.UIThread.Invoke(() =>
                imageLayoutTransformControl.LayoutTransform = rotateTransform);
        }
    }

    private ScaleTransform? _scaleTransform;
    public void Flip(bool animate)
    {
        if (getDataContext() is not MainViewModel vm || mainImage.Source is null)
        {
            return;
        }
        
        _scaleTransform ??= new ScaleTransform();

        var prevScaleX = vm.PicViewer.ScaleX;
        var newScaleX = prevScaleX == -1 ? 1 : -1;

        if (animate)
        {
            _scaleTransform.Transitions ??=
            [
                new DoubleTransition
                {
                    Property = ScaleTransform.ScaleXProperty,
                    Duration = TimeSpan.FromSeconds(.2)
                }
            ];
        }
        else
        {
            _scaleTransform.Transitions = null;
        }
        imageLayoutTransformControl.RenderTransform = _scaleTransform;
        _scaleTransform.ScaleX = newScaleX;
    }

    public void SetTransform(int scaleX, int rotationAngle)
    {
        if (getDataContext() is not MainViewModel vm)
        {
            return;
        }

        vm.PicViewer.ScaleX = scaleX;
        vm.RotationAngle = rotationAngle;
        imageLayoutTransformControl.RenderTransform = new ScaleTransform(vm.PicViewer.ScaleX, 1);
        imageLayoutTransformControl.LayoutTransform = new RotateTransform(rotationAngle);

        resetZoom?.Invoke();
    }

    public void SetTransform(EXIFHelper.EXIFOrientation? orientation, MagickFormat? format, bool reset = true)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyOrientationTransform(orientation, format, reset);
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() =>
                ApplyOrientationTransform(orientation, format, reset), DispatcherPriority.Send);
        }
    }

    private void ApplyOrientationTransform(EXIFHelper.EXIFOrientation? orientation, MagickFormat? format, bool reset)
    {
        if (Settings.Zoom.ScrollEnabled && imageLayoutTransformControl.Parent is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToHome();
        }

        if (format is MagickFormat.Heic or MagickFormat.Heif)
        {
            if (reset)
            {
                SetTransform(1, 0);
            }

            return;
        }

        switch (orientation)
        {
            case null:
            case EXIFHelper.EXIFOrientation.None:
            case EXIFHelper.EXIFOrientation.Horizontal:
                if (reset)
                {
                    SetTransform(1, 0);
                }

                break;
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
                SetTransform(-1, 90);
                break;
            case EXIFHelper.EXIFOrientation.Rotate90Cw:
                SetTransform(1, 90);
                break;
            case EXIFHelper.EXIFOrientation.MirrorHorizontalRotate90Cw:
                SetTransform(-1, 270);
                break;
            case EXIFHelper.EXIFOrientation.Rotated270Cw:
                SetTransform(1, 270);
                break;
        }
    }
}