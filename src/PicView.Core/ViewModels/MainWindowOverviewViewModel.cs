using System.Collections.ObjectModel;
using R3;

namespace PicView.Core.ViewModels;

public class MainWindowOverviewViewModel
{
    // The collection of all open windows
    public ObservableCollection<MainWindowViewModel> MainWindows { get; } = [];

    // The single "Correct" window that currently has focus
    public BindableReactiveProperty<MainWindowViewModel?> ActiveWindow { get; } = new();

    public void RegisterWindow(MainWindowViewModel windowVm)
    {
        if (!MainWindows.Contains(windowVm))
        {
            MainWindows.Add(windowVm);
        }

        // Automatically make the new window active upon creation
        ActiveWindow.Value = windowVm;
        
        // Subscribe to this window's activation requests
        // (Assuming MainWindowViewModel has an observable for when it gets focus)
        windowVm.RequestActive
            .Subscribe(_ => ActiveWindow.Value = windowVm);
    }

    public void UnregisterWindow(MainWindowViewModel windowVm)
    {
        MainWindows.Remove(windowVm);
        windowVm.Dispose(); // Ensure cleanup

        // If we closed the active window, fallback to another one
        if (ActiveWindow.Value == windowVm)
        {
            ActiveWindow.Value = MainWindows.FirstOrDefault();
        }
    }
}