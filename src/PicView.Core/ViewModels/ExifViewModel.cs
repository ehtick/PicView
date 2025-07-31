using PicView.Core.ImageDecoding;
using PicView.Core.ProcessHandling;
using R3;

namespace PicView.Core.ViewModels;

public class ExifViewModel : IDisposable
{
    public ExifViewModel()
    {
        OpenGoogleLinkCommand = new ReactiveCommand(OpenGoogleMaps);
        OpenBingLinkCommand = new ReactiveCommand(OpenBingMaps);

        SetExifRating0Command = new ReactiveCommand<string>(Set0Star);
        SetExifRating1Command = new ReactiveCommand<string>(Set1Star);
        SetExifRating2Command = new ReactiveCommand<string>(Set2Star);
        SetExifRating3Command = new ReactiveCommand<string>(Set3Star);
        SetExifRating4Command = new ReactiveCommand<string>(Set4Star);
        SetExifRating5Command = new ReactiveCommand<string>(Set5Star);
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

    public BindableReactiveProperty<string?> ResolutionUnit { get; } = new();

    public BindableReactiveProperty<string?> ColorRepresentation { get; } = new();

    public BindableReactiveProperty<string?> Compression { get; } = new();

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

    private void Set0Star(string value)
    {
        EXIFHelper.SetEXIFRating(value, 0);
        ExifRating.Value = 0;
    }

    private void Set1Star(string value)
    {
        EXIFHelper.SetEXIFRating(value, 1);
        ExifRating.Value = 1;
    }

    private void Set2Star(string value)
    {
        EXIFHelper.SetEXIFRating(value, 2);
        ExifRating.Value = 2;
    }

    private void Set3Star(string value)
    {
        EXIFHelper.SetEXIFRating(value, 3);
        ExifRating.Value = 3;
    }

    private void Set4Star(string value)
    {
        EXIFHelper.SetEXIFRating(value, 4);
        ExifRating.Value = 4;
    }

    private void Set5Star(string value)
    {
        EXIFHelper.SetEXIFRating(value, 5);
        ExifRating.Value = 5;
    }

    public void OpenGoogleMaps(Unit unit) => ProcessHelper.OpenLink(GoogleLink.CurrentValue);
    public void OpenBingMaps(Unit unit) => ProcessHelper.OpenLink(BingLink.CurrentValue);
}