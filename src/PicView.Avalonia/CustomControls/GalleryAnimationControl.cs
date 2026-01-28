using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
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
        RemoveHandler(PointerPressedEvent, PreviewPointerPressedEvent);
        _disposables?.Dispose();
    }

    #endregion

    #region Fields and Properties

    private const double FastAnimationSpeed = 0.3;
    private const double MediumAnimationSpeed = 0.5;
    private const double SlowAnimationSpeed = 0.6;
    private const double FullOpacity = 1.0;
    private const double NoOpacity = 0.0;
    private const int ZeroSize = 0;

    private TabViewModel? ViewModel => DataContext as TabViewModel;

    private CompositeDisposable? _disposables;
    private AutoScrollViewer? _scrollViewer;

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

    public GalleryAnimationControl()
    {
        Loaded += OnControlLoaded;
    }

    private void OnControlLoaded(object? sender, RoutedEventArgs e)
    {
        _disposables = new CompositeDisposable();

        AddHandler(PointerPressedEvent, PreviewPointerPressedEvent, RoutingStrategies.Tunnel);

        _scrollViewer = this.FindControl<AutoScrollViewer>("GalleryScrollViewer");

        if (ViewModel == null)
        {
            return;
        }

        // Subscribe to Mode changes
        ViewModel.Gallery.GalleryMode
            .SubscribeAwait(async (mode, _) => await OnGalleryModeChanged(mode))
            .AddTo(_disposables);
        
        // Also subscribe to DockPosition, as changing it while Docked might need layout updates
        ViewModel.Gallery.GalleryDockPosition
            .Subscribe(_ => UpdateLayoutForCurrentState())
            .AddTo(_disposables);

        if (Parent is Control parent)
        {
            parent.SizeChanged += ParentSizeChanged;
        }
    }

    #endregion

    #region Logic

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
        var dock = ViewModel.Gallery.GalleryDockPosition.Value;

        IsVisible = mode != GalleryMode2.Closed;
        Opacity = IsVisible ? FullOpacity : NoOpacity;

        if (mode == GalleryMode2.Closed)
        {
            Width = ZeroSize;
            Height = ZeroSize;
            return;
        }

        if (mode == GalleryMode2.Expanded)
        {
            SetExpandedLayout(dock);
        }
        else // Docked
        {
            SetDockedLayout(dock);
        }
    }
    
    private void SetExpandedLayout(GalleryDockPosition dock)
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
            _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }
        
        // Update ViewModel props
        ViewModel!.Gallery.GalleryOrientation.Value = Orientation.Vertical; 
        ViewModel.Gallery.GalleryVerticalAlignment.Value = VerticalAlignment.Stretch;
        // Margins etc can be set here or in VM
    }

    private void SetDockedLayout(GalleryDockPosition dock)
    {
        var size = Settings.Gallery.BottomGalleryItemSize + SizeDefaults.ScrollbarSize;

        if (dock is GalleryDockPosition.Top or GalleryDockPosition.Bottom)
        {
            Width = double.NaN;
            Height = size;
            
            ViewModel!.Gallery.GalleryOrientation.Value = Orientation.Horizontal;
            ViewModel.Gallery.GalleryVerticalAlignment.Value = dock == GalleryDockPosition.Top ? VerticalAlignment.Top : VerticalAlignment.Bottom;

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
            
            // For side docking, typically we want vertical list? Or horizontal wrap in narrow col?
            // Usually side docked gallery is a vertical strip.
            ViewModel!.Gallery.GalleryOrientation.Value = Orientation.Vertical; // Assuming vertical strip for side dock
             ViewModel.Gallery.GalleryVerticalAlignment.Value = VerticalAlignment.Stretch;

            if (_scrollViewer != null)
            {
                _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }
    }

    // Animations

    private async Task ClosedToDocked()
    {
        if (ViewModel == null) return;
        var dock = ViewModel.Gallery.GalleryDockPosition.Value;
        
        IsVisible = true;
        Opacity = FullOpacity;
        
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
        
        SetDockedLayout(dock); // Set internal props (orientation etc)
        // Reset size back to 0 for animation start after SetDockedLayout might have set it
        var targetSize = Settings.Gallery.BottomGalleryItemSize + SizeDefaults.ScrollbarSize;
        
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
    }

    private async Task DockedToClosed()
    {
         if (ViewModel == null) return;
         var dock = ViewModel.Gallery.GalleryDockPosition.Value;
         
         var currentSize = Settings.Gallery.BottomGalleryItemSize + SizeDefaults.ScrollbarSize;
         
         if (dock is GalleryDockPosition.Top or GalleryDockPosition.Bottom)
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
        var dock = ViewModel.Gallery.GalleryDockPosition.Value;
        
        SetExpandedLayout(dock); // Set props
        
        var startSize = Settings.Gallery.BottomGalleryItemSize + SizeDefaults.ScrollbarSize;
        
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
    }

    private async Task ExpandedToDocked()
    {
        if (ViewModel == null || Parent is not Control parent) return;
        var dock = ViewModel.Gallery.GalleryDockPosition.Value;
        
        // Animate from Full
        if (dock is GalleryDockPosition.Top or GalleryDockPosition.Bottom)
        {
            var startHeight = parent.Bounds.Height;
            var targetHeight = Settings.Gallery.BottomGalleryItemSize + SizeDefaults.ScrollbarSize;
            
            // Override height for animation start
            Height = startHeight;
            
            var anim = AnimationsHelper.HeightAnimation(startHeight, targetHeight, SlowAnimationSpeed);
            await anim.RunAsync(this);
            Height = targetHeight;
        }
        else
        {
            var startWidth = parent.Bounds.Width;
            var targetWidth = Settings.Gallery.BottomGalleryItemSize + SizeDefaults.ScrollbarSize;
            
            Width = startWidth;
            
            var anim = AnimationsHelper.WidthAnimation(startWidth, targetWidth, SlowAnimationSpeed);
            await anim.RunAsync(this);
            Width = targetWidth;
        }
        
        SetDockedLayout(dock); 
    }

    private async Task ClosedToExpanded()
    {
        var dock = ViewModel.Gallery.GalleryDockPosition.Value;

        IsVisible = true;
        Opacity = NoOpacity;
        
        SetExpandedLayout(dock);
        
        var anim = AnimationsHelper.OpacityAnimation(NoOpacity, FullOpacity, MediumAnimationSpeed);
        await anim.RunAsync(this);
        
        Opacity = FullOpacity;
    }

    private async Task ExpandedToClosed()
    {
        if (ViewModel == null) return;
        
        var anim = AnimationsHelper.OpacityAnimation(FullOpacity, NoOpacity, FastAnimationSpeed);
        await anim.RunAsync(this);
        
        Opacity = NoOpacity;
        IsVisible = false;
        
        // Reset sizes
        Width = ZeroSize;
        Height = ZeroSize;
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

    private void PreviewPointerPressedEvent(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        // Disable right click selection, to not interfere with context menu
        e.Handled = true;
    }

    #endregion
}
