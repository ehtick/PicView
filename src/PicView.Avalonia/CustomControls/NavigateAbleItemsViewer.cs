using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace PicView.Avalonia.CustomControls;

public readonly record struct ItemPosition(int Index, Point Position, Size Size);

[TemplatePart("PART_ScrollViewer", typeof(AutoScrollViewer))]
public class NavigateAbleItemsViewer : ItemsControl
{
    protected override Type StyleKeyOverride => typeof(NavigateAbleItemsViewer);

    private AutoScrollViewer? _scrollViewer;
    
    public int SelectedItemIndex { get; private set; } 

    public NavigateAbleItemsViewer()
    {
        PointerWheelChanged += OnPointerWheelChanged;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (Settings.Zoom.HorizontalReverseScroll)
        {
            if (e.Delta.Y < 0)
            {
                _scrollViewer.LineRight();
            }
            else
            {
                _scrollViewer.LineLeft();
            }
        }
        else
        {
            if (e.Delta.Y > 0)
            {
                _scrollViewer.LineRight();
            }
            else
            {
                _scrollViewer.LineLeft();
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _scrollViewer = e.NameScope.Find<AutoScrollViewer>("PART_ScrollViewer");
    }

    private void OnCurrentItemIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not int newIndex)
        {
            return;
        }
            
        // Sync internal selection if changed externally (and valid)
        if (newIndex != SelectedItemIndex && newIndex >= 0 && newIndex < ItemCount)
        {
            SetInternalSelection(newIndex);
        }
    }

    private void SetInternalSelection(int index)
    {
        var oldIndex = SelectedItemIndex;
        SelectedItemIndex = index;
        
        if (index >= 0)
        {
            SetSelectedItemAndScrollIntoView(index, oldIndex);
        }
    }

    private void SetSelectedItemAndScrollIntoView(int index, int prevIndex)
    {
        if (_scrollViewer == null || index < 0 || index >= ItemCount 
            || ContainerFromIndex(index) is not ContentPresenter presenter)
        {
            return;
        }

        if (presenter.Child is not NavigateAbleItem item)
        {
            return;
        }
    
        item.BringIntoView();
        item.SetSelected(true);

        // Only clear the old item if we actually moved to a different index
        if (prevIndex == index || prevIndex < 0 || prevIndex >= ItemCount 
            || ContainerFromIndex(prevIndex) is not ContentPresenter prevPresenter)
        {
            return;
        }
        
        if (prevPresenter.Child is not NavigateAbleItem prevItem)
        {
            return;
        }
        
        prevItem.SetSelected(false);
    }

    public void Navigate(NavigationDirection direction)
    {
        if (ItemCount == 0) return;

        if (direction == NavigationDirection.First)
        {
            SetInternalSelection(0);
            return;
        }

        if (direction == NavigationDirection.Last)
        {
            SetInternalSelection(ItemCount - 1);
            return;
        }

        var startIndex = SelectedItemIndex;
        if (startIndex < 0) startIndex = SelectedItemIndex;
        if (startIndex < 0) startIndex = 0;
        if (startIndex >= ItemCount) startIndex = ItemCount - 1;

        var items = GetItemPositions();
        if (items.Count == 0) return;

        if (items.All(x => x.Index != startIndex))
        {
            startIndex = items.Last().Index;
        }

        var currentItemPos = items.FirstOrDefault(x => x.Index == startIndex);

        var targetItem = direction switch
        {
            NavigationDirection.Up => GetClosestItemAbove(currentItemPos, items),
            NavigationDirection.Down => GetClosestItemBelow(currentItemPos, items),
            NavigationDirection.Left => GetClosestItemLeft(currentItemPos, items),
            NavigationDirection.Right => GetClosestItemRight(currentItemPos, items),
            _ => null
        };

        if (targetItem is not { } validItem)
        {
            return;
        }

        if (validItem.Index >= items.Count)
        {
            return;
        }
            
        switch (direction)
        {                
            case NavigationDirection.Left:
            case NavigationDirection.Right:
                if (validItem.Index is 0)
                {
                    // Don't loop or jump
                    return;
                }
                SetInternalSelection(validItem.Index);
                break;
                    
            case NavigationDirection.Down:
                if (validItem.Index is 0)
                {
                    // If at bottom of column, go to top of next column
                    var nextColumnItem = GetNextColumnTopItem(currentItemPos, items);
                    SetInternalSelection(nextColumnItem?.Index ?? startIndex);
                }
                else
                {
                    SetInternalSelection(validItem.Index);
                }

                break;
                    
            case NavigationDirection.Up:
                if (validItem.Index is 0)
                {
                    // If at top of column, go to bottom of previous column
                    var prevColumnItem = GetPreviousColumnBottomItem(currentItemPos, items);
                    if (currentItemPos.Index is 1)
                    {
                        SetInternalSelection(0);
                    }
                    else
                    {
                        SetInternalSelection(prevColumnItem?.Index ?? startIndex);
                    }
                }
                else
                {
                    SetInternalSelection(validItem.Index);
                }
                break;
                    
            default:
                return;
        }
    }

    private List<ItemPosition> GetItemPositions()
    {
        var list = new List<ItemPosition>();
        
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is not { IsVisible: true })
            {
                continue;
            }
                
            var position = container.TranslatePoint(new Point(0, 0), this);
            if (position.HasValue)
            {
                list.Add(new ItemPosition
                {
                    Index = i,
                    Position = position.Value,
                    Size = container.Bounds.Size
                });
            }
        }
        return list;
    }

    private static ItemPosition? GetClosestItemAbove(ItemPosition currentItem, IEnumerable<ItemPosition> items)
    {
        var candidates = items.Where(item => item.Position.Y + item.Size.Height <= currentItem.Position.Y);
        return candidates.OrderByDescending(item => item.Position.Y).ThenBy(item => Math.Abs(item.Position.X - currentItem.Position.X)).FirstOrDefault();
    }

    private static ItemPosition? GetClosestItemBelow(ItemPosition currentItem, IEnumerable<ItemPosition> items)
    {
        var candidates = items.Where(item => item.Position.Y >= currentItem.Position.Y + currentItem.Size.Height);
        return candidates.OrderBy(item => item.Position.Y).ThenBy(item => Math.Abs(item.Position.X - currentItem.Position.X)).FirstOrDefault();
    }

    private static ItemPosition? GetClosestItemLeft(ItemPosition currentItem, IEnumerable<ItemPosition> items)
    {
        var candidates = items.Where(item => item.Position.X + item.Size.Width <= currentItem.Position.X);
        return candidates.OrderByDescending(item => item.Position.X).ThenBy(item => Math.Abs(item.Position.Y - currentItem.Position.Y)).FirstOrDefault();
    }

    private static ItemPosition? GetClosestItemRight(ItemPosition currentItem, IEnumerable<ItemPosition> items)
    {
        var candidates = items.Where(item => item.Position.X >= currentItem.Position.X + currentItem.Size.Width);
        return candidates.OrderBy(item => item.Position.X).ThenBy(item => Math.Abs(item.Position.Y - currentItem.Position.Y)).FirstOrDefault();
    }

    private static ItemPosition? GetNextColumnTopItem(ItemPosition currentItem, IEnumerable<ItemPosition> items)
    {
        // Find items to the right of current item (next column)
        var nextColumnItems = items
            .Where(item => item.Position.X >= currentItem.Position.X + currentItem.Size.Width)
            .OrderBy(item => item.Position.X)
            .ToList();

        if (nextColumnItems.Count is 0)
        {
            return null;
        }

        // Get the X position of the next column
        var nextColumnX = nextColumnItems.First().Position.X;

        // Find the topmost item in that column
        return nextColumnItems
            .Where(item => Math.Abs(item.Position.X - nextColumnX) < 1.0) // Same column (account for floating point)
            .OrderBy(item => item.Position.Y)
            .FirstOrDefault();
    }

    private static ItemPosition? GetPreviousColumnBottomItem(ItemPosition currentItem, IEnumerable<ItemPosition> items)
    {
        // Find items to the left of current item (previous column)
        var prevColumnItems = items
            .Where(item => item.Position.X + item.Size.Width <= currentItem.Position.X)
            .OrderByDescending(item => item.Position.X)
            .ToList();

        if (prevColumnItems.Count == 0)
        {
            return null;
        }

        // Get the X position of the previous column
        var prevColumnX = prevColumnItems.First().Position.X;

        // Find the bottommost item in that column
        return prevColumnItems
            .Where(item => Math.Abs(item.Position.X - prevColumnX) < 1.0) // Same column (account for floating point)
            .OrderByDescending(item => item.Position.Y)
            .FirstOrDefault();
    }
}