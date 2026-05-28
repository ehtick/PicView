using Avalonia.Controls;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;
using PicView.Core.DebugTools;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Win32.Views;

public partial class WinTitleBar : MainTitleBar
{
    public WinTitleBar()
    {
        InitializeComponent();
        SharedDropDownMenuButton = DropDownMenuButton;
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
        if (DataContext is not MainWindowViewModel vm || TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
        {
            return;
        }
        
        MainMenu.Closed += (_, _) => { CloseMenu(); };
        
        Observable.EveryValueChanged(vm.TopTitlebarViewModel.IsMainMenuVisible, x => x.Value,
                UIHelper.GetFrameProvider)
            .Subscribe( isVisible =>
            {
                if (isVisible)
                {
                    // Overflow buttons if the window is too small
                    if (Bounds.Width - SearchButton.Bounds.Width - DropDownMenuButton.Bounds.Width - CreateTabButton.Bounds.Width < SizeDefaults.SecondaryWindowMinWidth)
                    {
                        TruncateMenu(vm);
                    }
                    else
                    {
                        FullMenu(vm);
                    }
                    MainMenu.Open();
                    FileMenuItem.Open();
                }
                else
                {
                    MainMenu.Close();
                    FullMenu(vm);
                }
            }, DebugHelper.LogError(nameof(WinTitleBar), nameof(InitializeEventHandlers)))
            .AddTo(mainWindow.Disposables);
    }

    private void TruncateMenu(MainWindowViewModel vm)
    {
        vm.TopTitlebarViewModel.IsBtnPanelVisible.Value = false;
        LogoBorder.IsVisible = false;
        vm.TopTitlebarViewModel.MaxItemWidth.Value = 60;
    }
    
    private void FullMenu(MainWindowViewModel vm)
    {
        vm.TopTitlebarViewModel.IsBtnPanelVisible.Value = true;
        LogoBorder.IsVisible = true;
        vm.TopTitlebarViewModel.MaxItemWidth.Value = double.NaN;
        DropDownMenuButton.IsVisible = Bounds.Width > SizeDefaults.MainTitleDropDownBtnBp;
    }

    private void CloseMenu()
    {
        MainMenu.Close();

        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        vm.TopTitlebarViewModel.CloseMenu();
    }
}