using System.Diagnostics;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.Navigation.Services;
using PicView.Core.DebugTools;
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
        var thumbnailService = new AvaloniaThumbnailLoader();

        // 2. Create SharedImageCache
        // We use the same loading logic as AvaloniaImageLoader (via GetImageModel)
        var sharedCache = core.SharedCache;
        var thumbnailCache = core.SharedThumbnailCache;

        Debug.Assert(core.PlatformService != null);
        var fileWatcher = new FileWatcherService(core.PlatformService.CompareStrings, sharedCache, thumbnailCache, thumbnailService);

        // 3. Create NavigationService (Core)
        core.SharedNavigationService ??= new NavigationService(imageLoader, archiveService, sharedCache, fileWatcher, core.PlatformService, thumbnailService, core.PlatformService.CompareStrings);

        Debug.Assert(core.MainWindows.ActiveWindow.CurrentValue != null);
        var tabOverView = core.MainWindows.ActiveWindow.CurrentValue.WindowTabs;
        var tab = tabOverView.ActiveTab.CurrentValue;

        // 4. Initialize ViewModel
        tabOverView.LoadAndInitialize(core.SharedNavigationService, sharedCache,thumbnailCache, thumbnailService, fileWatcher);
        tabOverView.SetParentContext(core.MainWindows.ActiveWindow.CurrentValue);
        InitializeNewTab(tab, core.MainWindows.ActiveWindow.CurrentValue);
        tab.Gallery.Initialize();
        core.GallerySettings.Initialize();
    }
    
    public static void Initialize(CoreViewModel core, FileInfo fileInfo)
    {
        Debug.Assert(core.PlatformService != null);
        Initialize(core, core.PlatformService.GetFiles(fileInfo));
    }
    
    public static void Initialize(CoreViewModel core, List<FileInfo> files)
    {
        // --- Initialization Logic ---
        // This is the initialization logic for the navigation system.
        // It is initialized after initial image load, to make it feel faster by showing the image asap. 
        
        // 1. Create dependencies
        var imageLoader = new AvaloniaImageLoader();
        var archiveService = new AvaloniaArchiveService();
        var thumbnailService = new AvaloniaThumbnailLoader();

        // 2. Create SharedImageCache
        // We use the same loading logic as AvaloniaImageLoader (via GetImageModel)
        var sharedCache = core.SharedCache;
        var thumbnailCache = core.SharedThumbnailCache;

        Debug.Assert(core.PlatformService != null);
        var fileWatcher = new FileWatcherService(core.PlatformService.CompareStrings, sharedCache, thumbnailCache, thumbnailService);

        // 3. Create NavigationService (Core)
        core.SharedNavigationService ??= new NavigationService(imageLoader, archiveService, sharedCache, fileWatcher, core.PlatformService, thumbnailService, core.PlatformService.CompareStrings);

        // 4. Initialize ViewModel
        Debug.Assert(core.MainWindows.ActiveWindow.CurrentValue != null);
        var tabOverView = core.MainWindows.ActiveWindow.CurrentValue.WindowTabs;
        var tab = tabOverView.ActiveTab.CurrentValue;
        tabOverView.LoadAndInitializeFromPath(files, core.SharedNavigationService, sharedCache, thumbnailCache, thumbnailService, fileWatcher);
        tabOverView.SetParentContext(core.MainWindows.ActiveWindow.CurrentValue);
        InitializeNewTab(tab, core.MainWindows.ActiveWindow.CurrentValue);
        tab.Gallery.Initialize();
        core.GallerySettings.Initialize();
    }
    
    public static void InitializeDetachedWindow(MainWindowViewModel parentVm, MainWindowViewModel newVm, TabViewModel tab)
    {
        newVm.WindowTabs.Tabs.Value[0] = tab;
        
        // Initialize the NEW window's tabs with the OLD window's services
        // This ensures both windows share the same memory cache
        if (parentVm.WindowTabs.SharedCache is not { } cache ||
            parentVm.WindowTabs.SharedThumbnailCache is not { } thumbCache || 
            parentVm.WindowTabs.SharedNavigation is not { } nav ||
            parentVm.WindowTabs.SharedThumbnailLoader is not { } thumbLoader ||
            parentVm.WindowTabs.SharedFileWatcher is not { } fileWatcher)
        {
            return;
        }
        
        newVm.WindowTabs.LoadAndInitialize(nav, cache, thumbCache, thumbLoader, fileWatcher);
        newVm.WindowTabs.SetParentContext(newVm);
        if (tab.FileInfo?.CurrentValue is not null)
        {
            newVm.WindowTabs.ActiveTab.CurrentValue.UpdateTabTitle();
        }
        else
        {
            newVm.WindowTabs.ActiveTab.CurrentValue.SetNewTabTitle();
        }
        newVm.WindowTabs.SelectTab(tab);
        Debug.Assert(tab.ImageIterator != null);
        tab.ImageIterator.UpdateNavigationProperties();
        
        // Need to properly remove it from the previous location
        parentVm.WindowTabs.RemoveTab(tab);
    }
    
    public static void InitializeNewTab(TabViewModel newTab, MainWindowViewModel mainWindowViewModel)
    {
        if (newTab is null)
        {
            return;
        }
        if (newTab.IsInitialized)
        {
#if DEBUG
            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(InitializeNewTab), $"Tab {newTab.Id} {newTab.Model?.FileInfo?.Name} is already initialized");
#endif
            return;
        }
        NavigationSubscriptions.ModelSubscription(newTab, mainWindowViewModel);
        newTab.IsInitialized = true;
    }
}