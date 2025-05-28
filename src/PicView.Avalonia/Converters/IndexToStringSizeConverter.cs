using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using PicView.Core.Localization;

namespace PicView.Avalonia.Converters;

public class IndexToStringSizeConverter : IValueConverter
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
            1 => TranslationManager.Translation.Thumbnail ?? "Thumb",
            2 when parameterIndex is 1 => "medium",
            2 => "small",
            3 when parameterIndex is 1 => "large",
            3 when parameterIndex is 2 => "medium",
            3 => "small",
            4 when parameterIndex is 1 => "large",
            4 when parameterIndex is 2 => "medium",
            4 when parameterIndex is 3 => "small",
            4 => "xs",
            5 when parameterIndex is 1 => "xl",
            5 when parameterIndex is 2 => "large",
            5 when parameterIndex is 3 => "medium",
            5 when parameterIndex is 4 => "small",
            5 => "xs",
            6 when parameterIndex is 1 => "xl",
            6 when parameterIndex is 2 => "large",
            6 when parameterIndex is 3 => "medium",
            6 when parameterIndex is 4 => "small",
            6 when parameterIndex is 5 => "xs",
            6 => "xxs",
            7 when parameterIndex is 1 => "xxl",
            7 when parameterIndex is 2 => "xl",
            7 when parameterIndex is 3 => "large",
            7 when parameterIndex is 4 => "medium",
            7 when parameterIndex is 5 => "small",
            7 when parameterIndex is 6 => "xs",
            7 => "xxs",
            _ => BindingOperations.DoNothing
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}