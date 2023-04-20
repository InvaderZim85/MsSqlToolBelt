namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a id / text entry
/// </summary>
internal class IdTextEntry
{
    /// <summary>
    /// Gets the id of the entry
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the text of the item
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Gets the original bound item
    /// </summary>
    public object? BoundItem { get; init; }

    /// <summary>
    /// Creates a new, empty instance of the <see cref="IdTextEntry"/>
    /// </summary>
    public IdTextEntry() { }

    /// <summary>
    /// Creates a new instance of the <see cref="IdTextEntry"/>
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="text">The text</param>
    /// <param name="boundItem">The bound item (optional)</param>
    public IdTextEntry(int id, string text, object? boundItem = null)
    {
        Id = id;
        Text = text;
        BoundItem = boundItem;
    }

    /// <summary>
    /// Returns the <see cref="Text"/> of the entry
    /// </summary>
    /// <returns>The text</returns>
    public override string ToString()
    {
        return Text;
    }
}