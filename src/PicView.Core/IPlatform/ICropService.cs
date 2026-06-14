using PicView.Core.ViewModels;

namespace PicView.Core.IPlatform;

public interface ICropService
{
    bool IsCropping { get; }
    Task StartCropControlAsync(MainWindowViewModel vm);
    void CloseCropControl();
    
    object? GetCroppedImage();
}
