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
        SizeChanged += OnSizeChanged;
        ResponsiveResize(Bounds.Width);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ResponsiveResize(e.NewSize.Width);
    }

    private void ResponsiveResize(double width)
    {
        const int firstBreakPoint = 450;
        const int secondBreakPoint = 550;

        switch (width)
        {
            case <= firstBreakPoint:
                RotateLeftButton.IsVisible = false;
                NextButton.Width = PreviousButton.Width = 65;
                break;
            case <= secondBreakPoint:
                RotateLeftButton.IsVisible = false;
                NextButton.Width = PreviousButton.Width = 70;
                break;
            default:
                RotateLeftButton.IsVisible = true;
                NextButton.Width = PreviousButton.Width = 80;
                break;
        }
    }

    private async Task ManagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var props = e.Properties;
        
        if (NextButton.IsPointerOver)
        {
            if (props.IsRightButtonPressed)
            {
                ShowNavigationDialog();
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
                ShowNavigationDialog();
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

    private void ShowNavigationDialog()
    {
        UIHelper.GetMainView.MainGrid.Children.Add(new NavigationDialog());
    }
    
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Loaded -= OnLoaded;
        SizeChanged -= OnSizeChanged;
        RemoveHandler(PointerPressedEvent, ManagePointerPressed);
    }
}