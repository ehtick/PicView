using PicView.Avalonia.Functions;
using PicView.Avalonia.Interfaces;
using PicView.Core.ViewModels;
using ImageViewer = PicView.Avalonia.Views.UC.ImageViewer;

namespace PicView.Avalonia.ViewModels;

// TODO deprecated, delete
public class MainViewModel
{
    public readonly IPlatformSpecificService? PlatformService;
    public readonly IPlatformWindowService? PlatformWindowService;
    
    public TranslationViewModel Translation { get; } = new();
    public GlobalSettingsViewModel GlobalSettings { get; } = new();
    public SettingsViewModel? SettingsViewModel { get; set; }
    public PicViewerModel PicViewer { get; } = new();
    public ExifViewModel? Exif { get; set; }
    public ImageInfoWindowViewModel? InfoWindow { get; set; }
    public FileAssociationsViewModel? AssociationsViewModel { get; set; }
    public BatchResizeViewModel? BatchResizeViewModel { get; set; }

    public MainViewModel(IPlatformSpecificService? platformSpecificService, IPlatformWindowService? platformWindowService)
    {
        FunctionsMapper.Vm = this;
        PlatformService = platformSpecificService;
        PlatformWindowService = platformWindowService;
    }

    public MainViewModel()
    {
        // Only use for unit test
    }
    
}