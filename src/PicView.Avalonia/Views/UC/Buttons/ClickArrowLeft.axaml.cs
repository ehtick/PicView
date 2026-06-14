using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PicView.Avalonia.UI;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.UC.Buttons;
public partial class ClickArrowLeft : UserControl
{
    public ClickArrowLeft()
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
                core.MainWindows.ActiveWindow.CurrentValue.IsClickArrowLeftClicked = true;
                core.MainWindows.ActiveWindow.CurrentValue.TopTitlebarViewModel.CloseDropDownMenu();
                UIHelper.SetButtonInterval(PolyButton);
            };
            
            _ = new HoverFadeButtonHandler(this, PolyButton);
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