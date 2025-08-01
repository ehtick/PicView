using System.Text;
using ImageMagick;
using PicView.Core.DebugTools;

namespace PicView.Core.Exif;

public static class ExifWriter
{
    public static bool SetExifRating(string filePath, ushort rating)
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
            DebugHelper.LogDebug(nameof(ExifWriter), nameof(SetExifRating), e);
            return false;
        }

        return true;
    }


    public static Task<bool> AddSubject(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.XPSubject, Encoding.Unicode.GetBytes(value));
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddSubject));

    public static Task<bool> AddTitle(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.XPTitle, Encoding.Unicode.GetBytes(value));
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddTitle));

    public static Task<bool> AddComment(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.UserComment, Encoding.ASCII.GetBytes(value));
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddComment));

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

    public static Task<bool> AddResolutionUnit(FileInfo? fileInfo, ushort value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue<ushort>(ExifTag.ResolutionUnit, 2);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddResolutionUnit));

    public static Task<bool> AddCompression(FileInfo? fileInfo, ushort? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Compression, value.Value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddCompression));

    public static Task<bool> AddCompressedBitsPerPixel(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage => { throw new NotImplementedException(); },
            nameof(ExifWriter), nameof(AddCompressedBitsPerPixel));

    public static Task<bool> AddCameraMaker(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Make, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddCameraMaker));

    public static Task<bool> AddCameraModel(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Model, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddCameraModel));

    public static Task<bool> AddFNumber(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage => { throw new NotImplementedException(); },
            nameof(ExifWriter), nameof(AddFNumber));

    public static Task<bool> AddMaxAperture(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage => { throw new NotImplementedException(); },
            nameof(ExifWriter), nameof(AddMaxAperture));

    public static Task<bool> AddExposureBias(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage => { throw new NotImplementedException(); },
            nameof(ExifWriter), nameof(AddExposureBias));

    public static Task<bool> AddExposureTime(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage => { throw new NotImplementedException(); },
            nameof(ExifWriter), nameof(AddExposureTime));

    public static Task<bool> AddExposureProgram(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var program))
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.ExposureProgram, program);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddExposureProgram));

    public static Task<bool> AddDigitalZoom(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            throw new NotImplementedException()
                ;
        }, nameof(ExifWriter), nameof(AddDigitalZoom));

    public static Task<bool> AddFocalLength(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage => { throw new NotImplementedException(); },
            nameof(ExifWriter), nameof(AddFocalLength));

    public static Task<bool> AddFocalLength35mm(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var length))
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.FocalLengthIn35mmFilm, length);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddFocalLength35mm));

    public static Task<bool> AddIsoSpeed(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var speed))
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.ISOSpeedRatings, [speed]);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddIsoSpeed));

    public static Task<bool> AddMeteringMode(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var mode))
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.MeteringMode, mode);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddMeteringMode));

    public static Task<bool> AddContrast(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var contrast))
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Contrast, contrast);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddContrast));

    public static Task<bool> AddSaturation(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var saturation))
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Saturation, saturation);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddSaturation));

    public static Task<bool> AddSharpness(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var sharpness))
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Sharpness, sharpness);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddSharpness));

    public static Task<bool> AddWhiteBalance(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var whiteBalance))
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.WhiteBalance, whiteBalance);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddWhiteBalance));

    public static Task<bool> AddFlashEnergy(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage => { throw new NotImplementedException(); },
            nameof(ExifWriter), nameof(AddFlashEnergy));

    public static Task<bool> AddFlashMode(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var flash))
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Flash, flash);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddFlashMode));

    public static Task<bool> AddLightSource(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var source))
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.LightSource, source);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddLightSource));

    public static Task<bool> AddBrightness(FileInfo? fileInfo, SignedRational? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage => { throw new NotImplementedException(); },
            nameof(ExifWriter), nameof(AddBrightness));

    public static Task<bool> AddPhotometricInterpretation(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (!ushort.TryParse(value, out var interpretation))
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.PhotometricInterpretation, interpretation);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddPhotometricInterpretation));

    public static Task<bool> AddLensMaker(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.LensMake, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddLensMaker));

    public static Task<bool> AddLensModel(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.LensModel, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddLensModel));

    public static Task<bool> AddExifVersion(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.ExifVersion, Encoding.ASCII.GetBytes(value));
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddExifVersion));

    /// <summary>
    /// Removes the EXIF profile metadata from the specified image file.
    /// </summary>
    /// <param name="fileInfo">The file information of the image from which the EXIF profile will be removed.</param>
    /// <returns>A task representing the asynchronous operation, containing a boolean value that indicates whether the EXIF profile was successfully removed.</returns>
    public static Task<bool> RemoveExifProfile(FileInfo fileInfo) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            var profile = magickImage.GetExifProfile();
            if (profile is null)
            {
                return false;
            }

            magickImage.RemoveProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(RemoveExifProfile));

    /// <summary>
    /// Adds authors metadata to the EXIF profile of the specified image file.
    /// </summary>
    /// <param name="fileInfo">The file information of the image to update.</param>
    /// <param name="value">The author(s) to add to the EXIF profile.</param>
    /// <returns>A task representing the asynchronous operation, containing a boolean indicator of success or failure.</returns>
    public static Task<bool> AddAuthors(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Artist, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddAuthors));

    /// <summary>
    /// Adds or updates the copyright information in the EXIF metadata of the specified image file.
    /// </summary>
    /// <param name="fileInfo">The file information of the image to update.</param>
    /// <param name="value">The copyright text to be added or updated in the EXIF metadata.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the operation succeeded; otherwise, false.</returns>
    public static Task<bool> AddCopyright(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Copyright, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddCopyright));

    public static Task<bool> AddSoftware(FileInfo? fileInfo, string? value) =>
        ExifFunctions.TryUpdateImageProfileAsync(fileInfo, magickImage =>
        {
            if (value is null)
            {
                return false;
            }

            var profile = magickImage.GetExifProfile() ?? new ExifProfile();
            profile.SetValue(ExifTag.Software, value);
            magickImage.SetProfile(profile);
            return true;
        }, nameof(ExifWriter), nameof(AddSoftware));
}