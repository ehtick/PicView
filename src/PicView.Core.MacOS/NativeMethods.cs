using System.Runtime.InteropServices;
using PicView.Core.DebugTools;

namespace PicView.Core.MacOS;

public static partial class NativeMethods
{
    private const string CoreGraphicsLib = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    
    #region macOS Native Interop SetCursorPos
    
    [LibraryImport(CoreGraphicsLib, StringMarshalling = StringMarshalling.Utf16)]
    private static partial int CGWarpMouseCursorPosition(CGPoint newCursorPosition);
    
    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;
        public double Y;
        
        public CGPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public static int SetCursorPos(double x, double y)
    {
        var point = new CGPoint(x, y);
        var result = CGWarpMouseCursorPosition(point);
        
        if (result != 0)
        {
            DebugHelper.LogDebug(nameof(NativeMethods), nameof(SetCursorPos), $"Failed to set cursor position: error code {result}");
        }
        return result;
    }
    
    #endregion
}