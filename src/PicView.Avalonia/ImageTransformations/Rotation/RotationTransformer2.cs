using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ImageTransformations;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.ImageTransformations.Rotation;

public class RotationTransformer2(
    LayoutTransformControl imageLayoutTransformControl,
    PicBox2 mainImage,
    MainWindowViewModel vm,
    Action resetZoom)
{
    public void Rotate(bool clockWise)
    {
        if (mainImage.Source is null)
        {
            return;
        }

        var angle = vm.WindowTabs.ActiveTab.CurrentValue.RotationAngle.CurrentValue;
        if (RotationHelper.IsValidRotation(angle))
        {
            var nextAngle = RotationHelper.Rotate(angle, clockWise);
            vm.WindowTabs.ActiveTab.CurrentValue.RotationAngle.Value = nextAngle switch
            {
                360 => 0,
                -90 => 270,
                _ => nextAngle
            };
        }
        else
        {
            vm.WindowTabs.ActiveTab.CurrentValue.RotationAngle.Value =
                RotationHelper.NextRotationAngle(angle, true);
        }

        WindowResizing2.SetSize(vm, WindowResizeReason.Layout);
        mainImage.InvalidateVisual();
    }

    public void Rotate(double angle)
    {
        WindowResizing2.SetSize(vm, WindowResizeReason.Layout);
        mainImage.InvalidateVisual();
    }

    private ScaleTransform? _scaleTransform;
    public void Flip(bool animate)
    {
        if (mainImage.Source is null)
        {
            return;
        }
        
        _scaleTransform ??= new ScaleTransform();

        var prevScaleX = vm.WindowTabs.ActiveTab.CurrentValue.ScaleX.CurrentValue;
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
        vm.WindowTabs.ActiveTab.CurrentValue.ScaleX.Value = newScaleX;
    }

}