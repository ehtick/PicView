using Microsoft.IO;
using PicView.Core.DebugTools;

namespace PicView.Core.FileHandling;

/// <summary>
/// Provides utility methods for working with file streams in an optimized manner.
/// </summary>
public static class FileStreamUtils
{
    /// <summary>
    /// Represents a private static instance of <see cref="Microsoft.IO.RecyclableMemoryStreamManager"/>,
    /// used to manage memory-efficient stream allocation and recycling within the FileStreamUtils class.
    /// This ensures optimal memory usage and reduces pressure on the garbage collector,
    /// especially when handling large or frequent streams.
    /// </summary>
    private static readonly RecyclableMemoryStreamManager Manager = new();
    
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


    /// <summary>
    /// Reads the content of a specified file asynchronously into a recyclable <see cref="MemoryStream" />.
    /// This method uses a shared recyclable memory stream manager to efficiently manage memory usage
    /// and reduce the performance overhead of frequent memory allocations.
    /// </summary>
    /// <param name="fileInfo">The <see cref="FileInfo" /> object representing the file to be read.</param>
    /// <returns>
    /// A <see cref="MemoryStream" /> containing the contents of the file. The caller is responsible
    /// for disposing the returned stream to release the resources back to the recyclable memory stream pool.
    /// </returns>
    /// <remarks>
    /// - This method uses an optimized <see cref="FileStream" /> to read the file asynchronously to improve I/O performance.
    /// - In case of an exception during the operation, the allocated <see cref="MemoryStream" /> is disposed,
    /// and the exception is re-thrown.
    /// - The position of the returned stream is set to the beginning so that it can be read directly by the caller.
    /// </remarks>
    public static async Task<MemoryStream> ReadFileToRecyclableStreamAsync(FileInfo fileInfo)
    {
        // Get a stream from the pool, providing the file name as a tag for diagnostics 
        // and the initial capacity for performance.
        var recyclableMemoryStream = Manager.GetStream(fileInfo.Name, fileInfo.Length);
        
        try
        {
            // Use FileStream with asynchronous options for best performance.
            await using var fileStream = GetOptimizedFileStream(fileInfo);

            // Asynchronously copy the file data into the recyclable stream.
            // The recyclable stream will automatically expand by renting buffers from the pool.
            await fileStream.CopyToAsync(recyclableMemoryStream);

            // Reset the stream's position to the beginning so the caller can read it.
            recyclableMemoryStream.Position = 0;
            return recyclableMemoryStream;
        }
        catch (Exception exception)
        {
            DebugHelper.LogDebug(nameof(FileStreamUtils), nameof(ReadFileToRecyclableStreamAsync), exception);
            
            // If an exception occurs, we must dispose of the stream to return its buffers to the pool.
            await recyclableMemoryStream.DisposeAsync();
            throw;
        }
    }
}