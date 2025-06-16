using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using ZLinq;

namespace PicView.Core.FileSorting;

public static class FileListRetriever
{
    public static IEnumerable<FileInfo> RetrieveFiles(FileInfo fileInfo)
    {
        var directoryPath = fileInfo switch
        {
            null => null,
            { Attributes: var attr } when attr.HasFlag(FileAttributes.Directory) => fileInfo.FullName,
            _ => fileInfo.DirectoryName
        };

        if (string.IsNullOrEmpty(directoryPath))
        {
            return [];
        }

        var recurseSubdirectories =
            Settings.Sorting.IncludeSubDirectories && string.IsNullOrWhiteSpace(TempFileHelper.TempFilePath);

        try
        {
            if (recurseSubdirectories)
            {
                return new DirectoryInfo(directoryPath)
                    .DescendantsAndSelf()
                    .OfType<FileInfo>()
                    .Where(x => x.Extension.IsSupported()).ToList();
            }
            return new DirectoryInfo(directoryPath)
                .ChildrenAndSelf()
                .OfType<FileInfo>()
                .Where(x => x.Extension.IsSupported()).ToList();
        }
        catch (Exception exception)
        {
            DebugHelper.LogDebug(nameof(FileListRetriever), nameof(RetrieveFiles), exception);
            return [];
        }
    }
}