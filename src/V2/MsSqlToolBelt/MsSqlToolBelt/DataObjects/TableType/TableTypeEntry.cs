using System.Collections.Generic;
using MsSqlToolBelt.DataObjects.Common;

namespace MsSqlToolBelt.DataObjects.TableType;

/// <summary>
/// Represents a table type
/// </summary>
internal class TableTypeEntry
{
    /// <summary>
    /// Gets or sets the id of the table type
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the table type
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list with the columns
    /// </summary>
    public List<ColumnEntry> Columns { get; set; } = new();
}