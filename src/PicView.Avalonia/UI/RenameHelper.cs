using ImageMagick;
using PicView.Avalonia.Input;
using PicView.Core.FileHandling;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.UI;

public static class RenameHelper
{
    public static void Rename(MainWindowViewModel vm)
    {
        if (!vm.IsTopToolbarShown.CurrentValue || vm.IsFullscreen.CurrentValue)
        {
            DialogManager.AddRenameDialog();
            return;
        }
        UIHelper.GetEditableTitlebar.SelectFileName();   
    }

    public static void RenameAction(MainWindowViewModel vm, string newName)
    {
        vm.IsLoadingIndicatorShown.Value = true;
        var tab = vm.WindowTabs.ActiveTab.CurrentValue;
        
        var oldPath = tab.FileInfo.CurrentValue.FullName;
        var newPath = Path.Combine(tab.FileInfo.CurrentValue.DirectoryName, newName);
        Task.Run(async () =>
        {
            if (newPath == oldPath)
            {
                // TODO
                //ShowFileExistsError(vm);
                return;
            }
    
            var currentExtension = Path.GetExtension(oldPath);
            var newExtension = Path.GetExtension(newPath);
            if (currentExtension.Equals(newExtension, StringComparison.OrdinalIgnoreCase))
            {
                FileHelper.RenameFile(oldPath, newPath);
            }
            else
            {
                using var magick = new MagickImage(oldPath);
                await magick.WriteAsync(newPath);
                File.Delete(oldPath);
                await tab.ImageIterator.ReloadAsync(tab.GetTabCancellation()).ConfigureAwait(false);
            }

            var newFileInfo = new FileInfo(newPath);
            tab.FileInfo.Value = newFileInfo;
            tab.Model.FileInfo = newFileInfo;
            tab.UpdateTabTitle();
            vm.IsLoadingIndicatorShown.Value = false;
        });
        
        UIHelper.GetMainView.Focus();
        MainKeyboardShortcuts.IsKeysEnabled = true;
    }
}