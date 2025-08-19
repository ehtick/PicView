using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using PicView.Avalonia.Animations;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;
using PicView.Core.Sizing;
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
    }

    #endregion

    #region Fields and Properties

    private const double FastAnimationSpeed = 0.3;
    private const double MediumAnimationSpeed = 0.5;
    private const double SlowAnimationSpeed = 0.6;
    private const double FullOpacity = 1.0;
    private const double NoOpacity = 0.0;
    private const int ZeroHeight = 0;

    private static readonly Thickness FullGalleryItemMargin = new(25);
    private static readonly Thickness BottomGalleryItemMargin = new(2, 0);

    public static readonly AvaloniaProperty<GalleryMode?> GalleryModeProperty =
        AvaloniaProperty.Register<GalleryAnimationControl, GalleryMode?>(nameof(GalleryMode));

    public GalleryMode GalleryMode
    {
        get => GetValue(GalleryModeProperty) as GalleryMode? ?? GalleryMode.Closed;
        set => SetValue(GalleryModeProperty, value);
    }

    private bool _isAnimating;
    private MainViewModel? ViewModel => DataContext as MainViewModel;

    #endregion

    #region Constructors

    public GalleryAnimationControl()
    {
        Loaded += OnControlLoaded;
    }

    private void OnControlLoaded(object? sender, RoutedEventArgs e)
    {
        AddHandler(PointerPressedEvent, PreviewPointerPressedEvent, RoutingStrategies.Tunnel);

        this.GetObservable(GalleryModeProperty).ToObservable()
            .SubscribeAwait( async (galleryMode, _) =>
            {
                try
                {
                    switch (galleryMode)
                    {
                        case GalleryMode.FullToBottom:
                            await FullToBottomAnimation();
                            break;
                        case GalleryMode.FullToClosed:
                            await FullToClosedAnimation();
                            break;
                        case GalleryMode.BottomToFull:
                            await BottomToFullAnimation();
                            break;
                        case GalleryMode.BottomToClosed:
                            await BottomToClosedAnimation();
                            break;
                        case GalleryMode.ClosedToFull:
                            await ClosedToFullAnimation();
                            break;
                        case GalleryMode.ClosedToBottom:
                            await ClosedToBottomAnimation();
                            break;
                        case GalleryMode.Closed:
                            CloseWithNoAnimation();
                            break;
                        case GalleryMode.BottomNoAnimation:
                            BottomNoAnimation();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(galleryMode), galleryMode, null);
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.LogDebug(nameof(GalleryAnimationControl), nameof(OnControlLoaded), ex);
                    _isAnimating = false;
                }
            });

        if (Parent is Control parent)
        {
            parent.SizeChanged += ParentSizeChanged;
        }
    }

    #endregion

    #region Animation Methods

    private void CloseWithNoAnimation()
    {
        IsVisible = false;
        Height = ZeroHeight;
    }

    private void BottomNoAnimation()
    {
        if (ViewModel == null)
        {
            return;
        }

        IsVisible = true;
        Opacity = FullOpacity;
        Height = double.NaN;
        ViewModel.Gallery.GalleryOrientation.Value = Orientation.Horizontal;
        ViewModel.Gallery.GalleryVerticalAlignment.Value = VerticalAlignment.Bottom;
    }

    private async Task ClosedToFullAnimation()
    {
        if (ViewModel == null || _isAnimating || Parent is not Control parent)
        {
            return;
        }

        try
        {
            _isAnimating = true;

            GalleryHelper.SetGalleryItemStretch(Settings.Gallery.FullGalleryStretchMode, ViewModel);

            // Setup initial state
            IsVisible = true;
            Opacity = NoOpacity;
            Height = parent.Bounds.Height;
            ViewModel.Gallery.GalleryItem.ItemMargin.Value = FullGalleryItemMargin;

            // Configure gallery
            ViewModel.Gallery.GalleryOrientation.Value = Orientation.Vertical;
            GalleryStretchMode.DetermineStretchMode(ViewModel);
            ViewModel.Gallery.IsGalleryExpanded.Value = true;

            // Animate opacity
            var opacityAnimation = AnimationsHelper.OpacityAnimation(NoOpacity, FullOpacity, MediumAnimationSpeed);
            await opacityAnimation.RunAsync(this);

            // Apply final state
            Opacity = FullOpacity;
            ViewModel.Gallery.GalleryVerticalAlignment.Value = VerticalAlignment.Stretch;
            GalleryNavigation.CenterScrollToSelectedItem(ViewModel);
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async Task FullToClosedAnimation()
    {
        if (ViewModel == null || _isAnimating || Parent is not Control parent)
        {
            return;
        }

        try
        {
            _isAnimating = true;

            // Setup initial state
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Height = parent.Bounds.Height;
            });

            // Animate opacity
            var opacityAnimation = AnimationsHelper.OpacityAnimation(FullOpacity, NoOpacity, FastAnimationSpeed);
            ViewModel.Gallery.GalleryMargin.Value = new Thickness(0);
            await opacityAnimation.RunAsync(this);

            // Apply final state
            Opacity = NoOpacity;
            IsVisible = false;
            Height = ZeroHeight;
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async Task ClosedToBottomAnimation()
    {
        if (ViewModel == null || _isAnimating)
        {
            return;
        }

        try
        {
            _isAnimating = true;

            // Setup gallery properties
            GalleryHelper.SetGalleryItemStretch(Settings.Gallery.BottomGalleryStretchMode, ViewModel);

            // Setup initial state
            Height = ZeroHeight;
            IsVisible = true;
            Opacity = FullOpacity;
            await WindowResizing.SetSizeAsync(ViewModel);
            ViewModel.Gallery.GalleryItem.ItemMargin.Value = BottomGalleryItemMargin;

            // Configure gallery
            ViewModel. Gallery.GalleryOrientation.Value = Orientation.Horizontal;
            GalleryStretchMode.DetermineStretchMode(ViewModel);
            ViewModel.Gallery.IsGalleryExpanded.Value = false;
            ViewModel.Gallery.GalleryVerticalAlignment.Value = VerticalAlignment.Bottom;

            // Animate height
            var to = GalleryFunctions.GetGalleryHeight(ViewModel);
            var heightAnimation = AnimationsHelper.HeightAnimation(ZeroHeight, to, FastAnimationSpeed);
            await heightAnimation.RunAsync(this);

            // Apply final state
            Height = to;
            IsVisible = true;
            GalleryNavigation.CenterScrollToSelectedItem(ViewModel);
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async Task BottomToClosedAnimation()
    {
        if (ViewModel == null || _isAnimating)
        {
            return;
        }

        try
        {
            _isAnimating = true;

            // Animate closing
            var from = ViewModel.Gallery.GalleryItem.BottomGalleryItemHeight.Value + SizeDefaults.ScrollbarSize;
            Height = from;
            Opacity = FullOpacity;
            IsVisible = true;

            // Configure gallery
            ViewModel.Gallery.GalleryOrientation.Value = Orientation.Horizontal;
            ViewModel.Gallery.IsGalleryExpanded.Value = false;

            // Animate height
            var heightAnimation = AnimationsHelper.HeightAnimation(from, ZeroHeight, FastAnimationSpeed);
            await heightAnimation.RunAsync(this);

            // Apply final state
            Height = ZeroHeight;
            IsVisible = false;
            await WindowResizing.SetSizeAsync(ViewModel);
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async Task BottomToFullAnimation()
    {
        if (ViewModel == null || _isAnimating || Parent is not Control parent)
        {
            return;
        }

        try
        {
            _isAnimating = true;

            // Configure gallery
            ViewModel.Gallery.GalleryOrientation.Value = Orientation.Vertical;
            ViewModel.Gallery.IsGalleryExpanded.Value = true;
            GalleryStretchMode.DetermineStretchMode(ViewModel);
            ViewModel.Gallery.GalleryItem.ItemMargin.Value = FullGalleryItemMargin;

            // Animate height
            var from = GalleryFunctions.GetGalleryHeight(ViewModel);
            var to = parent.Bounds.Height;
            var heightAnimation = AnimationsHelper.HeightAnimation(from, to, MediumAnimationSpeed);
            await heightAnimation.RunAsync(this);

            // Apply final state
            Height = to;
            ViewModel.Gallery.GalleryVerticalAlignment.Value = VerticalAlignment.Stretch;
            GalleryNavigation.CenterScrollToSelectedItem(ViewModel);
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async Task FullToBottomAnimation()
    {
        if (ViewModel == null || _isAnimating || Parent is not Control parent)
        {
            return;
        }

        try
        {
            _isAnimating = true;

            // Configure gallery
            ViewModel.Gallery.GalleryVerticalAlignment.Value = VerticalAlignment.Bottom;
            ViewModel.Gallery.IsGalleryExpanded.Value = false;

            // Animate height
            var from = Bounds.Height;
            var to = GalleryFunctions.GetGalleryHeight(ViewModel);
            var heightAnimation = AnimationsHelper.HeightAnimation(from, to, SlowAnimationSpeed);
            await heightAnimation.RunAsync(this);

            if (!GalleryLoad.IsLoading)
            {
                GalleryStretchMode.DetermineStretchMode(ViewModel);
            }

            // Apply final state
            Height = parent.Bounds.Height;
            ViewModel.Gallery.GalleryItem.ItemMargin.Value = BottomGalleryItemMargin;
            ViewModel.Gallery.GalleryOrientation.Value = Orientation.Horizontal;
            GalleryNavigation.CenterScrollToSelectedItem(ViewModel);
        }
        finally
        {
            _isAnimating = false;
        }
    }

    #endregion

    #region Events

    private void ParentSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (_isAnimating || !GalleryFunctions.IsFullGalleryOpen || sender is not Control parent)
        {
            return;
        }

        Width = parent.Bounds.Width;
        Height = parent.Bounds.Height;
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