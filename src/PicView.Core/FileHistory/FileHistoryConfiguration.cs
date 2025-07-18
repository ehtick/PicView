using PicView.Core.Config.ConfigFileManagement;

namespace PicView.Core.FileHistory;

internal class FileHistoryConfiguration() : ConfigFile("FileHistory.json")
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
}
