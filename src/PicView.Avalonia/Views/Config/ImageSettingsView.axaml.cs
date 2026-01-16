using Avalonia.Controls;
using PicView.Avalonia.ViewModels;

namespace PicView.Avalonia.Views.Config;

public partial class ImageSettingsView : UserControl
{
    public ImageSettingsView()
    {
        InitializeComponent();
        Loaded += delegate
        {
            ImageAliasingBox.SelectedIndex = Settings.ImageScaling.IsScalingSetToNearestNeighbor ? 1 : 0;
        
            ImageAliasingBox.SelectionChanged += async delegate
            {
                if (ImageAliasingBox.SelectedIndex == -1)
                {
                    return;
                }
                Settings.ImageScaling.IsScalingSetToNearestNeighbor = ImageAliasingBox.SelectedIndex == 1;
                if (DataContext is MainViewModel vm)
                {
                    vm.ImageViewer.TriggerScalingModeUpdate(true);
                }
                await SaveSettingsAsync();
            };
            ImageAliasingBox.DropDownOpened += delegate
            {
                if (ImageAliasingBox.SelectedIndex == -1)
                {
                    ImageAliasingBox.SelectedIndex = Settings.ImageScaling.IsScalingSetToNearestNeighbor ? 0 : 1;
                }
            };
        };
    }
}
