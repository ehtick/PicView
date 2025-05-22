using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views;
using PicView.Avalonia.Views.UC;

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
            var buttonName = GetNavigationButtonName(next, arrow);
            var control = GetButtonControl(buttonName, arrow);
            var point = GetClickPoint(next, arrow);
            var p = control.PointToScreen(point);
            vm.PlatformService?.SetCursorPos(p.X, p.Y);
        });

    /// <summary>
    ///     Gets the name of the navigation button based on input parameters.
    /// </summary>
    /// <param name="next">True for the next button, false for the previous button.</param>
    /// <param name="arrow">True if the navigation uses arrow keys.</param>
    /// <returns>The name of the navigation button.</returns>
    private static string GetNavigationButtonName(bool next, bool arrow) =>
        arrow
            ? next ? "ClickArrowRight" : "ClickArrowLeft"
            : next
                ? "NextButton"
                : "PreviousButton";

    /// <summary>
    ///     Gets the control associated with the specified button name.
    /// </summary>
    /// <param name="buttonName">The name of the button.</param>
    /// <param name="isArrowButton">True if the control is an arrow button.</param>
    /// <returns>The control associated with the button.</returns>
    private static Control GetButtonControl(string buttonName, bool isArrowButton) =>
        isArrowButton
            ? GetMainView.GetControl<UserControl>(buttonName)
            : GetBottomBar.GetControl<Button>(buttonName);

    /// <summary>
    ///     Gets the point to click on the button based on the input parameters.
    /// </summary>
    /// <param name="next">True for the next button, false for the previous button.</param>
    /// <param name="arrow">True if the navigation uses arrow keys.</param>
    /// <returns>The point to click on the button.</returns>
    private static Point GetClickPoint(bool next, bool arrow) =>
        arrow
            ? next ? new Point(65, 95) : new Point(15, 95)
            : new Point(50, 10);

    #endregion

    #region Navigation buttons

    /// <summary>
    /// Navigates to the next image using the bottom navigation button
    /// </summary>
    public static void NextButtonNavigation(MainViewModel vm) =>
        SetButtonIntervalAndNavigate(GetBottomBar?.NextButton, true, false, vm);

    /// <summary>
    /// Navigates to the previous image using the bottom navigation button
    /// </summary>
    public static void PreviousButtonNavigation(MainViewModel vm) =>
        SetButtonIntervalAndNavigate(GetBottomBar?.PreviousButton, false, false, vm);

    /// <summary>
    /// Navigates to the next image using the arrow button
    /// </summary>
    public static void NextArrowButtonNavigation(MainViewModel vm) =>
        SetButtonIntervalAndNavigate(GetMainView?.ClickArrowRight?.PolyButton, true, true, vm);

    /// <summary>
    /// Navigates to the previous image using the arrow button
    /// </summary>
    public static void PreviousArrowButtonNavigation(MainViewModel vm) =>
        SetButtonIntervalAndNavigate(GetMainView?.ClickArrowLeft?.PolyButton, false, true, vm);

    private static void SetButtonIntervalAndNavigate(RepeatButton? button, bool isNext, bool isArrow, MainViewModel vm)
    {
        if (button != null)
        {
            button.Interval = (int)TimeSpan.FromSeconds(Settings.UIProperties.NavSpeed).TotalMilliseconds;
        }

        Task.Run(() => NavigationManager.NavigateAndPositionCursor(isNext, isArrow, vm));
    }

    private static void SetButtonIntervalAndNavigate(IconButton? button, bool isNext, bool isArrow, MainViewModel vm)
    {
        if (button != null)
        {
            button.Interval = (int)TimeSpan.FromSeconds(Settings.UIProperties.NavSpeed).TotalMilliseconds;
        }

        Task.Run(() => NavigationManager.NavigateAndPositionCursor(isNext, isArrow, vm));
    }

    #endregion
}