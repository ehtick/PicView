using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation.Services;
using PicView.Avalonia.ViewModels;
using PicView.Core.Navigation;

namespace PicView.Avalonia.StartUp;

public static class TabNavigationInitializer
{
    public static void Initialize(MainViewModel vm)
    {
        // --- Initialization Logic ---
        // This is the initialization logic for the navigation system.
        // It is initialized after initial image load, to make it feel faster by showing the image asap. 
        
        // 1. Create dependencies
        var imageLoader = new AvaloniaImageLoader();
        var archiveService = new AvaloniaArchiveService();
        var galleryService = new AvaloniaGalleryService(vm);

        // 2. Create SharedImageCache
        // We use the same loading logic as AvaloniaImageLoader (via GetImageModel)
        var sharedCache = new SharedImageCache(GetImageModel.GetImageModelAsync);

        // 3. Create NavigationService (Core)
        var navService = new NavigationService(imageLoader, archiveService, sharedCache, vm.PlatformService.CompareStrings);

        var thumbnailService = new AvaloniaThumbnailLoader();

        var fileWatcher = new FileWatcherService(vm.PlatformService.CompareStrings, sharedCache);

        // 4. Initialize ViewModel
        vm.Tabs.LoadAndInitialize(galleryService, navService, sharedCache, thumbnailService, fileWatcher);
        vm.Tabs.ActiveTab.Value.UpdateTabTitle();
    }
    
    public static void Initialize(MainViewModel vm, FileInfo fileInfo)
    {
        // --- Initialization Logic ---
        // This is the initialization logic for the navigation system.
        // It is initialized after initial image load, to make it feel faster by showing the image asap. 
        
        // 1. Create dependencies
        var imageLoader = new AvaloniaImageLoader();
        var archiveService = new AvaloniaArchiveService();
        var galleryService = new AvaloniaGalleryService(vm);

        // 2. Create SharedImageCache
        // We use the same loading logic as AvaloniaImageLoader (via GetImageModel)
        var sharedCache = new SharedImageCache(GetImageModel.GetImageModelAsync);

        // 3. Create NavigationService (Core)
        var navService = new NavigationService(imageLoader, archiveService, sharedCache, vm.PlatformService.CompareStrings);

        var thumbnailService = new AvaloniaThumbnailLoader();
        var fileWatcher = new FileWatcherService(vm.PlatformService.CompareStrings, sharedCache);

        var files = vm.PlatformService.GetFiles(fileInfo);
        // 4. Initialize ViewModel
        vm.Tabs.LoadAndInitializeFromPath(files, galleryService, navService, sharedCache, thumbnailService, fileWatcher);
        vm.Tabs.SetParentContext(vm);
        vm.Tabs.ActiveTab.Value.UpdateTabTitle();
    }
}