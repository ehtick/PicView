using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using PicView.Avalonia.Functions;
using PicView.Avalonia.UI;
using PicView.Avalonia.Views.UC.PopUps;

namespace PicView.Avalonia.Views.UC;

public partial class HoverBar : UserControl
{
    public HoverBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        AddHandler(PointerPressedEvent, ManagePointerPressed, RoutingStrategies.Tunnel);
    }

    private async Task ManagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var props = e.Properties;
        
        if (NextButton.IsPointerOver)
        {
            if (props.IsRightButtonPressed)
            {
                OpenDialog();
            }
            else if (props.IsLeftButtonPressed)
            {
                await FunctionsMapper.Next();
            }
        }
        else if (PreviousButton.IsPointerOver)
        {
            if (props.IsRightButtonPressed)
            {
                OpenDialog();
            }
            else if (props.IsLeftButtonPressed)
            {
                await FunctionsMapper.Prev();
            }
        }
        else
        {
            if (props.IsRightButtonPressed)
            {
                if (UIHelper.GetMainView.Resources.TryGetResource("MainContextMenu", Application.Current.ActualThemeVariant, out var value))
                {
                    if (value is ContextMenu mainContextMenu)
                    {
                        mainContextMenu.Open();
                    }
                }
            }
        }
    }

    private void OpenDialog()
    {
        UIHelper.GetMainView.MainGrid.Children.Add(new NavigationDialog());
    }
    
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Loaded -= OnLoaded;
        RemoveHandler(PointerPressedEvent, ManagePointerPressed);
    }
}