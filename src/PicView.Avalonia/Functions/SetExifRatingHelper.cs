using PicView.Avalonia.ViewModels;
using PicView.Core.Exif;

namespace PicView.Avalonia.Functions;

public static class SetExifRatingHelper
{
    private static bool CanProceed(MainViewModel vm)
    {
        return vm?.PicViewer.FileInfo?.CurrentValue is not null;
    }

    private static void SetValue(MainViewModel vm, uint value)
    {
        if (vm.Exif is not null)
        {
            vm.Exif.ExifRating.Value = value;
        }
    }

    public static async Task Set0Star(MainViewModel vm)
    {
        if (!CanProceed(vm))
        {
            return;
        }

        await ExifWriter.SetExifRatingAsync(vm.PicViewer.FileInfo.CurrentValue, 0);
        SetValue(vm, 0);
    }

    public static async Task Set1Star(MainViewModel vm)
    {
        if (!CanProceed(vm))
        {
            return;
        }

        await ExifWriter.SetExifRatingAsync(vm.PicViewer.FileInfo.CurrentValue, 1);
        SetValue(vm, 1);
    }

    public static async Task Set2Star(MainViewModel vm)
    {
        if (!CanProceed(vm))
        {
            return;
        }

        await ExifWriter.SetExifRatingAsync(vm.PicViewer.FileInfo.CurrentValue, 2);
        SetValue(vm, 2);
    }

    public static async Task Set3Star(MainViewModel vm)
    {
        if (!CanProceed(vm))
        {
            return;
        }

        await ExifWriter.SetExifRatingAsync(vm.PicViewer.FileInfo.CurrentValue, 3);
        SetValue(vm, 3);
    }

    public static async Task Set4Star(MainViewModel vm)
    {
        if (!CanProceed(vm))
        {
            return;
        }

        await ExifWriter.SetExifRatingAsync(vm.PicViewer.FileInfo.CurrentValue, 4);
        SetValue(vm, 4);
    }

    public static async Task Set5Star(MainViewModel vm)
    {
        if (!CanProceed(vm))
        {
            return;
        }

        await ExifWriter.SetExifRatingAsync(vm.PicViewer.FileInfo.CurrentValue, 5);
        SetValue(vm, 5);
    }
}