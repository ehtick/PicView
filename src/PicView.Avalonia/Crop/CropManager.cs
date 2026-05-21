using PicView.Avalonia.UI;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Crop;

public static class CropManager
{
    public static bool IsCropping => GetService()?.IsCropping ?? false;

    private static CropService? GetService()
    {
        if (UIHelper.GetMainView.DataContext is not MainWindowViewModel vm)
        {
            return null;
        }

        var activeTab = vm.WindowTabs.ActiveTab.Value;
        if (activeTab is null)
        {
            return null;
        }

        if (activeTab.CropService is not CropService service)
        {
            activeTab.CropService = service = new CropService(activeTab);
        }

        return service;
    }

    /// <summary>
    /// Starts the cropping functionality by setting up the ImageCropperViewModel 
    /// and adding the CropControl to the main view.
    /// </summary>
    /// <param name="vm">The main view model instance containing image properties and state.</param>
    /// <remarks>
    /// This method checks if cropping can be enabled and if the image source is valid.
    /// If conditions are met, it configures the crop control with the appropriate dimensions
    /// and updates the view model's title and tooltip to reflect the cropping state.
    /// </remarks>
    public static async Task StartCropControlAsync(MainWindowViewModel vm)
    {
        var activeTab = vm.WindowTabs.ActiveTab.Value;
        if (activeTab is null)
        {
            return;
        }

        if (activeTab.CropService is not CropService service)
        {
            activeTab.CropService = service = new CropService(activeTab);
        }

        await service.StartCropControlAsync(vm);
    }

    public static void CloseCropControl(MainWindowViewModel vm)
    {
        var activeTab = vm.WindowTabs.ActiveTab.Value;

        (activeTab?.CropService as CropService)?.CloseCropControl();
    }
}