using ImageMagick;

namespace PicView.Core.ImageEffects;

public class ImageEffectConfig
{
    public Percentage Brightness { get; set; }
    public Percentage Contrast  { get; set; }
    public Percentage Solarize { get; set; }
    
    public double SketchStrokeWidth { get; set; }
    
    public int PosterizeLevel { get; set; }
    
    public bool Negative { get; set; }
    public bool BlackAndWhite { get; set; }
    public bool OldMovie { get; set; }
    public double BlurLevel { get; set; }
    
}
