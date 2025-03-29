using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PicView.Avalonia.UI;
using PicView.Core.Calculations;
using PicView.Core.FileHandling;
using PicView.Core.Localization;
using PicView.Core.WindowsNT.FileAssociation;

namespace PicView.Avalonia.Win32.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        MinHeight = ScreenHelper.ScreenSize.WorkingAreaHeight switch
        {
            < 650 => 600,
            >= 650 => 700,
            _ => SizeDefaults.WindowMinSize
        };
        InitializeComponent();
        if (Settings.Theme.GlassTheme)
        {
            LogoBorder.Background = Brushes.Transparent;
            LogoBorder.BorderThickness = new Thickness(0);
            
            SettingsButton.Background = Brushes.Transparent;
            SettingsButton.BorderThickness = new Thickness(0);
            
            CloseButton.Background = Brushes.Transparent;
            CloseButton.BorderThickness = new Thickness(0);
            
            MinimizeButton.Background = Brushes.Transparent;
            MinimizeButton.BorderThickness = new Thickness(0);
            
            TitleText.Background = Brushes.Transparent;
            
            if (!Application.Current.TryGetResource("SecondaryTextColor",
                    Application.Current.RequestedThemeVariant, out var textColor))
            {
                return;
            }

            if (textColor is not Color color)
            {
                return;
            }
            
            TitleText.Foreground = new SolidColorBrush(color);
            MinimizeButton.Foreground = new SolidColorBrush(color);
            CloseButton.Foreground = new SolidColorBrush(color);
        }
        else if (!Settings.Theme.Dark)
        {
            ParentBorder.Background = new SolidColorBrush(Color.FromArgb(114,132, 132, 132));
        }
        Loaded += delegate
        {
            MinWidth = Width;
            Title = TranslationManager.GetTranslation("Settings") + " - PicView";
        };
        KeyDown += (_, e) =>
        {
            var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.F when ctrl:
                    FocusFilterBox();
                    break;
            }
        };

        Closing += async delegate
        {
            Hide();
            await SaveSettingsAsync();
        };

        InitializeFileAssociationManager();
    }

    private void FocusFilterBox()
    {
        var fileAssociationsView = SettingsView.FindControl<Control>("FileAssociationsView");
        var filterBox = fileAssociationsView?.FindControl<Control>("FilterBox");
        var isFilterBoxEffectivelyVisible = filterBox?.Bounds is { Width: > 0, Height: > 0 };
        if (isFilterBoxEffectivelyVisible)
        {
            filterBox?.Focus();
        }
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is null) { return; }

        var hostWindow = (Window)VisualRoot;
        hostWindow?.BeginMoveDrag(e);
    }

    private void Close(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Minimize(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private static void InitializeFileAssociationManager()
    {
        var iIFileAssociationService = new WindowsFileAssociationService();
        FileAssociationManager.Initialize(iIFileAssociationService);
    }

}