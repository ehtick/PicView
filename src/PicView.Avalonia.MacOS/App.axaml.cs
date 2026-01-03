using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.MacOS.Views;
using PicView.Avalonia.StartUp;
using PicView.Core.FileAssociations;
using PicView.Core.FileSorting;
using PicView.Core.IPlatform;
using PicView.Core.MacOS;
using PicView.Core.MacOS.Cursor;
using PicView.Core.MacOS.FileAssociation;
using PicView.Core.MacOS.FileFunctions;
using PicView.Core.MacOS.Wallpaper;
using PicView.Core.ProcessHandling;
using PicView.Core.ViewModels;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

#pragma warning disable CS0618 // Type or member is obsolete

namespace PicView.Avalonia.MacOS;

public class App : Application, IPlatformSpecificService
{
    private MacMainWindow2? _mainWindow;
    private static MainWindowViewModel? _mainWindowViewModel;
    private static CoreViewModel? _coreViewModel;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            base.OnFrameworkInitializationCompleted();
            
            string? startUpFilePath = null;
            EventHandler<UrlOpenedEventArgs> handler = (_, e) => { startUpFilePath = e.Urls[0]; };
            Current.UrlsOpened += handler;

            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                return;
            }

            var settingsExists = LoadSettings();
            _coreViewModel = new CoreViewModel(this, GetImageModel.GetImageModelAsync);
            DataContext = _coreViewModel;

            ThemeManager.DetermineTheme(Current, settingsExists);
            
            _mainWindow = new MacMainWindow2();
            _mainWindowViewModel = _mainWindow.DataContext as MainWindowViewModel;
            _coreViewModel.MainWindows.ActiveWindow.Value = _mainWindowViewModel;
            if (string.IsNullOrWhiteSpace(startUpFilePath))
            {
                StartUpHelper2.StartWithArguments(_coreViewModel, settingsExists, desktop, _mainWindow);
            }
            else
            {
                StartUpHelper2.StartUpBlank(_coreViewModel, settingsExists,  true, desktop, _mainWindow);
            }
            _coreViewModel.MainWindows.MainWindows.Add(_mainWindowViewModel);
            _coreViewModel.MainWindows.ActiveWindow.Value = _mainWindowViewModel;
            
            // Register for macOS file opening
            Current.UrlsOpened += async (_, e) =>
            {
                if (Settings.UIProperties.OpenInSameWindow)
                {
                    Dispatcher.UIThread.Invoke(() => 
                    {
                        _mainWindow.Activate();
                    }, DispatcherPriority.Send);
                    await _mainWindowViewModel.WindowTabs.LoadFromStringAsync(e.Urls[0]);
                }
                else
                {
                    ProcessHelper.StartNewProcess(e.Urls[0]);
                }
            };
            Current.UrlsOpened -= handler;
        }
        catch (Exception)
        {
            //
        }
    }

    #region Interface implementations
    
    public void SetTaskbarProgress(ulong progress, ulong maximum)
    {
        // TODO: Implement SetTaskbarProgress
        // https://github.com/carina-studio/AppSuiteBase/blob/master/Core/AppSuiteApplication.MacOS.cs#L365
    }

    public void StopTaskbarProgress()
    {
        
    }

    public void SetCursorPos(int x, int y)
    {
        MacOSCursor.SetCursorPos(x, y);
    }

    public List<FileInfo> GetFiles(FileInfo fileInfo)
    {
        return FileListRetriever.RetrieveFiles(fileInfo, CompareStrings);
    }

    public int CompareStrings(string str1, string str2)
    {
        return string.CompareOrdinal(str1, str2);
    }

    public void OpenWith(string path)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var openWithView = new OpenWithView(path)
            {
                DataContext = null // TODO: fix
            };
            openWithView.Show();
        }, DispatcherPriority.Input);
        
    }

    public void LocateOnDisk(string path)
    {
        Process.Start("open", $"-R \"{path}\"");
    }
    
    public void ShowFileProperties(string path)
    {
        _ = FileProperties.ShowFilePropertiesAsync(path);
        // TODO: make interface async
    }

    public void Print(string path)
    {
        //_windowInitializer?.ShowPrintPreviewWindow(null, path);
    }

    public async Task SetAsWallpaper(string path, int wallpaperStyle)
    {
         await WallpaperHelper.SetWallpaper(path);
    }

    public bool SetAsLockScreen(string path)
    {
        // wallpaper and lockscreen are the same in macOS
        return false;
    }
    
    public bool CopyFile(string path)
    {
        // TODO: Implement copying file to clipboard
        return false;
    }
    
    public bool CutFile(string path)
    {
        // TODO: Implement cutting file to clipboard
        return false;
    }

    public Task CopyImageToClipboard(object image)
    {
        return Task.CompletedTask;
    }

    public Task<object?> GetImageFromClipboard()
    {
        return null;
    }

    public Task<bool> ExtractWithLocalSoftwareAsync(string path, string tempDirectory)
    {
        // TODO: Implement ExtractWithLocalSoftwareAsync
        return Task.FromResult(false);
    }
    
    public void DisableScreensaver()
    {
        // TODO: Implement DisableScreensaver
    }
    
    public void EnableScreensaver()
    {
        // TODO: Implement EnableScreensaver
    }

    public string DefaultJsonKeyMap()
    {
        return MacOsKeybindings.DefaultKeybindings;
    }

    public void InitiateFileAssociationService()
    {
        var iIFileAssociationService = new MacFileAssociationService();
        FileAssociationManager.Initialize(iIFileAssociationService);
    }

    public async Task<bool> DeleteFile(string path, bool recycle)
    {
        if (recycle)
        {
            return await Task.Run(() => OsxFileHelper.MoveFileToRecycleBinAsync(path));
        }
        await Task.Run(() => File.Delete(path));
        return !File.Exists(path); 
    }
    
    #endregion
}