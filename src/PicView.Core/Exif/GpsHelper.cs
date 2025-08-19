using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ImageMagick;

namespace PicView.Core.Exif;

public static class GpsHelper
{
    /// <summary>
    /// Gets the GPS values from the provided EXIF profile.
    /// </summary>
    /// <param name="profile">The EXIF profile.</param>
    /// <returns>An array containing the latitude, longitude, Google Maps link, and Bing Maps link.</returns>
    [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    public static string?[]? GetGpsValues(IExifProfile profile)
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
}