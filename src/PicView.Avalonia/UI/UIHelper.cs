using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;

namespace PicView.Avalonia.UI;

/// <summary>
/// Provides UI-related helper methods and properties
/// </summary>
public static class UIHelper
{
    #region Controls

    public static MainView? GetMainView { get; private set; }
    public static Control? GetTitlebar { get; private set; }
    public static EditableTitlebar? GetEditableTitlebar { get; private set; }
    public static GalleryAnimationControlView? GetGalleryView { get; private set; }
    public static BottomBar? GetBottomBar { get; private set; }
    public static ToolTipMessage? GetToolTipMessage { get; private set; }

    /// <summary>
    /// Sets up control references from the main desktop application
    /// </summary>
    public static void SetControls(IClassicDesktopStyleApplicationLifetime desktop)
    {
        GetMainView = desktop.MainWindow?.FindControl<MainView>("MainView");
        GetTitlebar = desktop.MainWindow?.FindControl<Control>("Titlebar");
        GetEditableTitlebar = GetTitlebar?.FindControl<EditableTitlebar>("EditableTitlebar");
        GetGalleryView = GetMainView?.MainGrid.GetControl<GalleryAnimationControlView>("GalleryView");
        GetBottomBar = desktop.MainWindow?.FindControl<BottomBar>("BottomBar");
        GetToolTipMessage = GetMainView?.MainGrid.FindControl<ToolTipMessage>("ToolTipMessage");
    }

    #endregion

    #region Helper functions
    
    public static bool TryGetMainViewModel([NotNullWhen(true)] out MainViewModel? vm)
    {
        vm = GetMainView.DataContext as MainViewModel;
        return vm is not null;
    }
    
    /// <summary>
    /// Centers the window or gallery based on current state
    /// </summary>
    public static void Center(MainViewModel? vm)
    {
        if (vm is null)
        {
            return;
        }
        
        if (GalleryFunctions.IsFullGalleryOpen)
        {
            GalleryFunctions.CenterGallery(vm);
        }
        else
        {
            WindowFunctions.CenterWindowOnScreen();
        }
    }
    
    /// <inheritdoc cref="Center"/>
    public static async Task CenterAsync(MainViewModel? vm)
    {
        if (vm is null)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Center(vm);
        });
    }

    /// <summary>
    ///     Scrolls to the end of the gallery if the <paramref name="last" /> parameter is true.
    /// </summary>
    /// <param name="last">True to scroll to the end of the gallery.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ScrollToEndIfNecessary(bool last)
    {
        if (!Settings.Gallery.IsBottomGalleryShown)
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

    /// <summary>
    ///     Moves the cursor on the navigation button.
    /// </summary>
    /// <param name="next">True to move the cursor to the next button, false for the previous button.</param>
    /// <param name="arrow">True to move the cursor on the arrow, false to move the cursor on the button.</param>
    /// <param name="vm">The main view model instance.</param>
    public static async Task MoveCursorOnButtonClick(bool next, bool arrow, MainViewModel vm) =>
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var buttonName = arrow
                ? next ? "ClickArrowRight" : "ClickArrowLeft"
                : next
                    ? "NextButton"
                    : "PreviousButton";
            Control control = arrow
                ? GetMainView.GetControl<UserControl>(buttonName)
                : GetBottomBar.GetControl<Button>(buttonName);
            var point = arrow
                ? next ? new Point(65, 95) : new Point(15, 95)
                : new Point(50, 10);
            var p = control.PointToScreen(point);
            vm.PlatformService?.SetCursorPos(p.X, p.Y);
        });

    #endregion

    #region Navigation buttons

    /// <summary>
    /// Navigates to the next image using the bottom navigation button
    /// </summary>
    public static async Task NextButtonNavigation(MainViewModel vm) =>
        await SetButtonIntervalAndNavigate(GetBottomBar?.NextButton, true, false, vm);

    /// <summary>
    /// Navigates to the previous image using the bottom navigation button
    /// </summary>
    public static async Task PreviousButtonNavigation(MainViewModel vm) =>
        await SetButtonIntervalAndNavigate(GetBottomBar?.PreviousButton, false, false, vm);

    /// <summary>
    /// Navigates to the next image using the arrow button
    /// </summary>
    public static async Task NextArrowButtonNavigation(MainViewModel vm) =>
        await SetButtonIntervalAndNavigate(GetMainView?.ClickArrowRight?.PolyButton, true, true, vm);

    /// <summary>
    /// Navigates to the previous image using the arrow button
    /// </summary>
    public static async Task PreviousArrowButtonNavigation(MainViewModel vm) =>
        await SetButtonIntervalAndNavigate(GetMainView?.ClickArrowLeft?.PolyButton, false, true, vm);

    private static async Task SetButtonIntervalAndNavigate(RepeatButton? button, bool isNext, bool isArrow,
        MainViewModel vm)
    {
        if (button != null)
        {
            button.Interval = (int)TimeSpan.FromSeconds(Settings.UIProperties.NavSpeed).TotalMilliseconds;
        }

        await NavigationManager.NavigateAndPositionCursor(isNext, isArrow, vm);
    }

    private static async Task SetButtonIntervalAndNavigate(IconButton? button, bool isNext, bool isArrow,
        MainViewModel vm)
    {
        if (button != null)
        {
            button.Interval = (int)TimeSpan.FromSeconds(Settings.UIProperties.NavSpeed).TotalMilliseconds;
        }

        await NavigationManager.NavigateAndPositionCursor(isNext, isArrow, vm);
    }

    #endregion
}