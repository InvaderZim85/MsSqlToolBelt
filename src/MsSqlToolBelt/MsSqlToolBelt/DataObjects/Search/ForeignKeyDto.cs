namespace MsSqlToolBelt.DataObjects.Search;

/// <summary>
/// Represents a foreign key
/// </summary>
internal class ForeignKeyDto
{
    /// <summary>
    /// Gets or sets the name of the foreign key
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the column which is used as foreign key
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the referenced table
    /// </summary>
    public string ReferencedTableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the referenced column
    /// </summary>
    public string ReferencedColumnName { get; set; } = string.Empty;
}