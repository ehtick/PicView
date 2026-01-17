using Avalonia;
using Avalonia.Controls;

namespace PicView.Avalonia.UI;

public class SearchProperties : AvaloniaObject
{
    public static readonly AttachedProperty<string?> KeywordsProperty =
        AvaloniaProperty.RegisterAttached<SearchProperties, Control, string?>("Keywords");

    public static string? GetKeywords(Control element) => element.GetValue(KeywordsProperty);
    public static void SetKeywords(Control element, string? value) => element.SetValue(KeywordsProperty, value);

    // IsMatch Property (Nullable Bool)
    // null = Not searching (Default)
    // true = Matched
    // false = Not Matched
    public static readonly AttachedProperty<bool?> IsMatchProperty =
        AvaloniaProperty.RegisterAttached<SearchProperties, Control, bool?>("IsMatch");

    public static bool? GetIsMatch(Control element) => element.GetValue(IsMatchProperty);
    public static void SetIsMatch(Control element, bool? value) => element.SetValue(IsMatchProperty, value);
}