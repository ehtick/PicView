using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.Functions;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.Linux.PlatformUpdate;
using PicView.Avalonia.Linux.Printing;
using PicView.Avalonia.Linux.Views;
using PicView.Avalonia.Services;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Config;
using PicView.Core.Update;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Linux.WindowImpl;

public class WindowInitializer : IPlatformSpecificUpdate
{
    private AboutWindow? _aboutWindow;
    private BatchResizeWindow? _batchResizeWindow;
    private ConvertWindow? _convertWindow;
    private EffectsWindow? _effectsWindow;
    private ImageInfoWindow? _imageInfoWindow;
    private KeybindingsWindow? _keybindingsWindow;
    private PrintPreviewWindow? _printPreviewWindow;
    private SettingsWindow? _settingsWindow;
    private SingleImageResizeWindow? _singleImageResizeWindow;


    public async Task HandlePlatofrmUpdate(UpdateInfo updateInfo, string tempPath)
    {
        await LinuxUpdateHelper.HandleLinuxUpdate(updateInfo, tempPath);
    }

    public void ShowAboutWindow()
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
            if (_aboutWindow is null)
            {
                _aboutWindow = new AboutWindow
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                _aboutWindow.Show();
                _aboutWindow.Closing += (_, _) => _aboutWindow = null;
            }
            else
            {
                if (_aboutWindow.WindowState == WindowState.Minimized)
                {
                    WindowFunctions.ShowMinimizedWindow(_aboutWindow);
                }
                else
                {
                    _aboutWindow.Activate();
                }
            }

            _ = FunctionsMapper.CloseMenus();
        }
    }

    public async Task ShowImageInfoWindow()
    {
        // if (_imageInfoWindow is null)
        // {
        //     if (vm.Window.ImageInfoWindowConfig?.WindowProperties is null)
        //     {
        //         vm.Window.ImageInfoWindowConfig = new ImageInfoWindowConfig();
        //         await vm.Window.ImageInfoWindowConfig.LoadAsync();
        //     }
        //
        //     await Dispatcher.UIThread.InvokeAsync(() =>
        //     {
        //         vm.Exif ??= new ExifViewModel();
        //         vm.InfoWindow = new ImageInfoWindowViewModel();
        //         _imageInfoWindow = new ImageInfoWindow(vm.Window.ImageInfoWindowConfig)
        //         {
        //             DataContext = vm
        //         };
        //         WindowFunctions.InitializeWindowSizeAndPosition(_imageInfoWindow,
        //             vm.Window.ImageInfoWindowConfig.WindowProperties);
        //         _imageInfoWindow.Show();
        //         _imageInfoWindow.Closing += (_, _) =>
        //         {
        //             _imageInfoWindow = null;
        //             vm.Exif.Dispose();
        //             vm.Exif = null;
        //             vm.InfoWindow.Dispose();
        //             vm.InfoWindow = null;
        //         };
        //     });
        // }
        // else
        // {
        //     await Dispatcher.UIThread.InvokeAsync(() =>
        //     {
        //         if (_imageInfoWindow.WindowState == WindowState.Minimized)
        //         {
        //             WindowFunctions.ShowMinimizedWindow(_imageInfoWindow);
        //         }
        //         else
        //         {
        //             _imageInfoWindow.Activate();
        //         }
        //     });
        // }
        //
        // await FunctionsMapper.CloseMenus();
    }

    public async Task ShowKeybindingsWindow()
    {
        // if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        // {
        //     return;
        // }
        //
        // if (_keybindingsWindow is null)
        // {
        //     if (vm.Window.KeybindingWindowConfig?.WindowProperties is null)
        //     {
        //         vm.Window.KeybindingWindowConfig = new KeybindingWindowConfig();
        //         await vm.Window.KeybindingWindowConfig.LoadAsync();
        //     }
        //
        //     if (vm.Keybindings is null)
        //     {
        //         vm.Keybindings = new KeybindingsViewModel();
        //         vm.Keybindings.ResetKeybindingsCommand = new ReactiveCommand(async (_, _) =>
        //         {
        //             _keybindingsWindow.Close();
        //             await Task.Run(() =>
        //             {
        //                 KeybindingManager.SetDefaultKeybindings(vm.PlatformService);
        //                 FunctionsKeyHelper.ResetKeybindings(vm.Keybindings);
        //             }, CancellationToken.None);
        //             await ShowKeybindingsWindow(vm);
        //         });
        //     }
        //     
        //     _ = Task.Run(() =>
        //     {
        //         FunctionsKeyHelper.LoadKeybindingsViewModel(vm.Keybindings);
        //     });
        //     
        //     await Dispatcher.UIThread.InvokeAsync(() =>
        //     {
        //         _keybindingsWindow = new KeybindingsWindow(vm.Window.KeybindingWindowConfig)
        //         {
        //             DataContext = vm
        //         };
        //         Show();
        //         _keybindingsWindow.Closing += (_, _) =>
        //         {
        //             _keybindingsWindow?.Dispose();
        //             _keybindingsWindow = null;
        //         };
        //     });
        // }
        // else
        // {
        //     await Dispatcher.UIThread.InvokeAsync(() =>
        //     {
        //         if (_keybindingsWindow.WindowState == WindowState.Minimized)
        //         {
        //             WindowFunctions.ShowMinimizedWindow(_keybindingsWindow);
        //         }
        //         else
        //         {
        //             Show();
        //         }
        //     });
        // }
        //
        // await FunctionsMapper.CloseMenus();
        //
        // return;
        //
        // void Show()
        // {
        //     WindowFunctions.InitializeWindowSizeAndPosition(_keybindingsWindow,
        //         vm.Window.KeybindingWindowConfig.WindowProperties);
        //     _keybindingsWindow.Show(desktop.MainWindow);
        // }
    }

    public async ValueTask ShowSettingsWindow()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        if (core.SettingsViewModel is null)
        {
            core.SettingsViewModel = new SettingsViewModel();
            core.SettingsViewModel.Initialize(new ThemeService(), new LanguageService(), new ImageSettingsService());
        }

        if (core.SettingsViewModel.SettingsWindowConfig is null)
        {
            core.SettingsViewModel.SettingsWindowConfig = new SettingsWindowConfig();
            await core.SettingsViewModel.SettingsWindowConfig.LoadAsync();
        }

        if (_settingsWindow is null)
        {
            // vm.AssociationsViewModel ??= new FileAssociationsViewModel();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _settingsWindow = new SettingsWindow(core.SettingsViewModel.SettingsWindowConfig)
                {
                    DataContext = core,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                Show();
                _settingsWindow.Closing += (_, _) => _settingsWindow = null;
            });
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_settingsWindow.WindowState == WindowState.Minimized)
                {
                    WindowFunctions.ShowMinimizedWindow(_settingsWindow);
                }
                else
                {
                    Show();
                }
            });
        }

        await FunctionsMapper.CloseMenus();

        return;

        void Show()
        {
            WindowFunctions.InitializeWindowSizeAndPosition(_settingsWindow,
                core.SettingsViewModel.SettingsWindowConfig.WindowProperties);
            _settingsWindow.Show(desktop.MainWindow);
        }
    }


    public void ShowSingleImageResizeWindow()
    {
        // if (Dispatcher.UIThread.CheckAccess())
        // {
        //     Set();
        // }
        // else
        // {
        //     Dispatcher.UIThread.InvokeAsync(Set);
        // }
        //
        // return;
        //
        // void Set()
        // {
        //     if (_singleImageResizeWindow is null)
        //     {
        //         _singleImageResizeWindow = new SingleImageResizeWindow
        //         {
        //             DataContext = vm,
        //             WindowStartupLocation = WindowStartupLocation.CenterOwner
        //         };
        //         _singleImageResizeWindow.Show();
        //         _singleImageResizeWindow.Closing += (_, _) => _singleImageResizeWindow = null;
        //     }
        //     else
        //     {
        //         if (_singleImageResizeWindow.WindowState == WindowState.Minimized)
        //         {
        //             WindowFunctions.ShowMinimizedWindow(_singleImageResizeWindow);
        //         }
        //         else
        //         {
        //             _singleImageResizeWindow.Activate();
        //         }
        //     }
        //
        //     _ = FunctionsMapper.CloseMenus();
        // }
    }

    public async Task ShowBatchResizeWindow()
    {
        // if (_batchResizeWindow is null)
        // {
        //     if (vm.Window.BatchResizeWindowConfig?.WindowProperties is null)
        //     {
        //         vm.Window.BatchResizeWindowConfig = new BatchResizeWindowConfig();
        //         await vm.Window.BatchResizeWindowConfig.LoadAsync();
        //     }
        //
        //     vm.BatchResizeViewModel = new BatchResizeViewModel(NavigationManager.CanNavigate(vm),
        //         FilePicker.SelectDirectory, FilePicker.SelectFile, vm.PicViewer.FileInfo.CurrentValue,
        //         vm.PlatformService.GetFiles);
        //     await Dispatcher.UIThread.InvokeAsync(() =>
        //     {
        //         _batchResizeWindow = new BatchResizeWindow(vm.Window.BatchResizeWindowConfig)
        //         {
        //             DataContext = vm,
        //             WindowStartupLocation = WindowStartupLocation.CenterOwner
        //         };
        //         Show();
        //         _batchResizeWindow.Closing += (_, _) =>
        //         {
        //             _batchResizeWindow = null;
        //             vm.BatchResizeViewModel.Dispose();
        //             vm.BatchResizeViewModel = null;
        //         };
        //     });
        // }
        // else
        // {
        //     await Dispatcher.UIThread.InvokeAsync(() =>
        //     {
        //         if (_batchResizeWindow.WindowState == WindowState.Minimized)
        //         {
        //             WindowFunctions.ShowMinimizedWindow(_batchResizeWindow);
        //         }
        //         else
        //         {
        //             Show();
        //         }
        //     });
        // }
        //
        // await FunctionsMapper.CloseMenus();
        //
        // return;
        //
        // void Show()
        // {
        //     WindowFunctions.InitializeWindowSizeAndPosition(_batchResizeWindow, vm.Window.BatchResizeWindowConfig.WindowProperties);
        //     _batchResizeWindow.Show();
        // }
    }

    public void ShowEffectsWindow()
    {
        // if (Dispatcher.UIThread.CheckAccess())
        // {
        //     Set();
        // }
        // else
        // {
        //     Dispatcher.UIThread.InvokeAsync(Set);
        // }
        //
        // return;
        //
        // void Set()
        // {
        //     if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        //     {
        //         return;
        //     }
        //
        //     if (_effectsWindow is null)
        //     {
        //         _effectsWindow = new EffectsWindow
        //         {
        //             DataContext = vm,
        //             WindowStartupLocation = WindowStartupLocation.CenterOwner
        //         };
        //         _effectsWindow.Show();
        //         _effectsWindow.Closing += (_, _) => _effectsWindow = null;
        //     }
        //     else
        //     {
        //         if (_effectsWindow.WindowState == WindowState.Minimized)
        //         {
        //             WindowFunctions.ShowMinimizedWindow(_effectsWindow);
        //         }
        //         else
        //         {
        //             _effectsWindow.Activate();
        //         }
        //     }
        //
        //     _ = FunctionsMapper.CloseMenus();
        // }
    }

    public void ShowConvertWindow()
    {
        // if (Dispatcher.UIThread.CheckAccess())
        // {
        //     Set();
        // }
        // else
        // {
        //     Dispatcher.UIThread.InvokeAsync(Set);
        // }
        //
        // return;
        //
        // void Set()
        // {
        //     if (_convertWindow is null)
        //     {
        //         _convertWindow = new ConvertWindow
        //         {
        //             DataContext = vm,
        //             WindowStartupLocation = WindowStartupLocation.CenterOwner
        //         };
        //         _convertWindow.Show();
        //         _convertWindow.Closing += (_, _) => _convertWindow = null;
        //     }
        //     else
        //     {
        //         if (_convertWindow.WindowState == WindowState.Minimized)
        //         {
        //             WindowFunctions.ShowMinimizedWindow(_convertWindow);
        //         }
        //         else
        //         {
        //             _convertWindow.Activate();
        //         }
        //     }
        //
        //     _ = FunctionsMapper.CloseMenus();
        // }
    }

    public void ShowPrintPreviewWindow(string path, MainWindowViewModel vm)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow == null)
        {
            return;
        }

        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            if (_printPreviewWindow is null)
            {
                vm.PrintPreview = new PrintPreviewViewModel();

                _printPreviewWindow = new PrintPreviewWindow
                {
                    DataContext = vm
                };

                vm.PrintPreview.PrintCommand.SubscribeAwait(async (_, _) =>
                {
                    await _printPreviewWindow.RunPrintAsync(vm);
                }).AddTo(vm.PrintPreview.Disposables);

                vm.PrintPreview.CancelCommand.SubscribeAwait(async (_, _) =>
                {
                    await Dispatcher.UIThread.InvokeAsync(() => _printPreviewWindow?.Close());
                }).AddTo(vm.PrintPreview.Disposables);

                _printPreviewWindow.Show(desktop.MainWindow);
                _printPreviewWindow.Closing += (_, _) => _printPreviewWindow = null;

                Task.Run(() => LinuxPrintInitialization.Initialize(vm, path, _printPreviewWindow));
            }
            else
            {
                if (_printPreviewWindow.WindowState == WindowState.Minimized)
                {
                    WindowFunctions.ShowMinimizedWindow(_printPreviewWindow);
                }
                else
                {
                    _printPreviewWindow.Show();
                }
            }
        }
        catch (Exception)
        {
            // Ignore
        }
    }
}