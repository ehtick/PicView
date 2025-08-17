using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;
using PicView.Core.Localization;
using GalleryItem = PicView.Avalonia.Views.Gallery.GalleryItem;

namespace PicView.Avalonia.Gallery;

public static class GalleryLoad
{
    private static string? _currentDirectory;
    private static CancellationTokenSource? _cancellationTokenSource;
    public static bool IsLoading { get; private set; }

    public static async ValueTask LoadGallery(MainViewModel vm, string currentDirectory)
    {
        // TODO: When list larger than 500, lazy load this when scrolling instead.
        // Figure out how to support virtualization.
        var (shouldProceed, galleryListBox) = await CanLoadGalleryAsync(vm, currentDirectory);
        if (!shouldProceed || galleryListBox is null)
        {
            return;
        }

        IsLoading = true;
        _currentDirectory = currentDirectory;
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            await PrepareGalleryUiAsync(vm);
            await CreateAndAddGalleryItemsAsync(vm, galleryListBox, token);
        }
        catch (OperationCanceledException)
        {
            await Dispatcher.UIThread.InvokeAsync(GalleryFunctions.Clear);
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(GalleryLoad), nameof(LoadGallery), e);
        }
        finally
        {
            CleanupAfterLoading();
        }
    }

    private static async ValueTask<(bool shouldProceed, GalleryListBox? galleryListBox)> CanLoadGalleryAsync(
        MainViewModel vm, string currentDirectory)
    {
        if (IsLoading || !NavigationManager.CanNavigate(vm) || string.IsNullOrEmpty(currentDirectory) ||
            _currentDirectory == currentDirectory)
        {
            return (false, null);
        }

        var galleryListBox = UIHelper.GetMainView?.GalleryView?.GalleryListBox;
        if (galleryListBox is null)
        {
            return (false, null);
        }

        return await Dispatcher.UIThread.InvokeAsync(() =>
            // Do not run if already populated.
            galleryListBox.Items.Count > 0 ? (false, galleryListBox) : (true, galleryListBox)
        );
    }

    private static async ValueTask PrepareGalleryUiAsync(MainViewModel vm)
    {
        await Dispatcher.UIThread.InvokeAsync(() => UIHelper.GetGalleryView.IsVisible = true);

        if (Settings.Gallery.IsBottomGalleryShown && !GalleryFunctions.IsFullGalleryOpen)
        {
            vm.Gallery.GalleryItem.ItemHeight.Value = vm.Gallery.GalleryItem.BottomGalleryItemHeight.CurrentValue;
        }

        GalleryStretchMode.DetermineStretchMode(vm);
    }

    private static async ValueTask CreateAndAddGalleryItemsAsync(MainViewModel vm, GalleryListBox galleryListBox,
        CancellationToken token)
    {
        var fileCount = NavigationManager.GetCount;
        var priority = GetDispatcherPriority(fileCount);

        var galleryItemSize = (uint)Math.Max(vm.Gallery.GalleryItem.BottomGalleryItemHeight.CurrentValue,
            vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.CurrentValue);

        for (var i = 0; i < fileCount; i++)
        {
            token.ThrowIfCancellationRequested();
            if (NavigationManager.GetInitialFileInfo?.DirectoryName != _currentDirectory &&
                _cancellationTokenSource is not null)
            {
                await _cancellationTokenSource.CancelAsync();
                token.ThrowIfCancellationRequested();
            }

            var x = i;

            GalleryItem? galleryItem = null;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                galleryItem = CreateGalleryItem(vm);
                galleryListBox.Items.Add(galleryItem);
                if (x == NavigationManager.GetCurrentIndex)
                {
                    galleryListBox.SelectedItem = galleryItem;
                }
            }, priority, token);
            _ = Task.Run(() => LoadThumbnailAsync(vm, galleryItemSize, galleryItem, galleryListBox, x), token);
        }
    }

    private static GalleryItem CreateGalleryItem(MainViewModel vm)
    {
        var galleryItem = new GalleryItem
        {
            DataContext = vm,
            FileName = { Text = TranslationManager.Translation.Loading },
            FileSize = { Text = TranslationManager.Translation.Loading },
            FileDate = { Text = TranslationManager.Translation.Loading },
            FileLocation = { Text = TranslationManager.Translation.Loading }
        };
        return galleryItem;
    }

    private static void UpdateGalleryItem(MainViewModel vm, FileInfo fileInfo,
        GalleryThumbInfo.GalleryThumbHolder thumbData, GalleryItem galleryItem)
    {
        galleryItem.FileName.Text = thumbData.FileName;
        galleryItem.FileName.Text = thumbData.FileName;
        galleryItem.FileSize.Text = thumbData.FileSize;
        galleryItem.FileDate.Text = thumbData.FileDate;
        galleryItem.FileLocation.Text = thumbData.FileLocation;

        galleryItem.PointerPressed += async (_, _) =>
        {
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                GalleryFunctions.ToggleGallery(vm);
            }

            await NavigationManager.Navigate(fileInfo, vm).ConfigureAwait(false);
        };
    }

    private static async ValueTask LoadThumbnailAsync(MainViewModel vm, uint galleryItemSize, GalleryItem galleryItem,
        GalleryListBox galleryListBox, int x)
    {
        var fileInfo = new FileInfo(NavigationManager.GetFileNameAt(x));

        var isSvg = fileInfo.Extension.Equals(".svg", StringComparison.OrdinalIgnoreCase) ||
                    fileInfo.Extension.Equals(".svgz", StringComparison.OrdinalIgnoreCase);
        Bitmap? thumb;
        if (isSvg)
        {
            thumb = null;
        }
        else
        {
            thumb = await GetThumbnails.GetThumbAsync(fileInfo, galleryItemSize);
        }

        var thumbData = GalleryThumbInfo.GalleryThumbHolder.GetThumbData(fileInfo);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            UpdateGalleryItem(vm, fileInfo, thumbData, galleryItem);
            if (isSvg)
            {
                galleryItem.GalleryImage.Source = new SvgImage { Source = SvgSource.Load(fileInfo.FullName) };
            }
            else
            {
                galleryItem.GalleryImage.Source = thumb;
            }
        }, DispatcherPriority.Render);

        if (x == NavigationManager.GetCurrentIndex)
        {
            Dispatcher.UIThread.Post(() => { galleryListBox.ScrollToCenterOfItem(galleryItem); },
                DispatcherPriority.SystemIdle);
        }
    }

    private static DispatcherPriority GetDispatcherPriority(int count) => count switch
    {
        >= 2000 => DispatcherPriority.Background,
        >= 1000 => DispatcherPriority.Loaded,
        _ => DispatcherPriority.Render
    };

    private static void CleanupAfterLoading()
    {
        IsLoading = false;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _currentDirectory = null;
    }

    public static async ValueTask ReloadGalleryAsync(MainViewModel vm, string currentDirectory)
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
        }

        // Wait for any ongoing loading to finish/cancel
        var checks = 0;
        while (IsLoading && checks < 50) // Timeout after ~10 seconds
        {
            await Task.Delay(200).ConfigureAwait(false);
            checks++;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                GalleryFunctions.Clear();
            }
            catch (Exception e)
            {
                DebugHelper.LogDebug(nameof(GalleryLoad), nameof(ReloadGalleryAsync), e);
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
    public static async ValueTask CheckAndReloadGallery(FileInfo fileInfo, MainViewModel vm)
    {
        if (Settings.Gallery.IsBottomGalleryShown || GalleryFunctions.IsFullGalleryOpen)
        {
            // Check if the bottom gallery should be shown
            if (!GalleryFunctions.IsFullGalleryOpen &&
                vm.Gallery.GalleryMode.CurrentValue is GalleryMode.BottomToClosed or GalleryMode.FullToClosed
                    or GalleryMode.Closed)
            {
                // Trigger animation to show it
                vm.Gallery.GalleryMode.Value = GalleryMode.ClosedToBottom;
            }

            await ReloadGalleryAsync(vm, fileInfo.DirectoryName);
        }
        else if (!GalleryFunctions.IsGalleryEmpty())
        {
            GalleryFunctions.Clear();
        }
    }

    public static async ValueTask CancelGalleryLoadAsync()
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
        }

        CleanupAfterLoading();
    }
}