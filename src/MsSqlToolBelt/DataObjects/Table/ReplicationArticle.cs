namespace MsSqlToolBelt.DataObjects.Table;

/// <summary>
/// Represents a replication article (aka table)
/// </summary>
public sealed class ReplicationArticle
{
    /// <summary>
    /// Gets or sets the name of the publication
    /// </summary>
    public string Publication { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the database
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the schema to witch the table is assigned to
    /// </summary>
    public string Schema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the article
    /// </summary>
    public string Article { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the table
    /// </summary>
    public string Table { get; set; } = string.Empty;
}