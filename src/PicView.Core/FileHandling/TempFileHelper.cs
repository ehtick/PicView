using System.Diagnostics;
using PicView.Core.DebugTools;

namespace PicView.Core.FileHandling;

public static class TempFileHelper
{
    /// <summary>
    /// File path for the extracted folder
    /// </summary>
    public static string? TempFilePath { get; set; }

    public static bool CreateTempDirectory()
    {
        TempFilePath = Path.GetTempPath() + Path.GetRandomFileName();
        Directory.CreateDirectory(TempFilePath);

        return Directory.Exists(TempFilePath);
    }
    
    /// <summary>
    /// Deletes the temporary files when an archived file has been opened
    /// </summary>
    public static void DeleteTempFiles()
    {
        if (!Directory.Exists(TempFilePath))
        {
            return;
        }

        try
        {
            Array.ForEach(Directory.GetFiles(TempFilePath), File.Delete);
#if DEBUG
            Trace.WriteLine("Temp zip files deleted");
#endif

            Directory.Delete(TempFilePath);
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