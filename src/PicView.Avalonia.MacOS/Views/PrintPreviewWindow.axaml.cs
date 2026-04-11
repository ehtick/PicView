using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.MacOS.Printing;
using PicView.Avalonia.Printing;
using PicView.Avalonia.UI;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.Localization;
using PicView.Core.Printing;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.MacOS.Views;

public partial class PrintPreviewWindow : Window, IPrintWindow
{
    private const float PreviewDpi = 96f;

    public PrintPreviewWindow()
    {
        InitializeComponent();
        GenericWindowHelper.GenericWindowInitialize(this,
            TranslationManager.Translation.Print + " - PicView");

        Loaded += (_, _) =>
        {
            var vm = DataContext as MainWindowViewModel;
            // Keep window position when resizing
            ClientSizeProperty.Changed.ToObservable()
                .Subscribe(size =>
                {
                    WindowResizing.KeepWindowSize(this, size);
                }, static result =>
                {
#if DEBUG
                    if (result is { IsFailure: true, Exception: not null })
                    {
                        DebugHelper.LogDebug(nameof(PrintPreviewWindow), nameof(ClientSizeProperty), result.Exception);
                    }
#endif
                })
                .AddTo(vm.PrintPreview.Disposables);
        };
    }

    public void Initialize()
    {
        var vm = Dispatcher.UIThread.Invoke(() => DataContext as MainWindowViewModel);
        
        vm.PrintPreview ??= new PrintPreviewViewModel();
        
        ClientSizeProperty.Changed.ToObservable()
            .ObserveOn(UIHelper2.GetFrameProvider)
            .Subscribe(_ =>
            {
                WindowFunctions2.CenterWindowOnOwnerWindow(this, Owner as Window);
            }, static result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(PrintPreviewWindow), nameof(Initialize), result.Exception);
                }
#endif
            })
            .AddTo(vm.PrintPreview.Disposables);

        var ps = vm.PrintPreview.PrintSettings.Value;

        // Printer change
        ps.PrinterName
            .AsObservable()
            .DistinctUntilChanged()
            .SubscribeAwait(async (_, _) =>
            {
                await UpdatePreviewAsync(vm.PrintPreview);
            }, static result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(PrintPreviewWindow), nameof(Initialize), result.Exception);
                }
#endif
            })
            .AddTo(vm.PrintPreview.Disposables);
        
        // Any setting change triggers preview update
        // ReSharper disable once InvokeAsExtensionMethod
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
            .SubscribeAwait(async (_, _) =>
            {
                await UpdatePreviewAsync(vm.PrintPreview);
            }, static result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(PrintPreviewWindow), nameof(Initialize), result.Exception);
                }
#endif
            })
            .AddTo(vm.PrintPreview.Disposables);
        
        vm.PrintPreview.IsProcessing.Value = false;
    }

    // -----------------------------------------------------------
    //   Preview rendering 
    // -----------------------------------------------------------

    private async ValueTask UpdatePreviewAsync(PrintPreviewViewModel vm)
    {
        var settings = vm.PrintSettings.Value;
        if (settings == null)
        {
            return;
        }

        var mainVm= Dispatcher.UIThread.Invoke(() => DataContext as MainWindowViewModel);
        
        if (mainVm?.WindowTabs.ActiveTab.CurrentValue.Model.CurrentValue.Image is not Bitmap avaloniaBmp)
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
            DebugHelper.LogDebug(nameof(PrintPreviewView), nameof(UpdatePreviewAsync), e);
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

        if (mainVm.WindowTabs.ActiveTab.CurrentValue.Model.CurrentValue.Image is not Bitmap avaloniaBmp)
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
            DebugHelper.LogDebug(nameof(PrintPreviewView), nameof(RunPrintAsync), ex);
        }
        finally
        {
            preview.IsProcessing.Value = false;
        }
    }

    private void MoveWindow(object? sender, PointerPressedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        
        if (topLevel is Window hostWindow)
        {
            hostWindow.BeginMoveDrag(e);
        }
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (vm.PrintPreview == null)
        {
            return;
        }

        var ps = vm.PrintPreview.PrintSettings.Value;
        var config = vm.PrintPreview.PrintWindowConfig;
        if (ps != null && config is { PrintProperties: not null })
        {
            var props = config.PrintProperties;
            props.PrinterName = ps.PrinterName.Value;
            props.PaperSize = ps.PaperSize.Value;
            props.Orientation = ps.Orientation.Value;
            props.ScaleMode = ps.ScaleMode.Value;
            props.ColorMode = ps.ColorMode.Value;
            props.Copies = ps.Copies.Value;
            props.MarginTop = ps.MarginTop.Value;
            props.MarginBottom = ps.MarginBottom.Value;
            props.MarginLeft = ps.MarginLeft.Value;
            props.MarginRight = ps.MarginRight.Value;

            _ = config.SaveAsync();
        }

        vm.PrintPreview.Dispose();
    }
}