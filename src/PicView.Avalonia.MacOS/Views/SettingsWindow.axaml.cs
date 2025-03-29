using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Core.Calculations;
using PicView.Core.FileHandling;
using PicView.Core.Localization;
using PicView.Core.MacOS.FileAssociation;

namespace PicView.Avalonia.MacOS.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        MinHeight = ScreenHelper.ScreenSize.WorkingAreaHeight switch
        {
            < 650 => 600,
            >= 650 => 700,
            _ => SizeDefaults.WindowMinSize
        };
        if (!Settings.Theme.Dark || Settings.Theme.GlassTheme)
        {
            TitleText.Background = Brushes.Transparent;
            XSettingsView.Background = Brushes.Transparent;
        }
        Loaded += delegate
        {
            MinWidth = MaxWidth = Width;
            Height = 500;
            Title = TranslationManager.Translation.Settings + " - PicView";
        };
        KeyDown += (_, e) =>
        {
            if (e.Key is Key.Escape)
            {
                e.Handled = true;
                MainKeyboardShortcuts.IsEscKeyEnabled = false;
                Close();
            }
        };
        
        Closing += async delegate
        {
            Hide();
            await SaveSettingsAsync();
        };
        
        InitializeFileAssociationManager();
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }
    
    private static void InitializeFileAssociationManager()
    {
        var iIFileAssociationService = new MacFileAssociationService();
        FileAssociationManager.Initialize(iIFileAssociationService);
    }
}