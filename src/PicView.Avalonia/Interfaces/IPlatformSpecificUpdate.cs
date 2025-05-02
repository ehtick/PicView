using PicView.Avalonia.Update;

namespace PicView.Avalonia.Interfaces;

public interface IPlatformSpecificUpdate
{
    public Task HandlePlatofrmUpdate(UpdateInfo updateInfo, string tempPath);
}