namespace PicView.Core.FileHistory;

public class FileHistoryEntries
{
    public bool IsSortingDescending { get; set; } = true;
    
    public List<Entry>? Entries { get; set; }
}

public class Entry
{
    public required string Path { get; set; }
    
    public bool IsPinned { get; set; }
}
