using PicView.Core.Update;

namespace PicView.Core.IPlatform;

public interface IPlatformSpecificUpdate
{
    public Task HandlePlatofrmUpdate(UpdateInfo updateInfo, string tempPath);
}