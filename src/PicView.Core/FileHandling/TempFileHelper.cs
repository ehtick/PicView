
using PicView.Core.DebugTools;
#if DEBUG
using System.Diagnostics;
#endif

namespace PicView.Core.FileHandling;

public static class TempFileHelper
{
    /// <summary>
    /// Gets or sets the path to a temporary file or directory that is used by the application
    /// for various operations. This property is commonly set to a value in the system's
    /// temporary folder and may hold paths for temporary files or directories created during runtime.
    /// </summary>
    public static string? TempFilePath { get; set; }

    /// <summary>
    /// Creates a temporary directory in the system's temporary path and assigns its path to the TempFilePath property.
    /// The method generates a unique directory name and ensures the directory is created.
    /// Returns true if the directory is successfully created, otherwise false.
    /// </summary>
    /// <returns>A boolean value indicating whether the temporary directory was successfully created.</returns>
    public static bool CreateTempDirectory()
    {
        TempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(TempFilePath);

        return Directory.Exists(TempFilePath);
    }

    /// <summary>
    /// Deletes temporary files and directories stored in the path specified by the TempFilePath property.
    /// If the path is a file, the file and its parent directory (if any) are deleted.
    /// If the path is a directory, all files within the directory are deleted, and the directory itself is removed.
    /// In case of errors during the deletion process, exceptions are logged for debugging purposes.
    /// </summary>
    public static void DeleteTempFiles()
    {
        if (string.IsNullOrEmpty(TempFilePath))
            return;

        var isFile = false;

        if (!Directory.Exists(TempFilePath))
        {
            if (!File.Exists(TempFilePath))
            {
                return;
            }

            isFile = true;
        }

        try
        {
            if (isFile)
            {
                File.Delete(TempFilePath);
                Directory.Delete(Path.GetDirectoryName(TempFilePath));
            }
            else
            {
                Array.ForEach(Directory.GetFiles(TempFilePath), File.Delete);
            }
#if DEBUG
            Trace.WriteLine("Temp zip files deleted");
#endif

            if (!isFile)
            {
                Directory.Delete(TempFilePath);
            }
#if DEBUG
            Trace.WriteLine("Temp zip folder " + TempFilePath + " deleted");
#endif
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(TempFileHelper), nameof(DeleteTempFiles), ex);
        }
    }
}