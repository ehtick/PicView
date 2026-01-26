using System.Collections.ObjectModel;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.ViewModels;

namespace PicView.Core.Gallery;

public class GalleryLoaderService
{
    public async Task LoadGalleryAsync(TabViewModel tab, IReadOnlyList<FileInfo> files, IThumbnailLoader thumbnailLoader, CancellationToken ct)
    {
        // Create new collection
        var newItems = new ObservableCollection<GalleryItemViewModel>();

        var currentHeight = Settings.Gallery.BottomGalleryItemSize;
        if (currentHeight <= 0)
        {
            currentHeight = GalleryDefaults.DefaultBottomGalleryHeight;
        }

        // Populate items with metadata
        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();
            
            var item = new GalleryItemViewModel
            {
                FileInfo = file
            };
            
            var thumbData = GalleryThumbInfo.GalleryThumbHolder.GetThumbData(file);
            item.FileName.Value = thumbData.FileName;
            item.FileSize.Value = thumbData.FileSize;
            item.FileDate.Value = thumbData.FileDate;
            item.FileLocation.Value = thumbData.FileLocation;

            // Set initial size
            item.ItemHeight.Value = currentHeight;
            // Width is usually auto or based on aspect ratio, handled by UI or loaded image
            
            newItems.Add(item);
        }

        // Update the collection in the VM to show items immediately
        tab.Gallery.GalleryItems.Value = newItems;

        // Load thumbnails asynchronously
        var parallelOptions = new ParallelOptions 
        { 
            CancellationToken = ct, 
            MaxDegreeOfParallelism = Environment.ProcessorCount - 2
        };
        
        try 
        {
            await Parallel.ForEachAsync(newItems, parallelOptions, async (item, token) =>
            {
                if (item.FileInfo is null) return;
                
                try 
                {
                    // Use a reasonable size for thumbnail loading
                    // We can check GalleryDefaults or use the current height
                    var size = (uint)Math.Max(currentHeight, 100);
                    
                    var thumb = await thumbnailLoader.GetThumbnailAsync(item.FileInfo, size).ConfigureAwait(false);
                    item.Image.Value = thumb;
                }
                catch
                {
                    // Ignore errors during thumbnail loading
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Allowed
        }
    }
}
