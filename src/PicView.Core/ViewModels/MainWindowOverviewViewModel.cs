using System.Collections.ObjectModel;
using R3;

namespace PicView.Core.ViewModels;

public class MainWindowOverviewViewModel
{
    // The collection of all open windows
    public ObservableCollection<MainWindowViewModel> MainWindows { get; } = [];

    // The single "Correct" window that currently has focus
    public BindableReactiveProperty<MainWindowViewModel?> ActiveWindow { get; } = new();
}