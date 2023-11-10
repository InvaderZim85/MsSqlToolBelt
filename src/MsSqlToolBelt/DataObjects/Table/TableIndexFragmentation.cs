namespace MsSqlToolBelt.DataObjects.Table;

/// <summary>
/// Represents the fragmentation of a table index
/// </summary>
public class TableIndexFragmentation
{
    /// <summary>
    /// Gets or sets the name of the index
    /// </summary>
    public string Index { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fragmentation of the index
    /// </summary>
    public decimal Fragmentation { get; set; }
}