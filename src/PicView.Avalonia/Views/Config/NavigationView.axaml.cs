using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace PicView.Avalonia.Views.Config;
public partial class NavigationView : UserControl
{
    public NavigationView()
    {
        InitializeComponent();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            TaskBarToggleButton.IsEnabled = false;
        }
    }
}
