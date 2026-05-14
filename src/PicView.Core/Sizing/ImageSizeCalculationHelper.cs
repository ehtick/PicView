namespace PicView.Core.Sizing;

public static class ImageSizeCalculationHelper
{
    public static ImageSize GetImageSize(
        double imageWidth,
        double imageHeight,
        ScreenSize screenSize,
        double containerWidth,
        double containerHeight,
        double rotationAngle,
        double uiTopSize,
        double uiBottomSize,
        double galleryWidth,
        double galleryHeight)
    {
        if (imageWidth <= 0 || imageHeight <= 0)
        {
            return new ImageSize(0, 0, 0, 0, 0,  0, 0);
        }
        
        double maxAvailableWidth, maxAvailableHeight;
        if (Settings.ImageScaling.ZoomToFit)
        {
            var (w, h) = GetMaxAvailableScreenSize(screenSize,  containerWidth, containerHeight, uiTopSize, uiBottomSize, galleryWidth, galleryHeight);
            maxAvailableWidth = w;
            maxAvailableHeight = h;
        }
        else
        {
            var (w, h) = GetMaxAvailableImageSize(screenSize, containerWidth, containerHeight,  imageWidth, imageHeight, uiTopSize, uiBottomSize, galleryWidth, galleryHeight);
            maxAvailableWidth = w;
            maxAvailableHeight = h;
        }

        var aspectRatio = CalculateAspectRatio(rotationAngle, maxAvailableWidth, maxAvailableHeight, imageWidth, imageHeight);


        double scrollWidth, scrollHeight;
        var calculatedImageWidth = imageWidth * aspectRatio;
        var calculatedImageHeight = imageHeight * aspectRatio;
        if (Settings.Zoom.ScrollEnabled)
        {            
            // TODO
            scrollWidth = double.NaN;
            scrollHeight = double.NaN;
        }
        else
        {
            scrollWidth = double.NaN;
            scrollHeight = double.NaN;
        }


        double windowWidth, windowHeight;
        if (Settings.Zoom.ScrollEnabled)
        {
            // TODO
            windowWidth = windowHeight = double.NaN;
        }
        else
        {
            windowWidth = calculatedImageWidth + galleryWidth;
            windowHeight = calculatedImageHeight + uiBottomSize + uiTopSize + galleryHeight;

        }

        var initialZoom = Settings.WindowProperties.AutoFit ? aspectRatio : 1.0;
        return new ImageSize(windowWidth, windowHeight, calculatedImageWidth, calculatedImageHeight, 
            scrollWidth, scrollHeight, initialZoom);
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

    private static (double maxWidth, double maxHeight) GetMaxAvailableScreenSize(
        ScreenSize screenSize, double containerWidth, double containerHeight, double uiTopSize, double uiBottomSize, double galleryWidth, double galleryHeight)
    {
        if (Settings.WindowProperties.Fullscreen)
        {
            return (Math.Max(0, screenSize.Width - galleryWidth), 
                Math.Max(0, screenSize.Height - galleryHeight));
        }

        if (!Settings.WindowProperties.AutoFit)
        {
            return (Math.Max(0, containerWidth - galleryWidth), Math.Max(0, containerHeight - galleryHeight));
        }
        // Calculate the absolute maximum allowed window size (Working area minus the margin)
        var maxWindowWidth = screenSize.WorkingAreaWidth - Settings.WindowProperties.Margin;
        var maxWindowHeight = screenSize.WorkingAreaHeight - Settings.WindowProperties.Margin;
            
        // The available space specifically for the *image* is the max window size minus UI elements
        var maxAvailableWidth = maxWindowWidth - galleryWidth;
        var maxAvailableHeight = maxWindowHeight - (galleryHeight + uiBottomSize + uiTopSize);

        // Ensure we don't return negative values if the UI is somehow larger than the screen bounds
        return (Math.Max(0, maxAvailableWidth), Math.Max(0, maxAvailableHeight));
    }

    private static (double maxWidth, double maxHeight) GetMaxAvailableImageSize(
        ScreenSize screenSize, double containerWidth, double containerHeight, double imageWidth, double imageHeight, double uiTopSize, double uiBottomSize, double galleryWidth, double galleryHeight)
    {
        // 1. Get the absolute maximum space the screen allows for an image
        var (screenMaxWidth, screenMaxHeight) = GetMaxAvailableScreenSize(screenSize, containerWidth, containerHeight, uiTopSize, uiBottomSize, galleryWidth, galleryHeight);

        // 2. Bound that available space by the native image resolution. 
        // This ensures the aspect ratio calculation never scales the image beyond 100%.
        var maxAvailableWidth = Math.Min(screenMaxWidth, imageWidth);
        var maxAvailableHeight = Math.Min(screenMaxHeight, imageHeight);

        return (maxAvailableWidth, maxAvailableHeight);
    }

    public static ImageSize GetSideBySideImageSize(
        double width,
        double height,
        double secondaryWidth,
        double secondaryHeight,
        ScreenSize screenSize,
        double containerWidth,
        double containerHeight,
        double rotationAngle,
        double uiTopSize,
        double uiBottomSize,
        double galleryWidth,
        double galleryHeight)
    {
        if (width <= 0 || height <= 0 || secondaryWidth <= 0 || secondaryHeight <= 0)
        {
            return new ImageSize(0, 0, 0, 0, 0, 0, 0);
        }

        // 1. Normalize the widths so both images mathematically share the largest height.
        // This guarantees they will scale evenly and utilize the maximum available container space.
        var largestHeight = Math.Max(height, secondaryHeight);
        var normalizedWidth = width * (largestHeight / height);
        var normalizedSecondaryWidth = secondaryWidth * (largestHeight / secondaryHeight);
        
        // 2. Combine the normalized widths to create our perfectly balanced virtual bounding box.
        var combinedWidth = normalizedWidth + normalizedSecondaryWidth;

        double maxAvailableWidth, maxAvailableHeight;
        if (Settings.WindowProperties.AutoFit)
        {
            var (w, h) = GetMaxAvailableScreenSize(screenSize, containerWidth, containerHeight, uiTopSize, uiBottomSize, galleryWidth, galleryHeight);
            maxAvailableWidth = w;
            maxAvailableHeight = h;
        }
        else
        {
            maxAvailableWidth = containerWidth;
            maxAvailableHeight = containerHeight;
        }
        
        // 3. Calculate the aspect ratio to fit this evenly balanced bounding box into the screen/container.
        var aspectRatio = CalculateAspectRatio(rotationAngle, maxAvailableWidth, maxAvailableHeight, combinedWidth, largestHeight);

        // 4. Apply the scaling factor to our virtual image.
        var calculatedCombinedWidth = combinedWidth * aspectRatio;
        var calculatedLargestHeight = largestHeight * aspectRatio;

        double windowWidth, windowHeight;
        if (Settings.Zoom.ScrollEnabled)
        {
            // TODO: Add scrolling logic in the future
            windowWidth = windowHeight = double.NaN;
        }
        else
        {
            windowWidth = calculatedCombinedWidth + galleryWidth;
            windowHeight = calculatedLargestHeight + uiBottomSize + uiTopSize + galleryHeight;
        }
        
        return new ImageSize(
            windowWidth, 
            windowHeight, 
            calculatedCombinedWidth, 
            calculatedLargestHeight, 
            double.NaN, // scrollWidth
            double.NaN, // scrollHeight
            aspectRatio);
    }
}