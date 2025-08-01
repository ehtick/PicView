namespace PicView.Core.Exif;

// https://exiftool.org/TagNames/EXIF.html
// 1 = Horizontal (normal)
// 2 = Mirror horizontal
// 3 = Rotate 180
// 4 = Mirror vertical
// 5 = Mirror horizontal and rotate 270 CW
// 6 = Rotate 90 CW
// 7 = Mirror horizontal and rotate 90 CW
// 8 = Rotate 270 CW
public enum ExifOrientation
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