namespace PicView.Core.FileHandling;

/// <summary>
/// Provides utility methods for working with file streams in an optimized manner.
/// </summary>
public static class FileStreamUtils
{
    
    /// <summary>
    ///     Opens a <see cref="FileStream" /> for the given <see cref="FileInfo" /> with optimized settings
    ///     for reading or writing based on the file size. The buffer size and file options are adjusted
    ///     to improve performance for different file sizes.
    /// </summary>
    /// <param name="fileInfo">The <see cref="FileInfo" /> object representing the file to be opened.</param>
    /// <param name="writeAccess">Specifies whether to open the file with write access. Defaults to <c>false</c>.</param>
    /// <returns>
    ///     A <see cref="FileStream" /> object configured for optimal file reading or writing,
    ///     with different settings based on the file size.
    /// </returns>
    /// <remarks>
    ///     - For files smaller than 1 MB, a buffer size of 4 KB is used, with asynchronous file access enabled.
    ///     - For files between 1 MB and 100 MB, a buffer size of 16 KB is used, with asynchronous file access enabled.
    ///     - For files larger than 100 MB, a buffer size of 16 KB is used, with <see cref="FileOptions.SequentialScan" />
    ///     enabled to optimize for large, sequential file reads.
    /// </remarks>
    public static FileStream GetOptimizedFileStream(FileInfo fileInfo, bool writeAccess = false)
    {
        // Define thresholds for file size and buffer sizes
        var fileSize = fileInfo.Length;
        FileOptions options;
        int bufferSize;

        switch (fileSize)
        {
            case <= 1048576L: // Less than 1 MB
                bufferSize = 4096;
                options = FileOptions.Asynchronous;
                break;
            case <= 104857600L: // Less than 100 MB
                bufferSize = 16384;
                options = FileOptions.Asynchronous;
                break;
            default: // Files larger than 100 MB
                bufferSize = 16384;
                options = FileOptions.SequentialScan;
                break;
        }

        // Open a FileStream with the selected buffer size and options
        return new FileStream(
            fileInfo.FullName,
            writeAccess ? FileMode.OpenOrCreate : FileMode.Open,
            writeAccess ? FileAccess.ReadWrite : FileAccess.Read,
            FileShare.ReadWrite,
            bufferSize,
            options
        );
    }
}