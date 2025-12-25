using PicView.Core.IPlatform;

namespace PicView.Core.ViewModels;

public class CoreViewModel
{
    public readonly IPlatformSpecificService? PlatformService;
    public readonly IPlatformWindowService? PlatformWindowService;
    
    // Shared view models
    public TranslationViewModel Translation { get; } = new();
    public GlobalSettingsViewModel GlobalSettings { get; } = new();
    public SettingsViewModel? SettingsViewModel { get; set; }
    //public GalleryViewModel SharedGallery { get; } = new();
    public ExifViewModel? Exif { get; set; }
    public KeybindingsViewModel? Keybindings { get; set; }
    //public WindowViewModel Window { get; } = new();


    // Collection view models
    public List<TabOverviewViewModel> TabsCollection { get; } = [];

}