using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.CustomControls;

public class DraggableTabStrip : TabStrip
{
    private const double DragThreshold = 4.0;
    private const string DraggingPseudoClass = ":dragging";

    private readonly Dictionary<TabStripItem, double> _originalX = new();
    private int _currentTargetIndex = -1;

    private double _draggedTabStartX;
    private double _draggedTabWidth;
    private bool _isDragging;

    private double _pointerOffsetWithinTab;
    private TabStripItem? _pressedContainer;
    private Point _pressedPoint;
    private int _sourceIndex = -1;
    
    

    protected override Type StyleKeyOverride => typeof(TabStrip);

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is not TabStripItem tsi)
        {
            return;
        }

        tsi.AddHandler(PointerPressedEvent, OnItemPointerPressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        tsi.AddHandler(PointerMovedEvent, OnItemPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        tsi.AddHandler(PointerReleasedEvent, OnItemPointerReleased,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        tsi.PointerCaptureLost += OnPointerCaptureLost;
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is TabStripItem tsi)
        {
            tsi.RemoveHandler(PointerPressedEvent, OnItemPointerPressed);
            tsi.RemoveHandler(PointerMovedEvent, OnItemPointerMoved);
            tsi.RemoveHandler(PointerReleasedEvent, OnItemPointerReleased);
            tsi.PointerCaptureLost -= OnPointerCaptureLost;

            // If the item was removed from the collection.
            if (tsi.DataContext is TabViewModel { IsClosing: true })
            {
                // Reset any transform that might be applied
                tsi.RenderTransform = null;

                // If the item being removed is the one currently being tracked/dragged, stop tracking it.
                if (_pressedContainer == tsi)
                {
                    EndDrag();
                }
            }
        }

        base.ClearContainerForItemOverride(container);
    }

    private void OnItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(this).Properties;
        if (!props.IsLeftButtonPressed)
        {
            return;
        }

        if (sender is not TabStripItem tsi)
        {
            return;
        }

        _pressedContainer = tsi;
        _pressedPoint = e.GetPosition(this);
        _isDragging = false;
        ItemFromContainer(tsi);
        _sourceIndex = IndexFromContainer(tsi);
        _currentTargetIndex = _sourceIndex;

        // Cache positions
        _originalX.Clear();
        foreach (var c in GetRealizedContainers())
        {
            if (c is TabStripItem t)
            {
                _originalX[t] = t.Bounds.X;
            }
        }

        _draggedTabStartX = tsi.Bounds.X;
        _draggedTabWidth = tsi.Bounds.Width;
        _pointerOffsetWithinTab = _pressedPoint.X - _draggedTabStartX;

        // Note: We do NOT capture the pointer immediately. 
        // We wait until the threshold is crossed to avoid interfering with standard clicks.
    }

    private void OnItemPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_pressedContainer == null)
        {
            return;
        }

        var pos = e.GetPosition(this);
        var deltaX = pos.X - _pressedPoint.X;

        // 1. Start Dragging if threshold crossed
        if (!_isDragging)
        {
            if (Math.Abs(deltaX) > DragThreshold)
            {
                _isDragging = true;

                // Now we capture
                e.Pointer.Capture(_pressedContainer);

                if (_pressedContainer is IPseudoClasses pcs)
                {
                    pcs.Set(DraggingPseudoClass, true);
                }

                _pressedContainer.ZIndex = 1000;
            }
            else
            {
                return;
            }
        }

        // 2. Handle Drag Logic
        var dragLeft = pos.X - _pointerOffsetWithinTab;
        var dragCenter = dragLeft + _draggedTabWidth / 2;

        var realized = GetRealizedContainers().OfType<TabStripItem>().ToList();
        if (realized.Count == 0)
        {
            return;
        }

        var newTargetIndex = _sourceIndex;

        for (var i = 0; i < realized.Count; i++)
        {
            var tab = realized[i];
            // Fallback if dictionary is stale
            var tabX = _originalX.TryGetValue(tab, out var val) ? val : tab.Bounds.X;
            var center = tabX + tab.Bounds.Width / 2;

            if (dragCenter > center)
            {
                newTargetIndex = i;
            }
        }

        _currentTargetIndex = newTargetIndex;

        // 3. Visual Updates (Transforms)
        for (var i = 0; i < realized.Count; i++)
        {
            var tab = realized[i];
            var startX = _originalX.TryGetValue(tab, out var val) ? val : tab.Bounds.X;
            double offset = 0;

            if (tab == _pressedContainer)
            {
                offset = dragLeft - startX;
            }
            else
            {
                // Logic: If the item needs to move left or right to fill the gap
                if (_currentTargetIndex > _sourceIndex) // Dragging Right
                {
                    if (i > _sourceIndex && i <= _currentTargetIndex)
                    {
                        offset = -_draggedTabWidth;
                    }
                }
                else if (_currentTargetIndex < _sourceIndex) // Dragging Left
                {
                    if (i >= _currentTargetIndex && i < _sourceIndex)
                    {
                        offset = _draggedTabWidth;
                    }
                }
            }
            
            tab.RenderTransform = new TranslateTransform(offset, 0);
        }

        e.Handled = true;
    }

    private void OnItemPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging && _pressedContainer != null && _currentTargetIndex >= 0)
        {
            // Perform the data move
            TryMoveItem(_sourceIndex, _currentTargetIndex);
            SelectedIndex = _currentTargetIndex;
        }

        EndDrag();
        e.Handled = true;
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        EndDrag();
    }

    private void EndDrag()
    {
        if (_pressedContainer != null)
        {
            if (_pressedContainer is IPseudoClasses pcs)
            {
                pcs.Set(DraggingPseudoClass, false);
            }

            _pressedContainer.Opacity = 1.0;
            _pressedContainer.ZIndex = 0; // RESET ZINDEX
        }

        // Iterate ALL containers and reset their transforms.
        // After the data move, layout will arrange them correctly. 
        // If we leave transforms, they will be visually offset from their new correct positions.
        foreach (var c in GetRealizedContainers())
        {
            c.RenderTransform = null;
        }

        _pressedContainer = null;
        _isDragging = false;
        _sourceIndex = -1;
        _currentTargetIndex = -1;
        _originalX.Clear();
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

        // Clamp newIndex
        if (newIndex < 0)
        {
            newIndex = 0;
        }

        if (newIndex >= list.Count)
        {
            newIndex = list.Count - 1;
        }

        try
        {
            var item = list[oldIndex];
            list.RemoveAt(oldIndex);

            // Adjust insert index because we removed an item
            // If we are moving it forward (0 -> 2), the indices shift down after removal
            // but the target index (2) was calculated based on the list WITH the item.
            // However, your logic `if (newIndex > oldIndex) newIndex--;` depends on how you calculated target.
            // With the "Drag Center" logic, simpler insertion usually works:

            list.Insert(newIndex, item);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void SetTransitions()
    {
        if (Settings.UIProperties.IsTabAnimated)
        {
            
        }
        else
        {
            
        }
    }
}