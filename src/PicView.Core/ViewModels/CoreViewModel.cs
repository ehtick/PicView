using PicView.Core.IPlatform;
using PicView.Core.Models;
using PicView.Core.Navigation;

namespace PicView.Core.ViewModels;

public class CoreViewModel(
    IPlatformSpecificService? platformSpecificService,
    Func<FileInfo, ValueTask<ImageModel>> imageLoader)
{
    // Shared Services
    public IPlatformSpecificService? PlatformService { get; } = platformSpecificService;
    public SharedImageCache SharedCache { get; } = new(imageLoader);
    public ThumbnailCache SharedThumbnailCache { get; } = new();
    
    // --- Globally Shared State ---
    public TranslationViewModel Translation { get; } = new();
    public GlobalSettingsViewModel? GlobalSettings { get; } = new();
    public GallerySharedSettingsViewModel GallerySettings { get; } = new();
    public KeybindingsViewModel? Keybindings { get; set; }
    public SettingsViewModel? SettingsViewModel { get; set; } // Single settings window

    // --- Overview models ---
    public MainWindowOverviewViewModel MainWindows { get; } = new();
    /// Bindable reactive properties that are Exif related
    public List<ExifViewModel> Exifs { get; } = []; 
    /// View models for the image info view
    public List<ImageInfoWindowViewModel?> InfoWindows { get; } = [];
}