using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.MacOS.Views;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileAssociations;
using PicView.Core.FileSorting;
using PicView.Core.IPlatform;
using PicView.Core.Localization;
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
    private static CoreViewModel? _coreViewModel;
    private static MainWindowViewModel? _mainWindowViewModel;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    // The startup procedure for macOS is a bit different than Windows.
    public override async void OnFrameworkInitializationCompleted()
    {
        try
        {
            base.OnFrameworkInitializationCompleted();

            // Capture the startup file immediately if the event fires early
            string? startUpFilePath = null;

            // We use a flag to track if THIS instance has handled its "startup" file yet.
            var hasHandledInitialFile = false;

            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                return;
            }

            // Capture the event early
            EventHandler<UrlOpenedEventArgs> earlyHandler = (_, e) => { startUpFilePath = e.Urls[0]; };
            Current.UrlsOpened += earlyHandler;

            var settingsExists = await Task.FromResult(LoadSettings()).ConfigureAwait(false);
            TranslationManager.Init();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _coreViewModel = new CoreViewModel(this, GetImageModel.GetImageModelAsync);
                DataContext = _coreViewModel;
                ThemeManager.DetermineTheme(Current, settingsExists);

                _mainWindow = new MacMainWindow2();
                _mainWindowViewModel = _mainWindow.DataContext as MainWindowViewModel;
                _coreViewModel.MainWindows.MainWindows.Add(_mainWindowViewModel);
                _coreViewModel.MainWindows.ActiveWindow.Value = _mainWindowViewModel;
                
                // If the event fired BEFORE we got here, start with that file.
                if (startUpFilePath is not null)
                {
                    StartUpHelper2.StartWithArguments(_coreViewModel, settingsExists, desktop, _mainWindow);
                    hasHandledInitialFile = true;

                    if (Settings.WindowProperties.AutoFit)
                    {
                        WindowFunctions.CenterWindowOnScreen();
                    }
                }
                else
                {
                    // If no file yet, start normally (Last File / StartUpMenu)
                    StartUpHelper2.RegularStartUp(_coreViewModel, settingsExists, desktop, _mainWindow);
                }
            }, DispatcherPriority.Send);

            // 4. Remove the temporary early handler
            Current.UrlsOpened -= earlyHandler;

            // 5. Register the PERMANENT handler with Logic to prevent double-opening
            Current.UrlsOpened += (_, e) =>
            {
                var incomingUrl = e.Urls[0];

                // SCENARIO A: Startup Race Condition Fix
                // If we haven't handled a startup file yet (meaning StartWithoutArguments ran),
                // AND this event comes in shortly after launch, this IS our startup file.
                // We must open it in THIS window, ignoring the "OpenInSameWindow=false" setting.
                if (!hasHandledInitialFile)
                {
                    hasHandledInitialFile = true;
                    startUpFilePath = incomingUrl; // Mark this as our startup file

                    // Force switch to ImageViewer (in case we were sitting on the Start Menu)
                    //_vm.ImageViewer ??= new ImageViewer();
                    //_vm.MainWindow.CurrentView.Value = _vm.ImageViewer;

                    //await NavigationManager.LoadPicFromStringAsync(incomingUrl, _vm).ConfigureAwait(false);
                    return;
                }

                // SCENARIO B: Duplicate Event Fix
                // macOS sometimes fires the event again for the file we just opened.
                // If the incoming URL is the exact same one we started with, ignore it.
                if (incomingUrl == startUpFilePath)
                {
                    return;
                }

                // SCENARIO C: Actual Drag & Drop / Next File
                if (Settings.UIProperties.OpenInSameWindow)
                {
                    Dispatcher.UIThread.Invoke(() => { _mainWindow.Activate(); }, DispatcherPriority.Send);
                    //await NavigationManager.LoadPicFromStringAsync(incomingUrl, _vm).ConfigureAwait(false);
                }
                else
                {
                    ProcessHelper.StartNewProcess(incomingUrl);
                }
            };
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