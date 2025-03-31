using System.Diagnostics;

namespace PicView.Core.ProcessHandling;

/// <summary>
///     Provides helper methods for process-related operations.
/// </summary>
public static class ProcessHelper
{
    /// <summary>
    ///     Gets the path to the current process.
    /// </summary>
    /// <returns>The path to the current process.</returns>
    public static string? GetPathToProcess()
    {
        var getAppPath = Environment.ProcessPath;

        if (getAppPath is not null &&
            Path.GetExtension(getAppPath).Equals(".dll", StringComparison.InvariantCultureIgnoreCase))
        {
            getAppPath = getAppPath.Replace(".dll", ".exe", StringComparison.InvariantCultureIgnoreCase);
        }

        return getAppPath;
    }

    /// <summary>
    ///     Starts the current process with elevated permissions (administrator rights).
    /// </summary>
    /// <param name="arguments">Command line arguments to pass to the elevated process.</param>
    /// <returns>
    ///     <c>true</c> if the process was successfully started with elevated permissions;
    ///     <c>false</c> if the user declined the UAC prompt or if another error occurred.
    /// </returns>
    public static bool StartProcessWithElevatedPermission(string arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule?.FileName,
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas"
            };

            process.Start();
            return true;
        }
        catch (Exception ex)
        {
            // User declined the UAC prompt or other error
            Debug.WriteLine($"Failed to start elevated process: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    ///     Restarts the current application.
    /// </summary>
    /// <param name="args">The command line arguments to pass to the restarted application.</param>
    public static void RestartApp(string? args)
    {
        var getAppPath = GetPathToProcess();

        Process.Start(new ProcessStartInfo(getAppPath, args));
    }

    /// <summary>
    ///     Starts a new instance of the current process with the provided file argument.
    /// </summary>
    /// <param name="argument">The file argument to be passed to the new instance of the current process.</param>
    public static void StartNewProcess(string argument)
    {
        var pathToExe = GetPathToProcess();

        // Sanitize file name
        var args = argument.Replace(@"\\", @"\");
        args = args.Insert(0, @"""");
        args = args.Insert(args.Length - 1, @"""");

        using Process process = new();
        process.StartInfo.FileName = pathToExe;
        process.StartInfo.Arguments = args;
        process.Start();
    }

    /// <summary>
    ///     Starts a new instance of the current process.
    /// </summary>
    public static void StartNewProcess()
    {
        var pathToExe = GetPathToProcess();

        using Process process = new();
        process.StartInfo.FileName = pathToExe;
        process.Start();
    }

    /// <summary>
    ///     Determines whether another instance of the application is already running.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if another instance of the application is already running; otherwise, <c>false</c>.
    /// </returns>
    public static bool CheckIfAnotherInstanceIsRunning()
    {
        try
        {
            // Get the current process name without the extension
            var currentProcessName = Process.GetCurrentProcess().ProcessName;

            // Check for other processes with the same name
            var processes = Process.GetProcessesByName(currentProcessName);

            // If there is more than one process, another instance is running
            return processes.Length > 1;
        }
        catch (Exception exception)
        {
#if DEBUG
            Trace.WriteLine($"{nameof(CheckIfAnotherInstanceIsRunning)}: caught exception:\n{exception.Message}");
#endif
            return false;
        }
    }

    /// <summary>
    ///     Opens a file using the Windows "Open with" dialog, allowing the user to select a program to open the file.
    /// </summary>
    /// <param name="path">The path to the file to be opened.</param>
    public static void OpenWith(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            using var process = new Process();
            process.StartInfo.FileName = "openwith";
            process.StartInfo.Arguments = $"\"{path}\"";
            process.StartInfo.ErrorDialog = true;

            process.Start();
        }
        catch (Exception e)
        {
#if DEBUG
            Console.WriteLine(e);
#endif
        }
    }

    /// <summary>
    ///     Sends a file to the default printer for printing.
    /// </summary>
    /// <param name="path">The path to the file to be printed.</param>
    public static void Print(string? path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo(path)
            {
                Verb = "print",
                UseShellExecute = true
            };
            process.Start();
        }
        catch (Exception e)
        {
#if DEBUG
            Console.WriteLine(e);
#endif
        }
    }

    /// <summary>
    ///     Opens a URL or file path in the default associated application.
    /// </summary>
    /// <param name="link">The URL or file path to open.</param>
    public static void OpenLink(string link)
    {
        if (string.IsNullOrWhiteSpace(link))
        {
            return;
        }

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(link)
        {
            UseShellExecute = true,
            Verb = "open"
        };
        process.Start();
    }
}