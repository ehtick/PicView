using Avalonia.Threading;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.Navigation.Interfaces;

namespace PicView.Avalonia.Navigation.Services;

public class AvaloniaThumbnailLoader : IThumbnailLoader
{
    public async ValueTask<object?> GetThumbnailAsync(FileInfo file)
    {
        var vm = await Dispatcher.UIThread.InvokeAsync(() => UIHelper.GetMainView.DataContext as MainViewModel);
        
        return await GetThumbnails.GetThumbAsync(file, (uint)vm.Gallery.GalleryItem.ItemHeight.Value).ConfigureAwait(false);
    }

    public async ValueTask<object?> GetThumbnailAsync(FileInfo file, uint size)
    {
        return await GetThumbnails.GetThumbAsync(file, size).ConfigureAwait(false);
    }
}