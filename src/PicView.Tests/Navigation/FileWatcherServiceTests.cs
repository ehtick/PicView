using PicView.Core.Config;
using PicView.Core.Models;
using PicView.Core.Navigation;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Tests.Navigation;

public class FileWatcherServiceTests : IDisposable
{
    private readonly FileWatcherService _service;
    private readonly MockImageCache _mockCache;
    private readonly string _testDirectory;

    public FileWatcherServiceTests()
    {
        // Ensure Settings are initialized for FileWatcherService usage
        SettingsManager.SetDefaults();

        _testDirectory = Path.Combine(Path.GetTempPath(), "PicViewTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDirectory);

        _mockCache = new MockImageCache();
        // Use default ordinal comparison for tests
        _service = new FileWatcherService((s1, s2) => string.CompareOrdinal(s1, s2), _mockCache);
    }

    [Fact]
    public void Watch_AddsSubscriber()
    {
        var tab = CreateTab(_testDirectory);
        _service.Watch(tab);
        
        // No direct way to verify internal state without reflection or observing behavior
        // But we can verify no exception is thrown
    }

    [Fact]
    public async Task OnFileCreated_UpdatesTabFiles()
    {
        var tab = CreateTab(_testDirectory);
        _service.Watch(tab);

        // Initial state: empty
        Assert.Empty(tab.ImageIterator.Files);

        // Act: Create a file
        var filePath = Path.Combine(_testDirectory, "test.jpg");
        await File.WriteAllTextAsync(filePath, "dummy content");

        // Wait for watcher event (async nature of FileSystemWatcher)
        await Task.Delay(500); // Small delay for FS event

        // Assert
        Assert.Single(tab.ImageIterator.Files);
        Assert.Equal(filePath, tab.ImageIterator.Files[0].FullName);
        Assert.True(_mockCache.Resynchronized);
    }

    [Fact]
    public async Task OnFileDeleted_UpdatesTabFiles_AndResyncs()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.jpg");
        await File.WriteAllTextAsync(filePath, "dummy content");
        
        var tab = CreateTab(_testDirectory);
        
        // Manually initialize tab with file
        var files = new List<FileInfo> { new FileInfo(filePath) };
        tab.InitializeImageIterator(files, _mockCache, new MockThumbnailLoader());
        tab.Model.Value = new ImageModel { FileInfo = files[0] };
        
        _service.Watch(tab);

        // Act: Delete file
        File.Delete(filePath);
        await Task.Delay(500);

        // Assert
        Assert.Empty(tab.ImageIterator.Files);
        Assert.True(_mockCache.Resynchronized);
    }
    
    [Fact]
    public async Task OnFileRenamed_UpdatesTabFiles_AndModel()
    {
        // Arrange
        var oldPath = Path.Combine(_testDirectory, "old.jpg");
        var newPath = Path.Combine(_testDirectory, "new.jpg");
        await File.WriteAllTextAsync(oldPath, "dummy content");
        
        var tab = CreateTab(_testDirectory);
        var files = new List<FileInfo> { new FileInfo(oldPath) };
        tab.InitializeImageIterator(files, _mockCache, new MockThumbnailLoader());
        tab.Model.Value = new ImageModel { FileInfo = files[0] };
        
        _service.Watch(tab);

        // Act
        File.Move(oldPath, newPath);
        await Task.Delay(500);

        // Assert
        Assert.Single(tab.ImageIterator.Files);
        Assert.Equal(newPath, tab.ImageIterator.Files[0].FullName);
        // Verify model update if it was current file
        Assert.Equal(newPath, tab.Model.Value.FileInfo.FullName);
        Assert.True(_mockCache.Resynchronized);
    }

    private TabViewModel CreateTab(string directory)
    {
        var tab = new TabViewModel("test", _ => ValueTask.CompletedTask);
        // We need to set a model so Watcher can find directory
        // But usually Watch() is called after InitializeImageIterator which likely has files.
        // If Model is null, Watch does nothing.
        // So let's fake a model with a file in that directory
        var dummyFile = new FileInfo(Path.Combine(directory, "placeholder.txt"));
        tab.Model.Value = new ImageModel { FileInfo = dummyFile };
        
        tab.Initialize(_mockCache, new MockThumbnailLoader());
        return tab;
    }

    public void Dispose()
    {
        _service.Dispose();
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch { /* Best effort cleanup */ }
        }
    }
    
    // Mocks
    private class MockImageCache : IImageCache
    {
        public bool Resynchronized { get; private set; }
        public void Resynchronize(string ownerId, IReadOnlyList<FileInfo> files) => Resynchronized = true;

        // Stub other methods
        public Task<ImageModel?> LoadAsync(string ownerId, int index, IReadOnlyList<FileInfo> list, CancellationToken ct = default) => Task.FromResult<ImageModel?>(null);
        public void RegisterOwner(string ownerId) { }
        public ValueTask RemoveOwner(string ownerId) => ValueTask.CompletedTask;
        public bool TryGet(FileInfo f, out PreLoadValue? value) { value = null; return false; }
        public bool TryGet(string ownerId, int index, out PreLoadValue? value) { value = null; return false; }
        public bool TryGet(ReadOnlySpan<char> f, out PreLoadValue? value) { value = null; return false; }
        public void Clear() { }
        public void Clear(string ownerId) { }
        public bool Contains(ReadOnlySpan<char> span, out PreLoadValue? value) { value = null; return false; }
        public bool Contains(PreLoadValue value) => false;
        public bool Add(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse) => false;
        public bool TryAdd(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse, out PreLoadValue? value) { value = null; return false; }
        public void Preload(string ownerId, int currentIndex, bool reversed, IReadOnlyList<FileInfo> files) { }
        public ValueTask Clear(TabViewModel tab) => ValueTask.CompletedTask;
        public void TryRemove(string ownerId, int index) { }
    }

    private class MockThumbnailLoader : IThumbnailLoader
    {
        public ValueTask<object?> GetThumbnailAsync(FileInfo file) => ValueTask.FromResult<object?>(null);
        public ValueTask<object?> GetThumbnailAsync(FileInfo file, uint size) => ValueTask.FromResult<object?>(null);
    }
}
