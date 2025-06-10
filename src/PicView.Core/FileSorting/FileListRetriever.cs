using PicView.Core.DebugTools;
using PicView.Core.FileHandling;

namespace PicView.Core.FileSorting;

public static class FileListRetriever
{
    public static IEnumerable<string> RetrieveFiles(FileInfo fileInfo)
    {
        if (fileInfo == null)
            return new List<string>();

        // Check if the file is a directory or not
        var isDirectory = fileInfo.Attributes.HasFlag(FileAttributes.Directory);

        // Get the directory path based on whether the file is a directory or not
        var directory = isDirectory ? fileInfo.FullName : fileInfo.DirectoryName;
        if (directory is null)
            return new List<string>();

        string[] enumerable;
        // Check if the subdirectories are to be included in the search
        var recurseSubdirectories =
            Settings.Sorting.IncludeSubDirectories && string.IsNullOrWhiteSpace(TempFileHelper.TempFilePath);
        try
        {
            // Get the list of files in the directory
            IEnumerable<string> files;
            if (recurseSubdirectories)
            {
                files = Directory.EnumerateFiles(directory, "*.*", new EnumerationOptions
                {
                    AttributesToSkip = default, // Pick up hidden files
                    RecurseSubdirectories = true,
                }).AsParallel();
            }
            else
            {
                files = Directory.EnumerateFiles(directory, "*.*", new EnumerationOptions
                {
                    AttributesToSkip = default,
                    RecurseSubdirectories = false
                });
            }

            enumerable = files as string[] ?? files.ToArray();
        }
        catch (Exception exception)
        {
            DebugHelper.LogDebug(nameof(FileSortHelper), nameof(RetrieveFiles), exception);
            return new List<string>();
        }

        return enumerable.Where(IsExtensionValid);

        bool IsExtensionValid(string f)
        {
            return SupportedFiles.FileExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase);
        }
    }
}