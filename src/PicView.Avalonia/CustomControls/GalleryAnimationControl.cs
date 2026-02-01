using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using PicView.Avalonia.Animations;
using PicView.Core.Config;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.CustomControls;

public class GalleryAnimationControl : UserControl
{
    #region Cleanup

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        if (Parent is Control parent)
        {
            parent.SizeChanged -= ParentSizeChanged;
        }

        Loaded -= OnControlLoaded;
        _disposables?.Dispose();
    }

    #endregion

    #region Fields and Properties

    private const double FastAnimationSpeed = 0.35;
    private const double MediumAnimationSpeed = 0.5;
    private const double SlowAnimationSpeed = 0.6;
    private const int ZeroSize = 0;

    private TabViewModel? ViewModel => DataContext as TabViewModel;

    private CompositeDisposable? _disposables;
    
    private AutoScrollViewer? _scrollViewer;
    private WrapPanel? _itemsPanel;
    
    private const int BorderTopAndBottomThickness = 2;

    // Tracks the previous mode to determine the animation transition
    private GalleryMode2 _previousMode = GalleryMode2.Closed;

    public static readonly AvaloniaProperty<GalleryMode?> GalleryModeProperty =
        AvaloniaProperty.Register<GalleryAnimationControl, GalleryMode?>(nameof(GalleryMode));

    public GalleryMode GalleryMode
    {
        get => GetValue(GalleryModeProperty) as GalleryMode? ?? GalleryMode.Closed;
        set => SetValue(GalleryModeProperty, value);
    }

    #endregion

    #region Constructors

    protected GalleryAnimationControl()
    {
        Loaded += OnControlLoaded;
    }

    private void OnControlLoaded(object? sender, RoutedEventArgs e)
    {
        _disposables = new CompositeDisposable();

        _scrollViewer = this.FindControl<AutoScrollViewer>("GalleryScrollViewer");
        if (_scrollViewer.Content is ItemsControl itemsControl)
        {
            _itemsPanel = itemsControl.ItemsPanelRoot as WrapPanel;
        }
        else
        {
            DebugHelper.LogDebug(nameof(GalleryAnimationControl), nameof(OnControlLoaded), "Could not find ItemsControl.ItemsPanelRoot");
        }

        if (ViewModel == null)
        {
            return;
        }

        // Subscribe to Mode changes
        ViewModel.Gallery.GalleryMode
            .SubscribeAwait(async (mode, _) => await OnGalleryModeChanged(mode))
            .AddTo(_disposables);
        
        Debug.Assert(Settings.Gallery is not null);
        // Also subscribe to DockPosition, as changing it while Docked might need layout updates
        Observable.EveryValueChanged(Settings.Gallery, gallery =>  gallery.DockPosition)
            .Skip(1)
            .Subscribe(SetDockedLayout)
            .AddTo(_disposables);

        if (Parent is Control parent)
        {
            parent.SizeChanged += ParentSizeChanged;
        }
    }

    #endregion

    #region Logic
    
    private static double GetDockedHeight =>
        Settings.Gallery.BottomGalleryItemSize + BorderTopAndBottomThickness + SizeDefaults.ScrollbarSize;

    private async ValueTask OnGalleryModeChanged(GalleryMode2 newMode)
    {
        if (ViewModel == null)
        {
            return;
        }

        try
        {
            var oldMode = _previousMode;
            _previousMode = newMode;

            switch (oldMode)
            {
                case GalleryMode2.Closed when newMode == GalleryMode2.Docked:
                    await ClosedToDocked();
                    break;
                case GalleryMode2.Closed when newMode == GalleryMode2.Expanded:
                    await ClosedToExpanded();
                    break;
                case GalleryMode2.Docked when newMode == GalleryMode2.Expanded:
                    await DockedToExpanded();
                    break;
                case GalleryMode2.Docked when newMode == GalleryMode2.Closed:
                    await DockedToClosed();
                    break;
                case GalleryMode2.Expanded when newMode == GalleryMode2.Docked:
                    await ExpandedToDocked();
                    break;
                case GalleryMode2.Expanded when newMode == GalleryMode2.Closed:
                    await ExpandedToClosed();
                    break;
                default:
                    // Initial state or same state, just apply layout
                    UpdateLayoutForCurrentState();
                    break;
            }
        }
        catch (Exception ex)
        {
            // Fallback
             UpdateLayoutForCurrentState();
             DebugHelper.LogDebug(nameof(GalleryAnimationControl), nameof(OnGalleryModeChanged), ex);
        }
    }
    
    private void UpdateLayoutForCurrentState()
    {
        var mode = ViewModel.Gallery.GalleryMode.Value;
        var dock = Settings.Gallery.DockPosition;

        IsVisible = mode != GalleryMode2.Closed;

        switch (mode)
        {
            case GalleryMode2.Closed:
                Width = ZeroSize;
                Height = ZeroSize;
                return;
            case GalleryMode2.Expanded:
                SetExpandedLayout(dock);
                break;
            case GalleryMode2.Docked:
            default:
                SetDockedLayout(dock);
                break;
        }
    }
    
    private void SetExpandedLayout(GalleryDockPosition dock)
    {
        SetExpandedLayoutCore(dock);
        SetExpandedThumbs();
    }
    
    private void SetExpandedLayoutCore(GalleryDockPosition dock)
    {
        if (Parent is Control parent)
        {
            // Full size relative to parent
            if (dock is GalleryDockPosition.Top or GalleryDockPosition.Bottom)
            {
                Width = double.NaN; 
                Height = parent.Bounds.Height;
            }
            else
            {
                Width = parent.Bounds.Width;
                Height = double.NaN; 
            }
        }
        
        // Configure ScrollViewer
        if (_scrollViewer != null)
        {
            _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }
        _itemsPanel?.Orientation = Orientation.Vertical;
        ViewModel.Gallery.ItemSpacing.Value = Settings.Gallery.ItemSpacing;
    }

    private void SetExpandedThumbs()
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        var gallerySettings = core.GallerySettings;

        // Set the height based on Expanded configuration
        gallerySettings.ItemHeight.Value = Settings.Gallery.ExpandedGalleryItemSize;

        string stretchValue;
        var isSquare = false;
        var mode = Settings.Gallery.FullGalleryStretchMode;

        // Determine stretch mode and squareness
        if (string.Equals(mode, "Square", StringComparison.OrdinalIgnoreCase))
        {
            stretchValue = "Uniform";
            isSquare = true;
        }
        else if (string.Equals(mode, "FillSquare", StringComparison.OrdinalIgnoreCase))
        {
            stretchValue = "Fill";
            isSquare = true;
        }
        else
        {
            stretchValue = mode;
        }

        // Apply final settings
        gallerySettings.GalleryStretch.Value = stretchValue;
        gallerySettings.ItemWidth.Value = isSquare ? gallerySettings.ItemHeight.CurrentValue : double.NaN;
    }

    private void SetDockedLayout(GalleryDockPosition dock)
    {
        SetDockLayoutCore(dock);
        
        SetDockedThumbs(dock);
    }

    private void SetDockLayoutCore(GalleryDockPosition dock)
    {
        var size = GetDockedHeight;

        if (dock is GalleryDockPosition.Top or GalleryDockPosition.Bottom)
        {
            Width = double.NaN;
            Height = size;
            
            _itemsPanel?.Orientation = Orientation.Horizontal;
            ViewModel.Gallery.GalleryVerticalAlignment.Value = dock is GalleryDockPosition.Top
                ? VerticalAlignment.Top : VerticalAlignment.Bottom;

            if (_scrollViewer == null)
            {
                return;
            }

            _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }
        else // Left or Right
        {
            Width = size;
            Height = double.NaN;
            
            _itemsPanel?.Orientation = Orientation.Vertical;
            ViewModel.Gallery.GalleryVerticalAlignment.Value = VerticalAlignment.Stretch;

            if (_scrollViewer == null)
            {
                return;
            }

            _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }
    }

    private void SetDockedThumbs(GalleryDockPosition dock)
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        var gallerySettings = core.GallerySettings;
        switch (dock)
        {
            case GalleryDockPosition.Top:
                DockPanel.SetDock(this, Dock.Top);
                gallerySettings.IsTopDocked.Value = true;
                gallerySettings.IsBottomDocked.Value = false;
                gallerySettings.IsLeftDocked.Value = false;
                gallerySettings.IsRightDocked.Value = false;
                break;
            case GalleryDockPosition.Left:
                DockPanel.SetDock(this, Dock.Left);
                gallerySettings.IsTopDocked.Value = false;
                gallerySettings.IsBottomDocked.Value = false;
                gallerySettings.IsLeftDocked.Value = true;
                gallerySettings.IsRightDocked.Value = false;
                break;
            case GalleryDockPosition.Right:
                DockPanel.SetDock(this, Dock.Right);
                gallerySettings.IsTopDocked.Value = false;
                gallerySettings.IsBottomDocked.Value = false;
                gallerySettings.IsLeftDocked.Value = false;
                gallerySettings.IsRightDocked.Value = true;
                break;
            case GalleryDockPosition.Bottom:
                DockPanel.SetDock(this, Dock.Bottom);
                gallerySettings.IsTopDocked.Value = false;
                gallerySettings.IsBottomDocked.Value = true;
                gallerySettings.IsLeftDocked.Value = false;
                gallerySettings.IsRightDocked.Value = false;
                break;
            case GalleryDockPosition.Closed:
            default:
                if (Settings.Gallery.IsGalleryDocked)
                {
                    goto case GalleryDockPosition.Bottom;
                }
                gallerySettings.IsLeftDocked.Value =
                    gallerySettings.IsRightDocked.Value =
                        gallerySettings.IsBottomDocked.Value =
                            gallerySettings.IsTopDocked.Value = false;
                IsVisible = false;
                return;
        }

        gallerySettings.ItemHeight.Value = Settings.Gallery.BottomGalleryItemSize;
        string stretchValue;
        var isSquare = false;
        var mode = Settings.Gallery.BottomGalleryStretchMode;

        if (string.Equals(mode, "Square", StringComparison.OrdinalIgnoreCase))
        {
            stretchValue = "Uniform";
            isSquare = true;
        }
        else if (string.Equals(mode, "FillSquare", StringComparison.OrdinalIgnoreCase))
        {
            stretchValue = "Fill";
            isSquare = true;
        }
        else
        {
            stretchValue = mode;
        }

        gallerySettings.GalleryStretch.Value = stretchValue;
        gallerySettings.ItemWidth.Value = isSquare ? gallerySettings.ItemHeight.CurrentValue : double.NaN; 

        ViewModel.Gallery.ItemSpacing.Value = 2;
    }

    // Animations

    private async Task ClosedToDocked()
    {
        if (ViewModel == null) return;
        var dock = Settings.Gallery.DockPosition;
        
        IsVisible = true;
        
        // Reset dimensions
        if (dock is GalleryDockPosition.Top or GalleryDockPosition.Bottom)
        {
            Height = ZeroSize;
            Width = double.NaN;
        }
        else
        {
            Width = ZeroSize;
            Height = double.NaN;
        }
        
        SetDockLayoutCore(dock);
        // Reset size back to 0 for animation start after SetDockedLayout might have set it
        var targetSize = GetDockedHeight;
        
         if (dock is GalleryDockPosition.Top or GalleryDockPosition.Bottom)
         {
             Height = ZeroSize;
             var anim = AnimationsHelper.HeightAnimation(ZeroSize, targetSize, FastAnimationSpeed);
             await anim.RunAsync(this);
             Height = targetSize;
         }
         else
         {
             Width = ZeroSize;
             var anim = AnimationsHelper.WidthAnimation(ZeroSize, targetSize, FastAnimationSpeed);
             await anim.RunAsync(this);
             Width = targetSize;
         }
         SetDockedThumbs(dock);
    }

    private async Task DockedToClosed()
    { 
         var dock = DockPanel.GetDock(this);

         var currentSize = GetDockedHeight;
         
         if (dock is Dock.Bottom or Dock.Top)
         {
             var anim = AnimationsHelper.HeightAnimation(currentSize, ZeroSize, FastAnimationSpeed);
             await anim.RunAsync(this);
             Height = ZeroSize;
         }
         else
         {
             var anim = AnimationsHelper.WidthAnimation(currentSize, ZeroSize, FastAnimationSpeed);
             await anim.RunAsync(this);
             Width = ZeroSize;
         }
         
         IsVisible = false;
    }

    private async Task DockedToExpanded()
    {
        if (ViewModel == null || Parent is not Control parent) return;
        var dock = Settings.Gallery.DockPosition;
        
        SetExpandedLayoutCore(dock); // Set props

        var startSize = GetDockedHeight;
        
        if (dock is GalleryDockPosition.Top or GalleryDockPosition.Bottom)
        {
            var targetHeight = parent.Bounds.Height;
            var anim = AnimationsHelper.HeightAnimation(startSize, targetHeight, MediumAnimationSpeed);
            await anim.RunAsync(this);
            Height = targetHeight;
        }
        else
        {
            var targetWidth = parent.Bounds.Width;
            var anim = AnimationsHelper.WidthAnimation(startSize, targetWidth, MediumAnimationSpeed);
            await anim.RunAsync(this);
            Width = targetWidth;
        }
        SetExpandedThumbs();
    }

    private async Task ExpandedToDocked()
    {
        if (ViewModel == null || Parent is not Control parent) return;
        var dock = Settings.Gallery.DockPosition;
        
        // Animate from Full
        if (dock is GalleryDockPosition.Top or GalleryDockPosition.Bottom)
        {
            var startHeight = parent.Bounds.Height;
            var targetHeight = GetDockedHeight;
            
            // Override height for animation start
            Height = startHeight;
            
            var anim = AnimationsHelper.HeightAnimation(startHeight, targetHeight, SlowAnimationSpeed);
            await anim.RunAsync(this);
            Height = targetHeight;
        }
        else
        {
            var startWidth = parent.Bounds.Width;
            var targetWidth = Settings.Gallery.BottomGalleryItemSize;
            
            Width = startWidth;
            
            var anim = AnimationsHelper.WidthAnimation(startWidth, targetWidth, SlowAnimationSpeed);
            await anim.RunAsync(this);
            Width = targetWidth;
        }
        
        SetDockedLayout(dock); 
    }

    private async Task ClosedToExpanded()
    {
        IsVisible = true;
        Width = Height = ZeroSize;
        double targetWidth, targetHeight;
        if (Parent is Control parent)
        {
            targetHeight = parent.Bounds.Height;
            targetWidth = parent.Bounds.Width;
        }
        else
        {
            return;
        }
        
        var widthAnim = AnimationsHelper.WidthAnimation(Width, targetWidth, MediumAnimationSpeed);
        var heightAnim = AnimationsHelper.HeightAnimation(Height, targetHeight, MediumAnimationSpeed);
        
        await Task.WhenAll(
            widthAnim.RunAsync(this),
            heightAnim.RunAsync(this)
        );
        
        // Configure ScrollViewer
        if (_scrollViewer != null)
        {
            _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }
        _itemsPanel?.Orientation = Orientation.Vertical;
        ViewModel.Gallery.ItemSpacing.Value = Settings.Gallery.ItemSpacing;
        
        SetExpandedThumbs();
    }

    private async Task ExpandedToClosed()
    {
        var widthAnim = AnimationsHelper.WidthAnimation(Bounds.Width, ZeroSize, FastAnimationSpeed);
        var heightAnim = AnimationsHelper.HeightAnimation(Bounds.Height, ZeroSize, FastAnimationSpeed);
        
        await Task.WhenAll(
            widthAnim.RunAsync(this),
            heightAnim.RunAsync(this)
        );
        
        IsVisible = false;
    }

    #endregion

    #region Events

    private void ParentSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (ViewModel?.Gallery.GalleryMode.Value == GalleryMode2.Expanded)
        {
            UpdateLayoutForCurrentState();
        }
    }

    #endregion
}
