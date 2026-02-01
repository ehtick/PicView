using System.Collections.ObjectModel;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.ViewModels;

namespace PicView.Core.Gallery;

public class GalleryLoaderService
{
    public static async Task LoadGalleryAsync(TabViewModel tab, IReadOnlyList<FileInfo> files, IThumbnailLoader thumbnailLoader, CancellationToken ct)
    {
        var dockedHeight = Settings.Gallery.BottomGalleryItemSize;
        var expandedHeight = Settings.Gallery.ExpandedGalleryItemSize;
        var maxHeight = Math.Max(dockedHeight, expandedHeight);
        if (maxHeight <= 0)
        {
            maxHeight = GalleryDefaults.DefaultBottomGalleryHeight;
        }

        const int batchSize = 20;
        var batchList = new List<GalleryItemViewModel>(batchSize);

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
            
            // Add to batch instead of direct collection
            batchList.Add(item);

            // Check if batch is full
            if (batchList.Count >= batchSize)
            {
                tab.Gallery.GalleryItems.Value.AddRange(batchList);
                batchList.Clear();
            }
        }

        // Add any remaining items in the final batch
        if (batchList.Count > 0)
        {
            tab.Gallery.GalleryItems.Value.AddRange(batchList);
        }

        // Load thumbnails asynchronously
        var parallelOptions = new ParallelOptions 
        { 
            CancellationToken = ct, 
            MaxDegreeOfParallelism = Environment.ProcessorCount - 2
        };
        
        try 
        {
            await Parallel.ForEachAsync(tab.Gallery.GalleryItems.Value, parallelOptions, async (item, token) =>
            {
                if (item.FileInfo is null) return;
                
                try 
                {
                    var thumb = await thumbnailLoader.GetThumbnailAsync(item.FileInfo, (uint)maxHeight).ConfigureAwait(false);
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