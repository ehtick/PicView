using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.Preloading;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Core.FileHandling;
using PicView.Core.ProcessHandling;
using PicView.Core.Sizing;

namespace PicView.Avalonia.DragAndDrop;

public static class DragAndDropHelper
{
    private static DragDropView? _dragDropView;

    private static PreLoadValue? _preLoadValue;

    public static async Task Drop(DragEventArgs e, MainViewModel vm)
    {
        RemoveDragDropView();
        
        var files = e.Data.GetFiles();
        if (files == null)
        {
            await HandleDropFromUrl(e, vm);
            return;
        }

        var storageItems = files as IStorageItem[] ?? files.ToArray();
        var firstFile = storageItems.FirstOrDefault();
        var path = firstFile.Path.LocalPath;
        if (e.Data.Contains("text/x-moz-url"))
        {
            await HandleDropFromUrl(e, vm);
            if (vm.CurrentView != vm.ImageViewer)
            {
                await Dispatcher.UIThread.InvokeAsync(() => vm.CurrentView = vm.ImageViewer);
            }
        }
        else if (path.IsSupported())
        {
            if (vm.CurrentView != vm.ImageViewer)
            {
                await Dispatcher.UIThread.InvokeAsync(() => vm.CurrentView = vm.ImageViewer);
            }
            
            if (_preLoadValue is not null)
            {
                
                if (_preLoadValue.ImageModel.FileInfo.DirectoryName == Path.GetDirectoryName(path))
                {
                    NavigationManager.AddToPreloader(path, _preLoadValue.ImageModel);
                    NavigationManager.ImageIterator.Resynchronize();
                    await NavigationManager.LoadPicFromFile(path, vm, _preLoadValue.ImageModel.FileInfo);
                }
                else
                {
                    await NavigationManager.LoadPicFromStringAsync(path, vm).ConfigureAwait(false);
                }
            }
            else
            {
                await NavigationManager.LoadPicFromStringAsync(path, vm).ConfigureAwait(false);
            }
            
        }
        else if (Directory.Exists(path))
        {
            if (vm.CurrentView != vm.ImageViewer)
            {
                await Dispatcher.UIThread.InvokeAsync(() => vm.CurrentView = vm.ImageViewer);
            }
            await NavigationManager.LoadPicFromDirectoryAsync(path, vm).ConfigureAwait(false);

        }
        else if (path.IsArchive())
        {
            if (vm.CurrentView != vm.ImageViewer)
            {
                await Dispatcher.UIThread.InvokeAsync(() => vm.CurrentView = vm.ImageViewer);
            }
            await NavigationManager.LoadPicFromArchiveAsync(path, vm).ConfigureAwait(false);
        }

        if (!Settings.UIProperties.OpenInSameWindow)
        {
            foreach (var file in storageItems.Skip(1))
            {
                var filepath = file.Path.LocalPath;
                if (filepath.IsSupported())
                {
                    ProcessHelper.StartNewProcess(filepath);
                }
            }
        }
    }

    private static async Task HandleDropFromUrl(DragEventArgs e, MainViewModel vm)
    {
        var urlObject = e.Data.Get("text/x-moz-url");
        if (urlObject is byte[] bytes)
        {
            var dataStr = Encoding.Unicode.GetString(bytes);
            var url = dataStr.Split((char)10).FirstOrDefault();
            if (url != null)
            {
                if (url.StartsWith("file://"))
                {
                    var file = url[7..];
                    if (file.StartsWith('/'))
                    {
                        file = file[1..];
                    }
                    if (file.IsArchive())
                    {
                        await NavigationManager.LoadPicFromArchiveAsync(file, vm).ConfigureAwait(false);
                    }
                    else
                    {
                        await NavigationManager.LoadPicFromFile(file, vm).ConfigureAwait(false);
                    }
                }
                else
                {
                    await NavigationManager.LoadPicFromUrlAsync(url, vm).ConfigureAwait(false);
                }
            }
        }
    }

    public static async Task DragEnter(DragEventArgs e, MainViewModel vm, Control control) =>
        await HandleDragEnter(e.Data.GetFiles(), e, vm, control);

    private static async Task HandleDragEnter(IEnumerable<IStorageItem> files, DragEventArgs e, MainViewModel vm, Control control)
    {
        IStorageItem[]? fileArray = null;
        if (files is not null)
        {
           fileArray = files as IStorageItem[] ?? files.ToArray();
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_dragDropView == null)
            {
                _dragDropView = new DragDropView
                {
                    DataContext = vm
                };
                if (!control.IsPointerOver)
                {
                    UIHelper.GetMainView.MainGrid.Children.Add(_dragDropView);
                }
            }
            else
            {
                _dragDropView.RemoveThumbnail();
            }
        });
        if (fileArray is null)
        {
            var handledFromUrl = await HandleDragEnterFromUrl(e, vm);
            if (!handledFromUrl)
            {
                RemoveDragDropView();
            }
            return;
        }
        var firstFile = fileArray[0];
        var path = firstFile.Path.LocalPath;
        if (Directory.Exists(path))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!control.IsPointerOver)
                {
                    _dragDropView.AddDirectoryIcon();
                }
            });
        }
        else
        {
            if (path.IsArchive())
            {
                if (!control.IsPointerOver)
                {
                    _dragDropView.AddZipIcon();
                }
            }
            else if (path.IsSupported())
            {
                var ext = Path.GetExtension(path);
                if (ext.Equals(".svg", StringComparison.InvariantCultureIgnoreCase) || ext.Equals(".svgz", StringComparison.InvariantCultureIgnoreCase))
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _dragDropView?.UpdateSvgThumbnail(path);
                    });
                }
                else
                {
                    Bitmap? thumb;
                    // ReSharper disable once MethodHasAsyncOverload
                    var preload = NavigationManager.GetPreLoadValue(path);
                    if (preload?.ImageModel?.Image is Bitmap bmp)
                    {
                        thumb = bmp;
                    }
                    else
                    {
                        thumb = await GetThumbnails.GetThumbAsync(path, SizeDefaults.WindowMinSize - 30)
                            .ConfigureAwait(false);
                    }
                    await Dispatcher.UIThread.InvokeAsync(() => { _dragDropView?.UpdateThumbnail(thumb); });
                    await Task.Run(async () =>
                    {
                        var fileInfo = new FileInfo(path);
                        if (fileInfo.DirectoryName == NavigationManager.ImageIterator.InitialFileInfo.DirectoryName)
                        {
                            if (preload is null)
                            {
                                _preLoadValue = await NavigationManager.GetPreLoadValueAsync(path);
                                thumb = _preLoadValue.ImageModel.Image as Bitmap;
                                if (thumb is not null)
                                {
                                    await Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        _dragDropView?.UpdateThumbnail(thumb);
                                    }, DispatcherPriority.Loaded);
                                }
                            }
                            else
                            {
                                _preLoadValue = preload;
                            }
                        }
                        else if (preload is null)
                        {
                            thumb = await GetImage.GetDefaultBitmapAsync(fileInfo);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                _dragDropView?.UpdateThumbnail(thumb);
                            }, DispatcherPriority.Render);
                        }
                    });
                }
            }
        }
    }
    
    private static async Task<bool> HandleDragEnterFromUrl(object? urlObject, MainViewModel vm)
    {
        if (urlObject is null)
        {
            _dragDropView.RemoveThumbnail();
            return false;
        }
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _dragDropView ??= new DragDropView { DataContext = vm };
            if (!_dragDropView.IsLinkChainVisible)
            {
                _dragDropView.AddLinkChain();
            }
            if (!UIHelper.GetMainView.MainGrid.Children.Contains(_dragDropView))
            {
                UIHelper.GetMainView.MainGrid.Children.Add(_dragDropView);
            }
        });
        return true;
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
        UIHelper.GetMainView.MainGrid.Children.Remove(_dragDropView);
        _dragDropView = null;
    }
}