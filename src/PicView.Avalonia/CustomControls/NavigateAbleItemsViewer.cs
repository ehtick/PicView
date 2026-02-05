using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Data;

namespace PicView.Avalonia.CustomControls;

public readonly record struct ItemPosition(int index, Point pos, Size Size)
{
    public int Index { get; init; } = index;
    public Point Position { get; init; } = pos;
    public Size Size { get; init; } = Size;
}

[TemplatePart("PART_ScrollViewer", typeof(AutoScrollViewer))]
public class NavigateAbleItemsViewer : ItemsControl
{
    protected override Type StyleKeyOverride => typeof(NavigateAbleItemsViewer);
    
    private const string CurrentItemPseudoClass = ":currentItem";
    private const string SelectedItemPseudoClass = ":selectedItem";

    private AutoScrollViewer? _scrollViewer;
    private int _internalSelectedIndex = -1;

    public static readonly StyledProperty<int> CurrentItemIndexProperty =
        AvaloniaProperty.Register<NavigateAbleItemsViewer, int>(nameof(CurrentItemIndex), defaultValue: -1, defaultBindingMode: BindingMode.TwoWay);

    public int CurrentItemIndex
    {
        get => GetValue(CurrentItemIndexProperty);
        set => SetValue(CurrentItemIndexProperty, value);
    }

    static NavigateAbleItemsViewer()
    {
        CurrentItemIndexProperty.Changed.AddClassHandler<NavigateAbleItemsViewer>((x, e) => x.OnCurrentItemIndexChanged(e));
    }

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

        var oldIndex = e.OldValue is int val ? val : -1;
        UpdatePseudoClasses(newIndex, CurrentItemPseudoClass, oldIndex);
            
        // Sync internal selection if changed externally (and valid)
        if (newIndex != _internalSelectedIndex && newIndex >= 0 && newIndex < ItemCount)
        {
            SetInternalSelection(newIndex);
        }
        else if (newIndex == -1)
        {
            SetInternalSelection(-1);
        }
    }

    private void SetInternalSelection(int index)
    {
        var oldIndex = _internalSelectedIndex;
        _internalSelectedIndex = index;
        UpdatePseudoClasses(index, SelectedItemPseudoClass, oldIndex);
        
        if (index >= 0)
        {
            ScrollIndexIntoView(index);
        }
    }

    private void UpdatePseudoClasses(int newIndex, string pseudoClass, int oldIndex)
    {
        if (oldIndex >= 0 && oldIndex < ItemCount)
        {
            PseudoClasses.Set(pseudoClass, false);
        }

        if (newIndex < 0 || newIndex >= ItemCount)
        {
            return;
        }

        var newContainer = ContainerFromIndex(newIndex);
        if (newContainer is not null)
        {
            PseudoClasses.Set(pseudoClass, true);
        }
    }

    private void ScrollIndexIntoView(int index)
    {
        if (_scrollViewer == null || index < 0 || index >= ItemCount) return;

        var container = ContainerFromIndex(index);
        // Use Dispatcher.Post to allow layout to settle if needed, or just run it. 
        // AutoScrollViewer might not have BringIntoView support natively if it inherits ScrollViewer. 
        // ScrollViewer has BringIntoView.
        container?.BringIntoView();
            
        // Or use specific logic from GalleryNavigation if standard BringIntoView isn't enough?
        // "it needs to... scroll items into view"
        // Standard BringIntoView should work if items are in the ScrollViewer.
    }

    public void Navigate(NavigationDirection direction)
    {
        if (ItemCount == 0) return;

        var startIndex = _internalSelectedIndex;
        if (startIndex < 0) startIndex = CurrentItemIndex;
        if (startIndex < 0) startIndex = 0;
        if (startIndex >= ItemCount) startIndex = ItemCount - 1;

        var items = GetItemPositions();
        if (items.Count == 0) return;

        // Ensure we have a valid start position
        if (items.All(x => x.Index != startIndex))
        {
            // If current index isn't realized/found, default to first or something?
            // Or just use the first item in the list
            startIndex = items.First().Index;
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

        if (targetItem is { } validItem)
        {
            SetInternalSelection(validItem.Index);
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
        var candidates = items.Where(item => item.Position.Y + item.Size.Height <= currentItem.Position.Y).ToList();
        return candidates.OrderByDescending(item => item.Position.Y).ThenBy(item => Math.Abs(item.Position.X - currentItem.Position.X)).FirstOrDefault();
    }

    private static ItemPosition? GetClosestItemBelow(ItemPosition currentItem, IEnumerable<ItemPosition> items)
    {
        var candidates = items.Where(item => item.Position.Y >= currentItem.Position.Y + currentItem.Size.Height).ToList();
        return candidates.OrderBy(item => item.Position.Y).ThenBy(item => Math.Abs(item.Position.X - currentItem.Position.X)).FirstOrDefault();
    }

    private static ItemPosition? GetClosestItemLeft(ItemPosition currentItem, IEnumerable<ItemPosition> items)
    {
        var candidates = items.Where(item => item.Position.X + item.Size.Width <= currentItem.Position.X).ToList();
        return candidates.OrderByDescending(item => item.Position.X).ThenBy(item => Math.Abs(item.Position.Y - currentItem.Position.Y)).FirstOrDefault();
    }

    private static ItemPosition? GetClosestItemRight(ItemPosition currentItem, IEnumerable<ItemPosition> items)
    {
        var candidates = items.Where(item => item.Position.X >= currentItem.Position.X + currentItem.Size.Width).ToList();
        return candidates.OrderBy(item => item.Position.X).ThenBy(item => Math.Abs(item.Position.Y - currentItem.Position.Y)).FirstOrDefault();
    }
}
