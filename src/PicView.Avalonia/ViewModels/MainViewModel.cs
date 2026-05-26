using PicView.Avalonia.Interfaces;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.ViewModels;

// TODO deprecated, delete
public class MainViewModel
{
    public readonly IPlatformSpecificService? PlatformService;
    
    public TranslationViewModel Translation { get; } = new();
    public FileAssociationsViewModel? AssociationsViewModel { get; set; }
}