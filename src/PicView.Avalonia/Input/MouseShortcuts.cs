using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using PicView.Avalonia.Functions;

namespace PicView.Avalonia.Input;

public static class MouseShortcuts
{
    public static async Task MainWindow_PointerPressed(PointerPressedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }
        var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
        var prop = e.GetCurrentPoint(topLevel).Properties;

        if (prop.IsXButton1Pressed)
        {
            if (Settings.Navigation.IsNavigatingFileHistory)
            {
                await FunctionsMapper.OpenPreviousFileHistoryEntry().ConfigureAwait(false);
            }
            else if (Settings.Navigation.IsNavigatingBetweenDirectories)
            {
                await FunctionsMapper.PrevFolder().ConfigureAwait(false);
            }
        }
        else if (prop.IsXButton2Pressed)
        {
            if (Settings.Navigation.IsNavigatingFileHistory)
            {
                await FunctionsMapper.OpenNextFileHistoryEntry().ConfigureAwait(false);
            }
            else if (Settings.Navigation.IsNavigatingBetweenDirectories)
            {
                await FunctionsMapper.NextFolder().ConfigureAwait(false);
            }
        }
    }
}
