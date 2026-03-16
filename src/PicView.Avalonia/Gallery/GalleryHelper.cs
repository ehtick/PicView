using Avalonia.Media;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Gallery;

public static class GalleryHelper
{
    public static void SetGalleryItemStretch(string value) => SetGalleryItemStretch(value, UIHelper.GetMainView.DataContext as MainViewModel);
    public static void SetGalleryItemStretch(string value, MainViewModel vm)
    {
        if (value.Equals("Square", StringComparison.OrdinalIgnoreCase))
        {
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                GalleryStretchMode.ChangeFullGalleryStretchSquare(vm);
            }
            else
            {
                GalleryStretchMode.ChangeBottomGalleryStretchSquare(vm);
            }

            return;
        }

        if (value.Equals("FillSquare", StringComparison.OrdinalIgnoreCase))
        {
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                GalleryStretchMode.ChangeFullGalleryStretchSquareFill(vm);
            }
            else
            {
                GalleryStretchMode.ChangeBottomGalleryStretchSquareFill(vm);
            }

            return;
        }

        if (Enum.TryParse<Stretch>(value, out var stretch))
        {
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                GalleryStretchMode.ChangeFullGalleryItemStretch(vm, stretch);
            }
            else
            {
                GalleryStretchMode.ChangeBottomGalleryItemStretch(vm, stretch);
            }
        }
    }
    
    public static double GetGalleryHeight(GallerySharedSettingsViewModel gallery, MainWindowViewModel main)
    {
        if (!Settings.Gallery.IsGalleryDocked || Slideshow.IsRunning)
        {
            return 0;
        }
        if (!Settings.Gallery.ShowBottomGalleryInHiddenUI && !main.IsUIShown.CurrentValue)
        {
            return 0;
        }

        return Settings.Gallery.IsGalleryDocked
            ? gallery.DockedGalleryItemSize.CurrentValue + (SizeDefaults.ScrollbarSize - 1)
            : 0;
    }
}