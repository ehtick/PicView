using System.Globalization;
using ImageMagick;
using PicView.Core.DebugTools;
using PicView.Core.Exif;
using PicView.Core.Localization;
using PicView.Core.ProcessHandling;
using PicView.Core.Sizing;
using PicView.Core.Titles;
using R3;

namespace PicView.Core.ViewModels;

public class ExifViewModel : IDisposable
{
    public ExifViewModel()
    {
        OpenGoogleLinkCommand = new ReactiveCommand(OpenGoogleMaps);
        OpenBingLinkCommand = new ReactiveCommand(OpenBingMaps);

        SetExifRating0Command = new ReactiveCommand<string>(async (s, _) => await SetRating(s, 0));
        SetExifRating1Command = new ReactiveCommand<string>(async (s, _) => await SetRating(s, 1));
        SetExifRating2Command = new ReactiveCommand<string>(async (s, _) => await SetRating(s, 2));
        SetExifRating3Command = new ReactiveCommand<string>(async (s, _) => await SetRating(s, 3));
        SetExifRating4Command = new ReactiveCommand<string>(async (s, _) => await SetRating(s, 4));
        SetExifRating5Command = new ReactiveCommand<string>(async (s, _) => await SetRating(s, 5));

        ResolutionUnits = new BindableReactiveProperty<string[]>([
            string.Empty,
            TranslationManager.Translation.None ?? string.Empty,
            TranslationManager.Translation.Inches ?? string.Empty,
            TranslationManager.Translation.Centimeters ?? string.Empty
        ]);

        ColorRepresentations = new BindableReactiveProperty<string[]>([
            string.Empty,
            "sRGB",
            "Adobe RGB",
            TranslationManager.Translation.Uncalibrated ?? "Uncalibrated"
        ]);

        Compressions = new BindableReactiveProperty<string[]>([
            string.Empty,
            TranslationManager.GetTranslation("Uncompressed"),
            "CCITT 1D",
            "T4/Group 3 Fax",
            "T6/Group 4 Fax",
            "LZW",
            "JPEG (old-style)",
            "JPEG",
            "Adobe Deflate",
            "JBIG B&W",
            "JBIG Color",
            "JPEG",
            "Kodak 262",
            "Next",
            "Sony ARW Compressed",
            "Packed RAW",
            "Samsung SRW Compressed",
            "CCIRLEW",
            "Samsung SRW Compressed 2",
            "PackBits",
            "Thunderscan",
            "Kodak KDC Compressed",
            "IT8CTPAD",
            "IT8LW",
            "IT8MP",
            "IT8BL",
            "PixarFilm",
            "PixarLog",
            "Deflate",
            "DCS",
            "Aperio JPEG 2000 YCbCr",
            "Aperio JPEG 2000 RGB",
            "JBIG",
            "SGILog",
            "SGILog24",
            "JPEG 2000",
            "Nikon NEF Compressed",
            "JBIG2 TIFF FX",
            "Microsoft Document Imaging (MDI)\n Binary Level Codec",
            "Microsoft Document Imaging (MDI)\n Progressive Transform Codec",
            "Microsoft Document Imaging (MDI) Vector",
            "ESRI Lerc",
            "Lossy JPEG",
            "LZMA2",
            "Zstd (old)",
            "WebP (old)",
            "PNG",
            "JPEG XR",
            "Zstd",
            "WebP",
            "JPEG XL (old)",
            "JPEG XL",
            "Kodak DCR Compressed",
            "Pentax PEF Compressed"
        ]);
    }
    
    public void UpdateExifValues(FileInfo fileInfo, ExifOrientation orientation, int pixelWidth, int pixelHeight, MagickImage? magick = null)
    {
        var shouldDispose = magick != null;

        try
        {
            if (fileInfo is null || !fileInfo.Exists)
            {
                return;
            }

            if (magick is null)
            {
                magick = new MagickImage();
                magick.Ping(fileInfo);
            }

            var profile = magick.GetExifProfile();

            if (profile != null)
            {
                DpiY.Value = profile?.GetValue(ExifTag.YResolution)?.Value.ToDouble() ?? 0;
                DpiX.Value = profile?.GetValue(ExifTag.XResolution)?.Value.ToDouble() ?? 0;
                var depth = profile?.GetValue(ExifTag.BitsPerSample)?.Value;
                if (depth is not null)
                {
                    var x = depth.Aggregate(0, (current, value) => current + value);
                    BitDepth.Value = x.ToString();
                }
                else
                {
                    BitDepth.Value = (magick.Depth * 3).ToString();
                }
            }

            Orientation.Value = orientation switch
            {
                ExifOrientation.Horizontal => TranslationManager.Translation.Normal,
                ExifOrientation.MirrorHorizontal => TranslationManager.Translation.Flipped,
                ExifOrientation.Rotate180 => $"{TranslationManager.Translation.Rotated} 180\u00b0",
                ExifOrientation.MirrorVertical =>
                    $"{TranslationManager.Translation.Rotated} 180\u00b0, {TranslationManager.Translation.Flipped}",
                ExifOrientation.MirrorHorizontalRotate270Cw =>
                    $"{TranslationManager.Translation.Rotated} 270\u00b0, {TranslationManager.Translation.Flipped}",
                ExifOrientation.Rotate90Cw => $"{TranslationManager.Translation.Rotated} 90\u00b0",
                ExifOrientation.MirrorHorizontalRotate90Cw =>
                    $"{TranslationManager.Translation.Rotated} 90\u00b0, {TranslationManager.Translation.Flipped}",
                ExifOrientation.Rotated270Cw => $"{TranslationManager.Translation.Rotated} 270\u00b0",
                _ => string.Empty
            };

            var meter = TranslationManager.Translation.Meter;

            if (string.IsNullOrEmpty(BitDepth.CurrentValue))
            {
                BitDepth.Value = (magick.Depth * 3).ToString();
            }

            if (DpiX.CurrentValue == 0 || DpiY.CurrentValue == 0) // Check for zero before division
            {
                PrintSizeCm.Value =
                    PrintSizeInch.Value =
                        SizeMp.Value =
                            Resolution.Value = string.Empty;
            }
            else
            {
                var printSizes =
                    PrintSizing.GetPrintSizes(pixelWidth, pixelHeight, DpiX.CurrentValue, DpiY.CurrentValue);

                PrintSizeCm.Value = printSizes.PrintSizeCm;
                PrintSizeInch.Value = printSizes.PrintSizeInch;
                SizeMp.Value = printSizes.SizeMp;

                Resolution.Value = $"{DpiX} x {DpiY} {TranslationManager.Translation.Dpi}";
            }

            var gcd = ImageTitleFormatter.GCD(pixelWidth, pixelHeight);
            if (gcd != 0) // Check for zero before division
            {
                AspectRatio.Value = ImageTitleFormatter.GetFormattedAspectRatio(gcd, pixelWidth, pixelHeight);
            }
            else
            {
                AspectRatio.Value = string.Empty; // Handle cases where gcd is 0
            }

            ExifRating.Value = profile?.GetValue(ExifTag.Rating)?.Value ?? 0;

            var gpsValues = GpsHelper.GetGpsValues(profile);

            if (gpsValues is not null)
            {
                Latitude.Value = gpsValues[0];
                Longitude.Value = gpsValues[1];

                GoogleLink.Value = gpsValues[2];
                BingLink.Value = gpsValues[3];
            }
            else
            {
                Latitude.Value =
                    Longitude.Value =
                        GoogleLink.Value =
                            BingLink.Value = string.Empty;
            }

            var altitude = profile?.GetValue(ExifTag.GPSAltitude)?.Value;
            Altitude.Value = altitude.HasValue
                ? $"{altitude.Value.ToDouble()} {meter}"
                : string.Empty;
            var getAuthors = profile?.GetValue(ExifTag.Artist)?.Value;
            Authors.Value = getAuthors ?? string.Empty;
            DateTaken.Value = ExifReader.GetDateTaken(profile);
            Copyright.Value = profile?.GetValue(ExifTag.Copyright)?.Value ?? string.Empty;
            Title.Value = ExifReader.GetTitle(profile);
            Subject.Value = ExifReader.GetSubject(profile);
            Software.Value = profile?.GetValue(ExifTag.Software)?.Value ?? string.Empty;
            ResolutionUnit.Value = ExifReader.GetResolutionUnit(profile);
            ColorRepresentation.Value = profile?.GetValue(ExifTag.ColorSpace)?.Value ?? null;
            Compression.Value = profile?.GetValue(ExifTag.Compression)?.Value ?? null;
            CompressedBitsPixel.Value =
                profile?.GetValue(ExifTag.CompressedBitsPerPixel)?.Value.ToString() ?? string.Empty;
            CameraMaker.Value = profile?.GetValue(ExifTag.Make)?.Value ?? string.Empty;
            CameraModel.Value = profile?.GetValue(ExifTag.Model)?.Value ?? string.Empty;
            ExposureProgram.Value = ExifReader.GetExposureProgram(profile);
            ExposureTime.Value = profile?.GetValue(ExifTag.ExposureTime)?.Value.ToString() ?? string.Empty;
            FNumber.Value = profile?.GetValue(ExifTag.FNumber)?.Value.ToString() ?? string.Empty;
            MaxAperture.Value = profile?.GetValue(ExifTag.MaxApertureValue)?.Value.ToString() ?? string.Empty;
            ExposureBias.Value = profile?.GetValue(ExifTag.ExposureBiasValue)?.Value.ToString() ?? string.Empty;
            DigitalZoom.Value = profile?.GetValue(ExifTag.DigitalZoomRatio)?.Value.ToString() ?? string.Empty;
            FocalLength35Mm.Value = profile?.GetValue(ExifTag.FocalLengthIn35mmFilm)?.Value.ToString() ?? string.Empty;
            FocalLength.Value = profile?.GetValue(ExifTag.FocalLength)?.Value.ToString() ?? string.Empty;
            ISOSpeed.Value = ExifReader.GetISOSpeed(profile);
            MeteringMode.Value = profile?.GetValue(ExifTag.MeteringMode)?.Value.ToString() ?? string.Empty;
            Contrast.Value = ExifReader.GetContrast(profile);
            Saturation.Value = ExifReader.GetSaturation(profile);
            Sharpness.Value = ExifReader.GetSharpness(profile);
            WhiteBalance.Value = ExifReader.GetWhiteBalance(profile);
            FlashMode.Value = ExifReader.GetFlashMode(profile);
            FlashEnergy.Value = profile?.GetValue(ExifTag.FlashEnergy)?.Value.ToString() ?? string.Empty;
            LightSource.Value = ExifReader.GetLightSource(profile);
            Brightness.Value = profile?.GetValue(ExifTag.BrightnessValue)?.Value.ToString(CultureInfo.CurrentCulture) ?? null;
            PhotometricInterpretation.Value = ExifReader.GetPhotometricInterpretation(profile);
            ExifVersion.Value = ExifReader.GetExifVersion(profile);
            LensModel.Value = profile?.GetValue(ExifTag.LensModel)?.Value ?? string.Empty;
            LensMaker.Value = profile?.GetValue(ExifTag.LensMake)?.Value ?? string.Empty;
            Comment.Value = ExifReader.GetUserComment(profile);
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(ExifViewModel), nameof(UpdateExifValues), e);
        }
        finally
        {
            if (shouldDispose)
            {
                magick.Dispose();
            }
        }
    }

    public ReactiveCommand? OpenGoogleLinkCommand { get; }
    public ReactiveCommand? OpenBingLinkCommand { get; }

    public ReactiveCommand<string>? SetExifRating0Command { get; set; }
    public ReactiveCommand<string>? SetExifRating1Command { get; set; }
    public ReactiveCommand<string>? SetExifRating2Command { get; set; }
    public ReactiveCommand<string>? SetExifRating3Command { get; set; }
    public ReactiveCommand<string>? SetExifRating4Command { get; set; }
    public ReactiveCommand<string>? SetExifRating5Command { get; set; }

    public BindableReactiveProperty<uint> ExifRating { get; } = new();
    public BindableReactiveProperty<double> DpiX { get; } = new();

    public BindableReactiveProperty<double> DpiY { get; } = new();

    public BindableReactiveProperty<string?> PrintSizeInch { get; } = new();

    public BindableReactiveProperty<string?> PrintSizeCm { get; } = new();

    public BindableReactiveProperty<string?> SizeMp { get; } = new();

    public BindableReactiveProperty<string?> Resolution { get; } = new();

    public BindableReactiveProperty<string?> BitDepth { get; } = new();

    public BindableReactiveProperty<string?> AspectRatio { get; } = new();

    public BindableReactiveProperty<string?> Latitude { get; } = new();

    public BindableReactiveProperty<string?> Longitude { get; } = new();

    public BindableReactiveProperty<string?> Altitude { get; } = new();

    public BindableReactiveProperty<string?> GoogleLink { get; } = new();

    public BindableReactiveProperty<string?> BingLink { get; } = new();

    public BindableReactiveProperty<string?> Authors { get; } = new();

    public BindableReactiveProperty<string?> DateTaken { get; } = new();

    public BindableReactiveProperty<string?> Copyright { get; } = new();

    public BindableReactiveProperty<string?> Title { get; } = new();

    public BindableReactiveProperty<string?> Subject { get; } = new();

    public BindableReactiveProperty<string?> Software { get; } = new();


    public BindableReactiveProperty<ushort?> ResolutionUnit { get; } = new();

    public BindableReactiveProperty<string[]> ResolutionUnits { get; }


    public BindableReactiveProperty<string[]> ColorRepresentations { get; }
    public BindableReactiveProperty<ushort?> ColorRepresentation { get; } = new();

    public BindableReactiveProperty<ushort?> Compression { get; } = new();
    public BindableReactiveProperty<string[]> Compressions { get; }

    public BindableReactiveProperty<string?> Comment { get; } = new();

    public BindableReactiveProperty<string?> CompressedBitsPixel { get; } = new();

    public BindableReactiveProperty<string?> CameraMaker { get; } = new();

    public BindableReactiveProperty<string?> CameraModel { get; } = new();

    public BindableReactiveProperty<string?> ExposureProgram { get; } = new();

    public BindableReactiveProperty<string?> ExposureTime { get; } = new();

    public BindableReactiveProperty<string?> ExposureBias { get; } = new();

    public BindableReactiveProperty<string?> FNumber { get; } = new();

    public BindableReactiveProperty<string?> MaxAperture { get; } = new();

    public BindableReactiveProperty<string?> DigitalZoom { get; } = new();

    public BindableReactiveProperty<string?> FocalLength35Mm { get; } = new();

    public BindableReactiveProperty<string?> FocalLength { get; } = new();

    // ReSharper disable once InconsistentNaming
    public BindableReactiveProperty<string?> ISOSpeed { get; } = new();

    public BindableReactiveProperty<string?> MeteringMode { get; } = new();

    public BindableReactiveProperty<string?> Contrast { get; } = new();

    public BindableReactiveProperty<string?> Saturation { get; } = new();

    public BindableReactiveProperty<string?> Sharpness { get; } = new();

    public BindableReactiveProperty<string?> WhiteBalance { get; } = new();

    public BindableReactiveProperty<string?> FlashMode { get; } = new();

    public BindableReactiveProperty<string?> FlashEnergy { get; } = new();

    public BindableReactiveProperty<string?> LightSource { get; } = new();

    public BindableReactiveProperty<string?> Brightness { get; } = new();

    public BindableReactiveProperty<string?> PhotometricInterpretation { get; } = new();

    public BindableReactiveProperty<string?> Orientation { get; } = new();

    public BindableReactiveProperty<string?> ExifVersion { get; } = new();

    public BindableReactiveProperty<string?> LensModel { get; } = new();

    public BindableReactiveProperty<string?> LensMaker { get; } = new();

    public BindableReactiveProperty<bool> IsExifAvailable { get; } = new();

    public void Dispose()
    {
        Disposable.Dispose(
            DpiX,
            DpiY,
            PrintSizeCm,
            PrintSizeCm,
            SizeMp,
            ResolutionUnit,
            BitDepth,
            AspectRatio,
            Latitude,
            Longitude,
            Altitude,
            GoogleLink,
            BingLink,
            Authors,
            DateTaken,
            Copyright,
            Title,
            Subject,
            Software,
            ResolutionUnit,
            ColorRepresentation,
            Compression,
            Comment,
            CompressedBitsPixel,
            CameraMaker,
            CameraModel,
            ExposureProgram,
            ExposureTime,
            ExposureBias,
            FNumber,
            MaxAperture,
            DigitalZoom,
            FocalLength35Mm,
            FocalLength,
            ISOSpeed,
            MeteringMode,
            Contrast,
            Saturation,
            Brightness,
            Sharpness,
            WhiteBalance,
            FlashMode,
            FlashEnergy,
            LightSource,
            PhotometricInterpretation,
            Orientation,
            ExifVersion,
            LensMaker,
            LensModel);
    }

    public void OpenGoogleMaps(Unit unit) => ProcessHelper.OpenLink(GoogleLink.CurrentValue);
    public void OpenBingMaps(Unit unit) => ProcessHelper.OpenLink(BingLink.CurrentValue);

    private async Task<bool> SetRating(string filePath, ushort rating)
    {
        var isRated = await ExifWriter.SetExifRatingAsync(new FileInfo(filePath), rating).ConfigureAwait(false);
        if (!isRated)
        {
            return false;
        }
        ExifRating.Value = rating;
        return true;
    }
}