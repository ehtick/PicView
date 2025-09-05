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
    private readonly (int MaxWidth, int ButtonWidth, bool ShowRotate)[] _breakpoints =
    [
        (MaxWidth: 450, ButtonWidth: 65, ShowRotate: false),
        (MaxWidth: 550, ButtonWidth: 70, ShowRotate:false),
        (MaxWidth: 650, ButtonWidth: 72, ShowRotate:false),
        (MaxWidth: 800, ButtonWidth: 75, ShowRotate:false),
        (int.MaxValue, ButtonWidth: 80, ShowRotate:true)
    ];

    public HoverBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        AddHandler(PointerPressedEvent, ManagePointerPressed, RoutingStrategies.Tunnel);
        SizeChanged += (_, args) => ApplyResponsiveResize(args.NewSize.Width);
        ApplyResponsiveResize(Bounds.Width);
    }

    private void ApplyResponsiveResize(double width)
    {
        var config = _breakpoints.First(bp => width <= bp.MaxWidth);
        RotateLeftButton.IsVisible = config.ShowRotate;
        NextButton.Width = PreviousButton.Width = config.ButtonWidth;
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
        else if (SettingsMenuButton.IsPointerOver)
        {
            if (props.IsRightButtonPressed)
            {
                ShowQuickSettingsDialog();
            }
            else if (props.IsLeftButtonPressed)
            {
                await FunctionsMapper.SettingsWindow();
            }
        }
        else if (ImageMenuButton.IsPointerOver)
        {
            if (props.IsRightButtonPressed || props.IsLeftButtonPressed)
            {
                ShowQuickEditingDialog();
            }
        }
        else if (ProgressBar.IsPointerOver)
        {
            if (props.IsRightButtonPressed)
            {
                //TODO: Create popup window to navigate to index
            }
        }
        else
        {
            if (props.IsRightButtonPressed)
            {
                ShowMainContextMenu();
            }
        }
    }

    private static void ShowNavigationDialog() =>
        UIHelper.GetMainView.MainGrid.Children.Add(new NavigationDialog());

    private static void ShowQuickSettingsDialog() =>
        UIHelper.GetMainView.MainGrid.Children.Add(new QuickSettingsDialog());
    
    private static void ShowQuickEditingDialog() =>
        UIHelper.GetMainView.MainGrid.Children.Add(new QuickEditingDialog());

    private static void ShowMainContextMenu()
    {
        if (UIHelper.GetMainView.Resources.TryGetResource("MainContextMenu", Application.Current.ActualThemeVariant, out var value)
            && value is ContextMenu mainContextMenu)
        {
            mainContextMenu.Open();
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Loaded -= OnLoaded;
        RemoveHandler(PointerPressedEvent, ManagePointerPressed);
    }
}
