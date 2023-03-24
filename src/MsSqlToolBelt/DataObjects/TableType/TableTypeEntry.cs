using MsSqlToolBelt.DataObjects.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.TableType;

/// <summary>
/// Represents a table type
/// </summary>
public class TableTypeEntry
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
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public List<ColumnEntry> Columns { get; set; } = new();
}