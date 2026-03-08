using Avalonia;
using Avalonia.Threading;
using PicView.Avalonia.Navigation.Services;
using PicView.Avalonia.UI;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.Gallery;
using PicView.Core.Localization;
using PicView.Core.Navigation;
using PicView.Core.ViewModels;
using R3;

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
        var thumbnailService = new AvaloniaThumbnailLoader();
        var navService = new NavigationService(imageLoader, archiveService, sharedCache, fileWatcher, core.PlatformService, tempFileService, thumbnailService, core.PlatformService.CompareStrings);

        var tabOverView = core.MainWindows.ActiveWindow.Value.WindowTabs;
        var tab = tabOverView.ActiveTab.CurrentValue;

        // 4. Initialize ViewModel
        tabOverView.LoadAndInitialize(navService, sharedCache,thumbnailCache, thumbnailService, fileWatcher);
        tabOverView.SetParentContext(core);
        InitializeNewTab(tab);
        tab.Gallery.Initialize();
        core.GallerySettings.Initialize();
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
        var thumbnailService = new AvaloniaThumbnailLoader();
        var navService = new NavigationService(imageLoader, archiveService, sharedCache, fileWatcher, core.PlatformService, tempFileService, thumbnailService, core.PlatformService.CompareStrings);

        var files = core.PlatformService.GetFiles(fileInfo);
        // 4. Initialize ViewModel
        var tabOverView = core.MainWindows.ActiveWindow.Value.WindowTabs;
        var tab = tabOverView.ActiveTab.CurrentValue;
        tabOverView.LoadAndInitializeFromPath(files, navService, sharedCache, thumbnailCache, thumbnailService, fileWatcher);
        tabOverView.SetParentContext(core);
        tab.UpdateTabTitle();
        InitializeNewTab(tab);
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
        newVm.WindowTabs.ActiveTab.CurrentValue.UpdateTabTitle();
        newVm.WindowTabs.SelectTab(tab);
        tab.ImageIterator.UpdateNavigationProperties();
        
        // Need to properly remove it from the previous location
        parentVm.WindowTabs.RemoveTab(tab);
        parentVm.WindowTabs.IsTabPanelVisible.Value = parentVm.WindowTabs.Tabs.CurrentValue.Count > 1;
    }
    
    public static void InitializeNewTab(TabViewModel newTab)
    {
        if (newTab.IsInitialized)
        {
            return;
        }
        ModelSubscription(newTab);
        newTab.IsInitialized = true;
    }
    
    private static void ModelSubscription(TabViewModel tabViewModel)
    {
        // Subscribing with AvaloniaRenderingFrameProvider is faster and fixes not being able to navigate while gallery is loading
        try
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Observable.EveryValueChanged(tabViewModel, tab => tab.Model.FileInfo, UIHelper2.GetFrameProvider)
                    .Subscribe(file =>
                    {
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
                        
                        // Trigger file changes to UI
                        tabViewModel.FileInfo.Value = file;

                        tabViewModel.TabTitle.Value = file.Name;
                        tabViewModel.TabTooltip.Value = file.FullName;
                        tabViewModel.UpdateTabTitle();
                    }, static result =>
                    {
#if DEBUG
                        if (result is { IsFailure: true, Exception: not null })
                        {
                            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(ModelSubscription), result.Exception);
                        }
#endif
                    })
                    .AddTo(tabViewModel.Disposables);
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
                    }, static result =>
                    {
#if DEBUG
                        if (result is { IsFailure: true, Exception: not null })
                        {
                            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(ModelSubscription), result.Exception);
                        }
#endif
                    })
                    .AddTo(tabViewModel.Disposables);
                Observable.EveryValueChanged(tabViewModel, tab => tab.Gallery.GalleryMode.Value, UIHelper2.GetFrameProvider)
                    .Skip(1)
                    .SubscribeAwait(async (mode, _) =>
                    {
                        if (mode is GalleryMode2.Docked or GalleryMode2.Expanded)
                        {
                            if (tabViewModel.Gallery.LoadingState is GalleryLoadingState.NotLoaded)
                            {
                                if (Application.Current.DataContext is not CoreViewModel core)
                                {
                                    return;
                                }
                                await GalleryLoader.LoadGalleryAsync(tabViewModel,
                                        tabViewModel.ImageIterator.Files,
                                        new AvaloniaThumbnailLoader(),
                                        core.SharedThumbnailCache,
                                        tabViewModel.GetTabCancellation().Token)
                                    .ConfigureAwait(false);
                            }
                        }
                    }, result =>
                    {
#if DEBUG
                        if (result is { IsFailure: true, Exception: not null })
                        {
                            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(ModelSubscription), result.Exception);
                        }
#endif
                    })
                    .AddTo(tabViewModel.Disposables);
                tabViewModel.Gallery.OpenSelectedItemCommand
                    .SubscribeAwait(async (index, _) =>
                    {
                        if (tabViewModel.ImageIterator is null)
                        {
                            return;
                        }
                        if (tabViewModel.Gallery.IsGalleryExpanded.Value)
                        {
                            tabViewModel.Gallery.ToggleGalleryCommand.Execute(Unit.Default);
                        }
            
                        await tabViewModel.ImageIterator.SkipToIndexAsync(index, tabViewModel.GetTabCancellation()).ConfigureAwait(false);
                    }, result =>
                    {
#if DEBUG
                        if (result is { IsFailure: true, Exception: not null })
                        {
                            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(ModelSubscription), result.Exception);
                        }
#endif
                    })
                    .AddTo(tabViewModel.Disposables);
            });
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(TabNavigationInitializer), nameof(ModelSubscription), e);
        }
    }
}