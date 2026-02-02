using Avalonia.Threading;
using PicView.Avalonia.Navigation.Services;
using PicView.Avalonia.UI;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.Localization;
using PicView.Core.Navigation;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.StartUp;

public static class TabNavigationInitializer
{
    public static void Initialize(CoreViewModel core, CompositeDisposable disposable)
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
        var tabOverView = core.MainWindows.ActiveWindow.Value.WindowTabs;
        var tab = tabOverView.ActiveTab.CurrentValue;

        // 4. Initialize ViewModel
        tabOverView.LoadAndInitialize(navService, sharedCache,thumbnailCache, thumbnailService, fileWatcher);
        tabOverView.SetParentContext(core);
        tab.UpdateTabTitle();
        ModelSubscription(tab, disposable);
    }
    
    public static void Initialize(CoreViewModel core, FileInfo fileInfo, CompositeDisposable disposable)
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
        var tabOverView = core.MainWindows.ActiveWindow.Value.WindowTabs;
        var tab = tabOverView.ActiveTab.CurrentValue;
        tabOverView.LoadAndInitializeFromPath(files, navService, sharedCache, thumbnailCache, thumbnailService, fileWatcher);
        tabOverView.SetParentContext(core);
        tab.UpdateTabTitle();
        ModelSubscription(tab, disposable);
    }
    
    public static void InitializeDetachedWindow(MainWindowViewModel parentVm, MainWindowViewModel newVm, TabViewModel tab, CompositeDisposable disposable)
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
        newVm.WindowTabs.ActiveTab.CurrentValue.UpdateTabTitle();
        newVm.WindowTabs.SelectTab(tab);
        tab.ImageIterator.UpdateNavigationProperties();
        
        // Need to properly remove it from the previous location
        parentVm.WindowTabs.RemoveTab(tab);
        parentVm.WindowTabs.IsTabPanelVisible.Value = parentVm.WindowTabs.Tabs.CurrentValue.Count > 1;
        ModelSubscription(tab, disposable);
    }
    
    private static void ModelSubscription(TabViewModel tabViewModel, CompositeDisposable disposable)
    {
        // Subscribing with AvaloniaRenderingFrameProvider is faster and fixes not being able to navigate while gallery is loading
        try
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Observable.EveryValueChanged(tabViewModel, tab => tab.Model.FileInfo, UIHelper2.GetFrameProvider)
                    .Subscribe(file =>
                    {
                        // Trigger file changes to UI
                        tabViewModel.FileInfo.Value = file;

                        // Update title to reflect file changes
                        if (file is null)
                        {
                            var noImage = TranslationManager.Translation?.NoImage;
                            if (string.IsNullOrEmpty(noImage))
                            {
                                return;
                            }

                            tabViewModel.TabTitle.Value = noImage;
                            tabViewModel.TabTooltip.Value = noImage;
                            return;
                        }

                        tabViewModel.TabTitle.Value = file.Name;
                        tabViewModel.TabTooltip.Value = file.FullName;
                        tabViewModel.UpdateTabTitle();
                    }).AddTo(disposable);
                Observable.EveryValueChanged(tabViewModel, tab => tab.Model.Image, UIHelper2.GetFrameProvider)
                    .Subscribe(image =>
                    {
                        // Trigger image change to UI
                        tabViewModel.Image.Value = image;

                        // Update tiff title if appropriate (there are no file changes in this instance
                        if (tabViewModel.Model.TiffNavigation is null)
                        {
                            return;
                        }

                        tabViewModel.UpdateTabTitle();
                    }).AddTo(disposable);
            });
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(ModelSubscription), e);
        }
    }
}