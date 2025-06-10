using Avalonia;
using Avalonia.Media.Imaging;
using PicView.Avalonia.Functions;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Gallery;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;

namespace PicView.Avalonia.Crop;

public static class CropFunctions
{
    public static bool IsCropping {get; private set;} 
    
    /// <summary>
    /// Starts the cropping functionality by setting up the ImageCropperViewModel 
    /// and adding the CropControl to the main view.
    /// </summary>
    /// <param name="vm">The main view model instance containing image properties and state.</param>
    /// <remarks>
    /// This method checks if cropping can be enabled and if the image source is valid.
    /// If conditions are met, it configures the crop control with the appropriate dimensions
    /// and updates the view model's title and tooltip to reflect the cropping state.
    /// </remarks>
    public static void StartCropControl(MainViewModel vm)
    {
        if (!DetermineIfShouldBeEnabled(vm))
        {
            return;
        }
        if (vm?.PicViewer.ImageSource is not Bitmap bitmap)
        {
            return;
        }
        // Hide bottom gallery when entering crop mode
        if (Settings.Gallery.IsBottomGalleryShown)
        {
            vm.GalleryMode = GalleryMode.Closed;
            // Reset setting before resizing
            Settings.Gallery.IsBottomGalleryShown = false;
            WindowResizing.SetSize(vm);
            Settings.Gallery.IsBottomGalleryShown = true;
        }
        var size = new Size(vm.PicViewer.ImageWidth, vm.PicViewer.ImageHeight);
        vm.Crop = new ImageCropperViewModel(bitmap)
        {
            ImageWidth = size.Width,
            ImageHeight = size.Height,
            AspectRatio = vm.PicViewer.AspectRatio
        };
        var cropControl = new CropControl
        {
            DataContext = vm,
            Width = size.Width,
            Height = size.Height,
            Margin = new Thickness(0)
        };
        vm.CurrentView = cropControl;
        
        IsCropping = true;
        vm.PicViewer.Title = TranslationManager.Translation.CropMessage;
        vm.PicViewer.TitleTooltip = TranslationManager.Translation.CropMessage;
        
        FunctionsMapper.CloseMenus();
    }
    
    public static void CloseCropControl(MainViewModel vm)
    {
        if (Settings.Gallery.IsBottomGalleryShown)
        {
            vm.GalleryMode = GalleryMode.ClosedToBottom;
            WindowResizing.SetSize(vm);
        }

        vm.CurrentView = vm.ImageViewer;
        IsCropping = false;
        TitleManager.SetTitle(vm);
        
        // Reset image type to fix issue with animated images
        switch (vm.PicViewer.ImageType)
        {
            case ImageType.AnimatedWebp:
                vm.PicViewer.ImageType = ImageType.Bitmap;
                vm.PicViewer.ImageType = ImageType.AnimatedWebp;
                break;
            case ImageType.AnimatedGif:
                vm.PicViewer.ImageType = ImageType.Bitmap;
                vm.PicViewer.ImageType = ImageType.AnimatedGif;
                break;
        }

        vm.Crop = null;
    }

    public static bool DetermineIfShouldBeEnabled(MainViewModel vm)
    {
        if (IsCropping)
        {
            return false;
        }
        if (vm?.PicViewer.ImageSource is not Bitmap)
        {
            vm.ShouldCropBeEnabled = false;
            return false;
        }

        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            vm.ShouldCropBeEnabled = false;
            return false;
        }

        if (DialogManager.IsDialogOpen)
        {
            return false;
        }

        if (vm.IsEditableTitlebarOpen)
        {
            return false;
        }

        if (vm.RotationAngle is 0 && vm.PicViewer.ScaleX is 1)
        {
            vm.ShouldCropBeEnabled = true;
            return true;
        }
        
        vm.ShouldCropBeEnabled = false;
        return false;
    }
}
