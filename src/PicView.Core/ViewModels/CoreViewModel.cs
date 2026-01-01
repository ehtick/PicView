using PicView.Core.IPlatform;
using PicView.Core.Models;
using PicView.Core.Navigation;
using R3;

namespace PicView.Core.ViewModels;

public class CoreViewModel
{
    // Shared Services
    public IPlatformSpecificService? PlatformService { get; }
    public IPlatformWindowService? PlatformWindowService { get; }
    public SharedImageCache SharedCache { get; }
    
    // --- Globally Shared State ---
    public TranslationViewModel Translation { get; } = new();
    public GlobalSettingsViewModel? GlobalSettings { get; } = new();
    public KeybindingsViewModel? Keybindings { get; set; }
    public SettingsViewModel? SettingsViewModel { get; set; } // Single settings window
    
    // --- Shared Navigation Services ---
    public SharedNavigationViewModel SharedNavigation { get; } = new();

    // --- Overview models ---
    public MainWindowOverviewViewModel MainWindows { get; } = new();
    public List<TabOverviewViewModel> TabsCollection { get; } = [];
    /// Bindable reactive properties that are Exif related
    public List<ExifViewModel> Exifs { get; } = []; 
    /// View models for the image info view
    public List<ImageInfoWindowViewModel?> InfoWindows { get; } = [];  
    
    public CoreViewModel(
        IPlatformSpecificService? platformSpecificService, 
        IPlatformWindowService? platformWindowService,
        Func<FileInfo, ValueTask<ImageModel>> imageLoader)
    {
        PlatformService = platformSpecificService;
        PlatformWindowService = platformWindowService;
        SharedCache = new SharedImageCache(imageLoader);
    }

    public CoreViewModel()
    {
        // Only use for unit test
    }

}