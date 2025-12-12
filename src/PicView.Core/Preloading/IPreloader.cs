using PicView.Core.Models;

namespace PicView.Core.Preloading;

public interface IPreloader
{
    void Add(string ownerId, int index, FileInfo file, ImageModel model, IReadOnlyList<FileInfo> list);

    ValueTask<ImageModel?> AddAsync(string ownerId, int index, IReadOnlyList<FileInfo> list, bool isReverse = false,
        CancellationToken ct = default);
    
    PreLoadValue? Get(FileInfo file, IReadOnlyList<FileInfo> list);
    PreLoadValue? Get(string ownerId, int index, IReadOnlyList<FileInfo> list);
    
    void Resynchronize(IReadOnlyList<FileInfo> files);
    
    Task PreloadAsync(string ownerId, int currentIndex, bool reversed, IReadOnlyList<FileInfo> files, CancellationToken ct);
}