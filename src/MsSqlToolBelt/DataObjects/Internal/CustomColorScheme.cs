using System.Windows.Media;

namespace MsSqlToolBelt.DataObjects.Internal;

/// <summary>
/// Represents a custom color scheme
/// </summary>
public sealed class CustomColorScheme
{
    /// <summary>
    /// Gets or sets the name of the color
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hex value of the color
    /// <para />
    /// For example: <c>#FFFF0000</c> for red
    /// </summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Gets the color value
    /// </summary>
    public Color ColorValue => ColorConverter.ConvertFromString(Color) is Color color
        ? color // The custom color
        : System.Windows.Media.Color.FromRgb(0, 138, 0); // Default color "Emerald"
}