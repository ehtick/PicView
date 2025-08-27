using BenchmarkDotNet.Attributes;
using ImageMagick;
using PicView.Avalonia.ImageHandling;
using PicView.Core.FileHandling;
using PicView.Core.ImageReading;
using PicView.Core.Preloading;
using ZLinq;

namespace PicView.Benchmarks.ImageBenchmarks;

[MemoryDiagnoser] // track allocations
public class PreloadingBenchmark
{
    private List<FileInfo>? _fileInfos;
    private const int MaxSize = 12;
    
    private PreLoader? _preLoader;
    
    [GlobalSetup]
    public async ValueTask Setup()
    {
        await LoadSettingsAsync();
        
        var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        _fileInfos = new DirectoryInfo(picturesPath)
            .DescendantsAndSelf()
            .OfType<FileInfo>()
            .Where(x => x.IsSupported())
            .Take(MaxSize * 6)
            .ToList();
        
        _preLoader = new PreLoader(GetImageModel.GetImageModelAsync);
    }
    
    [Benchmark]
    public async ValueTask PreloadImages()
    {
        for (var i = 0; i < MaxSize; i++)
        {
            await _preLoader.PreLoadAsync(0, false, _fileInfos)
                .ConfigureAwait(false);
        }
    }
}