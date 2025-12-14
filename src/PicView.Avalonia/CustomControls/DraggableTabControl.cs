using System.Collections;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.CustomControls;

[PseudoClasses(PseudoDragging, PseudoDetaching)]
public class DraggableTabControl : TabControl
{
    // --- Constants ---
    private const double DragThreshold = 4.0;
    private const double DetachThreshold = 50.0;
    private const string PseudoDragging = ":dragging";
    private const string PseudoDetaching = ":detaching";

    // Ghost Window Settings
    private const double GhostTargetHeight = 400.0;
    private const double GhostOpacity = 0.5;
    private const double GhostOffsetX = 100.0;
    private const double GhostOffsetY = 50.0;

    // --- State Fields ---
    private readonly Dictionary<TabItem, double> _originalXPositions = new();
    private int _currentTargetIndex = -1;
    private double _draggedTabStartX;
    private double _draggedTabWidth;

    private Window? _ghostWindow;
    private bool _isDetaching;

    private bool _isDragging;
    private double _pointerOffsetWithinTab;

    private TabItem? _pressedTab;

    private int _sourceIndex = -1;
    private Point _startClickPoint;

    protected override Type StyleKeyOverride => typeof(DraggableTabControl);

    // --- Events ---
    public event EventHandler<TabDetachEventArgs>? TabDetached;
    public event EventHandler<TabCreatedEventArgs>? TabCreated;

    #region Lifecycle Overrides

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);

        if (container is not TabItem tabItem)
        {
            return;
        }

        // Notify if this is a fresh tab (not closing)
        if (item is TabViewModel { IsClosing: false })
        {
            TabCreated?.Invoke(tabItem, new TabCreatedEventArgs(item, index));
        }

        // Attach Events
        tabItem.AddHandler(PointerPressedEvent, OnItemPointerPressed,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        tabItem.AddHandler(PointerMovedEvent, OnItemPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        tabItem.AddHandler(PointerReleasedEvent, OnItemPointerReleased,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        tabItem.PointerCaptureLost += OnPointerCaptureLost;
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is TabItem tabItem)
        {
            tabItem.RemoveHandler(PointerPressedEvent, OnItemPointerPressed);
            tabItem.RemoveHandler(PointerMovedEvent, OnItemPointerMoved);
            tabItem.RemoveHandler(PointerReleasedEvent, OnItemPointerReleased);
            tabItem.PointerCaptureLost -= OnPointerCaptureLost;

            // If the tab is closing while being dragged, clean up
            if (tabItem.DataContext is TabViewModel { IsClosing: true } && _pressedTab == tabItem)
            {
                EndDrag();
            }

            tabItem.RenderTransform = null;
        }

        base.ClearContainerForItemOverride(container);
    }

    #endregion

    #region Pointer Events

    private void OnItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(this).Properties;
        if (!props.IsLeftButtonPressed || sender is not TabItem tabItem)
        {
            return;
        }

        _pressedTab = tabItem;
        _startClickPoint = e.GetPosition(this);

        // Reset state
        _isDragging = false;
        _isDetaching = false;

        // Calculate indices
        ItemFromContainer(tabItem); // Ensure container realization
        _sourceIndex = IndexFromContainer(tabItem);
        _currentTargetIndex = _sourceIndex;

        // Cache geometry
        CacheTabPositions();

        _draggedTabStartX = tabItem.Bounds.X;
        _draggedTabWidth = tabItem.Bounds.Width;
        _pointerOffsetWithinTab = _startClickPoint.X - _draggedTabStartX;
    }

    private void OnItemPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_pressedTab == null)
        {
            return;
        }

        var currentPos = e.GetPosition(this);
        var deltaX = currentPos.X - _startClickPoint.X;
        var deltaY = currentPos.Y - _startClickPoint.Y;
        var absDeltaY = Math.Abs(deltaY);

        // 1. Check if we should start dragging
        if (!_isDragging)
        {
            if (!HandleDragStart(e, deltaX, absDeltaY))
            {
                return;
            }
        }

        // 2. Check if we should switch between Reordering and Detaching
        HandleStateTransition(absDeltaY);

        // 3. Execute Movement
        if (_isDetaching)
        {
            UpdateGhostWindowPosition(e);
        }
        else
        {
            var dragLeftPos = currentPos.X - _pointerOffsetWithinTab;
            UpdateTabReorderingVisuals(dragLeftPos);
        }

        e.Handled = true;
    }

    private void OnItemPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        CloseGhostWindow();

        if (_pressedTab == null)
        {
            return;
        }

        if (_isDetaching)
        {
            PerformDetach(sender, e);
        }
        else if (_isDragging && _currentTargetIndex >= 0)
        {
            // Commit the reorder
            if (TryMoveItem(_sourceIndex, _currentTargetIndex))
            {
                SelectedIndex = _currentTargetIndex;
            }
        }

        EndDrag();
        e.Handled = true;
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        EndDrag();
    }

    #endregion

    #region Drag Logic Helpers

    private void CacheTabPositions()
    {
        _originalXPositions.Clear();
        foreach (var item in GetRealizedContainers().OfType<TabItem>())
        {
            _originalXPositions[item] = item.Bounds.X;
        }
    }

    private bool HandleDragStart(PointerEventArgs e, double deltaX, double absDeltaY)
    {
        if (Math.Abs(deltaX) <= DragThreshold && absDeltaY <= DragThreshold)
        {
            return false;
        }

        _isDragging = true;
        if (_pressedTab == null)
        {
            return false;
        }

        e.Pointer.Capture(_pressedTab);
        PseudoClasses.Set(PseudoDragging, true);
        _pressedTab.ZIndex = 1000;

        return true;
    }

    private void HandleStateTransition(double absDeltaY)
    {
        if (_pressedTab == null)
        {
            return;
        }

        switch (_isDetaching)
        {
            // Transition: Attached -> Detaching
            case false when absDeltaY > DetachThreshold:
                _isDetaching = true;
                PseudoClasses.Set(PseudoDetaching, true);
                PseudoClasses.Set(PseudoDragging, false);

                _pressedTab.Opacity = 0; // Hide original tab
                CreateGhostWindow(_pressedTab);
                break;
            // Transition: Detaching -> Attached
            case true when absDeltaY <= DetachThreshold:
                _isDetaching = false;
                PseudoClasses.Set(PseudoDetaching, false);
                PseudoClasses.Set(PseudoDragging, true);

                _pressedTab.Opacity = 1.0; // Restore original tab
                CloseGhostWindow();
                break;
        }
    }

    private void UpdateTabReorderingVisuals(double dragLeftPos)
    {
        var dragCenter = dragLeftPos + _draggedTabWidth / 2;
        var realizedItems = GetRealizedContainers().OfType<TabItem>().ToArray();

        if (realizedItems.Length == 0)
        {
            return;
        }

        // 1. Calculate Target Index
        var newTargetIndex = _sourceIndex;
        for (var i = 0; i < realizedItems.Length; i++)
        {
            var tab = realizedItems[i];
            var tabStartX = _originalXPositions.TryGetValue(tab, out var val) ? val : tab.Bounds.X;
            var tabCenter = tabStartX + tab.Bounds.Width / 2;

            if (dragCenter > tabCenter)
            {
                newTargetIndex = i;
            }
        }

        _currentTargetIndex = newTargetIndex;

        // 2. Apply Transforms
        for (var i = 0; i < realizedItems.Length; i++)
        {
            var tab = realizedItems[i];
            var startX = _originalXPositions.GetValueOrDefault(tab, tab.Bounds.X);
            double offsetX = 0;

            if (tab == _pressedTab)
            {
                offsetX = dragLeftPos - startX;
            }
            else
            {
                // Shift items left or right to make room
                if (_currentTargetIndex > _sourceIndex && i > _sourceIndex && i <= _currentTargetIndex)
                {
                    offsetX = -_draggedTabWidth; // Shift Left
                }
                else if (_currentTargetIndex < _sourceIndex && i >= _currentTargetIndex && i < _sourceIndex)
                {
                    offsetX = _draggedTabWidth; // Shift Right
                }
            }

            // Note: Y is always 0 to snap back to the strip row
            tab.RenderTransform = new TranslateTransform(offsetX, 0);
        }
    }

    private void PerformDetach(object? sender, PointerReleasedEventArgs e)
    {
        if (_pressedTab == null)
        {
            return;
        }

        var item = ItemFromContainer(_pressedTab);
        if (item == null)
        {
            return;
        }

        var screenPos = _pressedTab.PointToScreen(e.GetPosition(_pressedTab));

        // Remove from collection
        var list = ItemsSource as IList ?? Items;
        if (list != null && _sourceIndex >= 0 && _sourceIndex < list.Count)
        {
            list.RemoveAt(_sourceIndex);
        }

        TabDetached?.Invoke(sender, new TabDetachEventArgs(item, screenPos));
    }

    private void EndDrag()
    {
        if (_pressedTab != null)
        {
            PseudoClasses.Remove(PseudoDragging);
            PseudoClasses.Remove(PseudoDetaching);
            _pressedTab.Opacity = 1.0;
            _pressedTab.ZIndex = 0;
        }

        // Reset all transforms to clean slate
        foreach (var c in GetRealizedContainers())
        {
            c.RenderTransform = null;
        }

        CloseGhostWindow();

        _pressedTab = null;
        _isDragging = false;
        _isDetaching = false;
        _sourceIndex = -1;
        _currentTargetIndex = -1;
        _originalXPositions.Clear();
    }

    private bool TryMoveItem(int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex)
        {
            return true;
        }

        var list = ItemsSource as IList ?? Items;
        if (list == null || oldIndex < 0 || oldIndex >= list.Count)
        {
            return false;
        }

        // Clamp index
        newIndex = Math.Clamp(newIndex, 0, list.Count - 1);

        try
        {
            var item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

#region Ghost Window Logic

    private void CreateGhostWindow(TabItem tabItem)
    {
        if (_ghostWindow != null)
        {
            return;
        }

        var contentBitmap = CaptureContentBitmap(tabItem);
        if (contentBitmap == null)
        {
            return;
        }

        var (windowWidth, windowHeight) = CalculateGhostSize(contentBitmap.Size);

        _ghostWindow = new Window
        {
            SystemDecorations = SystemDecorations.None,
            ShowInTaskbar = false,
            Topmost = true,
            Background = Brushes.Transparent,
            IsHitTestVisible = false,
            Width = windowWidth,
            Height = windowHeight,
            Content = new Border
            {
                MaxHeight = GhostTargetHeight,
                Opacity = GhostOpacity,
                Child = new Image
                {
                    Source = contentBitmap,
                    Stretch = Stretch.Uniform
                }
            }
        };

        _ghostWindow.Show();
    }

    private Bitmap? CaptureContentBitmap(TabItem tabItem)
    {
        var vm = tabItem.DataContext as TabViewModel;
        return CaptureVisual(vm.CurrentView.Value as Control);
    }

    private (double width, double height) CalculateGhostSize(Size contentSize)
    {
        var width = contentSize.Width;
        var height = contentSize.Height;

        if (height < GhostTargetHeight)
        {
            return (width, height);
        }

        var scale = GhostTargetHeight / height;
        height = GhostTargetHeight;
        width *= scale;

        return (width, height);
    }

    private void UpdateGhostWindowPosition(PointerEventArgs e)
    {
        if (_ghostWindow == null || _pressedTab == null)
        {
            return;
        }

        var screenPos = _pressedTab.PointToScreen(e.GetPosition(_pressedTab));
        _ghostWindow.Position = new PixelPoint(
            screenPos.X - (int)GhostOffsetX,
            screenPos.Y - (int)GhostOffsetY
        );
    }

    private void CloseGhostWindow()
    {
        _ghostWindow?.Close();
        _ghostWindow = null;
    }

    private static Bitmap? CaptureVisual(Visual visual)
    {
        var bounds = visual.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return null;
        }

        try
        {
            var pixelSize = new PixelSize((int)bounds.Width, (int)bounds.Height);
            var bitmap = new RenderTargetBitmap(pixelSize, new Vector(96, 96));
            bitmap.Render(visual);
            return bitmap;
        }
        catch
        {
            // Visual might not be ready for rendering
            return null;
        }
    }

    #endregion
}

public class TabDetachEventArgs(object detachedItem, PixelPoint screenPosition) : EventArgs
{
    public object DetachedItem { get; } = detachedItem;
    public PixelPoint ScreenPosition { get; } = screenPosition;
}

public class TabCreatedEventArgs(object createdItem, int index) : EventArgs
{
    public object CreatedItem { get; } = createdItem;
    public int Index { get; } = index;
}