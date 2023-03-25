using CommunityToolkit.Mvvm.ComponentModel;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.TableType;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.ClassGen;

/// <summary>
/// Represents a table (only needed for the class generator). For the normal table, see <see cref="Common.TableEntry"/>
/// </summary>
public class TableDto : ObservableObject
{
    /// <summary>
    /// Gets or sets the name of the table
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Backing field for <see cref="Alias"/>
    /// </summary>
    private string _alias = string.Empty;

    /// <summary>
    /// Gets or sets the alias of the column
    /// </summary>
    [Appearance(Ignore = true)]
    public string Alias
    {
        get => _alias;
        set
        {
            SetProperty(ref _alias, value);
            Use = !string.IsNullOrEmpty(value);
        }
    }

    /// <summary>
    /// Backing field for <see cref="Use"/>
    /// </summary>
    private bool _use = true;

    /// <summary>
    /// Gets or sets the value which indicates if the column should be used
    /// </summary>
    [Appearance(Ignore = true)]
    public bool Use
    {
        get => _use;
        set => SetProperty(ref _use, value);
    }

    /// <summary>
    /// Gets or sets the original table
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public object Table { get; init; } = new();

    /// <summary>
    /// Gets the schema of the table
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public string Schema => Table is TableEntry table ? table.Schema : string.Empty;

    /// <summary>
    /// Gets or sets the list with the columns
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public List<ColumnDto> Columns { get; set; } = new();

    /// <summary>
    /// Gets the type of the table
    /// </summary>
    public TableDtoType Type { get; init; }

    /// <summary>
    /// Converts the columns of the <see cref="Table"/> and stores them into <see cref="Columns"/>
    /// </summary>
    public void SetColumns()
    {
        Columns = Table switch
        {
            TableEntry table when table.Columns.Any() => table.Columns.Select(s => (ColumnDto) s).ToList(),
            TableTypeEntry tableType when tableType.Columns.Any() => tableType.Columns.Select(s => (ColumnDto) s)
                .ToList(),
            _ => new List<ColumnDto>()
        };
    }

    /// <summary>
    /// Converts the <see cref="TableEntry"/> into a <see cref="TableDto"/>
    /// </summary>
    /// <param name="table">The table</param>
    public static explicit operator TableDto(TableEntry table)
    {
        return new TableDto
        {
            Name = table.Name,
            Table = table,
            Columns = table.Columns.Any() ? table.Columns.Select(s => (ColumnDto) s).ToList() : new List<ColumnDto>(),
            Type = TableDtoType.Table
        };
    }

    /// <summary>
    /// Converts the <see cref="TableTypeEntry"/> into a <see cref="TableDto"/>
    /// </summary>
    /// <param name="table">The table</param>
    public static explicit operator TableDto(TableTypeEntry table)
    {
        return new TableDto
        {
            Name = table.Name,
            Table = table,
            Columns = table.Columns.Any() ? table.Columns.Select(s => (ColumnDto) s).ToList() : new List<ColumnDto>(),
            Type = TableDtoType.TableType
        };
    }
}