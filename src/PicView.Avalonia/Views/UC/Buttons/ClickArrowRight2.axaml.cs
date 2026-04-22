using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PicView.Avalonia.UI;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.UC.Buttons;
public partial class ClickArrowRight2 : UserControl
{
    public ClickArrowRight2()
    {
        InitializeComponent();
        Loaded += delegate
        {
            if (Application.Current.DataContext is not CoreViewModel core)
            {
                return;
            }
            AddHandler(PointerPressedEvent, ManagePointerPressed, RoutingStrategies.Tunnel);
            PolyButton.Click += (_, _) =>
            {
                core.MainWindows.ActiveWindow.CurrentValue.IsClickArrowRightClicked = true;
                core.MainWindows.ActiveWindow.CurrentValue.TopTitlebarViewModel.CloseDropDownMenu();
                UIHelper.SetButtonInterval(PolyButton);
            };
            _ = new HoverFadeButtonHandler(this, core.MainWindows.ActiveWindow.Value, PolyButton);
        };
    }

    private void ManagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var props = e.Properties;

        if (PolyButton.IsPointerOver)
        {
            if (props.IsRightButtonPressed)
            {
                DialogManager.AddNavigationDialog();
            }
        }
        else
        {
            if (props.IsRightButtonPressed)
            {
                UIHelper.ShowMainContextMenu();
            }
        }
    }
}
