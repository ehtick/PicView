using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Core.ProcessHandling;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Functions;

public static class AppFunctions
{
    public static void Restart(TabViewModel? senderTab = null)
    {
        var openFile = string.Empty;
        var getFromArgs = false;
        if (senderTab is not null)
        {
            if (senderTab.Model.FileInfo.Exists)
            {
                openFile = senderTab.Model.FileInfo.FullName;
            }
            else
            {
                getFromArgs = true;
            }
        }
        else
        {
            getFromArgs = true;
        }
        if (getFromArgs)
        {
            var args = Environment.GetCommandLineArgs();
            if (args is not null && args.Length > 1)
            {
                openFile = args[1];
            }
        }
        ProcessHelper.RestartApp(openFile);
        
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            Environment.Exit(0);
            return;
        }
        Dispatcher.UIThread.Invoke(() =>
        {
            desktop.MainWindow?.Close();
        });
    }
}