using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.UI;
using PicView.Core.Localization;
using PicView.Core.ViewModels;
using Observable = R3.Observable;

namespace PicView.Avalonia.Views.Config;

public partial class FileAssociationsView : UserControl
{
    public FileAssociationsView()
    {
        InitializeComponent();

        FileTypesScrollViewer.Height = ScreenHelper.ScreenSize.WorkingAreaHeight switch
        {
            > 500 and <= 650 => 340,
            >= 650 and <= 700 => 440,
            >= 700 => 500,
            _ => 240
        };

        Loaded += delegate
        {
            InitializeCheckBoxesCollection();

            KeyDown += (_, e) =>
            {
                var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
                if (e.Key == Key.F && ctrl)
                {
                    FilterBox.Focus();
                }
            };
        };
    }

    private void InitializeCheckBoxesCollection()
    {
    }
}