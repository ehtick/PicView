using System.Runtime.InteropServices;

namespace PicView.Core.MacOS;

internal static partial class NativeMethods
{
    private const string CoreGraphicsLib = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    private const string LibCups = "libcups.2.dylib"; 
    
    #region macOS Native Interop SetCursorPos
    
    [LibraryImport(CoreGraphicsLib, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int CGWarpMouseCursorPosition(CGPoint newCursorPosition);
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct CGPoint
    {
        public double X;
        public double Y;
        
        public CGPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
    
    #endregion
    
    #region macOS Native Interop Print
    
    // --- Data Structures ---
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct cups_dest_t
    {
        public IntPtr name;        // char*
        public IntPtr instance;    // char*
        public int is_default;     // int (boolean)
        public int num_options;    // int
        public IntPtr options;     // cups_option_t*
    }

    // --- P/Invoke Definitions ---

    [LibraryImport(LibCups)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int cupsGetDests(out IntPtr dests);

    [LibraryImport(LibCups)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void cupsFreeDests(int num_dests, IntPtr dests);

    [LibraryImport(LibCups, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int cupsPrintFile(string name, string filename, string title, int num_options, IntPtr options);
    
    #endregion
}