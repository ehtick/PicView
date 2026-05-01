using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Printing;
using PicView.Avalonia.UI;
using PicView.Avalonia.Win32.Printing;
using PicView.Core.DebugTools;
using PicView.Core.Localization;
using PicView.Core.Printing;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Win32.Views;

public partial class PrintPreviewWindow : PrintWindow, IPrintWindow
{
    public PrintPreviewWindow()
    {
        InitializeComponent();

        GenericWindowHelper.GenericWindowInitialize(this, TranslationManager.Translation.Print + " - PicView");
        ThemeUpdates();
    }

    private void ThemeUpdates()
    {
        if (!Settings.Theme.Dark)
        {
            PrintPreviewView.Background = UIHelper.GetMenuBackgroundColor();
        }

        // Glass/Transparent theme support
        if (!Settings.Theme.GlassTheme)
        {
            return;
        }

        PrintPreviewView.Background = Brushes.Transparent;
        IconBorder.Background = Brushes.Transparent;
        IconBorder.BorderThickness = new Thickness(0);
        MinimizeButton.Background = Brushes.Transparent;
        MinimizeButton.BorderThickness = new Thickness(0);
        CloseButton.Background = Brushes.Transparent;
        CloseButton.BorderThickness = new Thickness(0);
        BorderRectangle.Height = 0;
        TitleText.Background = Brushes.Transparent;
        var brush = UIHelper.GetBrush("SecondaryTextColor");
        TitleText.Foreground = brush;
        MinimizeButton.Foreground = brush;
        CloseButton.Foreground = brush;
    }

    private void Close(object? sender, RoutedEventArgs e) => Close();
    private void Minimize(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;


    // -----------------------------------------------------------
    //   Preview rendering (uses same layout as print)
    // -----------------------------------------------------------

    public async ValueTask UpdatePreviewAsync(PrintPreviewViewModel vm)
    {
        try
        {
            var settings = vm.PrintSettings.Value;
            if (settings == null)
            {
                return;
            }

            var mainVm = Dispatcher.UIThread.Invoke(() => DataContext as MainWindowViewModel);

            if (mainVm.WindowTabs.ActiveTab.CurrentValue.Image.CurrentValue is not Bitmap avaloniaBmp)
            {
                return;
            }

            // Grayscale if needed
            if (settings.ColorMode.Value == (int)ColorModes.BlackAndWhite)
            {
                vm.GrayCache ??= PrintCore.ToGrayScale(avaloniaBmp, PreviewDpi);
                avaloniaBmp = (Bitmap)vm.GrayCache;
            }
            else
            {
                vm.GrayCache = null;
            }

            // Resolve paper
            var paperInfo = PrintEngine.ResolvePaper(settings.PrinterName.Value, settings.PaperSize.Value,
                settings.Orientation.Value == (int)Orientations.Landscape);
            if (paperInfo is null)
            {
                return;
            }

            // Compute layout once
            var layout = PrintCore.ComputeLayout(
                paperInfo.WidthMm, paperInfo.HeightMm, settings,
                avaloniaBmp.PixelSize.Width,
                avaloniaBmp.PixelSize.Height,
                PreviewDpi);

            var rtb = new RenderTargetBitmap(
                new PixelSize((int)layout.PageWidthPx, (int)layout.PageHeightPx));

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                using var ctx = rtb.CreateDrawingContext();
                ctx.FillRectangle(Brushes.White, new Rect(0, 0, layout.PageWidthPx, layout.PageHeightPx));
                ctx.DrawRectangle(null, new Pen(Brushes.LightGray),
                    new Rect(0.5, 0.5, layout.PageWidthPx - 1, layout.PageHeightPx - 1));

                var destRect = new Rect(layout.DrawX, layout.DrawY, layout.DrawWidth, layout.DrawHeight);

                var dashPen = new Pen(Brushes.Gray, 2)
                {
                    DashStyle = new DashStyle([4, 2], 0)
                };
                ctx.DrawRectangle(null, dashPen,
                    new Rect(layout.ContentX, layout.ContentY, layout.ContentWidth, layout.ContentHeight));

                ctx.DrawImage(avaloniaBmp,
                    new Rect(0, 0, avaloniaBmp.PixelSize.Width, avaloniaBmp.PixelSize.Height),
                    destRect);
            });

            if (vm.PreviewImage.Value is Bitmap old)
            {
                old.Dispose();
            }

            vm.PreviewImage.Value = rtb;
            vm.PageWidth.Value = layout.PageWidthPx;
            vm.PageHeight.Value = layout.PageHeightPx;
            
            vm.IsProcessing.Value = false;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(PrintPreviewView), nameof(UpdatePreviewAsync), ex);
        }
    }


    // -----------------------------------------------------------
    //   Print command
    // -----------------------------------------------------------

    public async ValueTask RunPrintAsync(MainWindowViewModel vm)
    {
        if (vm.PrintPreview == null)
        {
            return;
        }

        var preview = vm.PrintPreview;
        var settings = preview.PrintSettings.Value!;
        if (DataContext is not MainWindowViewModel mainVm)
        {
            return;
        }

        if (mainVm.WindowTabs.ActiveTab.CurrentValue.Image.CurrentValue is not Bitmap avaloniaBmp)
        {
            return;
        }

        preview.IsProcessing.Value = true;

        try
        {
            await PrintEngine.RunPrintJob(settings, avaloniaBmp);
            await Dispatcher.UIThread.InvokeAsync(Close);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(PrintPreviewView), nameof(RunPrintAsync), ex);
        }
        finally
        {
            preview.IsProcessing.Value = false;
        }
    }
}