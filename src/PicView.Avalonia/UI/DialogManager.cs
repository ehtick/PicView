using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.Crop;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.Views.UC.PopUps;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.UI;

public static class DialogManager
{
    public static bool IsDialogOpen { get; set; }
    
    /// <summary>
    /// Handles close action based on current application state
    /// </summary>
    public static async Task HandleShouldClosing(MainWindowViewModel vm)
    {
        // Handle cropping mode
        if (CropManager.IsCropping)
        {
            CropManager.CloseCropControl(vm);
            return;
        }

        // Handle slideshow
        if (Slideshow.IsRunning)
        {
            Slideshow.StopSlideshow(vm);
            return;
        }
        
        // Handle window close
        await Dispatcher.UIThread.InvokeAsync(CloseWithOptionalDialog);
    }

    public static void CloseWithOptionalDialog()
    {
        if (Settings.UIProperties.ShowConfirmationOnEsc)
        {
            UIHelper.GetMainView?.MainPanel.Children.Add(new CloseDialog());
        }
        else
        {
            CloseMainWindow();
        }
    }

    public static void CloseMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow?.Close();
        }
    }

    public static void AddFileSearchDialog()
    {
        if (UIHelper.GetMainView.MainPanel.Children.OfType<FileSearchDialog>().Any() || UIHelper.GetMainView.DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        vm.TopTitlebarViewModel.CloseDropDownMenu();
        UIHelper.GetMainView.MainPanel.Children.Add(new FileSearchDialog());
        IsDialogOpen = true;
    }

    public static void AddNavigationDialog()
    {
        // TODO
        // if (UIHelper.GetMainView.MainGrid.Children.OfType<NavigationDialog>().Any())
        // {
        //     return;
        // }
        //
        // MenuManager.CloseMenus(UIHelper.GetMainView.DataContext as MainViewModel);
        // UIHelper.GetMainView.MainGrid.Children.Add(new NavigationDialog());
    }
}
