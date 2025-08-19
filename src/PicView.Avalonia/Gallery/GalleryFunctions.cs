using Avalonia.Layout;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;
using PicView.Core.Localization;
using PicView.Core.Sizing;
using GalleryItem = PicView.Avalonia.Views.Gallery.GalleryItem;

namespace PicView.Avalonia.Gallery;

public static class GalleryFunctions
{
    public static double GetGalleryHeight(MainViewModel vm)
    {
        if (vm?.Gallery is not { } gallery)
        {
            return 0;
        }

        if (!Settings.Gallery.IsBottomGalleryShown || vm.PicViewer.IsSingleImage.CurrentValue || Slideshow.IsRunning)
        {
            return 0;
        }

        if (Settings.WindowProperties.Fullscreen)
        {
            return Settings.Gallery.IsBottomGalleryShown
                ? gallery.GalleryItem.BottomGalleryItemHeight.CurrentValue + (SizeDefaults.ScrollbarSize - 1)
                : 0;
        }

        if (!Settings.Gallery.ShowBottomGalleryInHiddenUI && !vm.MainWindow.IsUIShown.CurrentValue)
        {
            return 0;
        }

        return gallery.GalleryItem.BottomGalleryItemHeight.CurrentValue + (SizeDefaults.ScrollbarSize - 1);
    }

    public static bool IsGalleryEmpty()
    {
        var mainView = UIHelper.GetMainView;
        var galleryListBox = mainView?.GalleryView?.GalleryListBox;
        if (galleryListBox == null)
        {
            return true;
        }
        return galleryListBox.Items.Count == 0;
    }

    public static bool RenameGalleryItem(int oldIndex, int newIndex, string newFileLocation, string newName)
    {
        var mainView = UIHelper.GetMainView;

        var galleryListBox = mainView.GalleryView.GalleryListBox;
        if (galleryListBox == null)
        {
            return false;
        }

        if (galleryListBox.Items.Count <= oldIndex)
        {
            return false;
        }

        if (galleryListBox.Items.Count < 0 || oldIndex >= galleryListBox.ItemCount)
        {
            return false;
        }

        if (galleryListBox.Items.Count <= 0 || oldIndex >= galleryListBox.Items.Count)
        {
            return false;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            return Rename();
        }

        Dispatcher.UIThread.InvokeAsync(Rename);

        return true;

        bool Rename()
        {
            if (galleryListBox.Items[oldIndex] is not GalleryItem galleryItem)
            {
                return false;
            }

            galleryItem.FileName.Text = newName;
            galleryItem.FileLocation.Text = newFileLocation;
            if (oldIndex == newIndex)
            {
                galleryListBox.Items[oldIndex] = galleryItem;
                return true;
            }

            if (newIndex >= 0 && newIndex < galleryListBox.Items.Count)
            {
                galleryListBox.Items.RemoveAt(oldIndex);
                galleryListBox.Items.Insert(newIndex, galleryItem);
                return true;
            }

            return false;
        }
    }

    public static bool RemoveGalleryItem(int index, MainViewModel? vm)
    {
        var mainView = UIHelper.GetMainView;

        var galleryListBox = mainView.GalleryView.GalleryListBox;
        if (galleryListBox == null)
        {
            return false;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Removal();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Removal);
        }

        return true;

        void Removal()
        {
            var removalIndex = galleryListBox.Items.Count > index ? index : galleryListBox.Items.Count - 1;
            if (removalIndex <= -1)
            {
                return;
            }

            if (galleryListBox.Items[removalIndex] is not GalleryItem galleryItem)
            {
                return;
            }

            galleryListBox.Items.Remove(galleryItem);
            if (galleryItem.GalleryImage.Source is IDisposable galleryImage)
            {
                galleryImage.Dispose();
            }
        }
    }

    public static async Task<bool> AddGalleryItem(int index, FileInfo fileInfo, MainViewModel? vm,
        DispatcherPriority? priority = null)
    {
        var mainView = UIHelper.GetMainView;

        var galleryListBox = mainView.GalleryView.GalleryListBox;
        if (galleryListBox == null)
        {
            return false;
        }

        GalleryItem? galleryItem;
        var thumb = await GetThumbnails.GetThumbAsync(fileInfo, (uint)vm.Gallery.GalleryItem.ItemHeight.Value);
        var galleryThumbInfo = GalleryThumbInfo.GalleryThumbHolder.GetThumbData(fileInfo);
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                galleryItem = new GalleryItem
                {
                    FileLocation =
                    {
                        Text = galleryThumbInfo.FileLocation
                    },
                    FileDate =
                    {
                        Text = galleryThumbInfo.FileDate
                    },
                    FileSize =
                    {
                        Text = galleryThumbInfo.FileSize
                    },
                    FileName =
                    {
                        Text = galleryThumbInfo.FileName
                    }
                };
                galleryItem.PointerPressed += async (_, _) =>
                {
                    if (IsFullGalleryOpen)
                    {
                        ToggleGallery(vm);
                    }

                    await NavigationManager.Navigate(fileInfo, vm).ConfigureAwait(false);
                };
                if (galleryListBox.Items.Count > index)
                {
                    galleryListBox.Items.Insert(index, galleryItem);
                }
                else
                {
                    galleryListBox.Items.Add(galleryItem);
                }

                var isSvg = fileInfo.Extension.Equals(".svg", StringComparison.OrdinalIgnoreCase) ||
                            fileInfo.Extension.Equals(".svgz", StringComparison.OrdinalIgnoreCase);
                if (isSvg)
                {
                    galleryItem.GalleryImage.Source = new SvgImage
                        { Source = SvgSource.Load(fileInfo.FullName) };
                }
                else if (thumb is not null)
                {
                    galleryItem.GalleryImage.Source = thumb;
                }
            }, priority ?? DispatcherPriority.Render);
            return true;
        }
        catch (Exception exception)
        {
            DebugHelper.LogDebug(nameof(GalleryFunctions), nameof(AddGalleryItem), exception);
        }

        return false;
    }

    public static void Clear()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            ClearItems();
        }
        else
        {
            Dispatcher.UIThread.Post(ClearItems);
        }

        return;

        void ClearItems()
        {
            try
            {
                var mainView = UIHelper.GetMainView;

                var galleryListBox = mainView?.GalleryView.GalleryListBox;
                if (galleryListBox == null)
                {
                    return;
                }

                for (var i = 0; i < galleryListBox.ItemCount; i++)
                {
                    if (galleryListBox.Items[i] is not GalleryItem galleryItem)
                    {
                        continue;
                    }

                    if (galleryItem.GalleryImage.Source is IDisposable galleryImage)
                    {
                        galleryImage.Dispose();
                    }

                    galleryListBox.Items.Remove(galleryItem);
                }

                galleryListBox.Items.Clear();
#if DEBUG
                Console.WriteLine("Gallery items cleared");
#endif
            }
            catch (Exception e)
            {
                DebugHelper.LogDebug(nameof(GalleryFunctions), nameof(ClearItems), e);
            }
        }
    }

    public static void CenterGallery(MainViewModel vm)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Center();
        }
        else
        {
            Dispatcher.UIThread.Post(Center);
        }

        return;

        void Center()
        {
            var mainView = UIHelper.GetMainView;

            var galleryListBox = mainView.GalleryView.GalleryListBox;
            if (vm.PicViewer.Index.Value < 0 || vm.PicViewer.Index.Value >= galleryListBox.Items.Count)
            {
                return;
            }

            if (galleryListBox.Items[vm.PicViewer.Index.CurrentValue] is GalleryItem centerItem)
            {
                galleryListBox.ScrollToCenterOfItem(centerItem);
            }
        }
    }

    #region Gallery toggle

    public static bool IsFullGalleryOpen { get; private set; }

    public static void ToggleGallery(MainViewModel vm)
    {
        if (vm is null || !NavigationManager.CanNavigate(vm))
        {
            return;
        }

        MenuManager.CloseMenus(vm);
        if (Settings.Gallery.IsBottomGalleryShown)
        {
            if (IsFullGalleryOpen)
            {
                // Switch to bottom gallery
                IsFullGalleryOpen = false;
                vm.Gallery.GalleryMode.Value = GalleryMode.FullToBottom;
                vm.Gallery.GalleryItem.ItemHeight.Value = vm.Gallery.GalleryItem.BottomGalleryItemHeight.CurrentValue;
            }
            else
            {
                // Switch to full gallery
                IsFullGalleryOpen = true;
                vm.Gallery.GalleryMode.Value = GalleryMode.BottomToFull;
                vm.Gallery.GalleryItem.ItemHeight.Value = vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.CurrentValue;;
            }
        }
        else
        {
            if (IsFullGalleryOpen)
            {
                // close full gallery
                IsFullGalleryOpen = false;
                vm.Gallery.GalleryMode.Value = GalleryMode.FullToClosed;
            }
            else
            {
                // open full gallery
                IsFullGalleryOpen = true;
                vm.Gallery.GalleryMode.Value = GalleryMode.ClosedToFull;
                vm.Gallery.GalleryItem.ItemHeight.Value = vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.CurrentValue;
            }
        }


        _ = Task.Run(() => GalleryLoad.LoadGallery(vm, NavigationManager.GetInitialFileInfo?.DirectoryName));
    }

    public static void OpenCloseBottomGallery(MainViewModel vm)
    {
        if (vm is null)
        {
            return;
        }

        MenuManager.CloseMenus(vm);

        if (Settings.Gallery.IsBottomGalleryShown)
        {
            vm.Gallery.GalleryMode.Value = GalleryMode.BottomToClosed;
            vm.Translation.IsShowingBottomGallery.Value = TranslationManager.Translation.ShowBottomGallery;
            Settings.Gallery.IsBottomGalleryShown = false;
            IsFullGalleryOpen = false;
            return;
        }

        IsFullGalleryOpen = false;
        Settings.Gallery.IsBottomGalleryShown = true;
        if (NavigationManager.CanNavigate(vm))
        {
            vm.Gallery.GalleryMode.Value = GalleryMode.ClosedToBottom;
        }

        vm.Translation.IsShowingBottomGallery.Value = TranslationManager.Translation.HideBottomGallery;
        vm.Gallery.IsBottomGalleryShown.Value = true;
        if (!NavigationManager.CanNavigate(vm))
        {
            return;
        }

        Task.Run(() => GalleryLoad.LoadGallery(vm, NavigationManager.GetInitialFileInfo?.DirectoryName));
    }

    public static void OpenBottomGallery(MainViewModel vm)
    {
        vm.Gallery.GalleryMode.Value = GalleryMode.ClosedToBottom;
        vm.Gallery.GalleryVerticalAlignment.Value = VerticalAlignment.Bottom;
    }

    public static void CloseGallery(MainViewModel vm)
    {
        if (IsFullGalleryOpen)
        {
            ToggleGallery(vm);
        }
        else
        {
            OpenCloseBottomGallery(vm);
        }
    }

    #endregion
}