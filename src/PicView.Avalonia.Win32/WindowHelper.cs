using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.Functions;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.Update;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Win32.PlatformUpdate;
using PicView.Avalonia.Win32.Views;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Win32;

public class WindowManager : IPlatformSpecificUpdate
{
    private ExifWindow? _exifWindow;
    private SettingsWindow? _settingsWindow;
    private  KeybindingsWindow? _keybindingsWindow;
    private AboutWindow? _aboutWindow;
    private SingleImageResizeWindow? _singleImageResizeWindow;
    private BatchResizeWindow? _batchResizeWindow;
    private EffectsWindow? _effectsWindow;
    
    public   void ShowAboutWindow(MainViewModel vm)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set);
        }
        return;

        void Set()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                return;
            }

            if (_aboutWindow is null)
            {
                _aboutWindow = new AboutWindow
                {
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                _aboutWindow.Show(desktop.MainWindow);
                _aboutWindow.Closing += (s, e) => _aboutWindow = null;
                
            }
            else
            {
                if (_aboutWindow.WindowState == WindowState.Minimized)
                {
                    WindowFunctions.ShowMinimizedWindow(_aboutWindow);
                }
                else
                {
                    _aboutWindow.Show();
                }       
            }

            _ = FunctionsMapper.CloseMenus();
        }
    }

    public   void ShowExifWindow(MainViewModel vm)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set);
        }
        return;

        void Set()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                return;
            }

            if (_exifWindow is null)
            {
                _exifWindow = new ExifWindow
                {
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                _exifWindow.Show(desktop.MainWindow);
                _exifWindow.Closing += (s, e) => _exifWindow = null;
            }
            else
            {
                if (_exifWindow.WindowState == WindowState.Minimized)
                {
                    WindowFunctions.ShowMinimizedWindow(_exifWindow);
                }
                else
                {
                    _exifWindow.Show();
                }       
            }

            _ = FunctionsMapper.CloseMenus();
        }
    }

    public   void ShowKeybindingsWindow(MainViewModel vm)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set);
        }
        return;

        void Set()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                return;
            }

            if (_keybindingsWindow is null)
            {
                _keybindingsWindow = new KeybindingsWindow
                {
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                _keybindingsWindow.Show(desktop.MainWindow);
                _keybindingsWindow.Closing += (s, e) => _keybindingsWindow = null;
            }
            else
            {
                if (_keybindingsWindow.WindowState == WindowState.Minimized)
                {
                    WindowFunctions.ShowMinimizedWindow(_keybindingsWindow);
                }
                else
                {
                    _keybindingsWindow.Show();
                }       
            }

            _ = FunctionsMapper.CloseMenus();
        }
    }

    public   void ShowSettingsWindow(MainViewModel vm)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set);
        }
        return;
        void Set()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                return;
            }
            if (_settingsWindow is null)
            {
                vm.AssociationsViewModel ??= new FileAssociationsViewModel();
                vm.SettingsViewModel ??= new SettingsViewModel();
                _settingsWindow = new SettingsWindow
                {
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                _settingsWindow.Show(desktop.MainWindow);
                _settingsWindow.Closing += (s, e) => _settingsWindow = null;
            }
            else
            {
                if (_settingsWindow.WindowState == WindowState.Minimized)
                {
                    WindowFunctions.ShowMinimizedWindow(_settingsWindow);
                }
                else
                {
                    _settingsWindow.Show();
                }         
            }
            _= FunctionsMapper.CloseMenus();
            
        }
    }

    public   void ShowSingleImageResizeWindow(MainViewModel vm)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set);
        }
        return;
        void Set()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                return;
            }
            if (_singleImageResizeWindow is null)
            {
                _singleImageResizeWindow = new SingleImageResizeWindow
                {
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                _singleImageResizeWindow.Show(desktop.MainWindow);
                _singleImageResizeWindow.Closing += (s, e) => _singleImageResizeWindow = null;
            }
            else
            {
                if (_singleImageResizeWindow.WindowState == WindowState.Minimized)
                {
                    WindowFunctions.ShowMinimizedWindow(_singleImageResizeWindow);
                }
                else
                {
                    _singleImageResizeWindow.Show();
                }         
            }
            _= FunctionsMapper.CloseMenus();
        }
    }
    
    public   void ShowBatchResizeWindow(MainViewModel vm)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set);
        }
        return;
        void Set()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                return;
            }
            if (_batchResizeWindow is null)
            {
                _batchResizeWindow = new BatchResizeWindow
                {
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                _batchResizeWindow.Show(desktop.MainWindow);
                _batchResizeWindow.Closing += (s, e) => _batchResizeWindow = null;
            }
            else
            {
                if (_batchResizeWindow.WindowState == WindowState.Minimized)
                {
                    WindowFunctions.ShowMinimizedWindow(_batchResizeWindow);
                }
                else
                {
                    _batchResizeWindow.Show();
                }
            }
            _= FunctionsMapper.CloseMenus();
        }   
    }
    
    public   void ShowEffectsWindow(MainViewModel vm)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set);
        }
        return;
        void Set()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                return;
            }
            if (_effectsWindow is null)
            {
                _effectsWindow = new EffectsWindow
                {
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,    
                };
                _effectsWindow.Show(desktop.MainWindow);
                _effectsWindow.Closing += (s, e) => _effectsWindow = null;
            }
            else
            {
                if (_effectsWindow.WindowState == WindowState.Minimized)
                {
                    WindowFunctions.ShowMinimizedWindow(_effectsWindow);
                }
                else
                {
                    _effectsWindow.Show();
                }
            }
            _= FunctionsMapper.CloseMenus();
        }
    }

    public async Task HandlePlatofrmUpdate(UpdateInfo updateInfo, string tempPath)
    {
        await WinUpdateHelper.HandleWindowsUpdate(updateInfo, tempPath);
    }
}
