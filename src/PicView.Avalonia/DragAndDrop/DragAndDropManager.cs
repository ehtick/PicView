using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.UI;
using PicView.Avalonia.Views.UC;
using PicView.Core.Extensions;
using PicView.Core.FileHandling;
using PicView.Core.Preloading;
using PicView.Core.ProcessHandling;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;
using PicView.Core.FileSorting;

namespace PicView.Avalonia.DragAndDrop;

public static class DragAndDropManager
{
    private static DragDropView? _dragDropView;
    private static PreLoadValue? _preLoadValue;

    #region Public Entry Points

    public static async ValueTask Drop(DragEventArgs e, TabOverviewViewModel tabOverview)
    {
        RemoveDragDropView();

        var files = e.DataTransfer.TryGetFiles();
        if (files == null)
        {
            await HandleDropFromUrl(e, tabOverview);
            return;
        }

        if (files.Length < 1)
        {
            return;
        }

        var firstFile = files[0];
        if (firstFile == null)
        {
            return;
        }
        
        // Handle opening additional files in new windows if needed
        if (files.Length > 1)
        {
            _ = Task.Run(() => HandleAdditionalFiles(files.Skip(1)));
        }

        var path = firstFile.Path.LocalPath;

        if (path.IsSupported())
        {
            await LoadSupportedFile(path, tabOverview);
        }
        else if (Directory.Exists(path) || path.IsArchive())
        {
            await tabOverview.LoadFromFileAsync(path);
        }
    }

    public static async ValueTask DragEnter(DragEventArgs e, Control control)
    {
        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            await HandleDragEnterWithFiles(files, control);
        }
        else
        {
            // // Try handling as URL
            var value = e.DataTransfer.Items[0];

            var handled = HandleDragEnterFromUrl(value);
            if (!handled)
            {
                RemoveDragDropView();
            }
        }
    }

    public static void DragLeave(Control control)
    {
        if (control.IsPointerOver)
        {
            return;
        }

        RemoveDragDropView();
        _preLoadValue = null;
    }

    public static void RemoveDragDropView()
    {
        if (_dragDropView is null)
        {
            return;
        }

        UIHelper.GetMainView?.MainPanel.Children.Remove(_dragDropView);
        _dragDropView = null;
    }

    #endregion

    #region Private Helpers

    private static void HandleAdditionalFiles(IEnumerable<IStorageItem> additionalFiles)
    {
        if (Settings.UIProperties.OpenInSameWindow)
        {
            return;
        }

        foreach (var file in additionalFiles)
        {
            var filepath = file.Path.LocalPath;
            if (filepath.IsSupported())
            {
                ProcessHelper.StartNewProcess(filepath);
            }
        }
    }

    private static async Task HandleDropFromUrl(DragEventArgs e, TabOverviewViewModel tabOverview)
    {
        var item = e.DataTransfer.Items[0].TryGetRaw(DataFormat.CreateBytesPlatformFormat("text/x-moz-url"));
        if (item is not byte[] bytes)
        {
            return;
        }

        var dataStr = Encoding.Unicode.GetString(bytes);
        var url = dataStr.Split((char)10).FirstOrDefault();
        if (url != null)
        {
            await LoadFromUrl(url, tabOverview);
        }
    }

    private static async Task LoadFromUrl(string url, TabOverviewViewModel tabOverview)
    {
        // Remove preview first and show loading
        RemoveDragDropView();
        
        var tab = tabOverview.ActiveTab.Value;
        
        if (url.StartsWith("file://"))
        {
            var file = url[7..];
            if (file.StartsWith('/'))
            {
                file = file[1..];
            }
            
            if (tab.CurrentView.CurrentValue is not ImageViewer)
            {
                tab.CurrentView.Value = new ImageViewer();
            }

            await tabOverview.LoadFromFileAsync(file);
        }
        else
        {
            if (tab.CurrentView.CurrentValue is not ImageViewer)
            {
                tab.CurrentView.Value = new ImageViewer();
            }
            await tabOverview.LoadFromStringAsync(url);
        }
    }

    private static async Task HandleDragEnterWithFiles(IEnumerable<IStorageItem> files, Control control)
    {
        var fileArray = files as IStorageItem[] ?? files.ToArray();
        if (fileArray.Length == 0)
        {
            return;
        }

        await EnsureDragDropViewCreated(control);

        var firstFile = fileArray[0];
        var path = firstFile.Path.LocalPath;

        if (Directory.Exists(path))
        {
            ShowDirectoryIcon(control);
        }
        else if (path.IsArchive())
        {
            ShowArchiveIcon(control);
        }
        else if (path.IsSupported())
        {
            await ShowFilePreview(new FileInfo(path));
        }
    }

    private static void ShowDirectoryIcon(Control control)
    {
        Dispatcher.CurrentDispatcher.InvokeAsync(() =>
        {
            if (!control.IsPointerOver)
            {
                _dragDropView?.AddDirectoryIcon();
            }
        });
    }

    private static void ShowArchiveIcon(Control control)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            if (!control.IsPointerOver)
            {
                _dragDropView?.AddZipIcon();
            }
        });
    }

    private static async Task ShowFilePreview(FileInfo fileInfo)
    {
        var ext = fileInfo.Extension;
        if (ext.Equals(".svg", StringComparison.InvariantCultureIgnoreCase) ||
            ext.Equals(".svgz", StringComparison.InvariantCultureIgnoreCase))
        {
            Dispatcher.CurrentDispatcher.Invoke(() => _dragDropView?.UpdateSvgThumbnail(fileInfo.FullName));
            return;
        }

        await LoadAndShowThumbnail(fileInfo);
    }

    private static async Task LoadAndShowThumbnail(FileInfo fileInfo)
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        Bitmap? thumb;
        // Try to get preloaded image first
        var preload = core.SharedCache.TryGet(fileInfo, out var preLoadValue);
        if (preload && preLoadValue?.ImageModel?.Image is Bitmap bmp)
        {
            thumb = bmp;
            UpdateThumbnailUI(thumb);
        }
        else
        {
            // Generate thumbnail
            thumb = await GetThumbnails.GetThumbAsync(fileInfo, SizeDefaults.WindowMinSize - 30)
                .ConfigureAwait(false);
            UpdateThumbnailUI(thumb);
            
            // Load full image in background
            await PreloadFullImageAsync(fileInfo, preLoadValue, thumb);
        }
    }

    private static async ValueTask PreloadFullImageAsync(FileInfo fileInfo, PreLoadValue? preload, Bitmap? thumb)
    {
        if (preload is null)
        {
            var model = await GetImageModel.GetImageModelAsync(fileInfo);
            UpdateThumbnailUI(thumb);
            _preLoadValue = new PreLoadValue(model);
        }
        else
        {
            _preLoadValue = preload;
            UpdateThumbnailUI(thumb);
        }
    }

    private static void UpdateThumbnailUI(Bitmap? thumb) =>
        Dispatcher.CurrentDispatcher.Invoke(() => _dragDropView?.UpdateThumbnail(thumb));

    private static bool HandleDragEnterFromUrl(object? urlObject)
    {
        if (urlObject is null)
        {
            _dragDropView?.RemoveThumbnail();
            return false;
        }

        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            _dragDropView ??= new DragDropView();
            if (!_dragDropView.IsLinkChainVisible)
            {
                _dragDropView.AddLinkChain();
            }

            if (UIHelper.GetMainView != null && !UIHelper.GetMainView.MainPanel.Children.Contains(_dragDropView))
            {
                UIHelper.GetMainView.MainPanel.Children.Add(_dragDropView);
            }
        });

        return true;
    }

    private static async Task EnsureDragDropViewCreated(Control control)
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
        {
            if (_dragDropView == null)
            {
                _dragDropView = new DragDropView();
                if (!control.IsPointerOver && UIHelper.GetMainView != null)
                {
                    UIHelper.GetMainView.MainPanel.Children.Add(_dragDropView);
                }
            }
            else
            {
                _dragDropView.RemoveThumbnail();
            }
        });
    }

    private static async Task LoadSupportedFile(string path, TabOverviewViewModel tabOverview)
    {
        var tab = tabOverview.ActiveTab.CurrentValue;
        var droppedFileInfo = new FileInfo(path);
        
        if (_preLoadValue is not null)
        {
             if (Application.Current?.DataContext is CoreViewModel core)
             {
                 var droppedDir = droppedFileInfo.DirectoryName ?? string.Empty;
                 var currentDir = tab.ImageIterator?.CurrentDirectory ?? string.Empty;

                 IReadOnlyList<FileInfo> files;
                 
                 if (string.Equals(droppedDir, currentDir, StringComparison.OrdinalIgnoreCase))
                 {
                     files = tab.ImageIterator?.Files ?? [];
                 }
                 else
                 {
                     files = FileListRetriever.RetrieveFiles(droppedFileInfo, core.PlatformService.CompareStrings);
                 }

                 var index = files.FindIndex(x => x.FullName.AsSpan().Equals(droppedFileInfo.FullName.AsSpan(), StringComparison.OrdinalIgnoreCase));

                 core.SharedCache.Add(tab.Id, index, _preLoadValue, files.Count, false);
             }
        }

        InitializeTab(tabOverview, droppedFileInfo);
        
        await tabOverview.LoadFromFileAsync(path);
    }
    
    private static void InitializeTab(TabOverviewViewModel tabOverview, FileInfo? fileInfo)
    {
        if (fileInfo is null)
        {
            return;
        }
        
        var tab = tabOverview.ActiveTab.Value;
        
        if (tab.CurrentView.CurrentValue is ImageViewer)
        {
            return;
        }
        tab.CurrentView.Value = new ImageViewer();
        if (Application.Current?.DataContext is not CoreViewModel core)
        {
            return;
        }
        TabNavigationInitializer.InitializeNewTab(tab, core.MainWindows.ActiveWindow.CurrentValue);
    }

    #endregion
}