using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using ImageMagick;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.Localization;

namespace PicView.Core.ImageDecoding;

public static class EXIFHelper
{
    // https://exiftool.org/TagNames/EXIF.html
    // 1 = Horizontal (normal)
    // 2 = Mirror horizontal
    // 3 = Rotate 180
    // 4 = Mirror vertical
    // 5 = Mirror horizontal and rotate 270 CW
    // 6 = Rotate 90 CW
    // 7 = Mirror horizontal and rotate 90 CW
    // 8 = Rotate 270 CW
    public enum EXIFOrientation
    {
        None = 0,
        Horizontal = 1,
        MirrorHorizontal = 2,
        Rotate180 = 3,
        MirrorVertical = 4,
        MirrorHorizontalRotate270Cw = 5,
        Rotate90Cw = 6,
        MirrorHorizontalRotate90Cw = 7,
        Rotated270Cw = 8
    }

    public static async Task<bool> RemoveExifProfile(FileInfo fileInfo)
    {
        try
        {
            using var magickImage = new MagickImage(fileInfo);
            var profile = magickImage.GetExifProfile();
            if (profile is null)
            {
                return false;
            }

            magickImage.RemoveProfile(profile);
            await using var fileStream = FileHelper.GetOptimizedFileStream(fileInfo, true);
            await magickImage.WriteAsync(fileStream);
            return true;
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(EXIFHelper), nameof(RemoveExifProfile), e);
            return false;
        }
    }

    public static EXIFOrientation GetImageOrientation(MagickImage magickImage)
    {
        var profile = magickImage.GetExifProfile();

        var orientationValue = profile?.GetValue(ExifTag.Orientation);
        if (orientationValue is null)
        {
            return EXIFOrientation.None;
        }

        return orientationValue.Value switch
        {
            0 => EXIFOrientation.None,
            1 => EXIFOrientation.Horizontal,
            2 => EXIFOrientation.MirrorHorizontal,
            3 => EXIFOrientation.Rotate180,
            4 => EXIFOrientation.MirrorVertical,
            5 => EXIFOrientation.MirrorHorizontalRotate270Cw,
            6 => EXIFOrientation.Rotate90Cw,
            7 => EXIFOrientation.MirrorHorizontalRotate90Cw,
            8 => EXIFOrientation.Rotated270Cw,
            _ => EXIFOrientation.None
        };
    }

    public static EXIFOrientation GetImageOrientation(string filePath)
    {
        using var magickImage = new MagickImage();
        magickImage.Ping(filePath);
        return GetImageOrientation(magickImage);
    }

    public static EXIFOrientation GetImageOrientation(FileInfo fileInfo)
    {
        return GetImageOrientation(fileInfo.FullName);
    }

    // ReSharper disable once InconsistentNaming
    public static bool SetEXIFRating(string filePath, ushort rating)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException();
        }

        if (rating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(rating));
        }

        try
        {
            using var image = new MagickImage(filePath);
            var profile = image?.GetExifProfile();
            if (profile is null)
            {
                profile = new ExifProfile(filePath);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (profile is null || image is null)
                {
                    throw new Exception("Failed to create EXIF profile or image.");
                }
            }
            else if (image is null)
            {
                throw new Exception("Failed to create image.");
            }

            profile.SetValue(ExifTag.Rating, rating);
            image.SetProfile(profile);

            image.Write(filePath);
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(EXIFHelper), nameof(SetEXIFRating), e);
            return false;
        }

        return true;
    }

    public static string GetDateTaken(IExifProfile profile)
    {
        var getDateTaken =
            profile?.GetValue(ExifTag.DateTime)?.Value ??
            profile?.GetValue(ExifTag.DateTimeOriginal)?.Value ??
            profile?.GetValue(ExifTag.DateTimeDigitized)?.Value ?? string.Empty;
        if (!string.IsNullOrEmpty(getDateTaken) &&
            DateTime.TryParseExact(getDateTaken, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var formattedDateTime))
        {
            return formattedDateTime.ToString(CultureInfo.CurrentCulture);
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the GPS values from the provided EXIF profile.
    /// </summary>
    /// <param name="profile">The EXIF profile.</param>
    /// <returns>An array containing the latitude, longitude, Google Maps link, and Bing Maps link.</returns>
    [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    public static string?[]? GetGPSValues(IExifProfile profile)
    {
        if (profile is null)
        {
            return null;
        }

        var gpsLong = profile.GetValue(ExifTag.GPSLongitude);
        var gpsLongRef = profile.GetValue(ExifTag.GPSLongitudeRef);
        var gpsLatitude = profile.GetValue(ExifTag.GPSLatitude);
        var gpsLatitudeRef = profile.GetValue(ExifTag.GPSLatitudeRef);

        if (gpsLong is null || gpsLongRef is null || gpsLatitude is null ||
            gpsLatitudeRef is null)
        {
            return null;
        }

        var latitudeValue = GetCoordinates(gpsLatitudeRef.ToString(), gpsLatitude.Value)
            .ToString(CultureInfo.InvariantCulture);
        var longitudeValue =
            GetCoordinates(gpsLongRef.ToString(), gpsLong.Value).ToString(CultureInfo.InvariantCulture);

        var googleLink = $"https://www.google.com/maps/search/?api=1&query={latitudeValue},{longitudeValue}";
        var bingLink = $"https://bing.com/maps/default.aspx?cp={latitudeValue}~{longitudeValue}&lvl=16.0&sty=c";

        var latitudeString =
            $"{gpsLatitude.Value[0]}\u00b0{gpsLatitude.Value[1]}'{gpsLatitude.Value[2].ToDouble():0.##}\"{gpsLatitudeRef}";
        var longitudeString =
            $"{gpsLong.Value[0]}\u00b0{gpsLong.Value[1]}'{gpsLong.Value[2].ToDouble():0.##}\"{gpsLongRef}";

        return [latitudeString, longitudeString, googleLink, bingLink];

        double GetCoordinates(string gpsRef, IReadOnlyList<Rational> rationals)
        {
            if (rationals[0].Denominator == 0 || rationals[1].Denominator == 0 || rationals[2].Denominator == 0)
            {
                return 0;
            }

            double degrees = rationals[0].Numerator / rationals[0].Denominator;
            double minutes = rationals[1].Numerator / rationals[1].Denominator;
            double seconds = rationals[2].Numerator / rationals[2].Denominator;

            var coordinate = degrees + minutes / 60d + seconds / 3600d;
            if (gpsRef is "S" or "W")
            {
                coordinate *= -1;
            }

            return coordinate;
        }
    }

    public static string GetColorSpace(IExifProfile profile)
    {
        var colorSpace = profile?.GetValue(ExifTag.ColorSpace)?.Value;
        if (colorSpace == null)
        {
            return string.Empty;
        }

        return colorSpace switch
        {
            1 => "sRGB",
            2 => "Adobe RGB",
            65535 => TranslationManager.Translation.Uncalibrated ?? "Uncalibrated",
            _ => string.Empty
        };
    }

    public static string GetExposureProgram(IExifProfile? profile)
    {
        var exposureProgram = profile?.GetValue(ExifTag.ExposureProgram)?.Value;
        if (exposureProgram is null)
        {
            return string.Empty;
        }

        return exposureProgram switch
        {
            0 => TranslationManager.GetTranslation("NotDefined"),
            1 => TranslationManager.GetTranslation("Manual"),
            2 => TranslationManager.GetTranslation("Normal"),
            3 => TranslationManager.GetTranslation("AperturePriority"),
            4 => TranslationManager.GetTranslation("ShutterPriority"),
            5 => TranslationManager.GetTranslation("CreativeProgram"),
            6 => TranslationManager.GetTranslation("ActionProgram"),
            7 => TranslationManager.GetTranslation("Portrait"),
            8 => TranslationManager.GetTranslation("Landscape"),
            _ => string.Empty
        };
    }

    public static string GetISOSpeed(IExifProfile? profile)
    {
        if (profile is null)
        {
            return string.Empty;
        }

        var isoSpeedRating = profile.GetValue(ExifTag.ISOSpeedRatings)?.Value;
        if (isoSpeedRating is not null)
        {
            return isoSpeedRating.GetValue(0)?.ToString() ?? string.Empty;
        }

        var isoSpeed = profile.GetValue(ExifTag.ISOSpeed)?.Value;
        if (isoSpeed is null)
        {
            return string.Empty;
        }

        return isoSpeed.ToString() ?? string.Empty;
    }

    public static string GetSaturation(IExifProfile? profile)
    {
        var saturation = profile?.GetValue(ExifTag.Saturation)?.Value;
        if (saturation is null)
        {
            return string.Empty;
        }

        return saturation switch
        {
            0 => TranslationManager.GetTranslation("Normal"),
            1 => TranslationManager.GetTranslation("Low"),
            2 => TranslationManager.GetTranslation("High"),
            _ => string.Empty
        };
    }

    public static string GetContrast(IExifProfile profile)
    {
        var contrast = profile?.GetValue(ExifTag.Contrast)?.Value;
        if (contrast is null)
        {
            return string.Empty;
        }

        return contrast switch
        {
            0 => TranslationManager.GetTranslation("Normal"),
            1 => TranslationManager.GetTranslation("Soft"),
            2 => TranslationManager.GetTranslation("Hard"),
            _ => string.Empty
        };
    }

    public static string GetSharpness(IExifProfile profile)
    {
        var sharpness = profile?.GetValue(ExifTag.Sharpness)?.Value;
        if (sharpness is null)
        {
            return string.Empty;
        }

        return sharpness switch
        {
            0 => TranslationManager.GetTranslation("Normal"),
            1 => TranslationManager.GetTranslation("Soft"),
            2 => TranslationManager.GetTranslation("Hard"),
            _ => string.Empty
        };
    }

    public static string GetWhiteBalance(IExifProfile profile)
    {
        var whiteBalance = profile?.GetValue(ExifTag.WhiteBalance)?.Value;
        if (whiteBalance is null)
        {
            return string.Empty;
        }

        return whiteBalance switch
        {
            0 => TranslationManager.GetTranslation("Auto"),
            1 => TranslationManager.GetTranslation("Manual"),
            _ => string.Empty
        };
    }

    public static string GetResolutionUnit(IExifProfile? profile)
    {
        var resolutionUnit = profile?.GetValue(ExifTag.ResolutionUnit)?.Value;
        if (resolutionUnit is null)
        {
            return string.Empty;
        }

        return resolutionUnit switch
        {
            1 => TranslationManager.GetTranslation("None"),
            2 => TranslationManager.GetTranslation("Inches"),
            3 => TranslationManager.GetTranslation("Centimeters"),
            _ => string.Empty
        };
    }

    // https://exiftool.org/TagNames/EXIF.html
    public static string GetFlashMode(IExifProfile profile)
    {
        var flash = profile?.GetValue(ExifTag.Flash)?.Value;
        if (flash is null)
        {
            return string.Empty;
        }

        switch (flash)
        {
            case 0:
            case 9:
            case 24:
            case 32:
                return TranslationManager.GetTranslation("FlashDidNotFire");

            case 1:
            case 13:
            case 25:
                return TranslationManager.GetTranslation("FlashFired");

            case 5:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightDetected");

            case 7:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightNotDetected");

            case 15:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightDetected");

            case 16:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightNotDetected");

            case 29:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightDetected");

            case 31:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightNotDetected");

            case 65:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("RedEyeReduction");

            case 69:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("RedEyeReduction") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightDetected");

            case 71:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("RedEyeReduction") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightNotDetected");

            case 73:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("RedEyeReduction");

            case 77:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("RedEyeReduction") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightDetected");

            case 79:
                return TranslationManager.GetTranslation("Unknown") + ", " +
                       TranslationManager.GetTranslation("RedEyeReduction") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightNotDetected");

            case 89:
                return TranslationManager.GetTranslation("FlashDidNotFire") + ", " +
                       TranslationManager.GetTranslation("RedEyeReduction");

            case 93:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("RedEyeReduction");

            case 95:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("RedEyeReduction") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightDetected");

            case 97:
                return TranslationManager.GetTranslation("FlashFired") + ", " +
                       TranslationManager.GetTranslation("RedEyeReduction") + ", " +
                       TranslationManager.GetTranslation("StrobeReturnLightNotDetected");

            default: return string.Empty;
        }
    }

    public static string GetLightSource(IExifProfile? profile)
    {
        var lightSource = profile?.GetValue(ExifTag.LightSource)?.Value;
        if (lightSource is null)
        {
            return string.Empty;
        }

        return lightSource switch
        {
            0 => TranslationManager.GetTranslation("Unknown"),
            1 => TranslationManager.GetTranslation("Daylight"),
            2 => TranslationManager.GetTranslation("Fluorescent"),
            3 => "Tungsten",
            4 => TranslationManager.GetTranslation("Flash"),
            9 => TranslationManager.GetTranslation("FineWeather"),
            10 => TranslationManager.GetTranslation("CloudyWeather"),
            11 => TranslationManager.GetTranslation("Shade"),
            12 => TranslationManager.GetTranslation("DaylightFluorescent"),
            13 => TranslationManager.GetTranslation("DayWhiteFluorescent"),
            14 => TranslationManager.GetTranslation("CoolWhiteFluorescent"),
            15 => TranslationManager.GetTranslation("WhiteFluorescent"),
            17 => "Illuminants A",
            18 => "Illuminants B",
            19 => "Illuminants C",
            20 => "D55",
            21 => "D65",
            22 => "D75",
            23 => "D50",
            24 => "ISO Studio Tungsten",
            255 => TranslationManager.GetTranslation("NotDefined"),
            _ => string.Empty
        };
    }

    public static string GetPhotometricInterpretation(IExifProfile? profile)
    {
        var photometricInterpretation = profile?.GetValue(ExifTag.PhotometricInterpretation)?.Value;
        if (photometricInterpretation is null)
        {
            return string.Empty;
        }

        return photometricInterpretation switch
        {
            0 => "WhiteIsZero",
            1 => "BlackIsZero",
            2 => "RGB",
            3 => "RGBPalette",
            4 => "TransparencyMask",
            5 => "CMYK",
            6 => "YCbCr",
            8 => "CIELab",
            9 => "ICCLab",
            10 => "ITULab",
            32803 => "ColorFilterArray",
            32844 => "PixarLogL",
            32845 => "PixarLogLuv",
            32892 => "LinearRaw",
            34892 => "LinearRaw",
            65535 => "Undefined",
            _ => string.Empty
        };
    }

    public static string GetExifVersion(IExifProfile? profile)
    {
        var exifVersion = profile?.GetValue(ExifTag.ExifVersion)?.Value;
        return exifVersion is null ? string.Empty : Encoding.ASCII.GetString(exifVersion);
    }

    public static string GetTitle(IExifProfile? profile)
    {
        var xPTitle = profile?.GetValue(ExifTag.XPTitle)?.Value;
        var title = xPTitle is null ? string.Empty : Encoding.ASCII.GetString(xPTitle);
        if (!string.IsNullOrEmpty(title))
        {
            return title;
        }

        var titleTag = profile?.GetValue(ExifTag.ImageDescription)?.Value;
        return titleTag ?? string.Empty;
    }

    public static string GetSubject(IExifProfile? profile)
    {
        var xPSubject = profile?.GetValue(ExifTag.XPSubject)?.Value;
        var subject = xPSubject is null ? string.Empty : Encoding.ASCII.GetString(xPSubject);
        if (!string.IsNullOrEmpty(subject))
        {
            return subject;
        }

        var subjectTag = profile?.GetValue(ExifTag.XPSubject)?.Value;
        return subjectTag?.GetValue(0)?.ToString() ?? string.Empty;
    }

    public static string GetUserComment(IExifProfile? profile)
    {
        var commentBytes = profile?.GetValue(ExifTag.UserComment)?.Value;
        var decodedComment = commentBytes is null ? string.Empty : Encoding.ASCII.GetString(commentBytes);
        if (string.IsNullOrEmpty(decodedComment))
        {
            return string.Empty;
        }

        return decodedComment.StartsWith("UNICODE") ? decodedComment.Replace("UNICODE", "") : decodedComment;
        ;
    }
}