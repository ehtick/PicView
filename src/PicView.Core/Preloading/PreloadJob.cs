namespace PicView.Core.Preloading;
internal readonly record struct PreloadJob(int Index, bool Reversed, IReadOnlyList<FileInfo> Files);