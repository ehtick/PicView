using Avalonia.Media;
using PicView.Avalonia.Converters;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using R3;

namespace PicView.Avalonia.Views.UC.Menus;

public partial class ToolsMenu : AnimatedMenu
{
    public ToolsMenu()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (Settings.Theme.GlassTheme)
            {
                BatchResizeButton.Classes.Remove("noBorderHover");
                BatchResizeButton.Classes.Add("hover");
                
                EffectsButton.Classes.Remove("noBorderHover");
                EffectsButton.Classes.Add("hover");
            }
            else if (!Settings.Theme.Dark)
            {
                TopBorder.Background = Brushes.White;
            }
            Observable.EveryValueChanged(this, x => x.IsOpen, UIHelper.GetFrameProvider)
                .Skip(1)
                .Subscribe(_ =>
                {
                    DetermineIfOptimizeImageShouldBeEnabled();
                });
        };
    }

    private void DetermineIfOptimizeImageShouldBeEnabled()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        ConversionHelper.DetermineIfOptimizeImageShouldBeEnabled(vm);
    }
}