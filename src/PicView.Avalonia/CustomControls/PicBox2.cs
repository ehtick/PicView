using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using ImageMagick;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Core.DebugTools;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.CustomControls;

/// <summary>
/// Displays a <see cref="Bitmap"/> image.
/// </summary>
public class PicBox2 : Control
{
    /// <summary>
    /// Defines the <see cref="Source"/> property.
    /// </summary>
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<PicBox2, IImage?>(nameof(Source));

    /// <summary>
    /// Defines the <see cref="BlendMode"/> property.
    /// </summary>
    public static readonly StyledProperty<BitmapBlendingMode> BlendModeProperty =
        AvaloniaProperty.Register<PicBox2, BitmapBlendingMode>(nameof(BlendMode));

    /// <summary>
    /// Defines the <see cref="Stretch"/> property.
    /// </summary>
    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<PicBox2, Stretch>(nameof(Stretch), Stretch.Uniform);

    /// <summary>
    /// Defines the <see cref="StretchDirection"/> property.
    /// </summary>
    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<PicBox2, StretchDirection>(
            nameof(StretchDirection),
            StretchDirection.Both);

    static PicBox2()
    {
        AffectsRender<PicBox2>(SourceProperty, StretchProperty, StretchDirectionProperty, BlendModeProperty);
        AffectsMeasure<PicBox2>(SourceProperty, StretchProperty, StretchDirectionProperty);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<PicBox2>(AutomationControlType.Image);
    }

    /// <summary>
    /// Gets or sets the image that will be displayed.
    /// </summary>
    [Content]
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the blend mode for the image.
    /// </summary>
    public BitmapBlendingMode BlendMode
    {
        get => GetValue(BlendModeProperty);
        set => SetValue(BlendModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value controlling how the image will be stretched.
    /// </summary>
    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// <summary>
    /// Gets or sets a value controlling in what direction the image will be stretched.
    /// </summary>
    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    /// <inheritdoc />
    protected override bool BypassFlowDirectionPolicies => true;

    /// <summary>
    /// Renders the control.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    public sealed override void Render(DrawingContext context)
    {
        var source = Source;

        if (source == null || !(Bounds.Width > 0) || !(Bounds.Height > 0))
        {
            return;
        }

        var viewPort = new Rect(Bounds.Size);
        var sourceSize = GetImageSize(source);

        var scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
        var scaledSize = sourceSize * scale;
        var destRect = viewPort
            .CenterRect(new Rect(scaledSize))
            .Intersect(viewPort);
        var sourceRect = new Rect(sourceSize)
            .CenterRect(new Rect(destRect.Size / scale));

        var options = new RenderOptions
        {
            BitmapBlendingMode = BlendMode,
            BitmapInterpolationMode = Settings.ImageScaling.IsScalingSetToNearestNeighbor
                ? BitmapInterpolationMode.None
                : BitmapInterpolationMode.HighQuality
        };
        try
        {
            using (context.PushRenderOptions(options))
            {
                context.DrawImage(source, sourceRect, destRect);
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(PicBox), nameof(Render), e);
        }
    }

    /// <summary>
    /// Measures the control.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var source = Source;
        var result = new Size();

        if (source == null)
        {
            return result;
        }

        var size = GetImageSize(source);
        result = Stretch.CalculateSize(availableSize, size, StretchDirection);

        return result;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var source = Source;

        if (source is null)
        {
            return new Size();
        }

        var sourceSize = GetImageSize(source);
        var result = Stretch.CalculateSize(finalSize, sourceSize);
        return result;
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ImageAutomationPeer(this);
    }

    private Size GetImageSize(IImage source)
    {
        try
        {
            return source?.Size ?? GetSizeFromAlternativeSources();
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(PicBox), nameof(GetImageSize), e);
            return GetSizeFromAlternativeSources();
        }
    }

    private Size GetSizeFromAlternativeSources()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return new Size();
        }

        var tabs = core.MainWindows.ActiveWindow.CurrentValue.WindowTabs;
        var model = tabs.ActiveTab.CurrentValue.Model;
        
        if (model.FileInfo?.Exists != true)
        {
            return new Size();
        }

        if (tabs.SharedCache.TryGet(model.FileInfo, out var preloadValue))
        {
            if (preloadValue?.ImageModel != null)
            {
                return new Size(preloadValue.ImageModel.PixelWidth, preloadValue.ImageModel.PixelHeight);
            }
        }

        try
        {
            using var magickImage = new MagickImage();
            magickImage.Ping(model.FileInfo);
            return new Size(magickImage.Width, magickImage.Height);
        }
        catch (Exception exception)
        {
            DebugHelper.LogDebug(nameof(PicBox), nameof(GetSizeFromAlternativeSources), exception);
        }

        return new Size();
    }
}