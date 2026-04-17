using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Views.Main;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.Views.UC.Buttons;
using PicView.Avalonia.Views.UC.Menus;
using PicView.Core.ViewModels;
using R3.Avalonia;
using GalleryAnimationControlView = PicView.Avalonia.Views.Gallery.GalleryAnimationControlView;

namespace PicView.Avalonia.UI;

/// <summary>
/// Provides UI-related helper methods and properties
/// </summary>
public static class UIHelper2
{
    #region Controls

    public static MainView3? GetMainView { get; private set; }
    public static DraggableTabControl? GetMainTabControl { get; private set; }
    public static Control? GetTitlebar { get; private set; }
    public static EditableTitlebar? GetEditableTitlebar { get; private set; }
    public static GalleryAnimationControlView? GetGalleryView { get; private set; }
    public static BottomBar2? GetBottomBar { get; private set; }
    public static DropDownMenu? GetDropDownMenu { get; private set; }
    public static ToolTipMessage2? GetToolTipMessage { get; private set; }
    
    public static AvaloniaRenderingFrameProvider? GetFrameProvider { get; private set; }

    public static void SetFrameProvider(AvaloniaRenderingFrameProvider frameProvider) =>
        GetFrameProvider = frameProvider;
    
    public static CoreViewModel CoreViewModel => Application.Current.DataContext as CoreViewModel ?? throw new InvalidOperationException("CoreViewModel is null");

    /// <summary>
    /// Sets up control references from the main desktop application
    /// </summary>
    public static void SetControls(Window mainWindow)
    {
        GetMainView = mainWindow?.FindControl<MainView3>("MainView");
        GetTitlebar = mainWindow?.FindControl<Control>("Titlebar");
        GetEditableTitlebar = GetTitlebar?.FindControl<EditableTitlebar>("EditableTitlebar");
        //GetGalleryView = GetMainView?.MainPanel.GetControl<GalleryAnimationControlView>("GalleryView");
        GetBottomBar = mainWindow?.FindControl<BottomBar2>("BottomBar");
        GetToolTipMessage = GetMainView?.MainPanel.FindControl<ToolTipMessage2>("ToolTipMessage");
        GetMainTabControl = GetMainView.MainTabControl;
    }

    public static HoverBar2? GetHoverBar()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return null;
        }

        if (core.MainWindows.ActiveWindow.CurrentValue.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
        {
            return imageViewer.HoverBar;
        }

        return null;
    }
    
    public static void AddDropDownMenu()
    {
        var dropDownMenu = new DropDownMenu
        {
            Name = "DropDownMenu",
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(3, 0, 3, 0),
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Right,
            ZIndex = 2
        };
        GetMainView.MainPanel.Children.Add(dropDownMenu);
        GetDropDownMenu = dropDownMenu;
    }

    #endregion

    #region Helper functions`
    
    public static ClickArrowRight2? GetClickArrowRight(MainWindowViewModel vm)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
        {
            return imageViewer.ClickArrowRight;
        }
        return null;
    }
    
    public static ClickArrowLeft2? GetClickArrowLeft(MainWindowViewModel vm)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
        {
            return imageViewer.ClickArrowLeft;
        }
        return null;
    }
    
    private const string BoldFontLocation = "avares://PicView.Avalonia/Assets/Fonts/Roboto-Medium.ttf#Roboto";
    public static FontFamily BoldFontFamily => new(BoldFontLocation);
    private const string MediumFontLocation = "avares://PicView.Avalonia/Assets/Fonts/Roboto-Medium.ttf#Roboto";
    public static FontFamily MediumFontFamily => new(MediumFontLocation);

    public static void ShowMainContextMenu()
    {
        if (GetMainView.Resources.TryGetResource("MainContextMenu", Application.Current.ActualThemeVariant,
                out var value)
            && value is ContextMenu mainContextMenu)
        {
            mainContextMenu.Open();
        }
    }

    /// <summary>
    /// Centers the window or gallery based on current state
    /// </summary>
    public static void Center(MainWindowViewModel? vm)
    {
        if (vm is null)
        {
            return;
        }

        // if (GalleryFunctions.IsFullGalleryOpen)
        // {
        //     GalleryFunctions.CenterGallery(vm);
        // }
        // else
        // {
        //     WindowFunctions.CenterWindowOnScreen();
        // }
    }

    /// <inheritdoc cref="Center"/>
    public static async Task CenterAsync(MainWindowViewModel? vm)
    {
        if (vm is null)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() => { Center(vm); });
    }

    /// <summary>
    ///     Scrolls to the end of the gallery if the <paramref name="last" /> parameter is true.
    /// </summary>
    /// <param name="last">True to scroll to the end of the gallery.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ScrollToEndIfNecessary(bool last)
    {
        if (!Settings.Gallery.IsGalleryDocked)
        {
            return;
        }

        if (last)
        {
            await Dispatcher.UIThread.InvokeAsync(() => { GetGalleryView.GalleryListBox.ScrollToEnd(); });
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() => { GetGalleryView.GalleryListBox.ScrollToHome(); });
        }
    }

    public static void SetButtonInterval(RepeatButton? button)
    {
        button?.Interval = (int)TimeSpan.FromSeconds(Settings.UIProperties.NavSpeed).TotalMilliseconds;
    }

    public static void SetButtonInterval(IconButton? button)
    {
        button?.Interval = (int)TimeSpan.FromSeconds(Settings.UIProperties.NavSpeed).TotalMilliseconds;
    }

    public static DrawingImage? GetIcon(string resourceName)
    {
        if (!Application.Current.TryGetResource(resourceName,
                Application.Current.RequestedThemeVariant, out var icon))
        {
            return null;
        }

        return icon as DrawingImage;
    }

    public static SolidColorBrush GetBrush(string resourceName) =>
        new(GetColor(resourceName));

    public static Color GetColor(string resourceName)
    {
        if (!Application.Current.TryGetResource(resourceName,
                Application.Current.RequestedThemeVariant, out var textColor))
        {
            return default;
        }

        return textColor is not Color color ? default : color;
    }

    public static SolidColorBrush? GetSolidColorBrush(string resourceName)
    {
        if (!Application.Current.TryGetResource(resourceName,
        Application.Current.RequestedThemeVariant, out var textColor))
        {
            return null;
        }

        return textColor as SolidColorBrush ?? null;
    }

    public static void SetButtonHover(Control button, SolidColorBrush brush)
    {
        button.PointerEntered += (_, _) =>
        {
            brush.Color = GetColor("SecondaryTextColor");
        };
        button.PointerExited += (s, e) =>
        {
            brush.Color = GetColor("MainTextColor");
        };
    }

    public static void SwitchHoverClass(Control control)
    {
        control.Classes.Remove("altHover");
        control.Classes.Add("hover");
    }
    
    public static void SwitchAccentHoverClass(Control control)
    {
        control.Classes.Remove("altHover");
        control.Classes.Add("accentHover");
    }

    public static void SwitchHoverBorderClass(Control control)
    {
        control.Classes.Remove("noBorderHover");
        control.Classes.Add("hover");
    }

    public static SolidColorBrush? GetMenuBackgroundColor() =>
        GetBrush("MenuBackgroundColor");

    #endregion
}