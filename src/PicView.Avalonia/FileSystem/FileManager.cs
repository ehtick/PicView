using PicView.Avalonia.Animations;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC.PopUps;
using PicView.Core.FileHandling;
using PicView.Core.Localization;

namespace PicView.Avalonia.FileSystem;

public static class FileManager
{
    public static async Task DeleteFile(bool recycle, MainViewModel vm)
    {
        if (vm.FileInfo is null)
        {
            return;
        }
        
        var errorMsg = string.Empty;
        
        if(!recycle)
        {
            var prompt = $"{TranslationHelper.GetTranslation("DeleteFilePermanently")}";
            var deleteDialog = new DeleteDialog(prompt, vm.FileInfo.FullName);
            UIHelper.GetMainView.MainGrid.Children.Add(deleteDialog);
        }
        else
        {
            errorMsg = await Task.FromResult(FileDeletionHelper.DeleteFileWithErrorMsg(vm.FileInfo.FullName, recycle));
        }

        if (!string.IsNullOrEmpty(errorMsg))
        {
            await TooltipHelper.ShowTooltipMessageAsync(errorMsg, true);
        }
    }
    
    public static async Task DuplicateFile(string path, MainViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        vm.IsLoading = true;
        if (path == vm.FileInfo.FullName)
        {
            await FunctionsHelper.DuplicateFile();
        }
        else
        {
            var duplicatedPath = await FileHelper.DuplicateAndReturnFileNameAsync(path);
            if (!string.IsNullOrWhiteSpace(duplicatedPath))
            {
                await AnimationsHelper.CopyAnimation();
            }
        }
        vm.IsLoading = false;
    }
    
    public static async Task ShowFileProperties(string path, MainViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }
        if (vm.PlatformService is null)
        {
            return;
        }
        await Task.Run(() =>
        {
            vm.PlatformService.ShowFileProperties(path);
        });
    }
    
    public static async Task Print(string path, MainViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }
        if (vm.PlatformService is null)
        {
            return;
        }
        await Task.Run(() =>
        {
            vm.PlatformService?.Print(path);
        });
    }
    
    public static async Task LocateOnDisk(string path, MainViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }
        if (vm.PlatformService is null)
        {
            return;
        }
        await Task.Run(() =>
        {
            vm.PlatformService?.LocateOnDisk(path);
        });
    }
    
    public static async Task OpenWith(string path, MainViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }
        if (vm.PlatformService is null)
        {
            return;
        }
        await Task.Run(() =>
        {
            vm.PlatformService?.OpenWith(path);
        });
    }
}
