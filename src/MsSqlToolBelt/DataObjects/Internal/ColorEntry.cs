namespace MsSqlToolBelt.DataObjects.Internal;

/// <summary>
/// Represents a color entry (only needed for the settings)
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ColorEntry"/>
/// </remarks>
/// <param name="name"></param>
/// <param name="customColor"></param>
internal class ColorEntry(string name, bool customColor)
{
    /// <summary>
    /// Gets the name of the color
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the value which indicates if the color is a custom color
    /// </summary>
    public bool CustomColor { get; } = customColor;

    /// <inheritdoc />
    public override string ToString()
    {
        return CustomColor ? $"{Name} (Custom)" : Name;
    }
}