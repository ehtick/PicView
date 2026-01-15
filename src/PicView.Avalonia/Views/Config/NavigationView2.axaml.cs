using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace PicView.Avalonia.Views.Config;
public partial class NavigationView2 : UserControl
{
    public NavigationView2()
    {
        InitializeComponent();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            TaskBarToggleButton.IsEnabled = false;
        }
    }
}
