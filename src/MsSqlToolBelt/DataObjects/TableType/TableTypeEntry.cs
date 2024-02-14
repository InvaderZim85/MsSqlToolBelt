using MsSqlToolBelt.Common;
using MsSqlToolBelt.DataObjects.Common;
using Newtonsoft.Json;
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
    [ClipboardProperty]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date / time
    /// </summary>
    public DateTime CreationDateTime { get; init; }

    /// <summary>
    /// Gets or sets the modification date / time
    /// </summary>
    public DateTime ModifiedDateTime { get; init; }

    /// <summary>
    /// Gets or sets the definition of the table
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public string Definition { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list with the columns
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public List<ColumnEntry> Columns { get; set; } = [];
}