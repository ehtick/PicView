using ImageMagick;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileHandling;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;
using PicView.Core.Navigation;

namespace PicView.Avalonia.UI;

public static class TitleManager
{
    /// <summary>
    ///     Sets the title of the window and the title displayed in the UI to the appropriate
    ///     value based on the current state of the application.
    /// </summary>
    /// <param name="vm">The main view model instance.</param>
    /// <remarks>Can be used to refresh the title when files are added or removed.</remarks>
    public static void SetTitle(MainViewModel vm)
    {
        if (!NavigationManager.CanNavigate(vm))
        {
            string title;
            var s = vm.PicViewer.Title;
            if (!string.IsNullOrWhiteSpace(s.GetURL()))
            {
                title = vm.PicViewer.Title.GetURL();
            }
            else if (s.Contains(TranslationManager.Translation.Base64Image))
            {
                title = TranslationManager.Translation.Base64Image;
            }
            else
            {
                title = TranslationManager.Translation.ClipboardImage!;
            }

            var singleImageWindowTitles =
                ImageTitleFormatter.GenerateTitleForSingleImage(vm.PicViewer.PixelWidth, vm.PicViewer.PixelWidth, title, vm.ZoomValue);
            vm.PicViewer.WindowTitle = singleImageWindowTitles.BaseTitle;
            vm.PicViewer.Title = singleImageWindowTitles.TitleWithAppName;
            vm.PicViewer.TitleTooltip = singleImageWindowTitles.TitleWithAppName;
            return;
        }

        if (NavigationManager.TiffNavigationInfo is not null)
        {
            SetTiffTitle(NavigationManager.TiffNavigationInfo, vm.PicViewer.PixelWidth, vm.PicViewer.PixelHeight,
                NavigationManager.GetCurrentIndex, vm.PicViewer.FileInfo, vm);
            return;
        }

        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            var imageModel1 = new ImageModel
            {
                FileInfo = vm.PicViewer.FileInfo,
                PixelWidth = vm.PicViewer.PixelWidth,
                PixelHeight = vm.PicViewer.PixelHeight
            };
            var nextFileName = NavigationManager.GetNextFileName;
            using var magickImage = new MagickImage();
            magickImage.Ping(nextFileName);
            var imageModel2 = new ImageModel
            {
                FileInfo = new FileInfo(nextFileName),
                PixelWidth = (int)magickImage.Width,
                PixelHeight = (int)magickImage.Height
            };
            SetSideBySideTitle(vm, imageModel1, imageModel2);
            return;
        }

        var windowTitles = ImageTitleFormatter.GenerateTitleStrings(vm.PicViewer.PixelWidth, vm.PicViewer.PixelHeight,
            NavigationManager.GetCurrentIndex,
            vm.PicViewer.FileInfo, vm.ZoomValue, NavigationManager.GetCollection);
        vm.PicViewer.WindowTitle = windowTitles.TitleWithAppName;
        vm.PicViewer.Title = windowTitles.BaseTitle;
        vm.PicViewer.TitleTooltip = windowTitles.FilePathTitle;
    }

    /// <summary>
    ///     Sets the title of the window and the title displayed in the UI
    ///     to a temporary "Loading..." title while the application is loading current image.
    /// </summary>
    /// <param name="vm">The main view model instance.</param>
    public static void SetLoadingTitle(MainViewModel vm)
    {
        vm.PicViewer.WindowTitle = $"{TranslationManager.Translation.Loading} - PicView";
        vm.PicViewer.Title = TranslationManager.Translation.Loading;
        vm.PicViewer.TitleTooltip = vm.PicViewer.Title;
    }

    /// <summary>
    ///     Sets the window title and UI title based on the provided image model.
    /// </summary>
    /// <param name="vm">The main view model instance.</param>
    /// <param name="imageModel">The image model containing information about the current image.</param>
    /// <remarks>If the image model or its file info is null, an error message is set as the title.</remarks>
    public static void SetTitle(MainViewModel vm, ImageModel? imageModel)
    {
        if (!ValidateImageModel(imageModel, vm))
        {
            if (vm.PicViewer.FileInfo is null)
            {
                ReturnError(vm);
                return;
            }
            imageModel = new ImageModel
            {
                FileInfo = vm.PicViewer.FileInfo,
                PixelWidth = vm.PicViewer.PixelWidth,
                PixelHeight = vm.PicViewer.PixelHeight
            };
        }

        var windowTitles = ImageTitleFormatter.GenerateTitleStrings(imageModel.PixelWidth, imageModel.PixelHeight,
            NavigationManager.GetCurrentIndex,
            imageModel.FileInfo, vm.ZoomValue, NavigationManager.GetCollection);
        vm.PicViewer.WindowTitle = windowTitles.TitleWithAppName;
        vm.PicViewer.Title = windowTitles.BaseTitle;
        vm.PicViewer.TitleTooltip = windowTitles.FilePathTitle;
    }

    /// <summary>
    ///     Sets the title of the window and the title displayed in the UI
    ///     based on the provided TIFF navigation info and image model.
    /// </summary>
    /// <param name="tiffNavigationInfo">The TIFF navigation info object.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="index">The index of the image in the list.</param>
    /// <param name="fileInfo">The FileInfo object representing the image file.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <remarks>
    ///     This method is used to set the title of the window and the title displayed in the UI
    ///     for TIFF images. The image title is generated using the TIFF navigation info and image model.
    /// </remarks>
    public static void SetTiffTitle(TiffManager.TiffNavigationInfo tiffNavigationInfo, int width, int height, int index,
        FileInfo fileInfo, MainViewModel vm)
    {
        var singeImageWindowTitles = ImageTitleFormatter.GenerateTiffTitleStrings(width, height, index, fileInfo,
            tiffNavigationInfo, 1, NavigationManager.GetCollection);
        vm.PicViewer.WindowTitle = singeImageWindowTitles.TitleWithAppName;
        vm.PicViewer.Title = singeImageWindowTitles.BaseTitle;
        vm.PicViewer.TitleTooltip = singeImageWindowTitles.BaseTitle;
    }

    /// <summary>
    ///     Sets the title of the window and the title displayed in the UI
    ///     based on the provided image model and main view model instance.
    ///     If the image is a TIFF with multiple pages, the title is generated
    ///     using the TIFF navigation info and image model. Otherwise, the single
    ///     image title is generated using the image model.
    /// </summary>
    /// <param name="imageModel">The image model containing the image information.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <remarks>
    ///     This method is used to set the title of the window and the title displayed in the UI
    ///     for TIFF images with multiple pages. If the image is not a TIFF with multiple pages,
    ///     the single image title is generated using the image model.
    /// </remarks>
    public static void TrySetTiffTitle(ImageModel? imageModel, MainViewModel vm)
    {
        if (!ValidateImageModel(imageModel, vm))
        {
            return;
        }

        if (TiffManager.GetTiffPageCount(imageModel.FileInfo.FullName) is { } pageCount and > 1)
        {
            var tiffNavigationInfo = new TiffManager.TiffNavigationInfo
            {
                CurrentPage = 0,
                PageCount = pageCount,
                Pages = TiffManager.LoadTiffPages(imageModel.FileInfo.FullName)
            };
            SetTiffTitle(tiffNavigationInfo, imageModel.PixelWidth, imageModel.PixelHeight,
                NavigationManager.GetCurrentIndex, imageModel.FileInfo, vm);
        }
        else
        {
            SetTitle(vm, imageModel);
        }
    }

    /// <summary>
    ///     Sets the title of the window and the title displayed in the UI when showing two images side by side.
    /// </summary>
    /// <param name="vm">The main view model instance.</param>
    /// <param name="imageModel1">The first image model containing the image information.</param>
    /// <param name="imageModel2">The second image model containing the image information.</param>
    /// <remarks>
    ///     This method is used to set the title of the window and the title displayed in the UI.
    ///     The title is a combination of the titles of the two images.
    /// </remarks>
    public static void SetSideBySideTitle(MainViewModel vm, ImageModel? imageModel1, ImageModel? imageModel2)
    {
        // Fix image models, which can be null caused by race conditions?
        if (!ValidateImageModel(imageModel1, vm))
        {
            if (vm.PicViewer.FileInfo is null)
            {
                return;
            }
            imageModel1 = new ImageModel
            {
                FileInfo = vm.PicViewer.FileInfo,
                PixelWidth = vm.PicViewer.PixelWidth,
                PixelHeight = vm.PicViewer.PixelHeight
            };
        }
        if (!ValidateImageModel(imageModel2, vm))
        {
            if (!NavigationManager.CanNavigate(vm))
            {
                return;
            }
            var nextFileName = NavigationManager.GetNextFileName;
            if (string.IsNullOrWhiteSpace(nextFileName))
            {
                return;
            }
            using var magickImage = new MagickImage();
            magickImage.Ping(nextFileName);
            imageModel2 = new ImageModel
            {
                FileInfo = new FileInfo(nextFileName),
                PixelWidth = (int)magickImage.Width,
                PixelHeight = (int)magickImage.Height
            };
        }
        
        var firstWindowTitles = ImageTitleFormatter.GenerateTitleStrings(imageModel1.PixelWidth,
            imageModel1.PixelHeight, NavigationManager.GetCurrentIndex,
            imageModel1.FileInfo, vm.ZoomValue, NavigationManager.GetCollection);
        var secondWindowTitles = ImageTitleFormatter.GenerateTitleStrings(imageModel2.PixelWidth,
            imageModel2.PixelHeight, NavigationManager.GetNextIndex,
            imageModel2.FileInfo, vm.ZoomValue, NavigationManager.GetCollection);
        var windowTitle = $"{firstWindowTitles.BaseTitle} \u21dc || \u21dd {secondWindowTitles.BaseTitle} - PicView";
        var title = $"{firstWindowTitles.BaseTitle} \u21dc || \u21dd  {secondWindowTitles.BaseTitle}";
        var titleTooltip = $"{firstWindowTitles.FilePathTitle} \u21dc || \u21dd  {secondWindowTitles.FilePathTitle}";
        vm.PicViewer.WindowTitle = windowTitle;
        vm.PicViewer.Title = title;
        vm.PicViewer.TitleTooltip = titleTooltip;
    }

    /// <summary>
    ///     Sets the window title and UI title to indicate no image is available.
    /// </summary>
    /// <param name="vm">The main view model instance.</param>
    /// <remarks>
    ///     This method sets the title, window title, and tooltip to a default message
    ///     indicating that no image is currently loaded or available.
    /// </remarks>
    public static void SetNoImageTitle(MainViewModel vm)
    {
        vm.PicViewer.Title = TranslationManager.Translation.NoImage;
        vm.PicViewer.WindowTitle = TranslationManager.Translation.NoImage + " - PicView";
        vm.PicViewer.TitleTooltip = TranslationManager.Translation.NoImage;
    }

    private static void ReturnError(MainViewModel vm)
    {
        vm.PicViewer.WindowTitle =
            vm.PicViewer.Title =
                vm.PicViewer.TitleTooltip = TranslationManager.GetTranslation("UnableToRender");
    }

    private static bool ValidateImageModel(ImageModel? imageModel, MainViewModel vm)
    {
        if (imageModel?.FileInfo is not null)
        {
            return true;
        }

        ReturnError(vm);
        return false;
    }
}