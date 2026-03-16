namespace PicView.Core.Sizing;

public static class ImageSizeCalculationHelper2
{
    private const int MinTitleWidth = 250;
    private const int MaxRotationAngle = 360;
    private const int MinRotationAngle = 0;

    public static ImageSize2 GetImageSize(
        double imageWidth,
        double imageHeight,
        ScreenSize screenSize,
        double minWidth,
        double minHeight,
        double interfaceSize,
        double rotationAngle,
        double dpiScaling,
        double uiTopSize,
        double uiBottomSize,
        double galleryHeight,
        double containerWidth,
        double containerHeight)
    {
        if (imageWidth <= 0 || imageHeight <= 0 || rotationAngle > MaxRotationAngle || rotationAngle < MinRotationAngle)
        {
            return ErrorImageSize(minWidth, minHeight, interfaceSize, containerWidth);
        }

        // When in fullscreen, we need to capture the entire screen estate
        var isFullscreen = Settings.WindowProperties.Fullscreen;
        // When in maximized mode, working area and interface size needs to be taken into consideration
        var isMaximized = Settings.WindowProperties.Maximized;

        var scrollEnabled = Settings.Zoom.ScrollEnabled;
        var stretchImage = Settings.ImageScaling.StretchImage;
        var autoFit = Settings.WindowProperties.AutoFit;
        var showBottomGallery = Settings.Gallery.IsGalleryDocked;
        var showInterface = Settings.UIProperties.ShowInterface;
        var showGalleryInHiddenUI = Settings.Gallery.ShowBottomGalleryInHiddenUI;

        // Calculate the possible surrounding area and borders between the picture and window
        var borderSpaceHeight = CalculateBorderSpaceHeight(isFullscreen, uiTopSize, uiBottomSize, galleryHeight);
        var borderSpaceWidth = isFullscreen  ? 0 : screenSize.Margin;

        var workArea = CalculateWorkArea(screenSize, isFullscreen, borderSpaceWidth, borderSpaceHeight);
        var screenMargin = isFullscreen || isMaximized ? 0 : screenSize.Margin;

        var (maxAvailableWidth, maxAvailableHeight, adjustedContainerWidth, adjustedContainerHeight) =
            CalculateMaxImageSize(scrollEnabled, stretchImage, autoFit,
                rotationAngle,
                workArea.width, workArea.height, screenMargin, imageWidth, imageHeight, dpiScaling, galleryHeight,
                containerWidth, containerHeight);

        var margin = CalculateGalleryMargin(showBottomGallery,
            showInterface, showGalleryInHiddenUI, galleryHeight);

        var aspectRatio =
            CalculateAspectRatio(rotationAngle, maxAvailableWidth, maxAvailableHeight, imageWidth, imageHeight);

        double displayedWidth, displayedHeight, scrollWidth, scrollHeight;
        if (scrollEnabled)
        {
            (displayedWidth, displayedHeight, scrollWidth, scrollHeight) = CalculateScrolledImageSize(
                isFullscreen, autoFit, screenSize, imageWidth, imageHeight, aspectRatio,
                adjustedContainerWidth, containerHeight, margin
            );
        }
        else
        {
            displayedWidth = imageWidth * aspectRatio;
            displayedHeight = imageHeight * aspectRatio;
            scrollWidth = double.NaN;
            scrollHeight = double.NaN;
        }

        double windowWidth, windowHeight;
        if (Settings.WindowProperties.AutoFit)
        {
            windowWidth = displayedWidth + 2;
            windowHeight = displayedHeight + borderSpaceHeight;
        }
        else
        {
            windowWidth = windowHeight = double.NaN;
        }

        return new ImageSize2(windowWidth, windowHeight, displayedWidth, displayedHeight, scrollWidth, scrollHeight, margin,
            aspectRatio);
    }

    private static (double width, double height) CalculateWorkArea(ScreenSize screenSize, bool fullscreen,
        double borderSpaceWidth, double borderSpaceHeight)
    {
        if (fullscreen)
        {
            return (screenSize.Width, screenSize.Height);
        }

        return (screenSize.WorkingAreaWidth - borderSpaceWidth,
            screenSize.WorkingAreaHeight - borderSpaceHeight);
    }


    private static double CalculateBorderSpaceHeight(bool fullscreen, double uiTop, double uiBottom, double gallery)
        => fullscreen ? 0 : uiTop + uiBottom + gallery;

    private static (double maxWidth, double maxHeight, double containerWidth, double containerHeight)
        CalculateMaxImageSize(
            bool scrollEnabled, bool stretchImage, bool autoFit, double rotationAngle,
            double workAreaWidth, double workAreaHeight, double margin,
            double width, double height, double dpiScaling, double galleryHeight, double containerWidth,
            double containerHeight)
    {
        // Swap width and height for 90/270 degree rotations to correctly cap the size
        // against the image's effective native resolution after rotation.
        if (rotationAngle is 90 or 270)
        {
            (width, height) = (height, width);
        }
        
        if (scrollEnabled)
        {
            workAreaWidth -= SizeDefaults.ScrollbarSize * dpiScaling;
            containerWidth -= SizeDefaults.ScrollbarSize * dpiScaling;
            return (stretchImage ? workAreaWidth : Math.Min(workAreaWidth - margin, width), workAreaHeight,
                containerWidth, containerHeight);
        }

        // ReSharper disable once InvertIf
        if (autoFit)
        {
            var mw = stretchImage ? workAreaWidth - margin : Math.Min(workAreaWidth - margin, width);
            var mh = stretchImage ? workAreaHeight - margin : Math.Min(workAreaHeight - margin, height);
            return (mw, mh, containerWidth, containerHeight);
        }

        return (
            stretchImage ? containerWidth : Math.Min(containerWidth, width),
            stretchImage ? containerHeight - galleryHeight : Math.Min(containerHeight - galleryHeight, height),
            containerWidth, containerHeight
        );
    }

    private static double CalculateAspectRatio(double rotationAngle, double maxWidth, double maxHeight, double width,
        double height)
    {
        switch (rotationAngle)
        {
            case 0:
            case 180:
                return Math.Min(maxWidth / width, maxHeight / height);
            case 90:
            case 270:
                return Math.Min(maxWidth / height, maxHeight / width);
            default:
                var radians = rotationAngle * Math.PI / 180;
                var rotatedWidth = Math.Abs(width * Math.Cos(radians)) + Math.Abs(height * Math.Sin(radians));
                var rotatedHeight = Math.Abs(width * Math.Sin(radians)) + Math.Abs(height * Math.Cos(radians));
                return Math.Min(maxWidth / rotatedWidth, maxHeight / rotatedHeight);
        }
    }

    private static double CalculateGalleryMargin(bool isBottomGalleryShown, bool showInterface,
        bool showGalleryInHidden, double galleryHeight)
    {
        if (!isBottomGalleryShown)
        {
            return 0;
        }

        if (!showInterface)
        {
            return showGalleryInHidden && galleryHeight > 0 ? galleryHeight : 0;
        }

        return galleryHeight > 0 ? galleryHeight : 0;
    }

    private static (double width, double height, double scrollWidth, double scrollHeight) CalculateScrolledImageSize(
        bool fullscreen, bool autoFit, ScreenSize screenSize, double width, double height, double aspectRatio,
        double containerWidth, double origContainerHeight, double margin)
    {
        if (fullscreen)
        {
            return (width * aspectRatio, height * aspectRatio, screenSize.Width, screenSize.Height);
        }

        if (autoFit)
        {
            var imgWidth = width * aspectRatio;
            var imgHeight = height * aspectRatio;
            var sw = Math.Max(imgWidth + SizeDefaults.ScrollbarSize,
                SizeDefaults.WindowMinSize + SizeDefaults.ScrollbarSize + screenSize.Margin + 16);
            var sh = origContainerHeight - margin;
            return (imgWidth, imgHeight, sw, sh);
        }

        var cWidth = containerWidth - SizeDefaults.ScrollbarSize + 10;
        var cHeight = height / width * cWidth;
        var sWidth = containerWidth + SizeDefaults.ScrollbarSize;
        var sHeight = origContainerHeight - margin;
        return (cWidth, cHeight, sWidth, sHeight);
    }

    public static ImageSize2 GetSideBySideImageSize(
        double width,
        double height,
        double secondaryWidth,
        double secondaryHeight,
        ScreenSize screenSize,
        double minWidth,
        double minHeight,
        double interfaceSize,
        double rotationAngle,
        double dpiScaling,
        double uiTopSize,
        double uiBottomSize,
        double galleryHeight,
        double containerWidth,
        double containerHeight)
    {
        if (width <= 0 || height <= 0 || secondaryWidth <= 0 || secondaryHeight <= 0 ||
            rotationAngle > MaxRotationAngle ||
            rotationAngle < MinRotationAngle)
        {
            return ErrorImageSize(minWidth, minHeight, interfaceSize, containerWidth);
        }

        // Get sizes for both images
        var firstSize = GetImageSize(width, height, screenSize, minWidth, minHeight,
            interfaceSize, rotationAngle, dpiScaling, uiTopSize, uiBottomSize, galleryHeight,
            containerWidth,
            containerHeight);
        var secondSize = GetImageSize(secondaryWidth, secondaryHeight, screenSize, minWidth,
            minHeight, interfaceSize, rotationAngle, dpiScaling, uiTopSize, uiBottomSize,
            galleryHeight,
            containerWidth, containerHeight);

        // Determine maximum height for both images
        var xHeight = Math.Max(firstSize.Height, secondSize.Height);

        // Recalculate the widths to maintain the aspect ratio with the new maximum height
        var xWidth1 = firstSize.Width / firstSize.Height * xHeight;
        var xWidth2 = secondSize.Width / secondSize.Height * xHeight;

        // Combined width of both images
        var combinedWidth = xWidth1 + xWidth2;

        if (Settings.WindowProperties.AutoFit)
        {
            var widthPadding = Settings.ImageScaling.StretchImage ? 4 : screenSize.Margin;
            var availableWidth = screenSize.WorkingAreaWidth - widthPadding;
            var availableHeight = screenSize.WorkingAreaHeight - (widthPadding + uiBottomSize + uiTopSize);
            if (rotationAngle is 0 or 180)
            {
                // If combined width exceeds available width, scale both images down proportionally
                if (combinedWidth > availableWidth)
                {
                    var scaleFactor = availableWidth / combinedWidth;
                    xWidth1 *= scaleFactor;
                    xWidth2 *= scaleFactor;
                    xHeight *= scaleFactor;

                    combinedWidth = xWidth1 + xWidth2;
                }
            }
            else
            {
                if (combinedWidth > availableHeight)
                {
                    var scaleFactor = availableHeight / combinedWidth;
                    xWidth1 *= scaleFactor;
                    xWidth2 *= scaleFactor;
                    xHeight *= scaleFactor;

                    combinedWidth = xWidth1 + xWidth2;
                }
            }
        }
        else
        {
            if (rotationAngle is 0 or 180)
            {
                if (combinedWidth > containerWidth)
                {
                    var scaleFactor = containerWidth / combinedWidth;
                    xWidth1 *= scaleFactor;
                    xWidth2 *= scaleFactor;
                    xHeight *= scaleFactor;

                    combinedWidth = xWidth1 + xWidth2;
                }
            }
            else
            {
                if (combinedWidth > containerHeight)
                {
                    var scaleFactor = containerHeight / combinedWidth;
                    xWidth1 *= scaleFactor;
                    xWidth2 *= scaleFactor;
                    xHeight *= scaleFactor;

                    combinedWidth = xWidth1 + xWidth2;
                }
            }
        }

        double scrollWidth, scrollHeight;
        if (Settings.Zoom.ScrollEnabled)
        {
            if (Settings.WindowProperties.AutoFit)
            {
                combinedWidth -= SizeDefaults.ScrollbarSize;
                scrollWidth = combinedWidth + SizeDefaults.ScrollbarSize + 8;

                var fullscreen = Settings.WindowProperties.Fullscreen ||
                                 Settings.WindowProperties.Maximized;
                var borderSpaceHeight = fullscreen ? 0 : uiTopSize + uiBottomSize + galleryHeight;
                var workAreaHeight = screenSize.WorkingAreaHeight * dpiScaling - borderSpaceHeight;
                scrollHeight = Math.Min(xHeight,
                    Settings.ImageScaling.StretchImage ? workAreaHeight : workAreaHeight - screenSize.Margin);
            }
            else
            {
                combinedWidth -= SizeDefaults.ScrollbarSize + 8;
                scrollWidth = double.NaN;
                scrollHeight = double.NaN;
            }
        }
        else
        {
            scrollWidth = double.NaN;
            scrollHeight = double.NaN;
        }

        var margin = firstSize.Height > secondSize.Height ? firstSize.Margin : secondSize.Margin;
        return new ImageSize2(0,0, combinedWidth, xHeight, xWidth2, scrollWidth, scrollHeight,
            firstSize.AspectRatio);
    }

    private static ImageSize2 ErrorImageSize(double monitorMinWidth, double monitorMinHeight, double interfaceSize,
        double containerWidth)
        => new(0, 0, 0, 0, 0,  0, 0, 0);
}