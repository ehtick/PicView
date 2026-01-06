using PicView.Core.DebugTools;
using PicView.Core.FileHandling.Interfaces;

namespace PicView.Core.FileHandling;

public class TempFileService : ITempFileService
{
    private readonly List<string> _tempFiles = [];
    private readonly Lock _lock = new();

    public string GetNewTempFilePath(string fileName)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempPath);
        var filePath = Path.Combine(tempPath, fileName);

        lock (_lock)
        {
            _tempFiles.Add(filePath);
        }

        return filePath;
    }

    public void Cleanup()
    {
        lock (_lock)
        {
            foreach (var file in _tempFiles)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                    
                    var dir = Path.GetDirectoryName(file);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                }
                catch (Exception e)
                {
                    DebugHelper.LogDebug(nameof(TempFileService), nameof(Cleanup), e);
                }
            }
            _tempFiles.Clear();
        }
    }
}