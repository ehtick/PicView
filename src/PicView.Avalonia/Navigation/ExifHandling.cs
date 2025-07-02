using Avalonia.Media.Imaging;
using ImageMagick;
using PicView.Avalonia.Resizing;
using PicView.Avalonia.ViewModels;
using PicView.Core.DebugTools;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;
using PicView.Core.Titles;

namespace PicView.Avalonia.Navigation;

public static class ExifHandling
{
    public static void UpdateExifValues(MainViewModel vm)
    {
        if (vm.PicViewer?.FileInfo is null || vm.PicViewer is { PixelWidth.CurrentValue: <= 0, PixelHeight.CurrentValue: <= 0 })
        {
            return;
        }
        using var magick = new MagickImage();
        
        try
        {
            if (!vm.PicViewer.FileInfo.CurrentValue.Exists)
            {
                return;
            }
            magick.Ping(vm.PicViewer.FileInfo.CurrentValue);
            var profile = magick.GetExifProfile();

            if (profile != null)
            {
                vm.Exif.DpiY.Value = profile?.GetValue(ExifTag.YResolution)?.Value.ToDouble() ?? 0;
                vm.Exif.DpiX.Value = profile?.GetValue(ExifTag.XResolution)?.Value.ToDouble() ?? 0;
                var depth = profile?.GetValue(ExifTag.BitsPerSample)?.Value;
                if (depth is not null)
                {
                    var x = depth.Aggregate(0, (current, value) => current + value);
                    vm.Exif.BitDepth.Value = x.ToString();
                }
                else
                {
                    vm.Exif.BitDepth.Value = (magick.Depth * 3).ToString();
                }
            }

            if (vm.Exif.DpiX.CurrentValue is 0 && vm.PicViewer.ImageType.CurrentValue is ImageType.Bitmap or ImageType.AnimatedGif or ImageType.AnimatedWebp)
            {
                if (vm.PicViewer.ImageSource.CurrentValue is Bitmap bmp)
                {
                    vm.Exif.DpiX.Value = bmp?.Dpi.X ?? 0;
                    vm.Exif.DpiY.Value = bmp?.Dpi.Y ?? 0;
                }
            }

            vm.Exif.Orientation.Value = vm.PicViewer.ExifOrientation.CurrentValue switch
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

            if (string.IsNullOrEmpty(vm.Exif.BitDepth.CurrentValue))
            {
                vm.Exif.BitDepth.Value = (magick.Depth * 3).ToString();
            }

            if (vm.Exif.DpiX.CurrentValue == 0 || vm.Exif.DpiY.CurrentValue == 0) // Check for zero before division
            {
                vm.Exif.PrintSizeCm.Value =
                    vm.Exif.PrintSizeInch.Value =
                        vm.Exif.SizeMp.Value =
                            vm.Exif.Resolution.Value = string.Empty;
            }
            else 
            {
                var printSizes = AspectRatioHelper.GetPrintSizes(vm.PicViewer.PixelWidth.CurrentValue,
                    vm.PicViewer.PixelHeight.CurrentValue,
                    vm.Exif.DpiX.CurrentValue,
                    vm.Exif.DpiY.CurrentValue);

                vm.Exif.PrintSizeCm.Value = printSizes.PrintSizeCm;
                vm.Exif.PrintSizeInch.Value = printSizes.PrintSizeInch;
                vm.Exif.SizeMp.Value = printSizes.SizeMp;

                vm.Exif.Resolution.Value = $"{vm.Exif.DpiX} x {vm.Exif.DpiY} {TranslationManager.Translation.Dpi}";
            }

            var gcd = ImageTitleFormatter.GCD(vm.PicViewer.PixelWidth.CurrentValue, vm.PicViewer.PixelHeight.CurrentValue);
            if (gcd != 0) // Check for zero before division
            {
                vm.Exif.AspectRatio.Value = AspectRatioHelper.GetFormattedAspectRatio(gcd, vm.PicViewer.PixelWidth.CurrentValue, vm.PicViewer.PixelHeight.CurrentValue);
            }
            else
            {
                vm.Exif.AspectRatio.Value = string.Empty; // Handle cases where gcd is 0
            }

            vm.EXIFRating = profile?.GetValue(ExifTag.Rating)?.Value ?? 0;

            var gpsValues = EXIFHelper.GetGPSValues(profile);

            if (gpsValues is not null)
            {
                vm.Exif.Latitude.Value = gpsValues[0];
                vm.Exif.Longitude.Value = gpsValues[1];

                vm.Exif.GoogleLink.Value = gpsValues[2];
                vm.Exif.BingLink.Value = gpsValues[3];
            }
            else
            {
                vm.Exif.Latitude.Value =
                    vm.Exif.Longitude.Value =
                        vm.Exif.GoogleLink.Value =
                            vm.Exif.BingLink.Value = string.Empty;
            }

            var altitude = profile?.GetValue(ExifTag.GPSAltitude)?.Value;
            vm.Exif.Altitude.Value = altitude.HasValue
                ? $"{altitude.Value.ToDouble()} {meter}"
                : string.Empty;
            var getAuthors = profile?.GetValue(ExifTag.Artist)?.Value;
            vm.Exif.Authors.Value = getAuthors ?? string.Empty;
            vm.Exif.DateTaken.Value = EXIFHelper.GetDateTaken(profile);
            vm.Exif.Copyright.Value = profile?.GetValue(ExifTag.Copyright)?.Value ?? string.Empty;
            vm.Exif.Title.Value = EXIFHelper.GetTitle(profile);
            vm.Exif.Subject.Value = EXIFHelper.GetSubject(profile);
            vm.Exif.Software.Value = profile?.GetValue(ExifTag.Software)?.Value ?? string.Empty;
            vm.Exif.ResolutionUnit.Value = EXIFHelper.GetResolutionUnit(profile);
            vm.Exif.ColorRepresentation.Value = EXIFHelper.GetColorSpace(profile);
            vm.Exif.Compression.Value = profile?.GetValue(ExifTag.Compression)?.Value.ToString() ?? string.Empty;
            vm.Exif.CompressedBitsPixel.Value = profile?.GetValue(ExifTag.CompressedBitsPerPixel)?.Value.ToString() ?? string.Empty;
            vm.Exif.CameraMaker.Value = profile?.GetValue(ExifTag.Make)?.Value ?? string.Empty;
            vm.Exif.CameraModel.Value = profile?.GetValue(ExifTag.Model)?.Value ?? string.Empty;
            vm.Exif.ExposureProgram.Value = EXIFHelper.GetExposureProgram(profile);
            vm.Exif.ExposureTime.Value = profile?.GetValue(ExifTag.ExposureTime)?.Value.ToString() ?? string.Empty;
            vm.Exif.FNumber.Value = profile?.GetValue(ExifTag.FNumber)?.Value.ToString() ?? string.Empty;
            vm.Exif.MaxAperture.Value = profile?.GetValue(ExifTag.MaxApertureValue)?.Value.ToString() ?? string.Empty;
            vm.Exif.ExposureBias.Value = profile?.GetValue(ExifTag.ExposureBiasValue)?.Value.ToString() ?? string.Empty;
            vm.Exif.DigitalZoom.Value = profile?.GetValue(ExifTag.DigitalZoomRatio)?.Value.ToString() ?? string.Empty;
            vm.Exif.FocalLength35Mm.Value = profile?.GetValue(ExifTag.FocalLengthIn35mmFilm)?.Value.ToString() ?? string.Empty;
            vm.Exif.FocalLength.Value = profile?.GetValue(ExifTag.FocalLength)?.Value.ToString() ?? string.Empty;
            vm.Exif.ISOSpeed.Value = EXIFHelper.GetISOSpeed(profile);
            vm.Exif.MeteringMode.Value = profile?.GetValue(ExifTag.MeteringMode)?.Value.ToString() ?? string.Empty;
            vm.Exif.Contrast.Value = EXIFHelper.GetContrast(profile);
            vm.Exif.Saturation.Value = EXIFHelper.GetSaturation(profile);
            vm.Exif.Sharpness.Value = EXIFHelper.GetSharpness(profile);
            vm.Exif.WhiteBalance.Value = EXIFHelper.GetWhiteBalance(profile);
            vm.Exif.FlashMode.Value = EXIFHelper.GetFlashMode(profile);
            vm.Exif.FlashEnergy.Value = profile?.GetValue(ExifTag.FlashEnergy)?.Value.ToString() ?? string.Empty;
            vm.Exif.LightSource.Value = EXIFHelper.GetLightSource(profile);
            vm.Exif.Brightness.Value = profile?.GetValue(ExifTag.BrightnessValue)?.Value.ToString() ?? string.Empty;
            vm.Exif.PhotometricInterpretation.Value = EXIFHelper.GetPhotometricInterpretation(profile);
            vm.Exif.ExifVersion.Value = EXIFHelper.GetExifVersion(profile);
            vm.Exif.LensModel.Value = profile?.GetValue(ExifTag.LensModel)?.Value ?? string.Empty;
            vm.Exif.LensMaker.Value = profile?.GetValue(ExifTag.LensMake)?.Value ?? string.Empty;
            vm.Exif.Comment.Value = EXIFHelper.GetUserComment(profile);
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(ExifHandling), nameof(UpdateExifValues), e);
        }
    }
}