using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using PicView.Avalonia.Animations;
using PicView.Core.Config;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;
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

    private const double FastAnimationSpeed = 0.3;
    private const double MediumAnimationSpeed = 0.5;
    private const double SlowAnimationSpeed = 0.6;
    private const double FullOpacity = 1.0;
    private const double NoOpacity = 0.0;
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
            .Subscribe(SetDockedLayout)
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
        var dock = Settings.Gallery.DockPosition;

        IsVisible = mode != GalleryMode2.Closed;
        Opacity = IsVisible ? FullOpacity : NoOpacity;

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
        ViewModel.Gallery.GalleryVerticalAlignment.Value = VerticalAlignment.Stretch;
        _itemsPanel?.Orientation = Orientation.Vertical;
    }

    private void SetDockedLayout(GalleryDockPosition dock)
    {
        var size = Settings.Gallery.BottomGalleryItemSize + BorderTopAndBottomThickness;

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
        
        ViewModel.Gallery.IsLeftDocked.Value = ViewModel.Gallery.IsRightDocked.Value = ViewModel.Gallery.IsBottomDocked.Value = ViewModel.Gallery.IsTopDocked.Value = false;
        switch (dock)
        {
            case GalleryDockPosition.Top:
                DockPanel.SetDock(this, Dock.Top);
                ViewModel.Gallery.IsTopDocked.Value = true;
                break;
            case GalleryDockPosition.Left:
                DockPanel.SetDock(this, Dock.Left);
                ViewModel.Gallery.IsLeftDocked.Value = true;
                break;
            case GalleryDockPosition.Right:
                DockPanel.SetDock(this, Dock.Right);
                ViewModel.Gallery.IsRightDocked.Value = true;
                break;
            case GalleryDockPosition.Bottom:
            default:
                DockPanel.SetDock(this, Dock.Bottom);
                ViewModel.Gallery.IsBottomDocked.Value = true;
                break;
        }
    }

    // Animations

    private async Task ClosedToDocked()
    {
        if (ViewModel == null) return;
        var dock = Settings.Gallery.DockPosition;
        
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
        var targetSize = Settings.Gallery.BottomGalleryItemSize + BorderTopAndBottomThickness;
        
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
         var dock = Settings.Gallery.DockPosition;
         
         var currentSize = Settings.Gallery.BottomGalleryItemSize + BorderTopAndBottomThickness;
         
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
        var dock = Settings.Gallery.DockPosition;
        
        SetExpandedLayout(dock); // Set props
        
        var startSize = Settings.Gallery.BottomGalleryItemSize + BorderTopAndBottomThickness;
        
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
        var dock = Settings.Gallery.DockPosition;
        
        // Animate from Full
        if (dock is GalleryDockPosition.Top or GalleryDockPosition.Bottom)
        {
            var startHeight = parent.Bounds.Height;
            var targetHeight = Settings.Gallery.BottomGalleryItemSize + BorderTopAndBottomThickness;
            
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
        var dock = Settings.Gallery.DockPosition;

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

    #endregion
}
