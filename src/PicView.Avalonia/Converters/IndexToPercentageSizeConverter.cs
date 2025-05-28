using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace PicView.Avalonia.Converters;

public class IndexToPercentageSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!int.TryParse(value?.ToString(), out var index) ||
            !int.TryParse(parameter?.ToString(), out var parameterIndex))
        {
            return BindingOperations.DoNothing;
        }

        return index switch
        {
            1 => 30,
            2 when parameterIndex is 1 => 30,
            2 => 50,
            3 when parameterIndex is 1 => 70,
            3 when parameterIndex is 2 => 50,
            3 => 30,
            4 when parameterIndex is 1 => 70,
            4 when parameterIndex is 2 => 50,
            4 when parameterIndex is 3 => 30,
            4 => 15,
            5 when parameterIndex is 1 => 80,
            5 when parameterIndex is 2 => 70,
            5 when parameterIndex is 3 => 50,
            5 when parameterIndex is 4 => 30,
            5 => 15,
            6 when parameterIndex is 1 => 80,
            6 when parameterIndex is 2 => 70,
            6 when parameterIndex is 3 => 60,
            6 when parameterIndex is 4 => 50,
            6 when parameterIndex is 5 => 40,
            6 => 30,
            7 when parameterIndex is 1 => 85,
            7 when parameterIndex is 2 => 75,
            7 when parameterIndex is 3 => 65,
            7 when parameterIndex is 4 => 50,
            7 when parameterIndex is 5 => 40,
            7 when parameterIndex is 6 => 30,
            7 => 20,
            _ => BindingOperations.DoNothing
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}