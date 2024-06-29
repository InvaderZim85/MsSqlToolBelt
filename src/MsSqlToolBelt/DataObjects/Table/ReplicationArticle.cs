using MsSqlToolBelt.DataObjects.Common;

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
    /// Gets or sets the name of the article
    /// </summary>
    public string Article { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the table name
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the destination table
    /// </summary>
    public string DestinationTable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the insert command
    /// </summary>
    public string InsertCommand { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the update command
    /// </summary>
    public string UpdateCommand { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the delete command
    /// </summary>
    public string DeleteCommand { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicates whether the table has a filter
    /// </summary>
    public bool HasFilter { get; set; }

    /// <summary>
    /// Gets or sets the filter
    /// </summary>
    public string FilterQuery { get; set; } = string.Empty;
}