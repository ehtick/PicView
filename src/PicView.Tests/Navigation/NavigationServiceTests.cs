using PicView.Core.FileHandling.Interfaces;
using PicView.Core.IPlatform;
using PicView.Core.Models;
using PicView.Core.Navigation;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Tests.Navigation;

public class NavigationServiceTests
{
    private readonly MockImageLoader _mockImageLoader;
    private readonly MockArchiveService _mockArchiveService;
    private readonly MockImageCache _mockCache;
    private readonly MockFileWatcherService _mockFileWatcherService;
    private readonly NavigationService _navigationService;
    private readonly string _testDirectory;

    public NavigationServiceTests()
    {
        ObservableSystem.DefaultFrameProvider = new MockFrameProvider();

        _testDirectory = Path.Combine(Path.GetTempPath(), "PicViewNavTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDirectory);

        _mockImageLoader = new MockImageLoader();
        _mockArchiveService = new MockArchiveService();
        _mockCache = new MockImageCache();
        _mockFileWatcherService = new MockFileWatcherService();

        _navigationService = new NavigationService(
            _mockImageLoader,
            _mockArchiveService,
            _mockCache,
            _mockFileWatcherService,
            new MockPlatformSpecificService(),
            new MockTempFileService(),
            string.CompareOrdinal
        );
    }

    [Fact]
    public async Task RepopulateIterator_UpdatesFileWatcher()
    {
        // Arrange
        var tab = CreateTab(_testDirectory);
        var fileInfo = new FileInfo(Path.Combine(_testDirectory, "test.jpg"));
        var cts = new CancellationTokenSource();

        // Act
        await _navigationService.RepopulateIterator(fileInfo, tab, cts);

        // Assert
        Assert.True(_mockFileWatcherService.UnwatchCalled, "Unwatch should be called");
        Assert.True(_mockFileWatcherService.WatchCalled, "Watch should be called");
        Assert.Equal(_testDirectory, _mockFileWatcherService.WatchedDirectory);
        Assert.Equal(tab, _mockFileWatcherService.WatchedTab);
    }

    private TabViewModel CreateTab(string directory)
    {
        var tab = new TabViewModel("test", _ => ValueTask.CompletedTask);
        // Initialize with mocks to avoid null refs
        tab.Initialize(_mockCache, new MockThumbnailCache(), new MockThumbnailLoader());
        tab.ImageIterator.Files = new List<FileInfo>();
        return tab;
    }

    // Mocks

    private class MockFileWatcherService : IFileWatcherService
    {
        public bool WatchCalled { get; private set; }
        public bool UnwatchCalled { get; private set; }
        public TabViewModel? WatchedTab { get; private set; }
        public string? WatchedDirectory { get; private set; }

        public void Watch(TabViewModel tab, string? directory = null)
        {
            WatchCalled = true;
            WatchedTab = tab;
            WatchedDirectory = directory;
        }

        public void Unwatch(TabViewModel tab)
        {
            UnwatchCalled = true;
        }
    }

    private class MockImageLoader : IImageLoader
    {
        public ValueTask<ImageModel> GetImageModelAsync(FileInfo file, CancellationToken ct)
        {
            return ValueTask.FromResult(new ImageModel { FileInfo = file });
        }
    }

    private class MockArchiveService : IArchiveService
    {
        public Task<DirectoryInfo> ExtractToTempAsync(FileInfo archive, CancellationToken ct)
        {
            return Task.FromResult(new DirectoryInfo(Path.GetTempPath()));
        }
    }

    private class MockImageCache : IImageCache
    {
        public Task<ImageModel?> LoadAsync(string ownerId, int index, IReadOnlyList<FileInfo> list, CancellationToken ct = default) => Task.FromResult<ImageModel?>(null);
        public bool TryGet(FileInfo f, out PreLoadValue? value) { value = null; return false; }
        public bool TryGet(string ownerId, int index, out PreLoadValue? value) { value = null; return false; }
        public void Clear() { }
        public void Clear(string ownerId) { }
        public bool Contains(PreLoadValue value) => false;
        public bool Add(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse) => false;
        public bool TryAdd(string ownerId, int index, PreLoadValue preLoadValue, int listCount, bool isReverse, out PreLoadValue? value) { value = null; return false; }
        public void Preload(string ownerId, int currentIndex, bool reversed, IReadOnlyList<FileInfo> files) { }
        public ValueTask RemoveOwner(string ownerId) => ValueTask.CompletedTask;
        public void RegisterOwner(string ownerId) { }
        public ValueTask Clear(TabViewModel tab) => ValueTask.CompletedTask;
        public void TryRemove(string ownerId, int index) { }
        public void Resynchronize(string ownerId, IReadOnlyList<FileInfo> files) { }
    }
    
    private class MockThumbnailLoader : IThumbnailLoader
    {
        public ValueTask<object?> GetThumbnailAsync(FileInfo file) => ValueTask.FromResult<object?>(null);
        public ValueTask<object?> GetThumbnailAsync(FileInfo file, uint size) => ValueTask.FromResult<object?>(null);
    }

    private class MockTempFileService : ITempFileService
    {
        public string GetNewTempFilePath(string fileName) => Path.Combine(Path.GetTempPath(), fileName);

        public void Cleanup() { }
    }
    
    private class MockThumbnailCache : IThumbnailCache
    {
        public void Add(string ownerId, string path, object thumbnail) { }
        public bool TryGet(string path, out object? thumbnail) { thumbnail = null; return false; }
        public void Remove(string path) { }
        public void RemoveOwner(string ownerId) { }
        public void Clear() { }
        public bool IsEmpty() => true;
    }

    private class MockPlatformSpecificService : IPlatformSpecificService
    {
        public void SetTaskbarProgress(ulong progress, ulong maximum) { }
        public void StopTaskbarProgress() { }
        public void SetCursorPos(int x, int y) { }
        public void DisableScreensaver() { }
        public void EnableScreensaver() { }
        public List<FileInfo> GetFiles(FileInfo fileInfo) => new();
        public int CompareStrings(string str1, string str2) => string.CompareOrdinal(str1, str2);
        public void OpenWith(string path) { }
        public void LocateOnDisk(string path) { }
        public void ShowFileProperties(string path) { }
        public void Print(string path) { }
        public Task SetAsWallpaper(string path, int wallpaperStyle) => Task.CompletedTask;
        public bool SetAsLockScreen(string path) => false;
        public bool CopyFile(string path) => false;
        public bool CutFile(string path) => false;
        public Task CopyImageToClipboard(object bitmap) => Task.CompletedTask;
        public Task<object?> GetImageFromClipboard() => Task.FromResult<object?>(null);
        public Task<bool> ExtractWithLocalSoftwareAsync(string path, string tempDirectory) => Task.FromResult(false);
        public string DefaultJsonKeyMap() => "{}";
        public void InitiateFileAssociationService() { }
        public Task<bool> DeleteFile(string path, bool recycle) => Task.FromResult(false);
    }

    private class MockFrameProvider : FrameProvider
    {
        public override long GetFrameCount() => 0;
        public override void Register(IFrameRunnerWorkItem callback) => callback.MoveNext(0);
    }
}
