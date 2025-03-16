using PicView.Avalonia.ImageEffects;
using PicView.Avalonia.ImageHandling;
using PicView.Core.ImageDecoding;
using ReactiveUI;

namespace PicView.Avalonia.ViewModels;

public class PicViewerModel : ReactiveObject
{
    public FileInfo? FileInfo
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// The image's pixel width
    /// </summary>
    public int PixelWidth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// The image's pixel height
    /// </summary>
    public int PixelHeight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    
    public object? ImageSource
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public object? SecondaryImageSource
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ImageType ImageType
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// The width to scale the image to
    /// </summary>
    public double ImageWidth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// The height to scale the image to
    /// </summary>
    public double ImageHeight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double SecondaryImageWidth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsShowingSideBySide
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double ScrollViewerWidth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = double.NaN;

    public double ScrollViewerHeight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = double.NaN;

    public double AspectRatio
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ImageEffectConfig? EffectConfig
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public EXIFHelper.EXIFOrientation? ExifOrientation
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    // Used to flip the flip button
    public int ScaleX
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 1;
}
