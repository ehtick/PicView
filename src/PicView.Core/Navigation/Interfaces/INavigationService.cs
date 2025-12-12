using PicView.Core.ViewModels;

namespace PicView.Core.Navigation.Interfaces;

/// <summary>
/// Defines the contract for the high-level navigation service.
/// <para>
/// Implementations acts as a bridge ("Traffic Controller") between user commands and 
/// the active <see cref="TabViewModel"/>. It orchestrates the loading of new sources 
/// and directs the tabs to navigate.
/// </para>
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Loads a file path into the specified tab, repopulating the iterator if necessary.
    /// </summary>
    ValueTask LoadFromFileAsync(string source, TabViewModel tab, CancellationTokenSource ct);

    /// <summary>
    /// Loads a specific FileInfo into the specified tab.
    /// </summary>
    ValueTask LoadFromFileAsync(FileInfo fileInfo, TabViewModel tab, CancellationTokenSource ct);

    /// <summary>
    /// Loads content from a string source (e.g., URL or base64) into the tab.
    /// </summary>
    ValueTask LoadFromStringAsync(string source, TabViewModel tab, CancellationTokenSource ct);

    /// <summary>
    /// Commands the specified tab to navigate in a specific direction (Next, Previous, First, Last).
    /// </summary>
    ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationTokenSource ct);

    /// <summary>
    /// Commands the specified tab to jump to a specific index.
    /// </summary>
    ValueTask NavigateToIndexAsync(TabViewModel tab, int index, CancellationTokenSource ct);

    /// <summary>
    /// Determines if the specified tab currently has content that can be navigated.
    /// </summary>
    bool CanNavigate(TabViewModel tab);
}