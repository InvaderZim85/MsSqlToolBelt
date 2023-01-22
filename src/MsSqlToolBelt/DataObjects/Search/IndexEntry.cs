namespace MsSqlToolBelt.DataObjects.Search;

/// <summary>
/// Represents an table index
/// </summary>
public class IndexEntry
{
    /// <summary>
    /// Gets or sets the name of the index
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of the columns
    /// </summary>
    public string Columns { get; set; } = string.Empty;
}