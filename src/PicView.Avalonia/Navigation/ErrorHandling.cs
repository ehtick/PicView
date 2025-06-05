using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.Clipboard;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.Gallery;
using PicView.Core.Sizing;
using StartUpMenu = PicView.Avalonia.Views.StartUpMenu;

namespace PicView.Avalonia.Navigation;

public static class ErrorHandling
{
    public static void ShowStartUpMenu(MainViewModel vm)
    {
        if (vm is null)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Start();
        }
        else
        {
            Dispatcher.UIThread.Post(Start);
        }

        return;
        void Start()
        {
            if (vm.CurrentView is not StartUpMenu)
            {
                var startUpMenu = new StartUpMenu();
                if (Settings.WindowProperties.AutoFit)
                {
                    startUpMenu.Width = SizeDefaults.WindowMinSize;
                    startUpMenu.Height = SizeDefaults.WindowMinSize;
                    if (Settings.Gallery.IsBottomGalleryShown)
                    {
                        vm.GalleryWidth = SizeDefaults.WindowMinSize;
                    }
                }
                vm.CurrentView = startUpMenu;
            }
            
            TitleManager.SetNoImageTitle(vm);

            vm.GalleryMode = GalleryMode.Closed;
            GalleryFunctions.Clear();
            MenuManager.CloseMenus(vm);
            vm.GalleryMargin = new Thickness(0, 0, 0, 0);
            vm.GetIndex = 0;
            vm.PlatformService.StopTaskbarProgress();
            vm.IsLoading = false;

            _ = NavigationManager.DisposeImageIteratorAsync();
            if (UIHelper.GetEditableTitlebar is not null)
            {
                UIHelper.GetEditableTitlebar.TextBlock.TextAlignment = TextAlignment.Center;
            }
        }
    }

    public static async Task ReloadAsync(MainViewModel vm)
    {
        vm.IsLoading = true;
        
        if (vm.PicViewer.ImageSource is null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ShowStartUpMenu(vm);
            });
            return;
        }
        
        if (!NavigationManager.CanNavigate(vm))
        {
            await NavigationManager.LoadPicFromStringAsync(FileHistoryManager.GetLastEntry(), vm).ConfigureAwait(false);
            return;
        }
        
        if (vm.PicViewer.ImageSource is null || !NavigationManager.CanNavigate(vm))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ShowStartUpMenu(vm);
            });
            return;
        }

        try
        {
            if (!NavigationManager.CanNavigate(vm))
            {
                var url = vm.PicViewer.Title.GetURL();
                if (!string.IsNullOrEmpty(url))
                {
                    await NavigationManager.LoadPicFromUrlAsync(url, vm).ConfigureAwait(false);
                }
                else 
                {
                    await ClipboardImageOperations.PasteClipboardImage(vm);
                }
            }
            else
            {
                await NavigationManager.QuickReload().ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
#if DEBUG
            Console.WriteLine(e);
#endif
            await Dispatcher.UIThread.InvokeAsync(() => { ShowStartUpMenu(vm); });
        }
        finally
        {
            vm.IsLoading = false;
        }
    }
}
