using Avalonia.Controls;

namespace PicView.Avalonia.Views.Config;

public partial class FileAssociationsView : UserControl
{
    public FileAssociationsView()
    {
        InitializeComponent();

        // FileTypesScrollViewer.Height = ScreenHelper.ScreenSize.WorkingAreaHeight switch
        // {
        //     > 500 and <= 650 => 340,
        //     >= 650 and <= 700 => 440,
        //     >= 700 => 500,
        //     _ => 240
        // };
        //
        // Loaded += delegate
        // {
        //     InitializeCheckBoxesCollection();
        //
        //     KeyDown += (_, e) =>
        //     {
        //         var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        //         if (e.Key == Key.F && ctrl)
        //         {
        //             FilterBox.Focus();
        //         }
        //     };
        // };
    }
}