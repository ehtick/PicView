using PicView.Avalonia.Navigation.Services;
using PicView.Core.FileHandling;
using PicView.Core.Navigation;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.StartUp;

public static class TabNavigationInitializer
{
    public static void Initialize(CoreViewModel core)
    {
        // --- Initialization Logic ---
        // This is the initialization logic for the navigation system.
        // It is initialized after initial image load, to make it feel faster by showing the image asap. 
        
        // 1. Create dependencies
        var imageLoader = new AvaloniaImageLoader();
        var archiveService = new AvaloniaArchiveService();

        // 2. Create SharedImageCache
        // We use the same loading logic as AvaloniaImageLoader (via GetImageModel)
        var sharedCache = core.SharedCache;
        var thumbnailCache = core.SharedThumbnailCache;
        
        var fileWatcher = new FileWatcherService(core.PlatformService.CompareStrings, sharedCache, thumbnailCache);

        // 3. Create NavigationService (Core)
        var tempFileService = new TempFileService();
        var navService = new NavigationService(imageLoader, archiveService, sharedCache, fileWatcher, core.PlatformService, tempFileService, core.PlatformService.CompareStrings);

        var thumbnailService = new AvaloniaThumbnailLoader();

        // 4. Initialize ViewModel
        core.MainWindows.ActiveWindow.Value.WindowTabs.LoadAndInitialize(navService, sharedCache, thumbnailService, fileWatcher, thumbnailCache);
        core.MainWindows.ActiveWindow.Value.WindowTabs.SetParentContext(core);
        core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.UpdateTabTitle();
    }
    
    public static void Initialize(CoreViewModel core, FileInfo fileInfo)
    {
        // --- Initialization Logic ---
        // This is the initialization logic for the navigation system.
        // It is initialized after initial image load, to make it feel faster by showing the image asap. 
        
        // 1. Create dependencies
        var imageLoader = new AvaloniaImageLoader();
        var archiveService = new AvaloniaArchiveService();

        // 2. Create SharedImageCache
        // We use the same loading logic as AvaloniaImageLoader (via GetImageModel)
        var sharedCache = core.SharedCache;
        var thumbnailCache = core.SharedThumbnailCache;
        
        var fileWatcher = new FileWatcherService(core.PlatformService.CompareStrings, sharedCache, thumbnailCache);

        // 3. Create NavigationService (Core)
        var tempFileService = new TempFileService();
        var navService = new NavigationService(imageLoader, archiveService, sharedCache, fileWatcher, core.PlatformService, tempFileService, core.PlatformService.CompareStrings);

        var thumbnailService = new AvaloniaThumbnailLoader();

        var files = core.PlatformService.GetFiles(fileInfo);
        // 4. Initialize ViewModel
        core.MainWindows.ActiveWindow.Value.WindowTabs.LoadAndInitializeFromPath(files, navService, sharedCache, thumbnailService, fileWatcher, thumbnailCache);
        core.MainWindows.ActiveWindow.Value.WindowTabs.SetParentContext(core);
        core.MainWindows.ActiveWindow.Value.WindowTabs.ActiveTab.Value.UpdateTabTitle();
    }
    
    public static void InitializeDetachedWindow(MainWindowViewModel parentVm, MainWindowViewModel newVm, TabViewModel tab)
    {
        newVm.WindowTabs.Tabs.Value[0] = tab;
        
        // Initialize the NEW window's tabs with the OLD window's services
        // This ensures both windows share the same memory cache
        if (parentVm.WindowTabs.SharedCache is not { } cache ||
            parentVm.WindowTabs.SharedNavigation is not { } nav ||
            parentVm.WindowTabs.SharedThumbnailLoader is not { } thumb ||
            parentVm.WindowTabs.SharedFileWatcher is not { } fileWatcher)
        {
            return;
        }
        
        var thumbnailCache = parentVm.WindowTabs.SharedThumbnailCache;
        if (thumbnailCache is null) return;
        
        newVm.WindowTabs.LoadAndInitialize(nav, cache, thumb, fileWatcher, thumbnailCache);
        newVm.WindowTabs.SetParentContext(newVm);
        newVm.WindowTabs.ActiveTab.CurrentValue.UpdateTabTitle();
        newVm.WindowTabs.SelectTab(tab);
        tab.ImageIterator.UpdateNavigationProperties();
        
        // Need to properly remove it from the previous location
        parentVm.WindowTabs.RemoveTab(tab);
        parentVm.WindowTabs.IsTabPanelVisible.Value = parentVm.WindowTabs.Tabs.CurrentValue.Count > 1;
    }
}