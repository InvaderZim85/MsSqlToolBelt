using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the functions for the interaction with the tables
/// </summary>
internal class TableManager : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The instance for the interaction with the database
    /// </summary>
    private readonly TableRepo _repo;

    /// <summary>
    /// Creates a new instance of the <see cref="TableManager"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    public TableManager(string dataSource, string database)
    {
        _repo = new TableRepo(dataSource, database);
    }

    /// <summary>
    /// Loads all tables
    /// </summary>
    /// <param name="search">The desired search string (optional, if empty, all user defined table will be loaded)</param>
    /// <returns>The awaitable task</returns>
    public async Task<List<TableEntry>> LoadTablesAsync(string search = "")
    {
        var result = await _repo.LoadTablesAsync(search);
        return result.OrderBy(o => o.Name).ToList();
    }

    /// <summary>
    /// Enriches the selected table with additional information (columns, primary key, indizes)
    /// </summary>
    /// <param name="table">The table which should be enriched</param>
    /// <returns>The awaitable task</returns>
    public async Task EnrichTableAsync(TableEntry table)
    {
        // Step 1: Columns
        await _repo.LoadColumnsAsync(table);

        // Step 2: Primary Key information
        await _repo.LoadPrimaryKeyInfoAsync(table);

        // Step 3: Indexes
        await LoadTableIndizesAsync(table);

        // Step 4: ForeignKeys
        await LoadForeignKeysAsync(table);
    }

    /// <summary>
    /// Loads and sets the table indizes
    /// </summary>
    /// <param name="table">The table</param>
    /// <returns>The awaitable task</returns>
    private async Task LoadTableIndizesAsync(TableEntry table)
    {
        var indizes = await _repo.LoadTableIndexAsync(table);

        var names = indizes.Select(s => s.Name).Distinct();

        table.Indexes = names.Select(s => new IndexEntry
        {
            Name = s,
            Columns = string.Join(", ", indizes.Where(w => w.Name.Equals(s)).Select(ss => ss.Column).OrderBy(o => o))
        }).ToList();

        // Update the columns
        foreach (var column in table.Columns.Where(w => !w.InIndex))
        {
            column.InIndex = indizes.Any(a => a.Column.Equals(column.Name));
        }
    }

    /// <summary>
    /// Loads and sets the foreign keys
    /// </summary>
    /// <param name="table">The table</param>
    /// <returns>The awaitable task</returns>
    private async Task LoadForeignKeysAsync(TableEntry table)
    {
        var foreignKeys = await _repo.LoadForeignKeyInfoAsync(table);

        var names = foreignKeys.Select(s => s.Name).Distinct();

        table.Indexes.AddRange(names.Select(s => new IndexEntry
        {
            Name = s,
            Columns = string.Join(", ", foreignKeys.Where(w => w.Name.Equals(s)).Select(ss => $"Column '{ss.ColumnName}' references '{ss.ReferencedTableName}.{ss.ReferencedColumnName}'"))
        }));

        // Update the columns
        foreach (var column in table.Columns.Where(w => !w.InIndex))
        {
            column.InIndex = foreignKeys.Any(a => a.ColumnName.Equals(column.Name));
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="TableManager"/>
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _repo.Dispose();

        _disposed = true;
    }
}