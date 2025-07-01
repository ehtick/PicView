using Avalonia.Controls;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using R3;

namespace PicView.Avalonia.Views;

public partial class ZoomSettingsView : UserControl
{
    public ZoomSettingsView()
    {
        InitializeComponent();
        Loaded += delegate
        {
            MouseWheelBox.SelectedIndex = Settings.Zoom.CtrlZoom ? 0 : 1;

            MouseWheelBox.SelectionChanged += async delegate
            {
                if (MouseWheelBox.SelectedIndex == -1)
                {
                    return;
                }

                Settings.Zoom.CtrlZoom = MouseWheelBox.SelectedIndex == 0;
                await SaveSettingsAsync();
            };
            MouseWheelBox.DropDownOpened += delegate
            {
                if (MouseWheelBox.SelectedIndex == -1)
                {
                    MouseWheelBox.SelectedIndex = Settings.Zoom.CtrlZoom ? 0 : 1;
                }
            };

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
