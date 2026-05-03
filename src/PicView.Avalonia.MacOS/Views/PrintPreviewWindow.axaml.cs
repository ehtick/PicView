using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.MacOS.Printing;
using PicView.Avalonia.Printing;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Config;
using PicView.Core.DebugTools;
using PicView.Core.Extensions;
using PicView.Core.Localization;
using PicView.Core.Printing;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class PrintPreviewWindow : PrintWindow, IPrintWindow
{

    public PrintPreviewWindow(PrintWindowConfig config)
    {
        Config = config;
        InitializeComponent();
        GenericWindowHelper.GenericWindowInitialize(this, StringExtensions.CombineWithAppName(TranslationManager.Translation.Print));
        
        SetWindowSize();
    }

    public void Initialize()
    {
        var vm = Dispatcher.UIThread.Invoke(() => DataContext as MainWindowViewModel);
        
        vm.PrintPreview ??= new PrintPreviewViewModel();
        
        vm.PrintPreview.IsProcessing.Value = false;
    }

    // -----------------------------------------------------------
    //   Preview rendering 
    // -----------------------------------------------------------

    public async ValueTask UpdatePreviewAsync(PrintPreviewViewModel vm)
    {
        var settings = vm.PrintSettings.Value;
        if (settings == null)
        {
            return;
        }

        var mainVm= Dispatcher.UIThread.Invoke(() => DataContext as MainWindowViewModel);
        
        if (mainVm?.WindowTabs.ActiveTab.CurrentValue.Model.Image is not Bitmap avaloniaBmp)
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
        
        var paper = MacPrintEngine.ResolvePaper(settings.PaperSize.Value, settings.Orientation.Value);
        PrintLayout? printLayout = null;

        try
        {
            printLayout = PrintCore.ComputeLayout(
                paper.WidthMm,
                paper.HeightMm,
                settings,
                avaloniaBmp?.PixelSize.Width ?? 0,
                avaloniaBmp?.PixelSize.Height ?? 0,
                PreviewDpi);
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(PrintPreviewWindow), nameof(UpdatePreviewAsync), e);
        }

        if (printLayout == null)
        {
            return;
        }

        var layout = printLayout.Value;
        
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

        vm.PreviewImage.Value = rtb;
        vm.PageWidth.Value = layout.PageWidthPx;
        vm.PageHeight.Value = layout.PageHeightPx;
        
        vm.IsProcessing.Value = false;
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
        var settings = preview.PrintSettings.Value;

        if (DataContext is not MainWindowViewModel mainVm)
        {
            return;
        }

        if (mainVm.WindowTabs.ActiveTab.CurrentValue.Model.Image is not Bitmap avaloniaBmp)
        {
            return;
        }

        preview.IsProcessing.Value = true;

        try
        {
            await MacPrintEngine.RunPrintJob(settings, avaloniaBmp);
            await Dispatcher.UIThread.InvokeAsync(Close);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(PrintPreviewWindow), nameof(RunPrintAsync), ex);
        }
        finally
        {
            preview.IsProcessing.Value = false;
        }
    }
}