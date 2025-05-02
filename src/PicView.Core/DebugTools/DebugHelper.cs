using System.Diagnostics;

namespace PicView.Core.DebugTools;

public static class DebugHelper
{
    public static void LogDebug(string className, string methodName, Exception exception)
    {
        Debug.WriteLine($"{className}:{methodName} exception {exception.Message}");
        Debug.WriteLine(Environment.NewLine + exception.StackTrace);
    }
}
