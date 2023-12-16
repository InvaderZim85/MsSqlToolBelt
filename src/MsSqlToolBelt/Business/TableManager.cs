using Microsoft.SqlServer.Management.Smo;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.DataObjects.Table;
using System.Data;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the functions for the interaction with the tables
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="TableManager"/>
/// </remarks>
/// <param name="dataSource">The name / path of the MSSQL server</param>
/// <param name="database">The name of the database</param>
public sealed class TableManager(string dataSource, string database) : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The name / path of the ms sql server
    /// </summary>
    private readonly string _dataSource = dataSource;

    /// <summary>
    /// The name of the database
    /// </summary>
    private readonly string _database = database;

    /// <summary>
    /// The instance for the interaction with the database
    /// </summary>
    private readonly TableRepo _repo = new(dataSource, database);

    /// <summary>
    /// Loads all tables
    /// </summary>
    /// <param name="search">The desired search string (optional, if empty, all user defined table will be loaded)</param>
    /// <returns>The list with the tables</returns>
    public async Task<List<TableEntry>> LoadTablesAsync(string search = "")
    {
        var result = await _repo.LoadTablesAsync(search);
        return [.. result.OrderBy(o => o.Name)];
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

        table.Indexes =
        [
            .. names.Select(s => new IndexEntry
            {
                Name = s,
                Columns = string.Join(", ", indizes.Where(w => w.Name.Equals(s)).Select(ss => ss.Column).OrderBy(o => o))
            })
        ];

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
    /// Loads the table index fragmentation and stores the information at the respective index
    /// </summary>
    /// <param name="table">The table</param>
    /// <returns>The awaitable task</returns>
    public void LoadTableIndexFragmentation(TableEntry table)
    {
        var dbTable = LoadTable(table.Name);

        var indexFragmentation = dbTable.EnumFragmentation(FragmentationOption.Fast);

        var fragmentationData = (from DataRow row in indexFragmentation.Rows
            select new TableIndexFragmentation
            {
                Index = row["Index_Name"].ToString() ?? string.Empty,
                Fragmentation = row["AverageFragmentation"].ToDecimal()
            }).ToList();

        foreach (var entry in table.Indexes)
        {
            var fragmentation =
                fragmentationData.FirstOrDefault(f => f.Index.Equals(entry.Name, StringComparison.OrdinalIgnoreCase));

            entry.Fragmentation = fragmentation == null
                ? "/"
                : $"{fragmentation.Fragmentation:N2} %";
        }
    }

    /// <summary>
    /// Rebuild the indexes of the desired table
    /// </summary>
    /// <param name="table">The table</param>
    /// <param name="fillFactor">The desired fill factor</param>
    public void RebuildTableIndexes(TableEntry table, int fillFactor)
    {
        var dbTable = LoadTable(table.Name);

        dbTable.RebuildIndexes(fillFactor);
    }

    /// <summary>
    /// Loads the desired table
    /// </summary>
    /// <param name="tableName">The name of the table</param>
    /// <returns>The table</returns>
    private Table LoadTable(string tableName)
    {
        var server = new Server(_dataSource);

        var tmpDatabase = server.Databases[_database];

        return tmpDatabase.Tables[tableName];
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