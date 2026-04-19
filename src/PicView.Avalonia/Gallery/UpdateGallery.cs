using Avalonia;
using PicView.Avalonia.Navigation.Services;
using PicView.Core.Gallery;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Gallery;

// TODO deprecated, delete
public static class UpdateGallery
{
    public static async ValueTask LoadGalleryIfDockedOrExpanded(TabViewModel tabViewModel, GalleryMode2 mode)
    {
        if (mode is GalleryMode2.Docked or GalleryMode2.Expanded)
        {
            if (tabViewModel.Gallery.LoadingState is GalleryLoadingState.NotLoaded)
            {
                if (Application.Current.DataContext is not CoreViewModel core)
                {
                    return;
                }
                await GalleryLoader.LoadGalleryAsync(tabViewModel,
                        tabViewModel.ImageIterator.Files,
                        new AvaloniaThumbnailLoader(),
                        core.SharedThumbnailCache,
                        tabViewModel.GetTabCancellation().Token)
                    .ConfigureAwait(false);
            }
        }
    }
    
    public static async ValueTask ToggleGalleryAndLoadItem(TabViewModel tabViewModel, int index)
    {
        if (tabViewModel.ImageIterator is null)
        {
            return;
        }
        if (tabViewModel.Gallery.IsGalleryExpanded.Value)
        {
            tabViewModel.Gallery.ToggleGalleryCommand.Execute(Unit.Default);
        }

        await tabViewModel.ImageIterator.SkipToIndexAsync(index, tabViewModel.GetTabCancellation()).ConfigureAwait(false);
    }
}