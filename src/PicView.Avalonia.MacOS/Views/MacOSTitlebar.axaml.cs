using Avalonia;
using Avalonia.Media;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacOSTitlebar : MainTitleBar
{
    public MacOSTitlebar()
    {
        InitializeComponent();
        
        Loaded += (_, _) =>
        {
            if (Settings.Theme.GlassTheme)
            {
                SetGlassTheme();
            }
            else if (!Settings.Theme.Dark)
            {
                SetLightTheme();
            }

        };
    }

    private void SetGlassTheme()
    {
        TopWindowBorder.Background = Brushes.Transparent;

        EditableTitlebar.Background = Brushes.Transparent;
        EditableTitlebar.BorderThickness = new Thickness(0);

        CreateTabButton.Background = Brushes.Transparent;
        CreateTabButton.BorderThickness = new Thickness(0);;
                
        DropMenuButton.Background = Brushes.Transparent;
        DropMenuButton.BorderThickness = new Thickness(0);;
                
        var brush = UIHelper.GetBrush("SecondaryTextColor");
        EditableTitlebar.Foreground = brush;
        SearchButton.Foreground = brush;
        CreateTabButton.Foreground = brush;
        DropMenuButton.Foreground = brush;
    }
    
    private void SetLightTheme()
    {
        DropMenuButton.Classes.Remove("altHover");
        DropMenuButton.Classes.Add("hover");
        
        CreateTabButton.Classes.Remove("altHover");
        CreateTabButton.Classes.Add("hover");
        
        SearchButton.Classes.Remove("altHover");
        SearchButton.Classes.Add("hover");
    }
}