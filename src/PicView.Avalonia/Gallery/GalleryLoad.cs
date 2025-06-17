using Avalonia.Svg.Skia;
using Avalonia.Threading;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Core.Gallery;

namespace PicView.Avalonia.Gallery;

public static class GalleryLoad
{
    private static string? _currentDirectory;
    private static CancellationTokenSource? _cancellationTokenSource;
    public static bool IsLoading { get; private set; }

    public static async Task LoadGallery(MainViewModel vm, string currentDirectory)
    {
        // TODO: When list larger than 500, lazy load this when scrolling instead.
        // Figure out how to support virtualization. 

        if (IsLoading || !NavigationManager.CanNavigate(vm) || string.IsNullOrEmpty(currentDirectory))
        {
            return;
        }

        var mainView = UIHelper.GetMainView;
        var galleryListBox = mainView.GalleryView.GalleryListBox;
        var toReturn = false;

        if (!string.IsNullOrEmpty(_currentDirectory))
        {
            if (_currentDirectory == currentDirectory)
            {
                return;
            }
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (galleryListBox is null)
            {
                toReturn = true;
                return;
            }

            if (galleryListBox.Items.Count > 0)
            {
                // Make sure to not run consecutively
                toReturn = true;
            }
        });
        if (toReturn)
        {
            return;
        }
        // Make sure gallery is visible
        await Dispatcher.UIThread.InvokeAsync(() => UIHelper.GetGalleryView.IsVisible = true);

        // Make sure height is set
        if (Settings.Gallery.IsBottomGalleryShown && !GalleryFunctions.IsFullGalleryOpen)
        {
            vm.GetGalleryItemHeight = vm.GetBottomGalleryItemHeight;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _currentDirectory = currentDirectory;
        IsLoading = true;
        var index = NavigationManager.GetCurrentIndex;
        var galleryItemSize = Math.Max(vm.GetBottomGalleryItemHeight, vm.GetFullGalleryItemHeight);

        var endIndex = NavigationManager.GetCount;
        // Set priority low when loading excess images to ensure app responsiveness
        var priority = endIndex switch
        {
            >= 2000 => DispatcherPriority.Background,
            >= 1000 => DispatcherPriority.Loaded,
            _ => DispatcherPriority.Render
        };

        GalleryStretchMode.SetSquareFillStretch(vm);
        var fileInfos = new FileInfo[endIndex];

        try
        {
            for (var i = 0; i < endIndex; i++)
            {
                if (NavigationManager.GetInitialFileInfo?.DirectoryName != _currentDirectory || _cancellationTokenSource.IsCancellationRequested)
                {
                    await _cancellationTokenSource.CancelAsync();
                    return;
                }

                fileInfos[i] = new FileInfo(NavigationManager.GetFileNameAt(i));
                var thumbData = GalleryThumbInfo.GalleryThumbHolder.GetThumbData(fileInfos[i]);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var galleryItem = new GalleryItem
                    {
                        DataContext = vm,
                        FileName = { Text = thumbData.FileName },
                        FileSize = { Text = thumbData.FileSize },
                        FileDate = { Text = thumbData.FileDate },
                        FileLocation = { Text = fileInfos[i].FullName }
                    };
                    var i1 = i;
                    galleryItem.PointerPressed += async (_, _) =>
                    {
                        if (GalleryFunctions.IsFullGalleryOpen)
                        {
                            GalleryFunctions.ToggleGallery(vm);
                        }

                        await NavigationManager.Navigate(fileInfos[i1], vm).ConfigureAwait(false);
                    };
                    galleryListBox.Items.Add(galleryItem);
                    if (i != NavigationManager.GetCurrentIndex)
                    {
                        return;
                    }

                    vm.SelectedGalleryItemIndex = i;
                    galleryListBox.SelectedItem = galleryItem;
                }, priority, _cancellationTokenSource.Token);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (galleryListBox.Items.Count == 0)
                {
                    return;
                }

                if (galleryListBox.Items[0] is not GalleryItem galleryItem)
                {
                    return;
                }

                var horizontalItems = (int)Math.Floor(galleryListBox.Bounds.Width / galleryItem.ImageBorder.MinWidth);
                index = (NavigationManager.GetCurrentIndex - horizontalItems) % NavigationManager.GetCount;
            });

            index = index < 0 ? 0 : index;
            var maxDegreeOfParallelism = Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : 2;
            ParallelOptions options = new() { MaxDegreeOfParallelism = maxDegreeOfParallelism };
            ParallelOptions secondOptions = new() { MaxDegreeOfParallelism = 2 };

            var positiveLoopTask = AsyncLoop(true, index, endIndex, options, _cancellationTokenSource.Token);
            var negativeLoopTask = AsyncLoop(false, 0, index, secondOptions, _cancellationTokenSource.Token);

            await Task.WhenAll(positiveLoopTask, negativeLoopTask).ConfigureAwait(false);

            GalleryStretchMode.DetermineStretchMode(vm);
            GalleryNavigation.CenterScrollToSelectedItem(vm);
        }
        catch (OperationCanceledException)
        {
            await Dispatcher.UIThread.InvokeAsync(GalleryFunctions.Clear);
        }
        catch (Exception e)
        {
#if DEBUG
            Console.WriteLine($"GalleryLoad exception:\n{e.Message}");
#endif
        }
        finally
        {
            IsLoading = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _currentDirectory = null;
        }

        return;

        async Task AsyncLoop(bool positive, int startPosition, int endPosition, ParallelOptions options,
            CancellationToken ct)
        {
            await Parallel.ForAsync(0, endPosition, options, async (i, _) =>
            {
                if (NavigationManager.GetInitialFileInfo?.DirectoryName != _currentDirectory || _cancellationTokenSource.IsCancellationRequested)
                {
                    await _cancellationTokenSource.CancelAsync();
                    return;
                }

                ct.ThrowIfCancellationRequested();

                if (i < 0 || i >= NavigationManager.GetCount)
                {
                    return;
                }

                var nextIndex = positive
                    ? (startPosition + i) % endPosition
                    : (startPosition - i + endPosition) % endPosition;

                var thumb = await GetThumbnails.GetThumbAsync(fileInfos[nextIndex], (uint)galleryItemSize);

                var isSvg = fileInfos[nextIndex].Extension.Equals(".svg", StringComparison.OrdinalIgnoreCase) ||
                            fileInfos[nextIndex].Extension.Equals(".svgz", StringComparison.OrdinalIgnoreCase);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (nextIndex < 0 || nextIndex >= galleryListBox.Items.Count)
                    {
                        return;
                    }

                    if (galleryListBox.Items[nextIndex] is not GalleryItem galleryItem)
                    {
                        return;
                    }

                    if (isSvg)
                    {
                        galleryItem.GalleryImage.Source = new SvgImage
                            { Source = SvgSource.Load(fileInfos[nextIndex].FullName) };
                    }
                    else if (thumb is not null)
                    {
                        galleryItem.GalleryImage.Source = thumb;
                    }

                    if (nextIndex == NavigationManager.GetCurrentIndex)
                    {
                        galleryListBox.ScrollToCenterOfItem(galleryItem);
                    }
                }, priority, ct);
            });
        }
    }

    public static async Task ReloadGalleryAsync(MainViewModel vm, string currentDirectory)
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                try
                {
                    GalleryFunctions.Clear();
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine($"GalleryLoad exception:\n{e.Message}");
#endif
                }
            });
        }

        var x = 0;
        while (IsLoading)
        {
            await Task.Delay(200).ConfigureAwait(false);
            x++;
            if (x > 100)
            {
                break;
            }
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                GalleryFunctions.Clear();
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine($"GalleryLoad exception:\n{e.Message}");
#endif
            }
        });

        await LoadGallery(vm, currentDirectory).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Checks and reloads the gallery if necessary based on the provided file info.
    /// </summary>
    /// <param name="fileInfo">The file info to check.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task CheckAndReloadGallery(FileInfo fileInfo, MainViewModel vm)
    {
        if (Settings.Gallery.IsBottomGalleryShown || GalleryFunctions.IsFullGalleryOpen)
        {
            GalleryFunctions.Clear();

            // Check if the bottom gallery should be shown
            if (!GalleryFunctions.IsFullGalleryOpen)
            {
                if (vm.GalleryMode is GalleryMode.BottomToClosed or GalleryMode.FullToClosed or GalleryMode.Closed)
                {
                    // Trigger animation to show it
                    vm.GalleryMode = GalleryMode.ClosedToBottom;
                }
            }

            await ReloadGalleryAsync(vm, fileInfo.DirectoryName);
        }
    }
}