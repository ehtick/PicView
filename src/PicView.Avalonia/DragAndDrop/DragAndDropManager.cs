using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.UI;
using PicView.Avalonia.Views.UC;
using PicView.Core.FileHandling;
using PicView.Core.Preloading;
using PicView.Core.ProcessHandling;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.DragAndDrop;

public static class DragAndDropManager
{
    private static DragDropView? _dragDropView;
    private static PreLoadValue? _preLoadValue;

    #region Public Entry Points

    public static async Task Drop(DragEventArgs e, TabOverviewViewModel tabOverview)
    {
        RemoveDragDropView();

        var files = e.DataTransfer.TryGetFiles();
        if (files == null)
        {
            await HandleDropFromUrl(e, tabOverview);
            return;
        }

        var filesArray = files as IStorageItem[] ?? files.ToArray();
        var firstFile = filesArray.FirstOrDefault();
        if (firstFile == null)
        {
            return;
        }
        
        // Handle opening additional files in new windows if needed
        if (filesArray.Length > 1)
        {
            _ = Task.Run(() => HandleAdditionalFiles(filesArray.Skip(1)));
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

    public static async Task DragEnter(DragEventArgs e, Control control)
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

            var handled = await HandleDragEnterFromUrl(value);
            if (!handled)
            {
                RemoveDragDropView();
            }
        }
    }

    public static void DragLeave(DragEventArgs e, Control control)
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
        if (_dragDropView != null)
        {
            UIHelper2.GetMainView?.MainPanel.Children.Remove(_dragDropView);
            _dragDropView = null;
        }
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
        
        // We might not have direct access to MainWindowViewModel here easily without passing it,
        // but we can use TabOverviewViewModel to load.
        
        if (url.StartsWith("file://"))
        {
            var file = url[7..];
            if (file.StartsWith('/'))
            {
                file = file[1..];
            }

            await tabOverview.LoadFromFileAsync(file);
        }
        else
        {
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
            await ShowDirectoryIcon(control);
        }
        else if (path.IsArchive())
        {
            await ShowArchiveIcon(control);
        }
        else if (path.IsSupported())
        {
            await ShowFilePreview(new FileInfo(path));
        }
    }

    private static async Task ShowDirectoryIcon(Control control)
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
        {
            if (!control.IsPointerOver)
            {
                _dragDropView?.AddDirectoryIcon();
            }
        });
    }

    private static async Task ShowArchiveIcon(Control control)
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
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
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => _dragDropView?.UpdateSvgThumbnail(fileInfo.FullName));
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
            await UpdateThumbnailUI(thumb);
        }
        else
        {
            // Generate thumbnail
            thumb = await GetThumbnails.GetThumbAsync(fileInfo, SizeDefaults.WindowMinSize - 30)
                .ConfigureAwait(false);
            await UpdateThumbnailUI(thumb);
            
            // Load full image in background
            await PreloadFullImage(fileInfo, preLoadValue, thumb);
        }
    }

    private static async Task PreloadFullImage(FileInfo fileInfo, PreLoadValue? preload, Bitmap? thumb)
    {
        await Task.Run(async () =>
        {
            if (preload is null)
            {
                var model = await GetImageModel.GetImageModelAsync(fileInfo);
                await UpdateThumbnailUI(thumb);
                _preLoadValue = new PreLoadValue(model);
            }
            else
            {
                _preLoadValue = preload;
                await UpdateThumbnailUI(thumb);
            }
        });
    }

    private static async Task UpdateThumbnailUI(Bitmap? thumb) =>
        await Dispatcher.CurrentDispatcher.InvokeAsync(() => _dragDropView?.UpdateThumbnail(thumb));

    private static async Task<bool> HandleDragEnterFromUrl(object? urlObject)
    {
        if (urlObject is null)
        {
            _dragDropView?.RemoveThumbnail();
            return false;
        }

        await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
        {
            _dragDropView ??= new DragDropView();
            if (!_dragDropView.IsLinkChainVisible)
            {
                _dragDropView.AddLinkChain();
            }

            if (UIHelper2.GetMainView != null && !UIHelper2.GetMainView.MainPanel.Children.Contains(_dragDropView))
            {
                UIHelper2.GetMainView.MainPanel.Children.Add(_dragDropView);
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
                if (!control.IsPointerOver && UIHelper2.GetMainView != null)
                {
                    UIHelper2.GetMainView.MainPanel.Children.Add(_dragDropView);
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
        if (_preLoadValue is not null)
        {
             // TODO: Add to shared cache
             // if (Application.Current.DataContext is CoreViewModel core)
             // {
             //     core.SharedCache.Add() = _preLoadValue;
             // }
        }
        
        await tabOverview.LoadFromFileAsync(path);
    }

    #endregion
}