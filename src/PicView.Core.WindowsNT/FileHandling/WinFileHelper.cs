using Microsoft.VisualBasic.FileIO;

namespace PicView.Core.WindowsNT.FileHandling;

public static class WinFileHelper
{
    public static bool DeleteFile(string filePath, bool moveToRecycleBin)
    {
        var recycleOption = moveToRecycleBin ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently;
        FileSystem.DeleteFile(filePath, UIOption.AllDialogs, recycleOption);
        return File.Exists(filePath);
    }
}
