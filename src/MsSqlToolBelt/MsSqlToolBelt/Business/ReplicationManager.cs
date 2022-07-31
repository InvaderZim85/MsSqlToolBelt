using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MsSqlToolBelt.DataObjects.Common;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the logic for the interaction with the replication tables
/// </summary>
internal class ReplicationManager : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private readonly TableManager _tableManager;

    /// <summary>
    /// Gets the list with the tables which are marked for the replication
    /// </summary>
    public List<TableEntry> Tables { get; private set; } = new();

    /// <summary>
    /// Gets or sets the selected table
    /// </summary>
    public TableEntry? SelectedTable { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="ReplicationManager"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL database</param>
    /// <param name="database">The name of the database</param>
    public ReplicationManager(string dataSource, string database)
    {
        _tableManager = new TableManager(dataSource, database);
    }

    /// <summary>
    /// Loads all tables which are marked for the replication and stores them into <see cref="Tables"/>
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task LoadTablesAsync()
    {
        var tables = await _tableManager.LoadTablesAsync();
        Tables = tables.Where(w => w.IsReplicated).ToList();
    }

    /// <summary>
    /// Enriches the selected table with additional information (columns, primary key, indizes)
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task EnrichTableAsync()
    {
        if (SelectedTable == null)
            return;

        await _tableManager.EnrichTableAsync(SelectedTable);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="ReplicationManager"/>
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _tableManager.Dispose();

        _disposed = true;
    }
}