using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Table;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the logic for the interaction with the replication tables
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ReplicationManager"/>
/// </remarks>
/// <param name="dataSource">The name / path of the MSSQL database</param>
/// <param name="database">The name of the database</param>
internal class ReplicationManager(string dataSource, string database) : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private readonly TableManager _tableManager = new(dataSource, database);

    /// <summary>
    /// Gets the list with the tables which are marked for the replication
    /// </summary>
    public List<TableEntry> Tables { get; private set; } = [];

    /// <summary>
    /// Gets or sets the selected table
    /// </summary>
    public TableEntry? SelectedTable { get; set; }

    /// <summary>
    /// Gets the list with the replication articles / tables
    /// </summary>
    public List<ReplicationArticle> ReplicationArticles { get; private set; } = [];

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
    /// Loads the replication articles / tables for the replications of the current database and stores them into <see cref="ReplicationArticles"/>
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task LoadReplicationArticlesAsync()
    {
        ReplicationArticles = await _tableManager.LoadReplicationArticlesAsync();
    }

    /// <summary>
    /// Enriches the selected table with additional information (columns, primary key, indizes)
    /// </summary>
    /// <returns>The awaitable task</returns>
    public Task EnrichTableAsync()
    {
        return SelectedTable == null ? Task.CompletedTask : _tableManager.EnrichTableAsync(SelectedTable);
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