using PicView.Core.Localization;

namespace PicView.Core.FileAssociations;

public static class FileTypeHelper
{
    public static FileTypeGroup[] GetFileTypes()
    {
        var groups = new[]
        {
            new FileTypeGroup(TranslationManager.Translation.Normal, [
                new FileTypeItem("Joint Photographic Experts Group", [".jpg", ".jpeg", ".jpe"]),
                new FileTypeItem("JPEG File Interchange Format", [".jfif"]),
                new FileTypeItem("Portable Network Graphics", [".png"]),
                new FileTypeItem("Windows Bitmap", [".bmp"]),
                new FileTypeItem("Graphics Interchange Format", [".gif"]),
                new FileTypeItem("WebP", [".webp"]),
                new FileTypeItem("Wireless Bitmap", [".wbmp"]),
                new FileTypeItem("Advanced Video Interlace Format", [".avif"]),
                new FileTypeItem("Icon", [".ico"])
            ]),

            new FileTypeGroup(TranslationManager.Translation.Graphics, [
                new FileTypeItem("Scalable Vector Graphics", [".svg", ".svgz"], null),
                new FileTypeItem("Photoshop", [".psd", ".psb"], null),
                new FileTypeItem("XCF", [".xcf"], null),
                new FileTypeItem("Tagged Image File Format", [".tif", ".tiff"]),
                new FileTypeItem("High-Enhanced Image File", [".heic", ".heif"]),
                new FileTypeItem("JPEG XL", [".jxl"]),
                new FileTypeItem("JPEG 2000", [".jp2"]),
                new FileTypeItem("High Dynamic Range", [".hdr"]),
                new FileTypeItem("Quite OK Image", [".qoi"]),
                new FileTypeItem("Direct Draw Surface", [".dds"]),
                new FileTypeItem("Truevision Targa", [".tga"]),
                new FileTypeItem("Industrial Light & Magic OpenEXR", [".exr"])
            ]),

            new FileTypeGroup(TranslationManager.Translation.RawCamera, [
                new FileTypeItem("Raw", [".raw"]),
                new FileTypeItem("Framed Raster", [".3fr"]),
                new FileTypeItem("Sony Digital Camera RAW", [".arw"]),
                new FileTypeItem("Canon Digital Camera RAW", [".cr2, .cr3, .crw"]),
                new FileTypeItem("Kodak Raw", [".dcr", ".kdc"]),
                new FileTypeItem("Digital Negative RAW", [".dng"]),
                new FileTypeItem("Epson Raw Image", [".erf"]),
                new FileTypeItem("Minolta Raw Image", [".mdc"]),
                new FileTypeItem("Nikon Raw Image", [".nef"]),
                new FileTypeItem("Mamiya Raw Image", [".mef"]),
                new FileTypeItem("Leaf/Aptus/Mamiya MOS Raw Image", [".mos"]),
                new FileTypeItem("Minolta Dimage RAW", [".mrw"]),
                new FileTypeItem("Nikon Raw Image", [".nef"]),
                new FileTypeItem("Nokia RAW Bitmap", [".nrw"]),
                new FileTypeItem("Olympus Raw Image", [".orf"]),
                new FileTypeItem("Pentax Raw Image", [".pef"]),
                new FileTypeItem("Sony SRF Raw", [".srf"]),
                new FileTypeItem("Sigma Foveon X3", [".x3f"]),
                new FileTypeItem("Kodak FlashPix Bitmap", [".fpx"]),
                new FileTypeItem("Kodak PhotoCD Bitmap", [".pcd"]),
                new FileTypeItem("Kodak Raw", [".dcr"]),
                new FileTypeItem("Windows Metafile Image", [".wmf", ".emf"])
            ]),

            new FileTypeGroup(TranslationManager.Translation.Uncommon, [
                new FileTypeItem("Wordperfect Graphics", [".wpg"]),
                new FileTypeItem("Paintbrush bitmap graphics", [".pcx"], null),
                new FileTypeItem("X Bitmap", [".xbm"]),
                new FileTypeItem("PX PixMap Bitmap", [".xpm"]),
                new FileTypeItem("Dr. Halo ", [".cut"]),
                new FileTypeItem("Truevision Thumb", [".thm"]),
                new FileTypeItem("Portable GrayMap Bitmap", [".ppm"]),
                new FileTypeItem("Portable PixMap Bitmap", [".pbm"]),
                new FileTypeItem("Base64", [".b64"])
            ]),

            new FileTypeGroup(TranslationManager.Translation.Archives, [
                new FileTypeItem("Zip", [".zip"], null),
                new FileTypeItem("Rar", [".rar"], null),
                new FileTypeItem("Gzip", [".gzip"], null),
                new FileTypeItem("CDisplay Archived Comic Book", [".cbr, .cbz, .cb7"])
            ], null)
        };

        return groups;
    }
}