using ImageMagick;
using PicView.Core.Exif;
using PicView.Core.ImageDecoding;
using PicView.Core.Navigation.Tiff;

namespace PicView.Core.Models;

public record ImageModel
{
    public object? Image { get; set; }
    public FileInfo? FileInfo { get; set; }
    public uint PixelWidth { get; set; }
    public uint PixelHeight { get; set; }
    public ExifOrientation? Orientation { get; set; }
    public ImageType ImageType { get; set; }
    public ushort DpiX { get; set; }
    public ushort DpiY { get; set; }
    
    public TiffNavigationInfo? TiffNavigation { get; set; }

    public int Rotation
    {
        get
        {
            if (!Orientation.HasValue)
            {
                return 0;
            }

            return Orientation switch
            {
                ExifOrientation.None or ExifOrientation.Horizontal or ExifOrientation.MirrorHorizontal => 0,
                ExifOrientation.Rotate180 or ExifOrientation.MirrorVertical => 180,
                ExifOrientation.MirrorHorizontalRotate270Cw or ExifOrientation.Rotated270Cw => 90,
                ExifOrientation.Rotate90Cw => 90,
                ExifOrientation.MirrorHorizontalRotate90Cw => 270,
                _ => 0
            };
        }
    }
}