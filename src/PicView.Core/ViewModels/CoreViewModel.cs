using PicView.Core.IPlatform;
using R3;

namespace PicView.Core.ViewModels;

public class CoreViewModel
{
    public readonly IPlatformSpecificService? PlatformService;
    public readonly IPlatformWindowService? PlatformWindowService;
    
    // --- Shared view models ---
    
    public TranslationViewModel Translation { get; } = new();
    public GlobalSettingsViewModel GlobalSettings { get; } = new(); // Bindable reactive properties for the entire UI
    public SettingsViewModel? SettingsViewModel { get; set; } // View model for the settings view (max 1 window)
    
    public SharedNavigationViewModel SharedNavigation { get; } = new();
    
    //public AboutViewModel? AboutView { get; set; }

    public KeybindingsViewModel? Keybindings { get; set; }
    //public WindowViewModel Window { get; } = new();

    // --- Collection of view models ---
    
    /// Tracks the correct position of the active window
    public BindableReactiveProperty<int> MainWindowIndex  { get; } = new(); 
    public List<TabOverviewViewModel> TabsCollection { get; } = [];
    /// Bindable reactive properties that are Exif related
    public List<ExifViewModel> Exifs { get; } = []; 
    /// View models for the image info view
    public List<ImageInfoWindowViewModel?> InfoWindows { get; } = [];  
    
    public CoreViewModel(IPlatformSpecificService? platformSpecificService, IPlatformWindowService? platformWindowService)
    {
        PlatformService = platformSpecificService;
        PlatformWindowService = platformWindowService;
    }

    public CoreViewModel()
    {
        // Only use for unit test
    }

}