using PicView.Avalonia.Converters;
using System.Globalization;

namespace PicView.Tests;

public class SearchVisibilityConverterTests
{
    private readonly SearchVisibilityConverter _converter = new();

    [Fact]
    public void Convert_ReturnsTrue_WhenSearchQueryIsEmpty()
    {
        var result = _converter.Convert(
            new List<object?> { string.Empty, new List<string> { "term" } },
            typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.True((bool)result);
    }

    [Fact]
    public void Convert_ReturnsTrue_WhenSearchQueryIsWhitespace()
    {
        var result = _converter.Convert(
            new List<object?> { "   ", new List<string> { "term" } },
            typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.True((bool)result);
    }
    
    [Fact]
    public void Convert_ReturnsTrue_WhenSearchQueryIsNull()
    {
        var result = _converter.Convert(
            new List<object?> { null, new List<string> { "term" } },
            typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.True((bool)result);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenTermsAreNull()
    {
        var result = _converter.Convert(
            new List<object?> { "search", null },
            typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.False((bool)result);
    }

    [Fact]
    public void Convert_ReturnsTrue_WhenTermMatchesQuery()
    {
        var result = _converter.Convert(
            new List<object?> { "Theme", new List<string> { "Dark Theme" } },
            typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.True((bool)result);
    }

    [Fact]
    public void Convert_ReturnsTrue_WhenTermMatchesQuery_CaseInsensitive()
    {
        var result = _converter.Convert(
            new List<object?> { "theme", new List<string> { "DARK THEME" } },
            typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.True((bool)result);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenNoTermMatchesQuery()
    {
        var result = _converter.Convert(
            new List<object?> { "Language", new List<string> { "Theme", "Color" } },
            typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.False((bool)result);
    }
}
