using System.Text.Json;
using System.Text.Json.Serialization;
using PicView.Core.ArchiveHandling;
using PicView.Core.Config;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;

namespace PicView.Core.FileHistory;

/// <summary>
///     JSON context for file history serialization.
/// </summary>
[JsonSourceGenerationOptions(AllowTrailingCommas = true, WriteIndented = true)]
[JsonSerializable(typeof(FileHistoryEntries))]
[JsonSerializable(typeof(List<Entry>))]
internal partial class FileHistoryGenerationContext : JsonSerializerContext;

/// <summary>
///     Manages the history of recently accessed files.
/// </summary>
public static class FileHistoryManager
{
    private static readonly List<Entry> Entries = [];
    private static string? _fileLocation;

    /// <summary>
    ///     Gets the number of entries in the file history.
    /// </summary>
    public static int Count => Entries.Count;

    /// <summary>
    ///     Gets or sets whether the history is sorted in descending order.
    /// </summary>
    public static bool IsSortingDescending { get; set; }

    /// <summary>
    ///     Gets all history entries.
    /// </summary>
    public static IReadOnlyList<Entry> AllEntries => Entries.AsReadOnly();

    /// <summary>
    /// Gets or sets the index of the current file entry in the history.
    /// The setter clamps the value between -1 and the total count of entries minus one.
    /// A value of -1 indicates no valid current entry.
    /// </summary>
    public static int CurrentIndex
    {
        get;
        private set => field = Math.Clamp(value, -1, Count - 1);
    } = -1;

    /// <summary>
    ///     Indicates whether there is a previous entry available in history (older entry).
    /// </summary>
    public static bool HasPrevious => CurrentIndex > 0;

    /// <summary>
    ///     Indicates whether there is a next entry available in history (newer entry).
    /// </summary>
    public static bool HasNext => CurrentIndex < Count - 1 && Count > 0;

    /// <summary>
    ///     Gets the current entry at the current index.
    /// </summary>
    public static string? CurrentEntry =>
        CurrentIndex >= 0 && CurrentIndex < Count ? Entries[CurrentIndex].Path : null;

    /// <summary>
    ///     Gets the file history file path with normalized slashes.
    /// </summary>
    public static string CurrentFileHistoryFile => _fileLocation?.Replace("/", "\\") ?? string.Empty;

    /// <summary>
    ///     Initializes the file history by loading entries from the history file.
    /// </summary>
    public static async Task InitializeAsync()
    {
        _fileLocation = ConfigFileManager.TryGetConfigFilePath(ConfigFileType.FileHistory);
        await LoadFromFileAsync().ConfigureAwait(false);

        // Set the current index to the most recent entry.
        CurrentIndex = Count > 0 ? Count - 1 : -1;
    }

    /// <summary>
    ///     Pins a file entry in history.
    /// </summary>
    public static void Pin(string path) =>
        SetPinnedState(path, true);

    /// <summary>
    ///     Unpins a file entry in history.
    /// </summary>
    public static void UnPin(string path) =>
        SetPinnedState(path, false);

    private static void SetPinnedState(string path, bool isPinned)
    {
        var entryIndex = Entries.FindIndex(x => x.Path == path);
        if (entryIndex < 0 || Entries[entryIndex].IsPinned == isPinned)
        {
            return;
        }

        if (isPinned && Entries.Count(e => e.IsPinned) >= FileHistoryConfiguration.MaxPinnedEntries)
        {
            // Unpin the oldest pinned entry to make room
            var oldestPinned = Entries.Where(e => e.IsPinned).OrderBy(e => Entries.IndexOf(e)).FirstOrDefault();
            if (oldestPinned != null)
            {
                var oldestIndex = Entries.IndexOf(oldestPinned);
                Entries[oldestIndex].IsPinned = false;
            }
        }

        Entries[entryIndex].IsPinned = isPinned;
    }

    /// <summary>
    ///     Adds an entry to the history.
    /// </summary>
    public static void Add(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        // Don't add if browsing an archive, unless the file is an archive itself.
        if (!string.IsNullOrWhiteSpace(ArchiveExtraction.TempZipDirectory) && !path.IsArchive())
        {
            return;
        }

        var existingIndex = Entries.FindIndex(x => x.Path == path);

        if (existingIndex >= 0)
        {
            // If entry already exists, update the current index to point to it.
            CurrentIndex = existingIndex;
            return;
        }

        // Count unpinned entries
        var unpinnedCount = Entries.Count(e => !e.IsPinned);

        // If we'll exceed the maximum unpinned entries, remove the oldest unpinned entry
        if (unpinnedCount >= FileHistoryConfiguration.MaxHistoryEntries)
        {
            for (var i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].IsPinned)
                {
                    continue;
                }

                Entries.RemoveAt(i);

                // Adjust the current index since we removed an item.
                if (CurrentIndex > i)
                {
                    CurrentIndex--;
                }

                break;
            }
        }

        Entries.Add(new Entry { Path = path });
        CurrentIndex = Entries.Count - 1;
    }

    /// <summary>
    ///     Gets the next entry in history (newer entry).
    /// </summary>
    public static string? GetNextEntry()
    {
        if (!HasNext)
        {
            return null;
        }

        CurrentIndex++;
        return CurrentEntry;
    }

    /// <summary>
    ///     Gets the previous entry in history (older entry).
    /// </summary>
    public static string? GetPreviousEntry()
    {
        if (!HasPrevious)
        {
            return null;
        }

        CurrentIndex--;
        return CurrentEntry;
    }

    /// <summary>
    ///     Gets an entry at the specified index.
    /// </summary>
    public static Entry? GetEntry(int index)
    {
        if (index < 0 || index >= Entries.Count)
        {
            return null;
        }

        return Entries[index];
    }

    /// <summary>
    ///     Gets the first entry in history (oldest).
    /// </summary>
    public static string? GetFirstEntry() =>
        Entries.Count > 0 ? Entries[0].Path : null;

    /// <summary>
    ///     Gets the last entry in history (newest).
    /// </summary>
    public static string? GetLastEntry() =>
        Entries.Count > 0 ? Entries[^1].Path : null;

    /// <summary>
    ///     Tries to find an entry that matches or contains the given string.
    /// </summary>
    public static Entry? GetEntryByString(string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
        {
            return null;
        }

        // First try exact match.
        var exactMatch = Entries.FirstOrDefault(e =>
            string.Equals(e.Path, searchString, StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null)
        {
            return exactMatch;
        }

        // Then try contains.
        return Entries.Find(e => e.Path.Contains(searchString, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Clears all history entries.
    /// </summary>
    public static void Clear()
    {
        Entries.Clear();
        CurrentIndex = -1;
    }

    /// <summary>
    ///     Removes a specific entry from history.
    /// </summary>
    public static bool Remove(string path)
    {
        var index = Entries.FindIndex(e => e.Path == path);
        if (index < 0)
        {
            return false;
        }

        Entries.RemoveAt(index);

        // Adjust current index if necessary.
        if (index <= CurrentIndex)
        {
            CurrentIndex = Math.Max(-1, CurrentIndex - 1);
        }

        return true;
    }

    /// <summary>
    ///     Renames a file in the history, replacing the old entry with the new one.
    /// </summary>
    public static void Rename(string oldName, string newName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            var entry = GetEntryByString(oldName);
            if (string.IsNullOrWhiteSpace(entry?.Path) || Entries.All(x => x.Path != entry?.Path))
            {
                return;
            }

            var index = Entries.FindIndex(x => x.Path == entry.Path);
            if (index >= 0)
            {
                Entries[index].Path = newName;
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(FileHistoryManager), nameof(Rename), e);
        }
    }

    /// <summary>
    ///     Saves the history to the history file.
    /// </summary>
    public static async Task SaveToFileAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_fileLocation))
            {
                _fileLocation = ConfigFileManager.TryGetConfigFilePath(ConfigFileType.FileHistory);
            }

            // Create a new sorted list with pinned entries first (max 5), then unpinned entries (max MaxHistoryEntries)
            var sortedEntries = new List<Entry>();

            // Add pinned entries (max 5)
            sortedEntries.AddRange(Entries.Where(e => e.IsPinned).Take(FileHistoryConfiguration.MaxPinnedEntries));
            // Add unpinned entries (max MaxHistoryEntries)
            sortedEntries.AddRange(Entries.Where(e => !e.IsPinned)
                .Take(FileHistoryConfiguration.MaxHistoryEntries));

            var historyEntries = new FileHistoryEntries
            {
                Entries = sortedEntries,
                IsSortingDescending = IsSortingDescending
            };
            _fileLocation = await ConfigFileManager.SaveConfigFileAndReturnPathAsync(ConfigFileType.FileHistory,
                _fileLocation, historyEntries, typeof(FileHistoryEntries), FileHistoryGenerationContext.Default);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(FileHistoryManager), nameof(SaveToFileAsync), ex);
        }
    }

    /// <summary>
    ///     Loads the history from the history file.
    /// </summary>
    private static async Task LoadFromFileAsync()
    {
        try
        {
            var jsonString = await File.ReadAllTextAsync(_fileLocation).ConfigureAwait(false);

            if (JsonSerializer.Deserialize(jsonString, typeof(FileHistoryEntries),
                    FileHistoryGenerationContext.Default)
                is not FileHistoryEntries entries)
            {
                throw new JsonException("Failed to deserialize settings");
            }

            IsSortingDescending = entries.IsSortingDescending;
            Entries.Clear();
            foreach (var entry in entries.Entries)
            {
                Entries.Add(entry);
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(FileHistoryManager), nameof(LoadFromFileAsync), ex);
        }
    }
}