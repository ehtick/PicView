using System.Collections;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Metadata;
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
    private bool _isForeignDragging; // True if this control is currently previewing a tab from another window
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
            // NEW: Handle Cross-Window Logic
            HandleExternalDrag(e);
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
            // NEW: Handle drop on external target
            if (TabDragContext.CurrentTarget != null && TabDragContext.CurrentTarget != this)
            {
                // Commit the move on the target
                TabDragContext.CurrentTarget.ExternalDrop(TabDragContext.DraggingItem!);

                // Fire detached event so Source VM can clean up
                var item = ItemFromContainer(_pressedTab);
                if (item != null)
                {
                    var screenPos = _pressedTab.PointToScreen(e.GetPosition(_pressedTab));
                    TabDetached?.Invoke(sender, new TabDetachEventArgs(item, screenPos));
                }
            }
            else
            {
                // Standard Detach (New Window) logic
                PerformDetach(sender, e);
            }
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

        // Initialize Context
        TabDragContext.StartDrag(DataContext as TabViewModel, this);
        if (_pressedTab.DataContext is TabViewModel vm)
        {
            TabDragContext.DraggingItem = vm;
        }

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
                // Only re-attach if we are NOT currently over a foreign target
                if (TabDragContext.CurrentTarget == null || TabDragContext.CurrentTarget == this)
                {
                    _isDetaching = false;
                    PseudoClasses.Set(PseudoDetaching, false);
                    PseudoClasses.Set(PseudoDragging, true);

                    _pressedTab.Opacity = 1.0; // Restore original tab
                    CloseGhostWindow();

                    // Clear any previous target interactions
                    if (TabDragContext.CurrentTarget != null && TabDragContext.CurrentTarget != this)
                    {
                        TabDragContext.CurrentTarget.ExternalDragLeave(TabDragContext.DraggingItem!);
                        TabDragContext.CurrentTarget = null;
                    }
                }

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
        TabDragContext.EndDrag();

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
        _isForeignDragging = false;
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

    #region External Drag Logic

    private void HandleExternalDrag(PointerEventArgs e)
    {
        if (_pressedTab == null)
        {
            return;
        }

        var screenPosPixel = _pressedTab.PointToScreen(e.GetPosition(_pressedTab));

        // Find window under mouse
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var window in desktop.Windows)
            {
                // Check if mouse is over this window
                // Convert screen pixel position to window-relative local coordinates to handle DPI/Scaling correctly
                var winPoint = window.PointToClient(screenPosPixel);
                var winRect = new Rect(0, 0, window.ClientSize.Width, window.ClientSize.Height);

                if (!winRect.Contains(winPoint))
                {
                    continue;
                }

                // Find DraggableTabControl in this window
                var tabControl = window.FindDescendantOfType<DraggableTabControl>();

                // Verify we are actually over the TabControl (or reasonably close) to prevent snapping when just over the window content
                var isOverTabControl = false;
                if (tabControl != null)
                {
                    var tabPoint = tabControl.PointToClient(screenPosPixel);
                    // Restrict target area to the top strip (header) where tabs reside
                    var tabStripBounds = new Rect(0, 0, tabControl.Bounds.Width, DetachThreshold);

                    if (tabStripBounds.Contains(tabPoint))
                    {
                        isOverTabControl = true;
                    }
                }

                if (tabControl == null || !isOverTabControl)
                {
                    continue;
                }

                if (tabControl != TabDragContext.CurrentTarget)
                {
                    // Leaving old target
                    if (TabDragContext.CurrentTarget != null && TabDragContext.CurrentTarget != this)
                    {
                        TabDragContext.CurrentTarget.ExternalDragLeave(TabDragContext.DraggingItem!);
                    }

                    // Entering new target
                    TabDragContext.CurrentTarget = tabControl;
                    if (tabControl != this)
                    {
                        tabControl.ExternalDragEnter(TabDragContext.DraggingItem!);
                    }
                }

                // Drag Over
                if (tabControl != this)
                {
                    tabControl.ExternalDragOver(screenPosPixel);
                    _ghostWindow?.Hide();
                }
                else
                {
                    // Back over source
                    UpdateGhostWindowPosition(e);
                    _ghostWindow?.Show();
                }

                return;
            }
        }

        // No target found (or left target)
        if (TabDragContext.CurrentTarget != null && TabDragContext.CurrentTarget != this)
        {
            TabDragContext.CurrentTarget.ExternalDragLeave(TabDragContext.DraggingItem!);
        }

        TabDragContext.CurrentTarget = null;

        UpdateGhostWindowPosition(e);
        _ghostWindow?.Show();
    }

    // Called by Source when drag enters this control
    internal void ExternalDragEnter(object item)
    {
        var list = ItemsSource as IList ?? Items;
        if (list == null || list.Contains(item))
        {
            return;
        }

        _isForeignDragging = true;
        list.Add(item);

        // Force layout update so we can find the container
        UpdateLayout();

        // Find the container for this item
        if (ContainerFromItem(item) is not TabItem container)
        {
            return;
        }

        _pressedTab = container;
        _sourceIndex = IndexFromContainer(container);
        _draggedTabWidth = container.Bounds.Width;
        _draggedTabStartX = container.Bounds.X;

        // Calculate fake click point relative to tab for smooth dragging
        // We assume the user grabbed it somewhat in the middle or we just center it
        _pointerOffsetWithinTab = _draggedTabWidth / 2;

        CacheTabPositions();
    }

    // Called by Source when drag moves over this control
    internal void ExternalDragOver(PixelPoint screenPos)
    {
        if (!_isForeignDragging || _pressedTab == null)
        {
            return;
        }

        var localPos = this.PointToClient(screenPos);
        var dragLeftPos = localPos.X - _pointerOffsetWithinTab;

        UpdateTabReorderingVisuals(dragLeftPos);
    }

    // Called by Source when drag leaves this control
    internal void ExternalDragLeave(object item)
    {
        if (!_isForeignDragging)
        {
            return;
        }

        var list = ItemsSource as IList ?? Items;
        if (list != null && list.Contains(item))
        {
            list.Remove(item);
        }

        _isForeignDragging = false;
        _pressedTab = null;
        _sourceIndex = -1;
        _currentTargetIndex = -1;

        // Reset transforms
        foreach (var c in GetRealizedContainers())
        {
            c.RenderTransform = null;
        }
    }

    // Called by Source when dropped on this control
    internal void ExternalDrop(object item)
    {
        if (!_isForeignDragging)
        {
            return;
        }

        // We just keep the item in the list
        _isForeignDragging = false;

        // Reset Visuals
        foreach (var c in GetRealizedContainers())
        {
            c.RenderTransform = null;
        }

        _pressedTab = null;
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
        var cornerRadius = new CornerRadius(16);
        var isMacOs = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        _ghostWindow = new Window
        {
            SystemDecorations = isMacOs ? SystemDecorations.BorderOnly : SystemDecorations.None,
            CornerRadius = cornerRadius,
            ShowInTaskbar = false,
            Topmost = true,
            Background = Brushes.Transparent,
            IsHitTestVisible = false,
            Width = windowWidth,
            Height = windowHeight,
            Content = new Border
            {
                CornerRadius = cornerRadius,
                Opacity = GhostOpacity,
                Child = new Image
                {
                    MaxHeight = GhostTargetHeight,
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

// Helper for Shared Drag State
internal static class TabDragContext
{
    public static object? DraggingItem { get; set; }
    public static DraggableTabControl? SourceControl { get; set; }
    public static DraggableTabControl? CurrentTarget { get; set; }

    public static void StartDrag(object? item, DraggableTabControl source)
    {
        DraggingItem = item;
        SourceControl = source;
        CurrentTarget = null;
    }

    public static void EndDrag()
    {
        DraggingItem = null;
        SourceControl = null;
        CurrentTarget = null;
    }
}