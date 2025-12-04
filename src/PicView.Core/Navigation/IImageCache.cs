using PicView.Core.Preloading;

namespace PicView.Core.Navigation;

public interface IImageCache
{
    bool TryGet(FileInfo f, out PreLoadValue? value);
    bool TryGet(int index, out PreLoadValue? value);
    void Add(FileInfo f, PreLoadValue v);
    void Remove(FileInfo f);
}