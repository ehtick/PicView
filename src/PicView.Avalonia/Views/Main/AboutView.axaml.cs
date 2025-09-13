using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using PicView.Core.Config;

namespace PicView.Avalonia.Views.Main;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            AppVersion.Text = VersionHelper.GetCurrentVersion();

            if (!Settings.Theme.Dark && !Settings.Theme.GlassTheme)
            {
                if (!Application.Current.TryGetResource("MainTextColor",
                        Application.Current.RequestedThemeVariant, out var textColor))
                {
                    return;
                }

                if (textColor is not Color color)
                {
                    return;
                }

                UpdateButton.Background = Brushes.White;
                UpdateButton.Foreground = new SolidColorBrush(color);
                UpdateButton.Classes.Remove("altHover");
                UpdateButton.Classes.Add("accentHover");
                UpdateButton.PointerEntered += (_, _) => { UpdateButton.Foreground = Brushes.White; };
                UpdateButton.PointerExited += (_, _) => { UpdateButton.Foreground = new SolidColorBrush(color); };
            }

            KofiImage.PointerEntered += (_, _) =>
            {
                if (!TryGetResource("kofi_button_redDrawingImage", ThemeVariant.Default, out var redDrawing))
                {
                    return;
                }

                if (redDrawing is DrawingImage drawingImage)
                {
                    KofiImage.Source = drawingImage;
                }
            };
            KofiImage.PointerExited += (_, _) =>
            {
                if (!TryGetResource("kofi_button_strokeDrawingImage", ThemeVariant.Default, out var drawing))
                {
                    return;
                }

                if (drawing is DrawingImage drawingImage)
                {
                    KofiImage.Source = drawingImage;
                }
            };
        };
    }
}