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
            PointerExited += (_, _) => { DragAndDropHelper.RemoveDragDropView(); };
            
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
                UIHelper.SetButtonInterval((IconButton?)PreviousButton);
            };
            NextButton.Click += (_, _) =>
            {
                vm.IsNavigationButtonRightClicked = true;
                UIHelper.SetButtonInterval((IconButton?)NextButton);
            };

            RotateRightButton.Click += (_, _) => { vm.IsBottomToolbarRotationClicked = true; };

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

                CropButton.Background = Brushes.Transparent;
                CropButton.Classes.Remove("noBorderHover");
                CropButton.Classes.Add("hover");

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

                FlipButton.Background = alphaBrush;
                FlipButton.Classes.Remove("noBorderHover");
                FlipButton.Classes.Add("hover");

                FileMenuButton.Foreground = new SolidColorBrush(color);
                CropButton.Foreground = new SolidColorBrush(color);
                ResetZoomButton.Foreground = new SolidColorBrush(color);
                SettingsMenuButton.Foreground = new SolidColorBrush(color);

                NextButton.Foreground = new SolidColorBrush(color);
                PreviousButton.Foreground = new SolidColorBrush(color);
            }
            else if (!Settings.Theme.Dark)
            {
                FileMenuButton.Classes.Remove("noBorderHover");
                FileMenuButton.Classes.Add("noBorderHoverAlt");

                CropButton.Classes.Remove("noBorderHover");
                CropButton.Classes.Add("noBorderHoverAlt");
                if (TryGetResource("ImageMenuBrush", Application.Current.RequestedThemeVariant,
                        out var imageMenuBrush))
                {
                    if (imageMenuBrush is SolidColorBrush brush)
                    {
                        UIHelper.SetButtonHover(CropButton, brush);
                    }
                }

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
}