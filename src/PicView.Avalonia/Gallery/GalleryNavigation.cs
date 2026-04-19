using Avalonia;
using PicView.Core.Gallery;

namespace PicView.Avalonia.Gallery;

// TODO deprecated, delete
public static class GalleryNavigation
{
    #region Position and calculations
    
    private class GalleryItemPosition
    {
        public int Index { get; init; }
        public Point Position { get; init; }
        public Size Size { get; init; }
    }
    
    private static GalleryItemPosition? GetClosestItemAbove(GalleryItemPosition currentItem, IEnumerable<GalleryItemPosition> items)
    {
        var candidates = items.Where(item => item.Position.Y + item.Size.Height <= currentItem.Position.Y).ToList();
        return candidates.OrderByDescending(item => item.Position.Y).ThenBy(item => Math.Abs(item.Position.X - currentItem.Position.X)).FirstOrDefault();
    }

    private static GalleryItemPosition? GetClosestItemBelow(GalleryItemPosition currentItem, IEnumerable<GalleryItemPosition> items)
    {
        var candidates = items.Where(item => item.Position.Y >= currentItem.Position.Y + currentItem.Size.Height).ToList();
        return candidates.OrderBy(item => item.Position.Y).ThenBy(item => Math.Abs(item.Position.X - currentItem.Position.X)).FirstOrDefault();
    }

    private static GalleryItemPosition? GetClosestItemLeft(GalleryItemPosition currentItem, IEnumerable<GalleryItemPosition> items)
    {
        var candidates = items.Where(item => item.Position.X + item.Size.Width <= currentItem.Position.X).ToList();
        return candidates.OrderByDescending(item => item.Position.X).ThenBy(item => Math.Abs(item.Position.Y - currentItem.Position.Y)).FirstOrDefault();
    }

    private static GalleryItemPosition? GetClosestItemRight(GalleryItemPosition currentItem, IEnumerable<GalleryItemPosition> items)
    {
        var candidates = items.Where(item => item.Position.X >= currentItem.Position.X + currentItem.Size.Width).ToList();
        return candidates.OrderBy(item => item.Position.X).ThenBy(item => Math.Abs(item.Position.Y - currentItem.Position.Y)).FirstOrDefault();
    }

    
    #endregion
}

