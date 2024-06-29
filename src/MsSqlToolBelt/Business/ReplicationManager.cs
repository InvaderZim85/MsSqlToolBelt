using MsSqlToolBelt.DataObjects.Common;
using ZimLabs.CoreLib;

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
    /// The name for "all"
    /// </summary>
    private const string AllPublicationName = "All";

    /// <summary>
    /// The name for a not existing publication
    /// </summary>
    private const string EmptyPublicationName = "No publication";

    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private readonly TableManager _tableManager = new(dataSource, database);

    /// <summary>
    /// Contains the list with the tables (needed for the filter)
    /// </summary>
    private List<TableEntry> _tables = [];

    /// <summary>
    /// Gets the list with the tables which are marked for the replication
    /// </summary>
    public List<TableEntry> Tables { get; private set; } = [];

    /// <summary>
    /// Gets the list with the publications
    /// </summary>
    public List<string> Publications { get; private set; } = [];

    /// <summary>
    /// Gets or sets the selected table
    /// </summary>
    public TableEntry? SelectedTable { get; set; }

    /// <summary>
    /// Gets the value which indicates whether replication tables are available
    /// </summary>
    public bool HasReplicatedTables { get; private set; }

    /// <summary>
    /// Loads the replication data and stores them into <see cref="Tables"/> (<b>Note</b>: The result is not filtered yet)
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task LoadDataAsync()
    {
        _tables = await _tableManager.LoadReplicationArticlesAsync();

        // Check if there is any table for replication
        HasReplicatedTables = _tables.Any(a => a.IsReplicated);

        Publications.Clear();

        var publications = _tables.SelectMany(sm => sm.ReplicationInformation.Select(s => s.Publication)).Distinct().ToList();
        // Check if there is a table which is not replicated
        var notReplicatedTables = _tables.Any(a => a.ReplicationInformation.Count == 0);

        if (publications.Count > 0)
            Publications.Add(AllPublicationName);

        if (notReplicatedTables)
            publications.Add(EmptyPublicationName);

        Publications.AddRange(publications.Select(s => string.IsNullOrWhiteSpace(s) ? EmptyPublicationName : s));
    }

    /// <summary>
    /// Filters the tables and stores the result in <see cref="Tables"/>
    /// </summary>
    /// <param name="filter">The desired filter</param>
    /// <param name="publication">The name of the publication</param>
    public void FilterTables(string filter, string publication)
    {
        Tables = string.IsNullOrWhiteSpace(filter)
            ? _tables
            : _tables.Where(w => w.Name.ContainsIgnoreCase(filter) ||
                                 w.Schema.ContainsIgnoreCase(filter) ||
                                 w.ReplicationInformation.Any(a => a.Publication.ContainsIgnoreCase(filter) ||
                                                                   a.Database.ContainsIgnoreCase(filter) ||
                                                                   a.Article.ContainsIgnoreCase(filter) ||
                                                                   a.InsertCommand.EqualsIgnoreCase(filter) ||
                                                                   a.UpdateCommand.EqualsIgnoreCase(filter) ||
                                                                   a.DeleteCommand.EqualsIgnoreCase(filter) ||
                                                                   a.FilterQuery.ContainsIgnoreCase(filter))).ToList();

        if (string.IsNullOrWhiteSpace(publication) || publication.EqualsIgnoreCase(AllPublicationName))
        {
            OrderTable();
            return;
        }

        Tables = publication.Equals(EmptyPublicationName)
            ? Tables.Where(w => !w.ReplicationInformation.Any()).ToList()
            : Tables.Where(w => w.ReplicationInformation.Any(a => a.Publication.EqualsIgnoreCase(publication)))
                .ToList();

        OrderTable();

        return;

        void OrderTable()
        {
            Tables = Tables.OrderBy(o => o.Name).ToList();
        }
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