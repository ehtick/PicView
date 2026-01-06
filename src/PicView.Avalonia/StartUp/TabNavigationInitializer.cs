using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation.Services;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileHandling;
using PicView.Core.Navigation;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.StartUp;

public static class TabNavigationInitializer
{
    public static void Initialize(CoreViewModel vm)
    {
        // --- Initialization Logic ---
        // This is the initialization logic for the navigation system.
        // It is initialized after initial image load, to make it feel faster by showing the image asap. 
        
        // 1. Create dependencies
        var imageLoader = new AvaloniaImageLoader();
        var archiveService = new AvaloniaArchiveService();
        var galleryService = new AvaloniaGalleryService(null);

        // 2. Create SharedImageCache
        // We use the same loading logic as AvaloniaImageLoader (via GetImageModel)
        var sharedCache = new SharedImageCache(GetImageModel.GetImageModelAsync);
        
        var fileWatcher = new FileWatcherService(vm.PlatformService.CompareStrings, sharedCache);

        // 3. Create NavigationService (Core)
        var tempFileService = new TempFileService();
        var navService = new NavigationService(imageLoader, archiveService, sharedCache, fileWatcher, vm.PlatformService, tempFileService, vm.PlatformService.CompareStrings);

        var thumbnailService = new AvaloniaThumbnailLoader();

        // 4. Initialize ViewModel
        vm.MainWindows.ActiveWindow.Value.WindowTabs.LoadAndInitialize(galleryService, navService, sharedCache, thumbnailService, fileWatcher);
        vm.MainWindows.ActiveWindow.Value.WindowTabs.SetParentContext(vm);
        vm.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.UpdateTabTitle();
    }
    
    public static void Initialize(CoreViewModel vm, FileInfo fileInfo)
    {
        // --- Initialization Logic ---
        // This is the initialization logic for the navigation system.
        // It is initialized after initial image load, to make it feel faster by showing the image asap. 
        
        // 1. Create dependencies
        var imageLoader = new AvaloniaImageLoader();
        var archiveService = new AvaloniaArchiveService();
        var galleryService = new AvaloniaGalleryService(null);

        // 2. Create SharedImageCache
        // We use the same loading logic as AvaloniaImageLoader (via GetImageModel)
        var sharedCache = new SharedImageCache(GetImageModel.GetImageModelAsync);
        
        var fileWatcher = new FileWatcherService(vm.PlatformService.CompareStrings, sharedCache);

        // 3. Create NavigationService (Core)
        var tempFileService = new TempFileService();
        var navService = new NavigationService(imageLoader, archiveService, sharedCache, fileWatcher, vm.PlatformService, tempFileService, vm.PlatformService.CompareStrings);

        var thumbnailService = new AvaloniaThumbnailLoader();

        var files = vm.PlatformService.GetFiles(fileInfo);
        // 4. Initialize ViewModel
        vm.MainWindows.ActiveWindow.Value.WindowTabs.LoadAndInitializeFromPath(files, galleryService, navService, sharedCache, thumbnailService, fileWatcher);
        vm.MainWindows.ActiveWindow.Value.WindowTabs.SetParentContext(vm);
        vm.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.UpdateTabTitle();
    }
}