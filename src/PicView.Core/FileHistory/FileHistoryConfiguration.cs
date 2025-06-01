using PicView.Core.Config;

namespace PicView.Core.FileHistory;

internal static class FileHistoryConfiguration
{
    /// <summary>
    /// Represents the maximum number of unpinned entries allowed in the file history.
    /// If this limit is exceeded, the oldest unpinned entry is removed to maintain consistency.
    /// </summary>
    internal const int MaxHistoryEntries = 50;

    /// <summary>
    /// Specifies the maximum number of pinned entries allowed in the file history.
    /// This limit ensures that pinned entries do not exceed this value, maintaining balance
    /// with unpinned entries in the file history configuration.
    /// </summary>
    internal const int MaxPinnedEntries = 5;
    
    internal const string HistoryFileName = "FileHistory.json";
    internal static string HistoryFilePath => Path.Combine(SettingsConfiguration.ConfigFolder, HistoryFileName);
    internal static string RoamingFileHistoryPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            HistoryFilePath);
    
    internal static string LocalFileHistoryPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", HistoryFileName);
    internal static string CurrentUserFileHistoryPath =>
        File.Exists(RoamingFileHistoryPath) ? RoamingFileHistoryPath :
        File.Exists(LocalFileHistoryPath) ? LocalFileHistoryPath : string.Empty;
}
