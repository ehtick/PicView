using Avalonia;
using Avalonia.Controls;

namespace PicView.Avalonia.UI;

public class SearchProperties : AvaloniaObject
{
    public static readonly AttachedProperty<IEnumerable<string>> TermsProperty =
        AvaloniaProperty.RegisterAttached<SearchProperties, Control, IEnumerable<string>>("Terms");

    public static IEnumerable<string> GetTerms(Control element)
    {
        return element.GetValue(TermsProperty);
    }

    public static void SetTerms(Control element, IEnumerable<string> value)
    {
        element.SetValue(TermsProperty, value);
    }
}
