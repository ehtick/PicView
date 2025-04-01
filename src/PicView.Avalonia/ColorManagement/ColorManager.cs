using Avalonia;
using Avalonia.Media;
using PicView.Core.ColorHandling;

namespace PicView.Avalonia.ColorManagement;

/// <summary>
/// Manages accent colors based on the selected color theme.
/// </summary>
public static class ColorManager
{
    // Color definitions for each theme
    private static readonly Dictionary<int, ThemeColors> ThemeColorMap = new()
    {
        [(int)ColorOptions.Blue] = new ThemeColors(
            logoLight: Color.FromRgb(225, 210, 80),
            logoDark: Color.FromRgb(255, 240, 90),
            primary: Color.FromRgb(26, 140, 240),
            secondary: Color.FromArgb(242, 66, 163, 249),
            buttonColor: Color.FromRgb(26, 140, 240)
        ),
        [(int)ColorOptions.Pink] = new ThemeColors(
            logoLight: Color.FromRgb(250, 180, 38),
            logoDark: Color.FromRgb(255, 237, 38),
            primary: Color.FromRgb(255, 53, 197),
            secondary: Color.FromArgb(230, 255, 98, 210),
            buttonColor: Color.FromRgb(255, 53, 197)
        ),
        [(int)ColorOptions.Orange] = new ThemeColors(
            logoLight: Color.FromRgb(248, 175, 60),
            logoDark: Color.FromRgb(248, 175, 60),
            primary: Color.FromRgb(219, 91, 61),
            secondary: Color.FromArgb(242, 245, 121, 57),
            buttonColor: Color.FromRgb(219, 91, 61)
        ),
        [(int)ColorOptions.Ruby] = new ThemeColors(
            logoLight: Color.FromRgb(175, 157, 38),
            logoDark: Color.FromRgb(209, 237, 93),
            primary: Color.FromRgb(255, 32, 110), 
            secondary: Color.FromArgb(242, 255, 80, 140), 
            buttonColor: Color.FromRgb(255, 32, 110)
        ),
        [(int)ColorOptions.Red] = new ThemeColors(
            logoLight: Color.FromRgb(250, 192, 92),
            logoDark: Color.FromRgb(250, 192, 92),
            primary: Color.FromRgb(249, 17, 16),
            secondary: Color.FromArgb(242, 249, 61, 60),
            buttonColor: Color.FromRgb(249, 17, 16)
        ),
        [(int)ColorOptions.Teal] = new ThemeColors(
            logoLight: Color.FromRgb(254, 172, 150),
            logoDark: Color.FromRgb(254, 172, 150),
            primary: Color.FromRgb(68, 161, 160),
            secondary: Color.FromArgb(242, 77, 195, 194),
            buttonColor: Color.FromRgb(68, 161, 160)
        ),
        [(int)ColorOptions.Raspberry] = new ThemeColors(
            logoLight: Color.FromRgb(228, 209, 17),
            logoDark: Color.FromRgb(228, 209, 17),
            primary: Color.FromRgb(181, 69, 126),
            secondary: Color.FromArgb(242, 201, 100, 156),
            buttonColor: Color.FromRgb(181, 69, 126)
        ),
        [(int)ColorOptions.Golden] = new ThemeColors(
            logoLight: Color.FromRgb(226, 180, 224),
            logoDark: Color.FromRgb(255, 253, 42),
            primary: Color.FromRgb(254, 169, 85),
            secondary: Color.FromArgb(242, 249, 187, 125),
            buttonColor: Color.FromRgb(254, 169, 85)
        ),
        [(int)ColorOptions.Purple] = new ThemeColors(
            logoLight: Color.FromRgb(226, 141, 223),
            logoDark: Color.FromRgb(237, 184, 135),
            primary: Color.FromRgb(151, 56, 235),
            secondary: Color.FromArgb(242, 194, 95, 255),
            buttonColor: Color.FromRgb(151, 56, 235)
        ),
        [(int)ColorOptions.Cyan] = new ThemeColors(
            logoLight: Color.FromRgb(215, 200, 70),
            logoDark: Color.FromRgb(255, 253, 66),
            primary: Color.FromRgb(27, 161, 226),
            secondary: Color.FromArgb(242, 89, 186, 233),
            buttonColor: Color.FromRgb(27, 161, 226)
        ),
        [(int)ColorOptions.Magenta] = new ThemeColors(
            logoLight: Color.FromRgb(226, 141, 223),
            logoDark: Color.FromRgb(255, 237, 38),
            primary: Color.FromRgb(230, 139, 238),
            secondary: Color.FromArgb(242, 255, 108, 212),
            buttonColor: Color.FromRgb(230, 139, 238)
        ),
        [(int)ColorOptions.Emerald] = new ThemeColors(
            logoLight: Color.FromRgb(255, 253, 42),
            logoDark: Color.FromRgb(255, 253, 42),
            primary: Color.FromRgb(0, 114, 0), // Changed from (32, 231, 107) to #007200
            secondary: Color.FromArgb(242, 50, 164, 50), // Updated secondary color to match new primary
            buttonColor: Color.FromRgb(0, 114, 0) // Button color matches primary
        )
    };

    /// <summary>
    /// Gets the logo accent color based on the current color theme.
    /// </summary>
    public static Color LogoAccentColor => GetThemeColors().GetLogoColor(Settings.Theme.Dark);

    /// <summary>
    /// Gets the secondary accent color based on the current color theme.
    /// A brighter shade of the primary accent color.
    /// </summary>
    public static Color SecondaryAccentColor => GetThemeColors().Secondary;

    /// <summary>
    /// Gets the primary accent color based on the current color theme.
    /// </summary>
    public static Color PrimaryAccentColor => GetThemeColors().Primary;
    
    /// <summary>
    /// Gets the button color for a specific theme
    /// </summary>
    public static Color GetButtonColor(int themeIndex) =>
        ThemeColorMap.TryGetValue(themeIndex, out var colors) ? colors.ButtonColor : ThemeColorMap[0].ButtonColor;

    /// <summary>
    /// Gets the color set for the current theme
    /// </summary>
    private static ThemeColors GetThemeColors()
    {
        var themeIndex = Settings.Theme.ColorTheme;
        
        if (ThemeColorMap.TryGetValue(themeIndex, out var colors))
        {
            return colors;
        }

        if (themeIndex is 1)
        {
            return ThemeColorMap[0];
        }
        
        throw new ArgumentOutOfRangeException(nameof(Settings.Theme.ColorTheme), 
            $"Color theme index {themeIndex} is not supported");
    }

    /// <summary>
    /// Updates the accent colors in the application resources based on the selected color theme.
    /// </summary>
    /// <param name="colorTheme">The color theme index to apply.</param>
    public static void UpdateAccentColors(int colorTheme)
    {
        Settings.Theme.ColorTheme = colorTheme;

        var primaryBrush = new SolidColorBrush(PrimaryAccentColor);
        var secondaryBrush = new SolidColorBrush(SecondaryAccentColor);
        var logoAccentBrush = new SolidColorBrush(LogoAccentColor);
        
        if (Settings.Theme.GlassTheme)
        {
            GlassThemeHelper.GlassThemeUpdates();
        }

        // Update application resources with the new brushes
        UpdateResourceIfExists("AccentColor", primaryBrush);
        UpdateResourceIfExists("SecondaryAccentColor", secondaryBrush);
        UpdateResourceIfExists("LogoAccentColor", logoAccentBrush);
    }
    
    /// <summary>
    /// Updates a resource if it exists in the application resources
    /// </summary>
    private static void UpdateResourceIfExists(string resourceKey, object value)
    {
        if (Application.Current.TryGetResource(resourceKey, Application.Current.RequestedThemeVariant, out _))
        {
            Application.Current.Resources[resourceKey] = value;
        }
    }
    
    /// <summary>
    /// Represents a set of colors for a theme
    /// </summary>
    private readonly struct ThemeColors(Color logoLight, Color logoDark, Color primary, Color secondary, Color buttonColor)
    {
        private Color LogoLight { get; } = logoLight;
        private Color LogoDark { get; } = logoDark;
        public Color Primary { get; } = primary;
        public Color Secondary { get; } = secondary;
        public Color ButtonColor { get; } = buttonColor;

        /// <summary>
        /// Gets the appropriate logo color based on dark mode setting
        /// </summary>
        public Color GetLogoColor(bool isDarkMode) => isDarkMode ? LogoDark : LogoLight;
    }
}