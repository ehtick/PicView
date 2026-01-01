using Avalonia.Controls;
using Avalonia.Input;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Sizing;
using R3;

namespace PicView.Avalonia.Win32.Views;

public partial class WinTitleBar2 : UserControl
{
    public WinTitleBar2()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            if (Settings.Theme.GlassTheme)
            {
                ApplyGlassThemeStyles();
            }

            InitializeEventHandlers();
        };
    }

    // Extract method: centralize glass theme styling to remove duplication
    private void ApplyGlassThemeStyles()
    {
        GlassThemeHelper.ApplyTransparentStyle(TopWindowBorder);
        GlassThemeHelper.ApplyTransparentStyle(LogoBorder);
        GlassThemeHelper.ApplyTransparentStyle(EditableTitlebar);
        GlassThemeHelper.ApplyTransparentStyle(CloseButton);
        GlassThemeHelper.ApplyTransparentStyle(MinimizeButton);
        GlassThemeHelper.ApplyTransparentStyle(RestoreButton);
        GlassThemeHelper.ApplyTransparentStyle(FullscreenButton);
        GlassThemeHelper.ApplyTransparentStyle(DropDownMenuButton);
        GlassThemeHelper.ApplyTransparentStyle(MenuButton);
        GlassThemeHelper.ApplyTransparentStyle(MainMenu);

        var glassForeground = UIHelper.GetBrush("SecondaryTextColor");
        EditableTitlebar.Foreground = glassForeground;
        CloseButton.Foreground = glassForeground;
        MinimizeButton.Foreground = glassForeground;
        RestoreButton.Foreground = glassForeground;
        DropDownMenuButton.Foreground = glassForeground;
        MenuButton.Foreground = glassForeground;
    }
    
    private void InitializeEventHandlers()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }
        
        PointerPressed += (_, e) => TryDragWindow(e);
        PointerExited += (_, _) => { DragAndDropHelper.RemoveDragDropView(); };
        MainMenu.Closed += (_, _) => { CloseMenu(); };

        Observable.EveryValueChanged(vm.MainWindow.TopTitlebarViewModel.IsMainMenuVisible, x => x.Value,
                UIHelper.GetFrameProvider)
            .Subscribe(isVisible =>
            {
                if (isVisible)
                {
                    // Overflow buttons if the window is too small
                    if (Bounds.Width - SearchButton.Bounds.Width - DropDownMenuButton.Bounds.Width - CreateTabButton.Bounds.Width < SizeDefaults.WindowMinSize)
                    {
                        HideButtons(vm.MainWindow);
                    }
                    else
                    {
                        ShowButtons(vm.MainWindow);
                    }
                    
                    MainMenu.Open();
                    //FileMenuItem.Open();
                }
                else
                {
                    MainMenu.Close();
                    ShowButtons(vm.MainWindow);
                }
            });
    }

    private void HideButtons(MainWindowViewModel vm)
    {
        vm.TopTitlebarViewModel.IsBtnPanelVisible.Value = false;
        SearchButton.IsVisible = false;
        DropDownMenuButton.IsVisible = false;
        CreateTabButton.IsVisible = false;
    }
    
    private void ShowButtons(MainWindowViewModel vm)
    {
        vm.TopTitlebarViewModel.IsBtnPanelVisible.Value = true;
        SearchButton.IsVisible = true;
        DropDownMenuButton.IsVisible = true;
        CreateTabButton.IsVisible = true;
    }

    private void CloseMenu()
    {
        MainMenu.Close();

        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        vm.MainWindow.TopTitlebarViewModel.CloseMenu();
    }

    private void TryDragWindow(PointerPressedEventArgs e)
    {
        if (VisualRoot is null || DataContext is not MainViewModel vm)
        {
            return;
        }

        if (vm.MainWindow.IsEditableTitlebarOpen.Value || MainMenu.IsOpen)
        {
            return;
        }

        WindowFunctions.WindowDragAndDoubleClickBehavior((Window)VisualRoot, e, vm.PlatformWindowService);
    }
}