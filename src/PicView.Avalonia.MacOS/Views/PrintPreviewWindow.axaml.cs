using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.MacOS.Printing;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.Printing;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.Main;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.Localization;
using PicView.Core.Printing;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class PrintPreviewWindow : Window
{
    private const float PreviewDpi = 96f;

    public PrintPreviewWindow()
    {
        InitializeComponent();
        GenericWindowHelper.GenericWindowInitialize(this,
            TranslationManager.Translation.Print + " - PicView");

        Loaded += (_, _) =>
        {
            // Keep window position when resizing
            ClientSizeProperty.Changed.ToObservable()
                .Subscribe(size =>
                {
                    WindowResizing.HandleWindowResize(this, size);
                });
        };
    }

    public void Initialize()
    {
        var vm = Dispatcher.UIThread.Invoke(() => DataContext as MainViewModel);
        
        if (vm.PrintPreview == null)
        {
            return;
        }

        // Initial render
        UpdatePreview(vm.PrintPreview);
        
        WindowFunctions.CenterWindowOnScreen(this);

        var ps = vm.PrintPreview.PrintSettings.Value;

        // Printer change
        ps.PrinterName
            .AsObservable()
            .DistinctUntilChanged()
            .Subscribe(_ => UpdatePreview(vm.PrintPreview))
            .AddTo(vm.PrintPreview.Disposables);

        // Any setting change triggers preview update
        Observable.CombineLatest(
                ps.Orientation.AsObservable(),
                ps.MarginTop.AsObservable(),
                ps.MarginBottom.AsObservable(),
                ps.MarginLeft.AsObservable(),
                ps.MarginRight.AsObservable(),
                ps.ScaleMode.AsObservable(),
                ps.ColorMode.AsObservable(),
                ps.PaperSize.AsObservable(),
                (orientation, top, bottom, left, right, scale, color, paper)
                    => (orientation, top, bottom, left, right, scale, color, paper))
            .ThrottleLast(TimeSpan.FromMilliseconds(100))
            .Subscribe(_ => UpdatePreview(vm.PrintPreview))
            .AddTo(vm.PrintPreview.Disposables);
    }

    // -----------------------------------------------------------
    //   Preview rendering (uses same layout as print)
    // -----------------------------------------------------------

    private void UpdatePreview(PrintPreviewViewModel vm)
    {
        var settings = vm.PrintSettings.Value;
        var mainVm = Dispatcher.UIThread.Invoke(() => DataContext as MainViewModel);
        var avaloniaBmp = mainVm.PicViewer?.ImageSource.Value as Bitmap;
        
        
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
        
        PrintLayout? printLayout;
        var paper = MacPrintEngine.ResolvePaper(settings.PaperSize.Value, settings.Orientation.Value);
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
            // Fix null exception by loading from cache
            DebugHelper.LogDebug(nameof(PrintPreviewView), nameof(UpdatePreview), e);
            var cached = NavigationManager.GetPreLoadValueAsync(mainVm.PicViewer.FileInfo.Value).Result;
            printLayout = PrintCore.ComputeLayout(
                paper.WidthMm,
                paper.HeightMm,
                settings,
                cached.ImageModel.PixelWidth,
                cached.ImageModel.PixelHeight,
                PreviewDpi);
            avaloniaBmp = cached.ImageModel.Image as Bitmap;
        }
        var layout = printLayout.Value;
        
        var rtb = new RenderTargetBitmap(
            new PixelSize((int)layout.PageWidthPx, (int)layout.PageHeightPx));
        
        Dispatcher.UIThread.Invoke(() =>
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
    }

    // -----------------------------------------------------------
    //   Print command
    // -----------------------------------------------------------

    public async Task RunPrintAsync(MainViewModel vm)
    {
        if (vm.PrintPreview == null) return;
        var preview = vm.PrintPreview;
        var settings = preview.PrintSettings.Value!;
        if (DataContext is not MainViewModel mainVm) return;

        if (mainVm.PicViewer?.ImageSource.Value is not Bitmap avaloniaBmp)
            return;

        preview.IsProcessing.Value = true;

        try
        {
            await Task.Run(() => MacPrintEngine.RunPrintJob(settings, avaloniaBmp));
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

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is Window hostWindow)
        {
            hostWindow.BeginMoveDrag(e);
        }
    }
}