using Avalonia.Media.Imaging;
using ImageMagick;
using PicView.Avalonia.Resizing;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;
using PicView.Core.Titles;

namespace PicView.Avalonia.Navigation;

public static class ExifHandling
{
    public static void UpdateExifValues(MainViewModel vm)
    {
        if (vm.PicViewer?.FileInfo is null || vm.PicViewer is { PixelWidth: <= 0, PixelHeight: <= 0 })
        {
            return;
        }
        using var magick = new MagickImage();
        
        try
        {
            if (!vm.PicViewer.FileInfo.Exists)
            {
                return;
            }
            magick.Ping(vm.PicViewer.FileInfo);
            var profile = magick.GetExifProfile();

            if (profile != null)
            {
                vm.Exif.DpiY = profile?.GetValue(ExifTag.YResolution)?.Value.ToDouble() ?? 0;
                vm.Exif.DpiX = profile?.GetValue(ExifTag.XResolution)?.Value.ToDouble() ?? 0;
                var depth = profile?.GetValue(ExifTag.BitsPerSample)?.Value;
                if (depth is not null)
                {
                    var x = depth.Aggregate(0, (current, value) => current + value);
                    vm.Exif.BitDepth = x.ToString();
                }
                else
                {
                    vm.Exif.BitDepth = (magick.Depth * 3).ToString();
                }
            }

            if (vm.Exif.DpiX is 0 && vm.PicViewer.ImageType is ImageType.Bitmap or ImageType.AnimatedGif or ImageType.AnimatedWebp)
            {
                if (vm.PicViewer.ImageSource is Bitmap bmp)
                {
                    vm.Exif.DpiX = bmp?.Dpi.X ?? 0;
                    vm.Exif.DpiY = bmp?.Dpi.Y ?? 0;
                }
            }

            vm.Exif.Orientation = vm.PicViewer.ExifOrientation switch
            {
                EXIFHelper.EXIFOrientation.Horizontal => TranslationManager.Translation.Normal,
                EXIFHelper.EXIFOrientation.MirrorHorizontal => TranslationManager.Translation.Flipped,
                EXIFHelper.EXIFOrientation.Rotate180 => $"{TranslationManager.Translation.Rotated} 180\u00b0",
                EXIFHelper.EXIFOrientation.MirrorVertical =>
                    $"{TranslationManager.Translation.Rotated} 180\u00b0, {TranslationManager.Translation.Flipped}",
                EXIFHelper.EXIFOrientation.MirrorHorizontalRotate270Cw =>
                    $"{TranslationManager.Translation.Rotated} 270\u00b0, {TranslationManager.Translation.Flipped}",
                EXIFHelper.EXIFOrientation.Rotate90Cw => $"{TranslationManager.Translation.Rotated} 90\u00b0",
                EXIFHelper.EXIFOrientation.MirrorHorizontalRotate90Cw =>
                    $"{TranslationManager.Translation.Rotated} 90\u00b0, {TranslationManager.Translation.Flipped}",
                EXIFHelper.EXIFOrientation.Rotated270Cw => $"{TranslationManager.Translation.Rotated} 270\u00b0",
                _ => string.Empty
            };

            var meter = TranslationManager.Translation.Meter;

            if (string.IsNullOrEmpty(vm.Exif.BitDepth))
            {
                vm.Exif.BitDepth = (magick.Depth * 3).ToString();
            }

            if (vm.Exif.DpiX == 0 || vm.Exif.DpiY == 0) // Check for zero before division
            {
                vm.Exif.PrintSizeCm = vm.Exif.PrintSizeInch = vm.Exif.SizeMp = vm.Exif.Resolution = string.Empty;
            }
            else 
            {
                var printSizes = AspectRatioHelper.GetPrintSizes( vm.PicViewer.PixelWidth, vm.PicViewer.PixelHeight, vm.Exif.DpiX, vm.Exif.DpiY);

                vm.Exif.PrintSizeCm = printSizes.PrintSizeCm;
                vm.Exif.PrintSizeInch = printSizes.PrintSizeInch;
                vm.Exif.SizeMp = printSizes.SizeMp;

                vm.Exif.Resolution = $"{vm.Exif.DpiX} x {vm.Exif.DpiY} {TranslationManager.Translation.Dpi}";
            }

            var gcd = ImageTitleFormatter.GCD(vm.PicViewer.PixelWidth, vm.PicViewer.PixelHeight);
            if (gcd != 0) // Check for zero before division
            {
                vm.Exif.AspectRatio = AspectRatioHelper.GetFormattedAspectRatio(gcd, vm.PicViewer.PixelWidth, vm.PicViewer.PixelHeight);
            }
            else
            {
                vm.Exif.AspectRatio = string.Empty; // Handle cases where gcd is 0
            }

            vm.EXIFRating = profile?.GetValue(ExifTag.Rating)?.Value ?? 0;

            var gpsValues = EXIFHelper.GetGPSValues(profile);

            if (gpsValues is not null)
            {
                vm.Exif.Latitude = gpsValues[0];
                vm.Exif.Longitude = gpsValues[1];

                vm.Exif.GoogleLink = gpsValues[2];
                vm.Exif.BingLink = gpsValues[3];
            }
            else
            {
                vm.Exif.Latitude = vm.Exif.Longitude = vm.Exif.GoogleLink = vm.Exif.BingLink = string.Empty;
            }

            var altitude = profile?.GetValue(ExifTag.GPSAltitude)?.Value;
            vm.Exif.Altitude = altitude.HasValue
                ? $"{altitude.Value.ToDouble()} {meter}"
                : string.Empty;
            var getAuthors = profile?.GetValue(ExifTag.Artist)?.Value;
            vm.Exif.Authors = getAuthors ?? string.Empty;
            vm.Exif.DateTaken = EXIFHelper.GetDateTaken(profile);
            vm.Exif.Copyright = profile?.GetValue(ExifTag.Copyright)?.Value ?? string.Empty;
            vm.Exif.Title = EXIFHelper.GetTitle(profile);
            vm.Exif.Subject = EXIFHelper.GetSubject(profile);
            vm.Exif.Software = profile?.GetValue(ExifTag.Software)?.Value ?? string.Empty;
            vm.Exif.ResolutionUnit = EXIFHelper.GetResolutionUnit(profile);
            vm.Exif.ColorRepresentation = EXIFHelper.GetColorSpace(profile);
            vm.Exif.Compression = profile?.GetValue(ExifTag.Compression)?.Value.ToString() ?? string.Empty;
            vm.Exif.CompressedBitsPixel = profile?.GetValue(ExifTag.CompressedBitsPerPixel)?.Value.ToString() ?? string.Empty;
            vm.Exif.CameraMaker = profile?.GetValue(ExifTag.Make)?.Value ?? string.Empty;
            vm.Exif.CameraModel = profile?.GetValue(ExifTag.Model)?.Value ?? string.Empty;
            vm.Exif.ExposureProgram = EXIFHelper.GetExposureProgram(profile);
            vm.Exif.ExposureTime = profile?.GetValue(ExifTag.ExposureTime)?.Value.ToString() ?? string.Empty;
            vm.Exif.FNumber = profile?.GetValue(ExifTag.FNumber)?.Value.ToString() ?? string.Empty;
            vm.Exif.MaxAperture = profile?.GetValue(ExifTag.MaxApertureValue)?.Value.ToString() ?? string.Empty;
            vm.Exif.ExposureBias = profile?.GetValue(ExifTag.ExposureBiasValue)?.Value.ToString() ?? string.Empty;
            vm.Exif.DigitalZoom = profile?.GetValue(ExifTag.DigitalZoomRatio)?.Value.ToString() ?? string.Empty;
            vm.Exif.FocalLength35Mm = profile?.GetValue(ExifTag.FocalLengthIn35mmFilm)?.Value.ToString() ?? string.Empty;
            vm.Exif.FocalLength = profile?.GetValue(ExifTag.FocalLength)?.Value.ToString() ?? string.Empty;
            vm.Exif.ISOSpeed = EXIFHelper.GetISOSpeed(profile);
            vm.Exif.MeteringMode = profile?.GetValue(ExifTag.MeteringMode)?.Value.ToString() ?? string.Empty;
            vm.Exif.Contrast = EXIFHelper.GetContrast(profile);
            vm.Exif.Saturation = EXIFHelper.GetSaturation(profile);
            vm.Exif.Sharpness = EXIFHelper.GetSharpness(profile);
            vm.Exif.WhiteBalance = EXIFHelper.GetWhiteBalance(profile);
            vm.Exif.FlashMode = EXIFHelper.GetFlashMode(profile);
            vm.Exif.FlashEnergy = profile?.GetValue(ExifTag.FlashEnergy)?.Value.ToString() ?? string.Empty;
            vm.Exif.LightSource = EXIFHelper.GetLightSource(profile);
            vm.Exif.Brightness = profile?.GetValue(ExifTag.BrightnessValue)?.Value.ToString() ?? string.Empty;
            vm.Exif.PhotometricInterpretation = EXIFHelper.GetPhotometricInterpretation(profile);
            vm.Exif.ExifVersion = EXIFHelper.GetExifVersion(profile);
            vm.Exif.LensModel = profile?.GetValue(ExifTag.LensModel)?.Value ?? string.Empty;
            vm.Exif.LensMaker = profile?.GetValue(ExifTag.LensMake)?.Value ?? string.Empty;
            vm.Exif.Comment = EXIFHelper.GetUserComment(profile);
        }
        catch (Exception e)
        {
            #if DEBUG
            Console.WriteLine(e);
            _ = TooltipHelper.ShowTooltipMessageAsync(e);
            #endif
        }
    }
}