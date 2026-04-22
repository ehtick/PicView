using Avalonia.Controls;
using PicView.Avalonia.UI;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.UC.Buttons;

public partial class GalleryShortcut2 : UserControl
{
    public GalleryShortcut2()
    {
        InitializeComponent();
        Loaded += delegate
        {
            if (DataContext is not TabViewModel tab)
            {
                return;
            }

            if (tab.ParentWindowContext is not CoreViewModel core)
            {
                return;
            }
            _ = new HoverFadeButtonHandler(this, core.MainWindows.ActiveWindow.CurrentValue, InnerButton);
        };
    }
}
