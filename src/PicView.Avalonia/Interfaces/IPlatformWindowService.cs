namespace PicView.Avalonia.Interfaces;

public interface IPlatformWindowService
{
    /// <summary>
    /// Gets or sets the padding used to calculate space between the screen and the window.
    /// </summary>
    double Padding { get; }

    /// <summary>
    /// Gets or sets the width of the buttons on the respective title bar implementation.
    /// </summary>
    int CombinedTitleButtonsWidth { get; set; }
    
    /// <summary>
    /// Platform specific maximize implementation.
    /// </summary>
    Task Maximize(bool saveSetting = true);
    
    /// <summary>
    /// Platform specific toggle between maximize and restore implementation.
    /// </summary>
    Task MaximizeRestore(bool saveSettings = true);
    
    /// <summary>
    /// Platform specific fullscreen implementation.
    /// </summary>
    Task Fullscreen(bool saveSetting = true);
    
    /// <summary>
    /// Platform specific toggle between fullscreen and restore implementation.
    /// </summary>
    Task ToggleFullscreen(bool saveSettings = true);
    
    /// <summary>
    /// Platform specific restore implementation.
    /// </summary>
    Task Restore();
    
    void ShowAboutWindow();

    void ShowExifWindow();

    void ShowKeybindingsWindow();

    void ShowSettingsWindow();
    
    void ShowEffectsWindow();
    
    void ShowSingleImageResizeWindow();
    
    void ShowBatchResizeWindow();
}
