using System.Globalization;
using Avalonia.Data.Converters;

namespace PicView.Avalonia.Converters;

public class SearchVisibilityConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // Value 0: SearchQuery (string)
        // Value 1: Terms (IEnumerable<string>)

        if (values is not { Count: 2 })
            return true;

        var query = values[0] as string;

        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        if (values[1] is not IEnumerable<string> terms)
        {
            return false;
        }
        
        foreach (var term in terms)
        {
            if (term != null && term.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
