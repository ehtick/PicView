using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PicView.Avalonia.UI;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.UC.Buttons;
public partial class ClickArrowLeft2 : UserControl
{
    public ClickArrowLeft2()
    {
        InitializeComponent();
        Loaded += delegate
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            PointerWheelChanged += async (_, e) => await ImageViewer.PreviewOnPointerWheelChanged(this, e);
            AddHandler(PointerPressedEvent, ManagePointerPressed, RoutingStrategies.Tunnel);
            PolyButton.Click += (_, _) =>
            {
                vm.IsClickArrowLeftClicked = true;
                UIHelper.SetButtonInterval(PolyButton);
            };
            
            _ = new HoverFadeButtonHandler2(this, vm, PolyButton);
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
