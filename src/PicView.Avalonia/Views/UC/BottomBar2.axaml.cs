using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.UC;

public partial class BottomBar2 : UserControl
{
    
    private RotationContextMenu? _rotationContextMenu;
    public BottomBar2()
    {
        InitializeComponent();

        Loaded += delegate
        {
            PointerPressed += (_, e) => MoveWindow(e);
            PointerExited += (_, _) => { DragAndDropManager.RemoveDragDropView(); };
            
            _rotationContextMenu = new RotationContextMenu();
            _rotationContextMenu.UpdateSubscription();
            FlipButton.ContextMenu = _rotationContextMenu;
            RotateRightButton.ContextMenu = _rotationContextMenu;
            FlipButton.PointerPressed += (_, e) => { OpenRotationContextMenu(e); };
            RotateRightButton.PointerPressed += (_, e) => { OpenRotationContextMenu(e); };

            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }

            PreviousButton.Click += (_, _) =>
            {
                vm.IsNavigationButtonLeftClicked = true;
                vm.TopTitlebarViewModel.CloseDropDownMenu();
                UIHelper.SetButtonInterval((IconButton?)PreviousButton);
            };
            NextButton.Click += (_, _) =>
            {
                vm.IsNavigationButtonRightClicked = true;
                vm.TopTitlebarViewModel.CloseDropDownMenu();
                UIHelper.SetButtonInterval((IconButton?)NextButton);
            };

            RotateRightButton.Click += (_, _) =>
            {
                vm.TopTitlebarViewModel.CloseDropDownMenu();
                vm.IsBottomToolbarRightRotationClicked = true;
            };
            RotateLeftButton.Click += (_, _) =>
            {
                vm.TopTitlebarViewModel.CloseDropDownMenu();
                vm.IsBottomToolbarLeftRotationClicked = true;
            };
            
            FileMenuButton.Click += (_, _) =>
            {
                vm.TopTitlebarViewModel.CloseDropDownMenu();
            };
            
            ZoomInButton.Click += (_, _) =>
            {
                vm.TopTitlebarViewModel.CloseDropDownMenu();
            };
            
            ZoomOutButton.Click += (_, _) =>
            {
                vm.TopTitlebarViewModel.CloseDropDownMenu();
            };
            
            ResetZoomButton.Click += (_, _) =>
            {
                vm.TopTitlebarViewModel.CloseDropDownMenu();
            };
            
            FlipButton.Click += (_, _) =>
            {
                vm.TopTitlebarViewModel.CloseDropDownMenu();
            };
            
            SettingsMenuButton.Click += (_, _) =>
            {
                vm.TopTitlebarViewModel.CloseDropDownMenu();
            };

            if (!Application.Current.TryGetResource("SecondaryTextColor",
                    Application.Current.RequestedThemeVariant, out var textColor))
            {
                return;
            }

            if (textColor is not Color color)
            {
                return;
            }

            if (Settings.Theme.GlassTheme)
            {
                var alphaBrush = new SolidColorBrush(Color.FromArgb(15, 100, 100, 100));
                
                MainBottomBorder.Background = Brushes.Transparent;
                MainBottomBorder.BorderThickness = new Thickness(0);

                FileMenuButton.Background = Brushes.Transparent;
                FileMenuButton.Classes.Remove("noBorderHover");
                FileMenuButton.Classes.Add("hover");

                ResetZoomButton.Background = Brushes.Transparent;
                ResetZoomButton.Classes.Remove("noBorderHover");
                ResetZoomButton.Classes.Add("hover");

                SettingsMenuButton.Background = Brushes.Transparent;
                SettingsMenuButton.Classes.Remove("noBorderHover");
                SettingsMenuButton.Classes.Add("hover");

                ZoomInButton.Background = alphaBrush;
                ZoomInButton.Classes.Remove("noBorderHover");
                ZoomInButton.Classes.Add("hover");

                ZoomOutButton.Background = alphaBrush;
                ZoomOutButton.Classes.Remove("noBorderHover");
                ZoomOutButton.Classes.Add("hover");

                NextButton.Background = alphaBrush;

                PreviousButton.Background = alphaBrush;

                RotateRightButton.Background = alphaBrush;
                RotateRightButton.Classes.Remove("noBorderHover");
                RotateRightButton.Classes.Add("hover");
                
                RotateLeftButton.Background = Brushes.Transparent;
                RotateLeftButton.Classes.Remove("noBorderHover");
                RotateLeftButton.Classes.Add("hover");

                FlipButton.Background = alphaBrush;
                FlipButton.Classes.Remove("noBorderHover");
                FlipButton.Classes.Add("hover");

                FileMenuButton.Foreground = new SolidColorBrush(color);
                RotateLeftButton.Foreground = new SolidColorBrush(color);
                ResetZoomButton.Foreground = new SolidColorBrush(color);
                SettingsMenuButton.Foreground = new SolidColorBrush(color);

                NextButton.Foreground = new SolidColorBrush(color);
                PreviousButton.Foreground = new SolidColorBrush(color);
            }
            else if (!Settings.Theme.Dark)
            {
                FileMenuButton.Classes.Remove("noBorderHover");
                FileMenuButton.Classes.Add("noBorderHoverAlt");

                RotateLeftButton.Classes.Remove("noBorderHover");
                RotateLeftButton.Classes.Add("noBorderHoverAlt");

                ResetZoomButton.Classes.Remove("noBorderHover");
                ResetZoomButton.Classes.Add("noBorderHoverAlt");

                SettingsMenuButton.Classes.Remove("noBorderHover");
                SettingsMenuButton.Classes.Add("noBorderHoverAlt");

                ZoomOutButton.Classes.Remove("noBorderHover");
                ZoomInButton.Classes.Remove("noBorderHover");
                ZoomOutButton.Classes.Add("noBorderHoverAlt");
                ZoomInButton.Classes.Add("noBorderHoverAlt");

                RotateRightButton.Classes.Remove("noBorderHover");
                FlipButton.Classes.Remove("noBorderHover");
                RotateRightButton.Classes.Add("noBorderHoverAlt");
                FlipButton.Classes.Add("noBorderHoverAlt");
            }
        };
    }

    private void MoveWindow(PointerPressedEventArgs e)
    {
        if (VisualRoot is null)
        {
            return;
        }

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            return;
        }

        if (DataContext is MainWindowViewModel vm)
        {
            vm.TopTitlebarViewModel.CloseDropDownMenu();
        }

        if (TopLevel.GetTopLevel(this) is Window window)
        {
            WindowFunctions.WindowDragBehavior(window, e);
        }
    }
    
    private void OpenRotationContextMenu(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            return;
        }

        // Context menu doesn't want to be opened normally
        _rotationContextMenu.Open();
    }

    public void ResponsiveNavigationBtnSize(AvaloniaPropertyChangedEventArgs<Size> size)
    {
        if (size.NewValue.Value.Width < 450)
        {
            PreviousButton.Width = NextButton.Width = 65;
        }
        else
        {
            PreviousButton.Width = NextButton.Width = 80;
        }
    }   
}