using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC.Menus;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;

namespace PicView.Avalonia.ImageTransformations.Rotation;

public static class RotationNavigation
{
    public static async Task RotateRight(MainViewModel? vm)
    {
        if (vm is null)
        {
            return;
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() => { vm.ImageViewer.Rotate(false); });
    }

    public static async Task RotateRight(MainViewModel? vm, RotationButton rotationButton)
    {
        await RotateRight(vm);

        // Check if it should move the cursor
        if (!Settings.WindowProperties.AutoFit)
        {
            return;
        }

        await MoveCursorAfterRotation(vm, rotationButton);
    }
    
    public static async Task RotateTo(MainViewModel? vm, int angle)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            vm.ImageViewer.Rotate(angle);
        });
        vm.GlobalSettings.RotationAngle.Value = angle;
        await WindowResizing.SetSizeAsync(vm);
    }

    private static async Task MoveCursorAfterRotation(MainViewModel? vm, RotationButton rotationButton)
    {
        // Move cursor when button is clicked
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                Button? button;
                ImageMenu? menu;
                switch (rotationButton)
                {
                    case RotationButton.WindowBorderButton:
                        button = UIHelper.GetTitlebar.GetControl<Button>("RotateRightButton");
                        break;
                    case RotationButton.RotateRightButton:
                        menu = UIHelper.GetMainView.MainGrid.Children.OfType<ImageMenu>().FirstOrDefault();
                        button = menu?.GetControl<Button>("RotateRightButton");
                        break;
                    case RotationButton.RotateLeftButton:
                        menu = UIHelper.GetMainView.MainGrid.Children.OfType<ImageMenu>().FirstOrDefault();
                        button = menu?.GetControl<Button>("RotateLeftButton");
                        break;
                    default:
                        return;
                }

                if (button is null || !button.IsPointerOver)
                {
                    return;
                }

                var p = button.PointToScreen(new Point(10, 15));
                vm.PlatformService?.SetCursorPos(p.X, p.Y);
            }
            catch (Exception e)
            {
                DebugHelper.LogDebug(nameof(Rotation), nameof(MoveCursorAfterRotation), e);
            }
        });
    }

    public static async Task RotateLeft(MainViewModel? vm)
    {
        if (vm is null)
        {
            return;
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() => { vm.ImageViewer.Rotate(true); });
    }

    public static async Task RotateLeft(MainViewModel vm, RotationButton rotationButton)
    {
        await RotateLeft(vm);

        // Check if it should move the cursor
        if (!Settings.WindowProperties.AutoFit)
        {
            return;
        }

        await MoveCursorAfterRotation(vm, rotationButton);
    }

    public static void Flip(MainViewModel vm)
    {
        Dispatcher.UIThread.Invoke(() => { vm.ImageViewer.Flip(true); });
        
        if (vm.PicViewer.ScaleX.CurrentValue == 1)
        {
            vm.PicViewer.ScaleX.Value = -1;
            vm.Translation.IsFlipped.Value = vm.Translation.UnFlip.CurrentValue;
        }
        else
        {
            vm.PicViewer.ScaleX.Value = 1;
            vm.Translation.IsFlipped.Value = vm.Translation.Flip.CurrentValue;
        }
    }

    /// <summary>
    /// Navigates up or rotates the image based on current state
    /// </summary>
    public static async Task NavigateUp(MainViewModel? vm)
    {
        if (vm is null)
        {
            return;
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            GalleryNavigation.NavigateGallery(Direction.Up, vm);
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (vm.GlobalSettings.IsScrollingEnabled.CurrentValue)
            {
                vm.ImageViewer.ImageScrollViewer.LineUp();
            }
            else
            {
                vm.ImageViewer.Rotate(true);
            }
        });
    }

    /// <summary>
    /// Navigates down or rotates the image based on current state
    /// </summary>
    public static async Task NavigateDown(MainViewModel? vm)
    {
        if (vm is null)
        {
            return;
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            GalleryNavigation.NavigateGallery(Direction.Down, vm);
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (vm.GlobalSettings.IsScrollingEnabled.CurrentValue)
            {
                vm.ImageViewer.ImageScrollViewer.LineDown();
            }
            else
            {
                vm.ImageViewer.Rotate(false);
            }
        });
    }
}