using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Clowd.Clipboard;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.Views.UC.Menus;
using PicView.Avalonia.Win32.WindowImpl;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileAssociations;
using PicView.Core.FileSorting;
using PicView.Core.Localization;
using PicView.Core.ProcessHandling;
using PicView.Core.ViewModels;
using PicView.Core.WindowsNT;
using PicView.Core.WindowsNT.Copy;
using PicView.Core.WindowsNT.FileAssociation;
using PicView.Core.WindowsNT.FileHandling;
using PicView.Core.WindowsNT.Taskbar;
using PicView.Core.WindowsNT.Wallpaper;
using R3;
using R3.Avalonia;

namespace PicView.Avalonia.Win32.Views;

public partial class WinMainWindow2 : Window, IPlatformSpecificService, IPlatformWindowService
{
    private static WindowInitializer? _windowInitializer;
    private readonly CompositeDisposable _disposables = new();
    private readonly AvaloniaRenderingFrameProvider _frameProvider;
    private TaskbarProgress? _taskbarProgress;
    private MainViewModel? _vm;

    public WinMainWindow2()
    {
        // initialize RenderingFrameProvider
        _frameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this)!);
        UIHelper.SetFrameProvider(_frameProvider);

        Initialization();
    }

    public WinMainWindow2(bool mainWindowAlreadyExists)
    {
        if (mainWindowAlreadyExists)
        {
            // initialize RenderingFrameProvider
            _frameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this)!);
            UIHelper.SetFrameProvider(_frameProvider);

            _vm = new MainViewModel(this, this);
            DataContext = _vm;

            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                return;
            }

            ThemeManager.DetermineTheme(Application.Current, true);
            StartUpHelper2.StartUpBlank(_vm, true, false, desktop, this);
            _windowInitializer = new WindowInitializer();

            Initialization();
            return;
        }

        var settingsExists = LoadSettings();

        // initialize RenderingFrameProvider
        _frameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this)!);
        UIHelper.SetFrameProvider(_frameProvider);

        Initialization();
        WindowInitialization(settingsExists);
    }

    private void WindowInitialization(bool settingsExists)
    {
        TranslationManager.Init();

        _vm = new MainViewModel(this, this);
        DataContext = _vm;

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        ThemeManager.DetermineTheme(Application.Current, settingsExists);
        StartUpHelper2.StartWithArguments(_vm, settingsExists, desktop, this);
        _windowInitializer = new WindowInitializer();
    }

    private void Initialization()
    {
        InitializeComponent();

        LoadedInitialization();
    }

    private void LoadedInitialization()
    {
        Loaded += delegate
        {
            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            Observable.EveryValueChanged(MainTabControl.Items, x => x.Count).Subscribe(count =>
            {
                vm.Tabs.IsTabPanelVisible.Value = count > 1;
            }).AddTo(_disposables);

            // Keep window position when resizing
            ClientSizeProperty.Changed.ToObservable()
                .ObserveOn(_frameProvider)
                .Subscribe(size =>
                {
                    if (Win32Window.IsChangingWindowState || WindowState != WindowState.Normal)
                    {
                        return;
                    }

                    WindowResizing.HandleWindowResize(this, size);
                });
            ScalingChanged += (_, _) =>
            {
                ScreenHelper.UpdateScreenSize(this);
                WindowResizing.SetSize(DataContext as MainViewModel);
            };
            PointerExited += (_, _) => { DragAndDropHelper.RemoveDragDropView(); };

            Observable.EveryValueChanged(this, x => x.WindowState, _frameProvider).Subscribe(state =>
            {
                switch (state)
                {
                    case WindowState.FullScreen:
                        if (!Settings.WindowProperties.Fullscreen)
                        {
                            vm.PlatformWindowService.Fullscreen();
                        }

                        break;
                    case WindowState.Maximized:
                        if (!Settings.WindowProperties.Maximized)
                        {
                            vm.PlatformWindowService.Maximize();
                        }

                        break;
                    case WindowState.Normal:
                        if (Settings.WindowProperties.Fullscreen || Settings.WindowProperties.Maximized)
                        {
                            vm.PlatformWindowService.Restore();
                        }

                        break;
                }
            });

            MainTabControl.TabDetached += MainTabControlOnTabDetached;
            MainTabControl.TabCreated += MainTabControlOnTabCreated;
            MainTabControl.SelectionChanged += MainTabControlOnSelectionChanged;

            var dropDownMenu = new DropDownMenu
            {
                Name = "DropDownMenu",
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(3, 0, 3, 0),
                IsVisible = false,
                HorizontalAlignment = HorizontalAlignment.Right,
                ZIndex = 2
            };
            MainPanel.Children.Add(dropDownMenu);

            // Close tabMenu when clicking outside of it
            PointerPressed += (_, _) =>
            {
                if (vm.MainWindow.IsEditableTitlebarOpen.Value && !Titlebar.IsPointerOver)
                {
                    Titlebar.EditableTitlebar.CloseTitlebar();
                }
            };
        };
    }

    private void MainTabControlOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (e.AddedItems[0] is not TabViewModel tab)
        {
            return;
        }

        vm.Tabs.SelectTab(tab);
        tab.UpdateTabTitle();
    }

    private void MainTabControlOnTabCreated(object? sender, TabCreatedEventArgs e)
    {
        // Only set the StartUpMenu if the View is currently null.
        // This prevents overwriting the view (e.g. an image) when reordering tabs,
        // as reordering triggers the TabCreated event again by recreating containers.
        if (e.CreatedItem is TabViewModel { CurrentView.Value: null } tabViewModel)
        {
            tabViewModel.CurrentView.Value = new StartUpMenu();
        }
    }

    private void MainTabControlOnTabDetached(object? sender, TabDetachEventArgs e)
    {
        if (e.DetachedItem is not TabViewModel tab)
        {
            return;
        }

        if (DataContext is not MainViewModel parentVm)
        {
            return;
        }

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        // 1. Try to find a target window under the mouse
        WinMainWindow2? targetWindow = null;

        foreach (var window in desktop.Windows)
        {
            if (window == this || window is not WinMainWindow2 macWindow)
            {
                continue;
            }

            var clientPoint = macWindow.PointToClient(e.ScreenPosition);
            if (!new Rect(0, 0, macWindow.ClientSize.Width, macWindow.ClientSize.Height).Contains(clientPoint))
            {
                continue;
            }

            targetWindow = macWindow;
            break;
        }

        // 2. If dropped on an existing window, attach the tab there
        if (targetWindow != null)
        {
            if (targetWindow.DataContext is not MainViewModel targetVm)
            {
                return;
            }

            // Need to properly remove it from the previous location
            parentVm.Tabs.RemoveTab(tab);

            // Add to new window (if not already added by drag preview)
            if (!targetVm.Tabs.Tabs.Value.Contains(tab))
            {
                targetVm.Tabs.Tabs.Value.Add(tab);
            }

            targetVm.Tabs.SelectTab(tab);

            // Update context
            tab.ParentWindowContext = targetVm;

            // Refresh bindings
            if (tab.CurrentView.CurrentValue is Control control)
            {
                control.DataContext = tab;
            }

            return;
        }

        // 3. Fallback: Create a new window (Detaching behavior)
        Task.Run(() =>
        {
            MainViewModel? newVm = null;
            Dispatcher.UIThread.Invoke(() =>
            {
                // Create a new window with the detached tab
                var newWindow = new WinMainWindow2(true)
                {
                    Position = new PixelPoint(e.ScreenPosition.X - 100, e.ScreenPosition.Y - 50),
                    Width = Width,
                    Height = Height
                };

                newVm = newWindow.DataContext as MainViewModel;

                // Fix null DataContext
                if (tab.CurrentView.CurrentValue is Control control)
                {
                    control.DataContext = tab;
                }
            }, DispatcherPriority.Send);

            newVm.Tabs.Tabs.Value[0] = tab;

            // Initialize the NEW window's tabs with the OLD window's services
            // This ensures both windows share the same memory cache
            if (parentVm.Tabs.SharedCache is not { } cache ||
                parentVm.Tabs.SharedNavigation is not { } nav ||
                parentVm.Tabs.SharedThumbnailLoader is not { } thumb ||
                parentVm.Tabs.SharedGallery is not { } gallery ||
                parentVm.Tabs.SharedFileWatcher is not { } fileWatcher)
            {
                return;
            }

            if (newVm.Tabs.ActiveTab.CurrentValue.ImageIterator?.Files?.Count > 0)
            {
                newVm.Tabs.LoadAndInitializeFromPath(newVm.Tabs.ActiveTab.CurrentValue.ImageIterator.Files, gallery,
                    nav,
                    cache, thumb, fileWatcher);
            }
            else
            {
                newVm.Tabs.LoadAndInitialize(gallery, nav, cache, thumb, fileWatcher);
            }

            // Need to properly remove it from the previous location
            parentVm.Tabs.RemoveTab(tab);
        });
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        await WindowFunctions.WindowClosingBehavior(this);
        base.OnClosing(e);
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext == null)
        {
            return;
        }

        if (e is { HeightChanged: false, WidthChanged: false })
        {
            return;
        }

        if (Settings.WindowProperties.AutoFit)
        {
            return;
        }

        var wm = (MainViewModel)DataContext;
        WindowResizing.SetSize(wm);
    }

    protected override void OnClosed(EventArgs e)
    {
        _frameProvider?.Dispose();
        _disposables.Dispose();
        base.OnClosed(e);
    }

    #region Interface Implementations

    public int CombinedTitleButtonsWidth
    {
        get => (int)(Settings.WindowProperties.Maximized && !Settings.WindowProperties.Fullscreen
            ? OffScreenMargin.Left + OffScreenMargin.Right + field
            : field);
        set;
    } = 185;

    public Task<bool> DeleteFile(string path, bool recycle) =>
        Task.Run(() => WinFileHelper.DeleteFile(path, recycle));

    public void SetTaskbarProgress(ulong progress, ulong maximum)
    {
        if (_taskbarProgress is null)
        {
            var handle = TryGetPlatformHandle()?.Handle;

            // Ensure the handle is valid before proceeding
            if (handle == IntPtr.Zero || handle is null)
            {
                return;
            }

            _taskbarProgress = new TaskbarProgress(handle.Value);
        }

        _taskbarProgress.SetProgress(progress, maximum);
    }

    public void StopTaskbarProgress()
    {
        var handle = TryGetPlatformHandle()?.Handle;

        // Ensure the handle is valid before proceeding
        if (handle == IntPtr.Zero || handle is null)
        {
            return;
        }

        _taskbarProgress?.StopProgress();

        _taskbarProgress = null;
    }

    public void SetCursorPos(int x, int y)
    {
        NativeMethods.SetCursorPos(x, y);
    }

    public List<FileInfo> GetFiles(FileInfo fileInfo)
    {
        return FileListRetriever.RetrieveFiles(fileInfo, CompareStrings);
    }

    public int CompareStrings(string str1, string str2)
    {
        return NativeMethods.StrCmpLogicalW(str1, str2);
    }

    public void OpenWith(string path)
    {
        ProcessHelper.OpenWith(path);
    }

    public void LocateOnDisk(string path)
    {
        var folder = Path.GetDirectoryName(path);
        FileExplorer.OpenFolderAndSelectFile(folder, path);
    }

    public void ShowFileProperties(string path)
    {
        FileExplorer.ShowFileProperties(path);
    }

    public void Print(string path)
    {
        if (Settings.UIProperties.ShowPrintPreview)
        {
            _windowInitializer?.ShowPrintPreviewWindow(_vm, path);
        }
        else
        {
            ProcessHelper.Print(path);
        }
    }

    public async Task SetAsWallpaper(string path, int wallpaperStyle)
    {
        await Task.Run(() =>
        {
            var style = (WallpaperHelper.WallpaperStyle)wallpaperStyle;
            WallpaperHelper.SetDesktopWallpaper(path, style);
        });
    }

    public bool SetAsLockScreen(string path)
    {
        return false;
        // return LockscreenHelper.SetLockScreenImage(path);
    }

    public bool CopyFile(string path)
    {
        return Win32Clipboard.CopyFileToClipboard(false, path);
    }

    public bool CutFile(string path)
    {
        return Win32Clipboard.CopyFileToClipboard(true, path);
    }

    public async Task CopyImageToClipboard(Bitmap bitmap)
    {
        await ClipboardAvalonia.SetImageAsync(bitmap).ConfigureAwait(false);
    }

    public async Task<Bitmap?> GetImageFromClipboard()
    {
        return await ClipboardAvalonia.GetImageAsync().ConfigureAwait(false);
    }

    public async Task<bool> ExtractWithLocalSoftwareAsync(string path, string tempDirectory)
    {
        return await ArchiveExtractionHelper.ExtractWithLocalSoftwareAsync(path, tempDirectory);
    }

    public string DefaultJsonKeyMap()
    {
        return WindowsKeybindings.DefaultKeybindings;
    }

    public void InitiateFileAssociationService()
    {
        var iIFileAssociationService = new WindowsFileAssociationService();
        FileAssociationManager.Initialize(iIFileAssociationService);
    }

    public void DisableScreensaver()
    {
        NativeMethods.DisableScreensaver();
    }

    public void EnableScreensaver()
    {
        NativeMethods.EnableScreensaver();
    }

    #endregion

    #region Window interface implementations

    public void ShowAboutWindow() =>
        _windowInitializer?.ShowAboutWindow(_vm);

    public async Task ShowImageInfoWindow() =>
        await _windowInitializer?.ShowImageInfoWindow(_vm);

    public async Task ShowKeybindingsWindow() =>
        await _windowInitializer?.ShowKeybindingsWindow(_vm);

    public async Task ShowSettingsWindow() =>
        await _windowInitializer?.ShowSettingsWindow(_vm);

    public void ShowSingleImageResizeWindow() =>
        _windowInitializer?.ShowSingleImageResizeWindow(_vm);

    public async Task ShowBatchResizeWindow() =>
        await _windowInitializer?.ShowBatchResizeWindow(_vm);

    public void ShowEffectsWindow() =>
        _windowInitializer?.ShowEffectsWindow(_vm);

    public void ShowConvertWindow() =>
        _windowInitializer?.ShowConvertWindow(_vm);

    /// <inheritdoc />
    public async Task Maximize(bool saveSetting = true) =>
        await Win32Window.Maximize(this, _vm, saveSetting);

    /// <inheritdoc />
    public async Task MaximizeRestore(bool saveSetting = true) =>
        await Win32Window.ToggleMaximize(this, _vm, saveSetting);

    /// <inheritdoc />
    public async Task Fullscreen(bool saveSetting = true) =>
        await Win32Window.Fullscreen(this, _vm, saveSetting);

    /// <inheritdoc />
    public async Task ToggleFullscreen(bool saveSetting = true) =>
        await Win32Window.ToggleFullscreen(this, _vm, saveSetting);

    /// <inheritdoc />
    public async Task Restore() =>
        await Win32Window.Restore(this, _vm);

    #endregion
}