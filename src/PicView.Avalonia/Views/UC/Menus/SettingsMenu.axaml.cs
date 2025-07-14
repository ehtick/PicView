using Avalonia;
using Avalonia.Media;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;

namespace PicView.Avalonia.Views.UC.Menus;

public partial class SettingsMenu : AnimatedMenu
{
    public SettingsMenu()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (Settings.Theme.GlassTheme || !Settings.Theme.Dark)
            {
                UIHelper.SwitchHoverBorderClass(SettingsButton);
                UIHelper.SwitchHoverBorderClass(AboutWindowButton);
            }

            if (!Settings.Theme.Dark && !Settings.Theme.GlassTheme)
            {
                TopBorder.Background = Brushes.White;
                UIHelper.SwitchHoverClass(StretchButton);
                UIHelper.SwitchHoverClass(ScrollButton);
                UIHelper.SwitchHoverClass(LoopingButton);
                UIHelper.SwitchHoverClass(AutofitButton);
                UIHelper.SwitchHoverClass(TopMostButton);
                UIHelper.SwitchHoverClass(SubDirButton);
            }
        };

        if (TryGetResource("ScrollingBrush", Application.Current.RequestedThemeVariant,
                out var scrollingBrush))
        {
            if (scrollingBrush is SolidColorBrush brush)
            {
                UIHelper.SetButtonHover(ScrollButton, brush);
            }
        }

        if (TryGetResource("StretchBrush", Application.Current.RequestedThemeVariant,
                out var stretchBrush))
        {
            if (stretchBrush is SolidColorBrush brush)
            {
                UIHelper.SetButtonHover(StretchButton, brush);
            }
        }

        if (TryGetResource("LoopingBrush", Application.Current.RequestedThemeVariant,
                out var loopingBrush))
        {
            if (loopingBrush is SolidColorBrush brush)
            {
                UIHelper.SetButtonHover(LoopingButton, brush);
            }
        }

        if (TryGetResource("AutofitBrush", Application.Current.RequestedThemeVariant,
                out var autofitBrush))
        {
            if (autofitBrush is SolidColorBrush brush)
            {
                UIHelper.SetButtonHover(AutofitButton, brush);
            }
        }

        if (TryGetResource("TopMostBrush", Application.Current.RequestedThemeVariant,
                out var topMostBrush))
        {
            if (topMostBrush is SolidColorBrush brush)
            {
                UIHelper.SetButtonHover(TopMostButton, brush);
            }
        }

        if (TryGetResource("SubDirBrush", Application.Current.RequestedThemeVariant,
                out var subDirBrush))
        {
            if (subDirBrush is SolidColorBrush brush)
            {
                UIHelper.SetButtonHover(SubDirButton, brush);
            }
        }
    }
}