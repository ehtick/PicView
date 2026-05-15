using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.UC;

public partial class FileHistoryItem : UserControl
{
    public FileHistoryItem()
    {
        InitializeComponent();
        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
        MainButton.AddHandler(PointerPressedEvent, MainButtonOnClick, RoutingStrategies.Tunnel);
    }

    private async ValueTask MainButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current.DataContext is not CoreViewModel core || DataContext is not FileHistoryEntryViewModel entry)
        {
            return;
        }
        var window = core.MainWindows.ActiveWindow.CurrentValue;
        var tabs = window.WindowTabs;
        window.TopTitlebarViewModel.DropDownMenu.IsDropDownMenuVisible.Value = false;
        var isViewStartUpMenu = false;
        if (tabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is StartUpMenu)
        {
            tabs.ActiveTab.CurrentValue.CurrentView.Value = new ImageViewer();
            isViewStartUpMenu = true;
        }
        
        var isLoadedSuccessfully = await tabs.LoadFromStringAsync(entry.FilePath.CurrentValue);
        if (!isLoadedSuccessfully && isViewStartUpMenu)
        {
            tabs.ActiveTab.CurrentValue.CurrentView.Value = new StartUpMenu();
        }
        else
        {
            WindowResizing.SetSize(window, WindowResizeReason.Layout);
        }
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        ButtonPanel.IsVisible = true;
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        ButtonPanel.IsVisible = false;
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        PointerEntered -= OnPointerEntered;
        PointerExited -= OnPointerExited;
    }
}