using PicView.Avalonia.Interfaces;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.ViewModels;

// TODO deprecated, delete
public class MainViewModel
{
    public readonly IPlatformSpecificService? PlatformService;
    public readonly IPlatformWindowService? PlatformWindowService;
    
    public TranslationViewModel Translation { get; } = new();
    public GlobalSettingsViewModel GlobalSettings { get; } = new();
    public SettingsViewModel? SettingsViewModel { get; set; }
    public ImageInfoWindowViewModel? InfoWindow { get; set; }
    public FileAssociationsViewModel? AssociationsViewModel { get; set; }
}