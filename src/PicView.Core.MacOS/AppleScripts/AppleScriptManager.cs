using System.Diagnostics;
using PicView.Core.DebugTools;

namespace PicView.Core.MacOS.AppleScripts;

public static class AppleScriptManager
{
    public static async Task<bool> ExecuteAppleScriptAsync(string appleScript)
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), $"picview_script_{Guid.NewGuid():N}.scpt");
        await File.WriteAllTextAsync(scriptPath, appleScript);

        // Execute the script
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = scriptPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var output = new List<string>();
        var errors = new List<string>();

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                output.Add(e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errors.Add(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        // Clean up the script file
        try
        {
            File.Delete(scriptPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to delete temporary script: {ex.Message}");
        }

        // Check for success
        if (process.ExitCode != 0)
        {
            Debug.WriteLine($"AppleScript execution failed with code {process.ExitCode}");
            foreach (var error in errors)
            {
                Debug.WriteLine($"Error: {error}");
            }

            return false;
        }

        if (output.Count <= 0)
        {
            return process.ExitCode == 0;
        }

        // Check output for result
        var lastOutput = output.Last().Trim().ToLowerInvariant();
        return lastOutput is "true" or "1";
    }
    
    public static async Task<string?> ExecuteAppleScriptWithResultAsync(string appleScript)
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), $"picview_script_{Guid.NewGuid():N}.scpt");
        await File.WriteAllTextAsync(scriptPath, appleScript);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = scriptPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        try
        {
            File.Delete(scriptPath);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(AppleScriptManager), nameof(ExecuteAppleScriptWithResultAsync), ex);
        }

        return process.ExitCode == 0 ? output.Trim() : null;
    }
}