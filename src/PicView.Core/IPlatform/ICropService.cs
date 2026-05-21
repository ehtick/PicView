using PicView.Core.ViewModels;

namespace PicView.Core.IPlatform;

public interface ICropService
{
    bool IsCropping { get; }
    Task StartCropControlAsync(MainWindowViewModel vm);
    void CloseCropControl();
    bool DetermineIfShouldBeEnabled(MainWindowViewModel vm);
    
    object? GetCroppedImage();
}
