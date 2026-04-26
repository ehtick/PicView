namespace PicView.Core.Sizing;

public static class ImageSizeCalculationHelper
{
    public static ImageSize GetImageSize(
        double imageWidth,
        double imageHeight,
        ScreenSize screenSize,
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
        
        var (maxAvailableWidth, maxAvailableHeight) = GetMaxAvailableScreenSize(screenSize, uiTopSize, uiBottomSize, galleryWidth, galleryHeight);

        var aspectRatio =
            CalculateAspectRatio(rotationAngle, maxAvailableWidth, maxAvailableHeight, imageWidth, imageHeight);

        double calculatedImageWidth, calculatedImageHeight, scrollWidth, scrollHeight;
        calculatedImageWidth = imageWidth * aspectRatio;
        calculatedImageHeight = imageHeight * aspectRatio;
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
        
        return new ImageSize(windowWidth, windowHeight, calculatedImageWidth, calculatedImageHeight, scrollWidth, scrollHeight,
            aspectRatio);
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

    private static (double maxWidth, double maxHeight) GetMaxAvailableScreenSize(ScreenSize screenSize, double uiTopSize, double uiBottomSize, double galleryWidth, double galleryHeight)
    {
        double maxAvailableWidth, maxAvailableHeight;
        if (Settings.WindowProperties.Fullscreen)
        {
            maxAvailableWidth = screenSize.Width - galleryWidth;
            maxAvailableHeight = screenSize.Height - galleryHeight;
        }
        else
        {
            maxAvailableWidth = screenSize.WorkingAreaWidth - galleryWidth;
            maxAvailableHeight = screenSize.WorkingAreaHeight - (galleryHeight + Settings.WindowProperties.Margin + uiBottomSize + uiTopSize);
        }
        return (maxAvailableWidth, maxAvailableHeight);
    }

    public static ImageSize GetSideBySideImageSize(
        double width,
        double height,
        double secondaryWidth,
        double secondaryHeight,
        ScreenSize screenSize,
        double rotationAngle,
        double uiTopSize,
        double uiBottomSize,
        double galleryWidth,
        double galleryHeight)
    {
        // 1. Guard clause for invalid dimensions
        if (width <= 0 || height <= 0 || secondaryWidth <= 0 || secondaryHeight <= 0)
        {
            return new ImageSize(0, 0, 0, 0, 0, 0, 0);
        }

        // 2. Treat the two side-by-side images as one large "virtual" image bounds
        var combinedWidth = width + secondaryWidth;
        var largestHeight = Math.Max(height, secondaryHeight);

        // 3. Get the maximum available screen space (same as GetImageSize)
        var (maxAvailableWidth, maxAvailableHeight) = GetMaxAvailableScreenSize(screenSize, uiTopSize, uiBottomSize, galleryWidth, galleryHeight);
        
        // 4. Calculate a single aspect ratio that fits the entire combined bounding box into the screen
        var aspectRatio = CalculateAspectRatio(rotationAngle, maxAvailableWidth, maxAvailableHeight, combinedWidth, largestHeight);

        // 5. Apply the scaling factor to our virtual image
        var calculatedCombinedWidth = combinedWidth * aspectRatio;
        var calculatedLargestHeight = largestHeight * aspectRatio;

        // 6. Calculate the final Window Width and Height (ignoring scrolling for now)
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
        
        // 7. Return the struct
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