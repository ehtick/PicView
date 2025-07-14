using System.Runtime.InteropServices;
using Avalonia.Media;
using PicView.Avalonia.CustomControls;

namespace PicView.Avalonia.Views.UC.Menus;

public partial class FileMenu : AnimatedMenu
{
    public FileMenu()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (Settings.Theme.GlassTheme)
            {
            }
            else if (!Settings.Theme.Dark)
            {
                TopBorder.Background = Brushes.White;

                NewWindowButton.Classes.Remove("altHover");
                NewWindowButton.Classes.Add("hover");

                PasteButton.Classes.Remove("altHover");
                PasteButton.Classes.Add("hover");

                SaveAsButton.Classes.Remove("altHover");
                SaveAsButton.Classes.Add("hover");

                ShowInFolderButton.Classes.Remove("altHover");
                ShowInFolderButton.Classes.Add("hover");

                OpenWithButton.Classes.Remove("altHover");
                OpenWithButton.Classes.Add("hover");

                OpenButton.Classes.Remove("altHover");
                OpenButton.Classes.Add("hover");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                PrintButton.IsEnabled = false;
            }
        };
    }
}