using Avalonia;
using Avalonia.Controls;

namespace PicView.Avalonia.CustomControls;

public class ResponsiveTabPanel : Panel
{
    public static readonly StyledProperty<double> MinTabWidthProperty =
        AvaloniaProperty.Register<ResponsiveTabPanel, double>(
            nameof(MinTabWidth), 140.0);

    public double MinTabWidth
    {
        get => GetValue(MinTabWidthProperty);
        set => SetValue(MinTabWidthProperty, value);
    }

    protected override Size MeasureOverride(Size available)
    {
        var n = Children.Count;
        if (n is 0)
        {
            return default;
        }

        // When inside ScrollViewer available.Width can be Infinity.
        // In that case fall back to our current Bounds.Width
        var width = double.IsInfinity(available.Width) ? Bounds.Width : available.Width;

        // If this is the very first measure, Bounds.Width can be 0.
        // Just behave as if there is no width yet.
        if (width <= 0 && !double.IsInfinity(available.Width))
            width = available.Width;

        double maxH = 0;

        switch (n)
        {
            case 1:
                Children[0].Measure(new Size(width, available.Height));
                maxH = Children[0].DesiredSize.Height;
                // Important: report full width
                return new Size(width, maxH);
            case >= 2 and <= 4:
            {
                var each = width / n;
                foreach (var c in Children)
                {
                    c.Measure(new Size(each, available.Height));
                    maxH = Math.Max(maxH, c.DesiredSize.Height);
                }

                // **This must be the full available width, not MinTabWidth * n**
                return new Size(width, maxH);
            }
        }

        // n >= 5  -> fixed MinTabWidth, scrollable
        var w = Math.Max(MinTabWidth, 0.0);
        foreach (var c in Children)
        {
            c.Measure(new Size(w, available.Height));
            maxH = Math.Max(maxH, c.DesiredSize.Height);
        }

        return new Size(w * n, maxH);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var n = Children.Count;
        switch (n)
        {
            case 0:
                return finalSize;
            case >= 2 and <= 4:
            {
                var rawEach = finalSize.Width / n;
                double x = 0;

                for (var i = 0; i < n; ++i)
                {
                    var w = (i == n - 1)
                        ? finalSize.Width - x  // last one takes the remainder
                        : rawEach;

                    Children[i].Arrange(new Rect(x, 0, w, finalSize.Height));
                    x += w;
                }

                return finalSize;
            }
        }

        // n >= 5
        var fixedWidth = MinTabWidth;
        double pos = 0;
        foreach (var c in Children)
        {
            c.Arrange(new Rect(pos, 0, fixedWidth, finalSize.Height));
            pos += fixedWidth;
        }

        return new Size(pos, finalSize.Height);
    }

}
