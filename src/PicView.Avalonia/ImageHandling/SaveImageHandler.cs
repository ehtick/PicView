using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;

namespace PicView.Avalonia.ImageHandling;

public static class SaveImageHandler
{
    public static async Task SaveImageWithPossibleNavigation(MainViewModel vm,
        string path,
        string destination,
        bool sameFile,
        string ext,
        uint? width,
        uint? height,
        uint? quality,
        uint? rotationAngle,
        bool isKeepingAspectRatio)
    {

        var success = await SaveImageFileHelper.SaveImageAsync(
            null, path, sameFile ? null : destination, width, height, quality,
            ext, rotationAngle, null, isKeepingAspectRatio).ConfigureAwait(false);

        if (!success)
        {
            TooltipHelper.ShowTooltipMessage(TranslationManager.Translation.SavingFileFailed);
            return;
        }
        
        if (Path.GetExtension(path) != ext && sameFile)
        {
            // Delete the old file
            await vm.PlatformService.DeleteFile(path, true); 
        }

        if (destination == path)
        {
            await NavigationManager.QuickReload().ConfigureAwait(false);
        }
        else if (Path.GetDirectoryName(path) == Path.GetDirectoryName(destination))
        {
            // Load the file if saved within same directory
            await NavigationManager.LoadPicFromFile(destination, vm).ConfigureAwait(false);
        }
    }
}