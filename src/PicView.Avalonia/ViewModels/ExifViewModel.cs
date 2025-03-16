using ReactiveUI;

namespace PicView.Avalonia.ViewModels;

public class ExifViewModel : ReactiveObject
{
    public double DpiX
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double DpiY
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    public string? PrintSizeInch
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? PrintSizeCm
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SizeMp
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Resolution
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? BitDepth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? AspectRatio
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Latitude
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Longitude
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Altitude
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? GoogleLink
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? BingLink
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Authors
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? DateTaken
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Copyright
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Title
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Subject
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Software
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ResolutionUnit
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ColorRepresentation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Compression
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CompressedBitsPixel
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CameraMaker
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? CameraModel
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ExposureProgram
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ExposureTime
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ExposureBias
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FNumber
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? MaxAperture
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? DigitalZoom
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FocalLength35Mm
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FocalLength
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ISOSpeed
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? MeteringMode
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Contrast
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Saturation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Sharpness
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? WhiteBalance
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FlashMode
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? FlashEnergy
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? LightSource
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Brightness
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? PhotometricInterpretation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? Orientation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? ExifVersion
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? LensModel
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? LensMaker
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }    
}
