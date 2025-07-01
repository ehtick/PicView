using Avalonia.Controls;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using R3;

namespace PicView.Avalonia.Views;

public partial class WindowSettingsView : UserControl
{
    public WindowSettingsView()
    {
        InitializeComponent();
        Loaded += delegate
        {
            // TODO add this to SettingsViewModel
            if (DataContext is not MainViewModel vm)
            {
                return;
            }
            vm.SettingsViewModel.WindowMargin = Settings.WindowProperties.Margin;
            vm.SettingsViewModel.ObservePropertyChanged(x => x.WindowMargin)
                .SubscribeAwait(async (x, _) =>
                {
                    Settings.WindowProperties.Margin = x;
                    if (Settings.WindowProperties.AutoFit)
                    {
                        await WindowResizing.SetSizeAsync(vm);
                        WindowFunctions.CenterWindowOnScreen();
                    }
                });
        };
    }
}
