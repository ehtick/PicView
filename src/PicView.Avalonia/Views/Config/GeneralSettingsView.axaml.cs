using Avalonia.Controls;

namespace PicView.Avalonia.Views.Config;

public partial class GeneralSettingsView : UserControl
{
    public GeneralSettingsView()
    {
        InitializeComponent();
        Loaded += delegate
        {
            ApplicationStartupBox.SelectedIndex = Settings.StartUp.OpenLastFile ? 1 : 0;
            
            ApplicationStartupBox.SelectionChanged += async delegate
            {
                if (ApplicationStartupBox.SelectedIndex == -1)
                {
                    return;
                }
                Settings.StartUp.OpenLastFile = ApplicationStartupBox.SelectedIndex == 1;
                await SaveSettingsAsync();
            };
            ApplicationStartupBox.DropDownOpened += delegate
            {
                if (ApplicationStartupBox.SelectedIndex == -1)
                {
                    ApplicationStartupBox.SelectedIndex = Settings.StartUp.OpenLastFile ? 0 : 1;
                }
            };
        };
    }
}