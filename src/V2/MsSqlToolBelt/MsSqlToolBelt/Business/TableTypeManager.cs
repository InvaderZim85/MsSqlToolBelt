using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.TableType;

namespace MsSqlToolBelt.Business;

internal class TableTypeManager : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The instance for the interaction with the database
    /// </summary>
    private readonly TableTypeRepo _repo;

    /// <summary>
    /// Gets or sets the list with the table types
    /// </summary>
    public List<TableTypeEntry> TableTypes { get; private set; } = new();

    /// <summary>
    /// Gets or sets the selected table type
    /// </summary>
    public TableTypeEntry? SelectedTableType { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="TableTypeManager"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    public TableTypeManager(string dataSource, string database)
    {
        _repo = new TableTypeRepo(dataSource, database);
    }

    /// <summary>
    /// Loads all available user defined table types and stores them into <see cref="TableTypes"/>
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task LoadTableTypesAsync()
    {
        TableTypes = await _repo.LoadTableTypesAsync();
    }

    /// <summary>
    /// Enriches the selected table type
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task EnrichTableTypeAsync()
    {
        if (SelectedTableType == null || SelectedTableType.Columns.Any())
            return;

        await _repo.EnrichTableTypeAsync(SelectedTableType);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="TableTypeManager"/>
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _repo.Dispose();

        _disposed = true;
    }
}