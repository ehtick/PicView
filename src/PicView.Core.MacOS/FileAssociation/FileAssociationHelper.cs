using System.Diagnostics;

namespace PicView.Core.MacOS.FileAssociation;

public static class FileAssociationHelper
{
    public static async Task<bool> RegisterFileAssociation(string extension, string contentType)
    {
        try
        {
            // Remove leading period if present
            if (extension.StartsWith('.'))
            {
                extension = extension[1..];
            }

            // Use duti to set default application for this extension
            const string bundleId = "com.ruben2776.picview";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "duti",
                    Arguments = $"-s {bundleId} .{extension} all",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"Error registering file association on macOS: {ex.Message}");
#endif
            return false;
        }
    }

    public static async Task<bool> UnregisterFileAssociationMacOS(string extension)
    {
        try
        {
            // For macOS, there isn't a direct way to "unregister" a file association
            // Instead, you'd typically set another app as the default handler
            // Here we use lsregister to reset the association to default

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName =
                        "/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister",
                    Arguments = "-u -all",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"Error unregistering file association on macOS: {ex.Message}");
#endif
            return false;
        }
    }
}