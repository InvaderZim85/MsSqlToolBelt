using System.Collections.Generic;
using System.Linq;
using MsSqlToolBelt.DataObjects.Common;
using Newtonsoft.Json;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.ClassGen;

/// <summary>
/// Represents a table (only needed for the class generator). For the normal table, see <see cref="Common.TableEntry"/>
/// </summary>
public class TableDto
{
    /// <summary>
    /// Gets or sets the name of the table
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original table
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public TableEntry Table { get; set; } = new();

    /// <summary>
    /// Gets or sets the list with the columns
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public List<ColumnDto> Columns { get; set; } = new();

    /// <summary>
    /// Converts the columns of the <see cref="Table"/> and stores them into <see cref="Columns"/>
    /// </summary>
    public void SetColumns()
    {
        if (Table.Columns.Any())
            Columns = Table.Columns.Select(s => (ColumnDto) s).ToList();
    }

    /// <summary>
    /// Converts the <see cref="TableEntry"/> into a <see cref="TableDto"/>
    /// </summary>
    /// <param name="table">The table</param>
    public static explicit operator TableDto(TableEntry table)
    {
        var result = new TableDto
        {
            Name = table.Name,
            Table = table
        };

        if (table.Columns.Any())
            result.Columns = table.Columns.Select(s => (ColumnDto) s).ToList();

        return result;
    }
}