namespace MsSqlToolBelt.DataObjects.Search;

/// <summary>
/// Represents an index of a table
/// </summary>
internal class IndexDto
{
    /// <summary>
    /// Gets or sets the name of the index
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the index column
    /// </summary>
    public string Column { get; set; } = string.Empty;
}