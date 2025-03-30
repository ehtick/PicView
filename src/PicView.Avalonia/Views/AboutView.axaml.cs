using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using PicView.Avalonia.Update;
using PicView.Avalonia.ViewModels;
using PicView.Core.Config;
using PicView.Core.Localization;
using PicView.Core.ProcessHandling;

namespace PicView.Avalonia.Views;

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
                UpdateButton.PointerEntered += (_, _) =>
                {
                    UpdateButton.Foreground = Brushes.White;
                };
                UpdateButton.PointerExited += (_, _) =>
                {
                    UpdateButton.Foreground = new SolidColorBrush(color);
                };
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
            
            UpdateButton.Click += async (_, _) =>
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // TODO: replace with auto update service
                    ProcessHelper.OpenLink("https://PicView.org/avalonia-download");
                    return;
                }
                //Set loading and prevent user from interacting with UI
                ParentContainer.Opacity = .1;
                ParentContainer.IsHitTestVisible = false;
                SpinWaiter.IsVisible = true;
                try
                {
                   var checkIfNewUpdate = await UpdateManager.UpdateCurrentVersion(DataContext as MainViewModel);
                   UpdateButton.IsVisible = false;
                   UpdateStatus.IsVisible = true;
                   if (!checkIfNewUpdate)
                   {
                       UpdateStatus.Text = TranslationManager.Translation.NoUpdateFound;
                   }
                }
                finally
                {
                    SpinWaiter.IsVisible = false;
                    ParentContainer.IsHitTestVisible = true;
                    ParentContainer.Opacity = 1;
                }
            };
        };
    }
}