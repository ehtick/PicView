using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using PicView.Core.FileHandling;
using PicView.Core.Localization;
using ReactiveUI;

namespace PicView.Core.ViewModels;

public class FileAssociationsViewModel : ReactiveObject
{
    private readonly ReadOnlyObservableCollection<FileTypeGroup> _fileTypeGroups;
    private readonly SourceList<FileTypeGroup> _fileTypeGroupsList = new();

    public ReadOnlyObservableCollection<FileTypeGroup> FileTypeGroups => _fileTypeGroups;

    public string? FilterText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    public ReactiveCommand<Unit, Unit> ApplyCommand { get; }
    public ReactiveCommand<Unit, string> ClearFilterCommand { get; }
    
    public FileAssociationsViewModel()
    {
        // Create file type groups and populate with data
        InitializeFileTypes();
        
        // Setup the filtering
        var filter = this.WhenAnyValue(x => x.FilterText)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Select(BuildFilter);
            
        _fileTypeGroupsList.Connect()
            .AutoRefresh()
            .Filter(filter)
            .Bind(out _fileTypeGroups)
            .Subscribe();
            
        // Initialize commands
        ApplyCommand = ReactiveCommand.CreateFromTask(async () => await ApplyFileAssociations());
        ClearFilterCommand = ReactiveCommand.Create(() => FilterText = string.Empty);
    }
    
    private Func<FileTypeGroup, bool> BuildFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            // Reset all items to visible when filter is empty
            foreach (var group in _fileTypeGroupsList.Items)
            {
                foreach (var item in group.FileTypes)
                {
                    item.IsVisible = true;
                }
            }
            return _ => true;
        }
        
        return group => {
            // Update visibility of items based on filter
            var anyVisible = false;
            foreach (var item in group.FileTypes)
            {
                item.IsVisible = item.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) || 
                                 item.Extension.Contains(filter, StringComparison.OrdinalIgnoreCase);
                if (item.IsVisible)
                    anyVisible = true;
            }
        
            // Only show groups that have at least one visible item
            return anyVisible;
        };
    }
    
    private void SyncUIStateToViewModel()
    {
        // Force property notifications to ensure all changes are processed
        foreach (var group in FileTypeGroups)
        {
            group.IsSelected = group.IsSelected;
            foreach (var fileType in group.FileTypes)
            {
                fileType.IsSelected = fileType.IsSelected;
            }
        }
    }

    private async Task ApplyFileAssociations()
    {
        // Ensure all UI changes are synced to the ViewModel
        SyncUIStateToViewModel();
        
        // Now process the associations
        foreach (var group in FileTypeGroups)
        {
            foreach (var fileType in group.FileTypes)
            {
                foreach (var extension in fileType.Extensions)
                {
                    // Make sure to properly handle extensions that contain commas
                    var individualExtensions = extension.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
    
                    foreach (var ext in individualExtensions)
                    {
                        var cleanExt = ext.Trim();
                        if (!cleanExt.StartsWith('.'))
                            cleanExt = "." + cleanExt;
            
                        if (fileType.IsSelected.HasValue)
                        {
                            if (fileType.IsSelected.Value)
                                await FileAssociationManager.AssociateFile(cleanExt);
                            else
                                await FileAssociationManager.UnassociateFile(cleanExt);
                        }
                    }
                }
            }
        }
    }
    
    public void InitializeFileTypes()
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
            
            new FileTypeGroup(TranslationManager.GetTranslation("Graphics"), [
                new FileTypeItem("Scalable Vector Graphics", [".svg", ".svgz"]),
                new FileTypeItem("Photoshop", [".psd", ".psb"]),
                new FileTypeItem("XCF", [".xcf"]),
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
            
            new FileTypeGroup(TranslationManager.GetTranslation("Raw"), [
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
                new FileTypeItem("Windows Metafile Image", [".wmf", ".emf"]),
            ]),
            
            new FileTypeGroup(TranslationManager.GetTranslation("Uncommon"), [
                new FileTypeItem("Wordperfect Graphics", [".wpg"]),
                new FileTypeItem("Paintbrush bitmap graphics", [".pcx"]),
                new FileTypeItem("X Bitmap", [".xbm"]),
                new FileTypeItem("PX PixMap Bitmap", [".xpm"]),
                new FileTypeItem("Dr. Halo ", [".cut"]),
                new FileTypeItem("Truevision Thumb", [".thm"]),
                new FileTypeItem("Portable GrayMap Bitmap", [".ppm"]),
                new FileTypeItem("Portable PixMap Bitmap", [".pbm"]),
                new FileTypeItem("Base64", [".b64"])
            ]),
            
            new FileTypeGroup(TranslationManager.GetTranslation("Archives"), [
                new FileTypeItem("Zip", [".zip"], null),
                new FileTypeItem("Rar", [".rar"], null),
                new FileTypeItem("Gzip", [".gzip"], null),
                new FileTypeItem("CDisplay Archived Comic Book", [".cbr, .cbz, .cb7"])
            ], null)
        };
        
        _fileTypeGroupsList.Edit(list =>
        {
            list.Clear();
            list.AddRange(groups);
        });
    }
}

public class FileTypeGroup : ReactiveObject
{
    public string Name { get; set; }
    public ObservableCollection<FileTypeItem> FileTypes { get; }

    public bool? IsSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public FileTypeGroup(string name, IEnumerable<FileTypeItem> fileTypes, bool? isSelected = true)
    {
        Name = name;
        FileTypes = new ObservableCollection<FileTypeItem>(fileTypes);
        IsSelected = isSelected;
    }
}

public class FileTypeItem : ReactiveObject
{
    public string Description { get; }
    public string[] Extensions { get; }
    
    public string Extension => string.Join(", ", Extensions);

    public bool? IsSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsVisible
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    public FileTypeItem(string description, string[] extensions, bool? isSelected = true)
    {
        Description = description;
        Extensions = extensions;
        IsSelected = isSelected;
    }
}

