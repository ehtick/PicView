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

    /// <summary>
    /// Determines whether the specified file is an EXIF-supported image type.
    /// </summary>
    /// <param name="fileInfo">The file information of the image to check.</param>
    /// <returns>True if the file is an EXIF-supported image type; otherwise, false.</returns>
    public static bool IsExifImage(this FileInfo fileInfo)
    {
        if (fileInfo is null)
        {
            return false;
        }

        return fileInfo.Extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or
                ".tif" or ".tiff" or
                ".dng" or                 // Adobe Digital Negative
                ".cr2" or ".cr3" or       // Canon RAW
                ".nef" or                 // Nikon RAW
                ".arw" or                 // Sony RAW
                ".orf" or                 // Olympus RAW
                ".rw2" or                 // Panasonic RAW
                ".pef" or                 // Pentax RAW
                ".raf" or                 // Fujifilm RAW
                ".srw" or                 // Samsung RAW
                ".heif" or ".heic"        // High Efficiency Image Format
                => true,

            _ => false,
        };
    }

    /// <summary>
    /// Removes the EXIF profile metadata from the specified image file.
    /// </summary>
    /// <param name="fileInfo">The file information of the image from which the EXIF profile will be removed.</param>
    /// <returns>A task representing the asynchronous operation, containing a boolean value that indicates whether the EXIF profile was successfully removed.</returns>
    public static Task<bool> RemoveExifProfile(FileInfo fileInfo) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            var profile = magickImage.GetExifProfile();
            if (profile is null)
            {
                return false;
            }

            magickImage.RemoveProfile(profile);
            return true;
        }, nameof(RemoveExifProfile));

    /// <summary>
    /// Adds authors metadata to the EXIF profile of the specified image file.
    /// </summary>
    /// <param name="fileInfo">The file information of the image to update.</param>
    /// <param name="value">The author(s) to add to the EXIF profile.</param>
    /// <returns>A task representing the asynchronous operation, containing a boolean indicator of success or failure.</returns>
    public static Task<bool> AddAuthors(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Artist, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddAuthors));

    /// <summary>
    /// Adds or updates the copyright information in the EXIF metadata of the specified image file.
    /// </summary>
    /// <param name="fileInfo">The file information of the image to update.</param>
    /// <param name="value">The copyright text to be added or updated in the EXIF metadata.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the operation succeeded; otherwise, false.</returns>
    public static Task<bool> AddCopyright(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Copyright, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddCopyright));

    public static Task<bool> AddSoftware(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Software, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddSoftware));

    #region Add EXIF Properties

    // Note: Most 'Add' methods take a string and attempt to parse it.
    // This is to easily integrate with UI text boxes.

    public static Task<bool> AddSubject(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.XPSubject, Encoding.Unicode.GetBytes(value));
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddSubject));

    public static Task<bool> AddTitle(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.XPTitle, Encoding.Unicode.GetBytes(value));
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddTitle));

    public static Task<bool> AddComment(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            // UserComment requires a specific encoding prefix
            var bytes = Encoding.Unicode.GetBytes(value);
            var allBytes = new byte[bytes.Length + 8];
            "UNICODE\0"u8.ToArray().CopyTo(allBytes, 0);
            bytes.CopyTo(allBytes, 8);
            profile.SetValue(ExifTag.UserComment, allBytes);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddComment));

    public static Task<bool> AddLatitude(FileInfo? fileInfo, string? value)
    {
        // TODO: Implement robust parsing for GPS coordinates.
        // The value needs to be parsed into Rational[3] for Deg/Min/Sec and a GPSLatitudeRef.
        return Task.FromResult(false);
    }

    public static Task<bool> AddLongitude(FileInfo? fileInfo, string? value)
    {
        // TODO: Implement robust parsing for GPS coordinates.
        // The value needs to be parsed into Rational[3] for Deg/Min/Sec and a GPSLongitudeRef.
        return Task.FromResult(false);
    }

    public static Task<bool> AddAltitude(FileInfo? fileInfo, string? value)
    {
        // TODO: Implement robust parsing for GPS Altitude.
        // The value needs to be parsed into a Rational and a GPSAltitudeRef.
        return Task.FromResult(false);
    }

    public static Task<bool> AddColorRepresentation(FileInfo? fileInfo, ushort value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.ColorSpace, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddColorRepresentation));

    public static Task<bool> AddResolutionUnit(FileInfo? fileInfo, ushort value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue<ushort>(ExifTag.ResolutionUnit, 2);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddResolutionUnit));

    public static Task<bool> AddCompression(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var compression)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Compression, compression);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddCompression));

    public static Task<bool> AddCompressedBitsPerPixel(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!TryParseRational(value, out var rational)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.CompressedBitsPerPixel, rational);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddCompressedBitsPerPixel));

    public static Task<bool> AddCameraMaker(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Make, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddCameraMaker));

    public static Task<bool> AddCameraModel(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Model, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddCameraModel));

    public static Task<bool> AddFNumber(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!TryParseRational(value, out var rational)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.FNumber, rational);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddFNumber));

    public static Task<bool> AddMaxAperture(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!TryParseRational(value, out var rational)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.MaxApertureValue, rational);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddMaxAperture));

    public static Task<bool> AddExposureBias(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!TryParseRational(value, out var rational)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.ExposureBiasValue, new SignedRational((int)rational.Numerator, (int)rational.Denominator));
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddExposureBias));

    public static Task<bool> AddExposureTime(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!TryParseRational(value, out var rational)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.ExposureTime, rational);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddExposureTime));

    public static Task<bool> AddExposureProgram(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var program)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.ExposureProgram, program);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddExposureProgram));

    public static Task<bool> AddDigitalZoom(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!TryParseRational(value, out var rational)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.DigitalZoomRatio, rational);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddDigitalZoom));

    public static Task<bool> AddFocalLength(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!TryParseRational(value, out var rational)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.FocalLength, rational);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddFocalLength));

    public static Task<bool> AddFocalLength35mm(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var length)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.FocalLengthIn35mmFilm, length);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddFocalLength35mm));

    public static Task<bool> AddIsoSpeed(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var speed)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.ISOSpeedRatings, [speed]);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddIsoSpeed));

    public static Task<bool> AddMeteringMode(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var mode)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.MeteringMode, mode);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddMeteringMode));

    public static Task<bool> AddContrast(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var contrast)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Contrast, contrast);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddContrast));

    public static Task<bool> AddSaturation(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var saturation)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Saturation, saturation);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddSaturation));

    public static Task<bool> AddSharpness(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var sharpness)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Sharpness, sharpness);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddSharpness));

    public static Task<bool> AddWhiteBalance(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var whiteBalance)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.WhiteBalance, whiteBalance);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddWhiteBalance));

    public static Task<bool> AddFlashEnergy(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!TryParseRational(value, out var rational)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.FlashEnergy, rational);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddFlashEnergy));

    public static Task<bool> AddFlashMode(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var flash)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Flash, flash);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddFlashMode));

    public static Task<bool> AddLightSource(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var source)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.LightSource, source);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddLightSource));

    public static Task<bool> AddBrightness(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            // It would be better to parse directly to SignedRational.
            // This assumes you have a helper method like TryParseSignedRational.
            if (!TryParseSignedRational(value, out var signedRational)) return false;
            
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            // Use the correctly parsed SignedRational object directly.
            profile.SetValue(ExifTag.BrightnessValue, signedRational);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddBrightness));

    public static Task<bool> AddPhotometricInterpretation(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var interpretation)) return false;
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.PhotometricInterpretation, interpretation);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddPhotometricInterpretation));

    public static Task<bool> AddLensMaker(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.LensMake, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddLensMaker));

    public static Task<bool> AddLensModel(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.LensModel, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddLensModel));

    public static Task<bool> AddExifVersion(FileInfo? fileInfo, string? value) =>
        TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.ExifVersion, Encoding.ASCII.GetBytes(value));
            magickImage.SetProfile(profile);
            return true;
        }, nameof(AddExifVersion));

    #endregion

    // You would need to implement this new helper method.
    private static bool TryParseSignedRational(string input, out SignedRational result)
    {
        // This method would parse the input string, including negative values,
        // and create a SignedRational object.
        result = default; // Placeholder for implementation
        
        // Example parsing logic (you would need to make it more robust):
        if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
        {
            // A simple conversion from decimal to a signed rational.
            // More complex logic might be needed for precision.
            result = new SignedRational((double)dec);
            return true;
        }
        return false;
    }

    private static async Task<bool> TryUpdateImageProfileAsync(FileInfo fileInfo, Func<MagickImage, bool> updateAction, string callingMethodName)
    {
        try
        {
            using var magickImage = new MagickImage(fileInfo);

            // If the update action returns false, it means no changes were made that require saving.
            if (!updateAction(magickImage))
            {
                return false;
            }

            await using var fileStream = FileStreamUtils.GetOptimizedFileStream(fileInfo, true);
            await magickImage.WriteAsync(fileStream);
            return true;
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(EXIFHelper), callingMethodName, e);
            return false;
        }
    }

    private static bool TryParseRational(string input, out Rational result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        // Handle fraction format "num/den"
        if (input.Contains('/'))
        {
            var parts = input.Split('/');
            if (parts.Length == 2 && double.TryParse(parts[0], CultureInfo.InvariantCulture, out var num) && double.TryParse(parts[1], CultureInfo.InvariantCulture, out var den))
            {
                if (den == 0) return false;
                result = new Rational((uint)num, (uint)den);
                return true;
            }
        }

        // Handle decimal format
        if (double.TryParse(input, CultureInfo.InvariantCulture, out var val))
        {
            result = new Rational(val);
            return true;
        }

        return false;
    }


    public static EXIFOrientation GetImageOrientation(MagickImage magickImage)
    {
        if (magickImage.Orientation is not OrientationType.Undefined)
        {
            return magickImage.Orientation switch
            {
                OrientationType.BottomLeft => EXIFOrientation.MirrorVertical,
                OrientationType.BottomRight => EXIFOrientation.Rotate180,
                OrientationType.TopLeft => EXIFOrientation.Horizontal,
                OrientationType.TopRight => EXIFOrientation.MirrorHorizontal,
                OrientationType.RightBottom => EXIFOrientation.MirrorHorizontalRotate90Cw,
                OrientationType.RightTop => EXIFOrientation.Rotate90Cw,
                OrientationType.LeftBottom => EXIFOrientation.Rotated270Cw,
                OrientationType.LeftTop => EXIFOrientation.MirrorHorizontalRotate270Cw,
                _ => EXIFOrientation.None
            };
        }

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

    public static ushort GetResolutionUnit(IExifProfile? profile)
    {
        var resolutionUnit = profile?.GetValue(ExifTag.ResolutionUnit)?.Value;
        if (resolutionUnit is null)
        {
            return 0;
        }

        return resolutionUnit switch
        {
            1 => 1,
            2 => 2,
            3 => 3,
            _ => 0
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
        var title = xPTitle is null ? string.Empty : Encoding.Unicode.GetString(xPTitle).TrimEnd('\0');
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
    }
}