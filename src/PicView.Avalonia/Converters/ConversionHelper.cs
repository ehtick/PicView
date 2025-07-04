using ImageMagick;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.DebugTools;
using PicView.Core.ImageDecoding;

namespace PicView.Avalonia.Converters;

internal static class ConversionHelper
{
    public static async Task<bool> ResizeImageByPercentage(FileInfo fileInfo, int selectedIndex)
    {
        var percentage = 100 - selectedIndex * 5;

        if (percentage is < 5 or > 100)
        {
            return false;
        }

        var magickPercentage = new Percentage(percentage);
        return await SaveImageFileHelper.ResizeImageAsync(fileInfo, 0, 0, 100, magickPercentage).ConfigureAwait(false);
    }
    
    public static async Task ResizeImageByPercentage(int percentage, MainViewModel vm)
    {
        TitleManager.SetLoadingTitle(vm);
        var success = await ResizeImageByPercentage(vm.PicViewer.FileInfo.CurrentValue, percentage);
        if (success)
        {
            await NavigationManager.QuickReload();
        }
        else
        {
            TitleManager.SetTitle(vm);
        }
    }

    public static async Task<bool> ResizeByWidth(FileInfo fileInfo, double width)
    {
        if (width <= 0)
        {
            return false;
        }

        return await SaveImageFileHelper.ResizeImageAsync(fileInfo, (uint)width, 0).ConfigureAwait(false);
    }

    public static async Task<bool> ResizeByHeight(FileInfo fileInfo, double height)
    {
        if (height <= 0)
        {
            return false;
        }

        return await SaveImageFileHelper.ResizeImageAsync(fileInfo, 0, (uint)height).ConfigureAwait(false);
    }

    public static async Task<string> ConvertTask(FileInfo fileInfo, int selectedIndex, IPlatformSpecificService platform)
    {
        var currentExtension = fileInfo.Extension.ToLower();
        var newExtension = selectedIndex switch
        {
            1 => ".png",
            2 => ".jpg",
            3 => ".webp",
            4 => ".avif",
            5 => ".heic",
            6 => ".jxl",
            _ => currentExtension
        };
        if (currentExtension == newExtension)
        {
            return string.Empty;
        }
        var oldPath = fileInfo.FullName;
        var newPath = Path.ChangeExtension(fileInfo.FullName, newExtension);

        var success = await SaveImageFileHelper.SaveImageAsync(null, oldPath, null, null, null, null,
            newExtension);
        if (!success)
        {
            return string.Empty;
        }

        await platform.DeleteFile(oldPath, true);
        return newPath;
    }
    
    public static async Task ConvertFileExtension(int index, MainViewModel vm)
    {
        if (vm.PicViewer.FileInfo is null)
        {
            return;
        }

        var newPath = await ConvertTask(vm.PicViewer.FileInfo.CurrentValue, index, vm.PlatformService);
        if (!string.IsNullOrWhiteSpace(newPath))
        {
            await NavigationManager.LoadPicFromStringAsync(newPath, vm);
        }
    }
    
    public static void DetermineIfOptimizeImageShouldBeEnabled(MainViewModel vm)
    {
        if (vm.PicViewer.FileInfo is null)
        {
            vm.ShouldOptimizeImageBeEnabled = false;
            return;
        }

        try
        {
            if (vm.PicViewer.FileInfo.CurrentValue.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase)
                || vm.PicViewer.FileInfo.CurrentValue.Extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                || vm.PicViewer.FileInfo.CurrentValue.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase)
                || vm.PicViewer.FileInfo.CurrentValue.Extension.Equals(".gif", StringComparison.InvariantCultureIgnoreCase))
            {
                vm.ShouldOptimizeImageBeEnabled = true;
            }
            else
            {
                vm.ShouldOptimizeImageBeEnabled = false;
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(ConversionHelper), nameof(DetermineIfOptimizeImageShouldBeEnabled), e);
            vm.ShouldOptimizeImageBeEnabled = false;
        }
    }
}