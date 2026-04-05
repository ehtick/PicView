using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using PicView.Avalonia.Animations;
using PicView.Avalonia.UI;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.CustomControls;

public class GalleryAnimationControl : UserControl
{
    #region Fields and Properties

    private const int ZeroSize = 0;
    private const int BorderTopAndBottomThickness = 2;

    private TabViewModel? TabViewModel => DataContext as TabViewModel;
    private static CoreViewModel? CoreViewModel => Application.Current?.DataContext as CoreViewModel;
    private Control? ParentControl => Parent as Control;

    private CompositeDisposable? _disposables;
    private NavigateAbleItemsViewer? _viewer;
    private WrapPanel? _itemsPanel;

    /// Tracks the previous mode to determine the animation transition
    private GalleryMode2 _previousMode = GalleryMode2.Closed;

    public static readonly StyledProperty<GalleryMode2> GalleryModeProperty =
        AvaloniaProperty.Register<GalleryAnimationControl, GalleryMode2>(nameof(GalleryMode));

    public GalleryMode2 GalleryMode
    {
        get => GetValue(GalleryModeProperty);
        set => SetValue(GalleryModeProperty, value);
    }

    private static Thickness GetDockedMargin => new(0);
    private static Thickness GetExpandedMargin => new(15, 40, 15, 5);
    private static double GetDockedSize => Settings.Gallery.BottomGalleryItemSize + BorderTopAndBottomThickness + SizeDefaults.ScrollbarSize;

    #endregion

    #region Constructors & Setup

    protected GalleryAnimationControl()
    {
        Loaded += OnControlLoaded;
    }

    private void OnControlLoaded(object? sender, RoutedEventArgs e)
    {
        _viewer = this.FindControl<NavigateAbleItemsViewer>("GalleryItemsControl");

        if (_viewer?.ItemsPanelRoot is WrapPanel panel)
        {
            _itemsPanel = panel;
        }
        else
        {
            DebugHelper.LogDebug(nameof(GalleryAnimationControl), nameof(OnControlLoaded), "Could not find ItemsControl.ItemsPanelRoot");
        }

        if (Settings.Gallery.IsGalleryDocked)
        {
            SetDockedLayout(Settings.Gallery.DockPosition);
            _previousMode = GalleryMode2.Docked;
        }
        else
        {
            IsVisible = false; // Don't take up space initially
        }

        if (TabViewModel == null) return;

        SetupSubscriptions();

        if (ParentControl != null)
        {
            ParentControl.SizeChanged += ParentSizeChanged;
        }
    }

    private void SetupSubscriptions()
    {
        _disposables = new CompositeDisposable();
        Debug.Assert(Settings.Gallery is not null);

        // Change layout corresponding to DockPositions
        Observable.EveryValueChanged(Settings.Gallery, gallery => gallery.DockPosition, UIHelper2.GetFrameProvider)
            .Skip(1)
            .Subscribe(SetDockedLayout, LogError(nameof(SetDockedLayout)))
            .AddTo(_disposables);

        if (CoreViewModel == null) return;

        // Update expanded item sizes
        Observable.EveryValueChanged(CoreViewModel.GallerySettings, gallery => gallery.ExpandedGalleryItemSize.CurrentValue, UIHelper2.GetFrameProvider)
            .Skip(1)
            .Subscribe(UpdateExpandedItemHeight, LogError(nameof(UpdateExpandedItemHeight)))
            .AddTo(_disposables);

        // Update docked item sizes
        Observable.EveryValueChanged(CoreViewModel.GallerySettings, gallery => gallery.DockedGalleryItemSize.CurrentValue, UIHelper2.GetFrameProvider)
            .Skip(1)
            .Subscribe(UpdateDockedItemHeight, LogError(nameof(UpdateDockedItemHeight)))
            .AddTo(_disposables);
    }

    // Properly handles R3's Action<Result> overload while preserving exact method context
    private static Action<Result> LogError(string methodName) => result =>
    {
#if DEBUG
        if (result is { IsFailure: true, Exception: not null })
        {
            DebugHelper.LogDebug(nameof(GalleryAnimationControl), methodName, result.Exception);
        }
#endif
    };

    #endregion

    #region Logic & Layout

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == GalleryModeProperty && change.NewValue is GalleryMode2 mode)
        {
            Dispatcher.UIThread.InvokeAsync(() => OnGalleryModeChanged(mode));
        }
    }

    private async ValueTask OnGalleryModeChanged(GalleryMode2 newMode)
    {
        if (TabViewModel == null) return;

        try
        {
            var oldMode = _previousMode;
            _previousMode = newMode;
            IsVisible = true;

            switch (oldMode, newMode)
            {
                case (GalleryMode2.Closed, GalleryMode2.Docked): await ClosedToDocked(); break;
                case (GalleryMode2.Closed, GalleryMode2.Expanded): await ClosedToExpanded(); break;
                case (GalleryMode2.Docked, GalleryMode2.Expanded): await DockedToExpanded(); break;
                case (GalleryMode2.Docked, GalleryMode2.Closed): await DockedToClosed(); break;
                case (GalleryMode2.Expanded, GalleryMode2.Docked): await ExpandedToDocked(); break;
                case (GalleryMode2.Expanded, GalleryMode2.Closed): await ExpandedToClosed(); break;
                default: UpdateLayoutForCurrentState(); break;
            }
        }
        catch (Exception ex)
        {
            UpdateLayoutForCurrentState(); // Fallback
            DebugHelper.LogDebug(nameof(GalleryAnimationControl), nameof(OnGalleryModeChanged), ex);
        }
    }

    private void UpdateLayoutForCurrentState()
    {
        var dock = Settings.Gallery.DockPosition;
        IsVisible = GalleryMode != GalleryMode2.Closed;

        switch (GalleryMode)
        {
            case GalleryMode2.Closed:
                Width = Height = ZeroSize;
                break;
            case GalleryMode2.Expanded:
                SetExpandedLayout(dock);
                break;
            case GalleryMode2.Docked:
            default:
                SetDockedLayout(dock);
                break;
        }
    }

    #endregion

    #region Expanded Configuration

    private void SetExpandedLayout(GalleryDockPosition dock)
    {
        SetExpandedLayoutCore(dock);
        SetExpandedThumbs();
    }

    private void SetExpandedLayoutCore(GalleryDockPosition dock)
    {
        if (ParentControl != null)
        {
            if (IsHorizontalDock(dock))
            {
                Width = double.NaN;
                Height = ParentControl.Bounds.Height;
            }
            else
            {
                Width = ParentControl.Bounds.Width;
                Height = double.NaN;
            }
        }

        _itemsPanel?.Orientation = Orientation.Vertical;
        TabViewModel?.Gallery.ItemSpacing.Value = Settings.Gallery.ItemSpacing;
        _viewer?.SetHorizontalScrolling();
    }

    private void SetExpandedThumbs()
    {
        ApplyThumbSettings(
            Settings.Gallery.ExpandedGalleryItemSize,
            Settings.Gallery.FullGalleryStretchMode,
            GetExpandedMargin);
    }

    private void UpdateExpandedItemHeight(double itemHeight)
    {
        if (CoreViewModel == null || TabViewModel?.Gallery.IsGalleryExpanded.CurrentValue != true) return;
        CoreViewModel.GallerySettings.ItemHeight.Value = itemHeight;
    }

    #endregion

    #region Docked Configuration

    private void SetDockedLayout(GalleryDockPosition dock)
    {
        SetDockLayoutCore(dock);
        SetDockedThumbs(dock);
    }

    private void SetDockLayoutCore(GalleryDockPosition dock)
    {
        var size = GetDockedSize;

        if (IsHorizontalDock(dock))
        {
            Width = double.NaN;
            Height = size;

            _itemsPanel?.Orientation = Orientation.Horizontal;
            BorderThickness = dock == GalleryDockPosition.Top ? new Thickness(0, 0, 0, 1) : new Thickness(0, 1, 0, 0);

            _viewer?.SetHorizontalScrolling();
        }
        else // Left or Right
        {
            Width = size;
            Height = double.NaN;

            _itemsPanel?.Orientation = Orientation.Vertical;
            BorderThickness = dock == GalleryDockPosition.Right ? new Thickness(1, 0, 0, 0) : new Thickness(0, 0, 1, 0);

            _viewer?.SetVerticalScrolling();
        }
    }

    private void SetDockedThumbs(GalleryDockPosition dock)
    {
        if (CoreViewModel == null) return;
        var gallerySettings = CoreViewModel.GallerySettings;

        // Reset all dock flags
        gallerySettings.IsTopDocked.Value = gallerySettings.IsBottomDocked.Value =
        gallerySettings.IsLeftDocked.Value = gallerySettings.IsRightDocked.Value = false;

        switch (dock)
        {
            case GalleryDockPosition.Top:
                DockPanel.SetDock(this, Dock.Top);
                gallerySettings.IsTopDocked.Value = true;
                break;
            case GalleryDockPosition.Left:
                DockPanel.SetDock(this, Dock.Left);
                gallerySettings.IsLeftDocked.Value = true;
                break;
            case GalleryDockPosition.Right:
                DockPanel.SetDock(this, Dock.Right);
                gallerySettings.IsRightDocked.Value = true;
                break;
            case GalleryDockPosition.Bottom:
                DockPanel.SetDock(this, Dock.Bottom);
                gallerySettings.IsBottomDocked.Value = true;
                break;
            case GalleryDockPosition.Closed:
            default:
                if (Settings.Gallery.IsGalleryDocked) goto case GalleryDockPosition.Bottom;
                IsVisible = false;
                return;
        }

        IsVisible = true;
        ApplyThumbSettings(
            Settings.Gallery.BottomGalleryItemSize,
            Settings.Gallery.BottomGalleryStretchMode,
            GetDockedMargin,
            spacing: 2);
    }

    private void UpdateDockedItemHeight(double itemHeight)
    {
        if (CoreViewModel == null || TabViewModel?.Gallery.IsGalleryExpanded.CurrentValue == true) return;
        CoreViewModel.GallerySettings.ItemHeight.Value = itemHeight;

        // Resize control bounds
        var size = itemHeight + BorderTopAndBottomThickness + SizeDefaults.ScrollbarSize;
        if (IsHorizontalDock(Settings.Gallery.DockPosition))
        {
            Width = double.NaN;
            Height = size;
        }
        else
        {
            Width = size;
            Height = double.NaN;
        }
    }

    #endregion

    #region Helpers

    private bool IsHorizontalDock(GalleryDockPosition dock) => dock is GalleryDockPosition.Top or GalleryDockPosition.Bottom;

    private void ApplyThumbSettings(double size, string modeStr, Thickness margin, double spacing = 0)
    {
        if (CoreViewModel == null) return;

        var settings = CoreViewModel.GallerySettings;
        settings.ItemHeight.Value = size;

        var (stretch, isSquare) = ParseStretchMode(modeStr);
        settings.GalleryStretch.Value = stretch;
        settings.ItemWidth.Value = isSquare ? size : double.NaN;

        if (spacing > 0)
            TabViewModel?.Gallery.ItemSpacing.Value = spacing;

        if (_itemsPanel != null)
            _itemsPanel.Margin = margin;
    }

    private (string Stretch, bool IsSquare) ParseStretchMode(string mode)
    {
        if (string.Equals(mode, "Square", StringComparison.OrdinalIgnoreCase)) return ("Uniform", true);
        if (string.Equals(mode, "FillSquare", StringComparison.OrdinalIgnoreCase)) return ("Fill", true);
        return (mode, false);
    }

    #endregion

    #region Animations

    private async Task ClosedToDocked()
    {
        if (!Settings.Gallery.IsGalleryDocked) return;

        var dock = Settings.Gallery.DockPosition;
        IsVisible = true;
        SetDockLayoutCore(dock);

        var targetSize = GetDockedSize;

        if (IsHorizontalDock(dock))
        {
            Height = ZeroSize;
            var anim = AnimationsHelper.HeightAnimation(ZeroSize, targetSize, GalleryDefaults.VeryFastAnimationSpeed);
            await anim.RunAsync(this);
            Height = targetSize;
        }
        else
        {
            Width = ZeroSize;
            var anim = AnimationsHelper.WidthAnimation(ZeroSize, targetSize, GalleryDefaults.VeryFastAnimationSpeed);
            await anim.RunAsync(this);
            Width = targetSize;
        }

        SetDockedThumbs(dock);
        _viewer?.ScrollToCenterOfCurrentItem();
    }

    private async Task DockedToClosed()
    {
        var isHorizontal = IsHorizontalDock(Settings.Gallery.DockPosition);
        var currentSize = GetDockedSize;

        if (isHorizontal)
        {
            await AnimationsHelper.HeightAnimation(currentSize, ZeroSize, GalleryDefaults.MediumAnimationSpeed).RunAsync(this);
            Height = ZeroSize;
        }
        else
        {
            await AnimationsHelper.WidthAnimation(currentSize, ZeroSize, GalleryDefaults.MediumAnimationSpeed).RunAsync(this);
            Width = ZeroSize;
        }

        IsVisible = false;
    }

    private async Task DockedToExpanded()
    {
        if (TabViewModel == null || ParentControl == null) return;

        var dock = Settings.Gallery.DockPosition;
        SetExpandedLayoutCore(dock);

        var startSize = GetDockedSize;

        if (IsHorizontalDock(dock))
        {
            var targetHeight = ParentControl.Bounds.Height;
            await AnimationsHelper.HeightAnimation(startSize, targetHeight, GalleryDefaults.MediumAnimationSpeed).RunAsync(this);
            Height = targetHeight;
        }
        else
        {
            var targetWidth = ParentControl.Bounds.Width;
            await AnimationsHelper.WidthAnimation(startSize, targetWidth, GalleryDefaults.MediumAnimationSpeed).RunAsync(this);
            Width = targetWidth;
        }

        SetExpandedThumbs();
        _viewer?.ScrollToCenterOfCurrentItem();
    }

    private async Task ExpandedToDocked()
    {
        if (TabViewModel == null || ParentControl == null) return;

        var dock = Settings.Gallery.DockPosition;

        if (IsHorizontalDock(dock))
        {
            var startHeight = ParentControl.Bounds.Height;
            var targetHeight = GetDockedSize;
            Height = startHeight;
            await AnimationsHelper.HeightAnimation(startHeight, targetHeight, GalleryDefaults.SlowAnimationSpeed).RunAsync(this);
            Height = targetHeight;
        }
        else
        {
            var startWidth = ParentControl.Bounds.Width;
            var targetWidth = Settings.Gallery.BottomGalleryItemSize;
            Width = startWidth;
            await AnimationsHelper.WidthAnimation(startWidth, targetWidth, GalleryDefaults.SlowAnimationSpeed).RunAsync(this);
            Width = targetWidth;
        }

        SetDockedLayout(dock);
        _viewer?.ScrollToCenterOfCurrentItem();
    }

    private async Task ClosedToExpanded()
    {
        if (ParentControl == null) return;

        IsVisible = true;
        Width = Height = ZeroSize;

        var targetHeight = ParentControl.Bounds.Height;
        var targetWidth = ParentControl.Bounds.Width;

        await Task.WhenAll(
            AnimationsHelper.WidthAnimation(ZeroSize, targetWidth, GalleryDefaults.MediumAnimationSpeed).RunAsync(this),
            AnimationsHelper.HeightAnimation(ZeroSize, targetHeight, GalleryDefaults.MediumAnimationSpeed).RunAsync(this)
        );

        _itemsPanel?.Orientation = Orientation.Vertical;
        TabViewModel?.Gallery.ItemSpacing.Value = Settings.Gallery.ItemSpacing;

        SetExpandedThumbs();
        _viewer?.ScrollToCenterOfCurrentItem();
    }

    private async Task ExpandedToClosed()
    {
        await Task.WhenAll(
            AnimationsHelper.WidthAnimation(Bounds.Width, ZeroSize, GalleryDefaults.FastAnimationSpeed).RunAsync(this),
            AnimationsHelper.HeightAnimation(Bounds.Height, ZeroSize, GalleryDefaults.FastAnimationSpeed).RunAsync(this)
        );

        IsVisible = false;
    }

    private void ParentSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // Keep the layout correct when the view is resized
        if (GalleryMode == GalleryMode2.Expanded)
        {
            UpdateLayoutForCurrentState();
        }
    }

    #endregion

    #region Cleanup

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        if (ParentControl != null)
        {
            ParentControl.SizeChanged -= ParentSizeChanged;
        }

        Loaded -= OnControlLoaded;
        _disposables?.Dispose();
    }

    #endregion
}