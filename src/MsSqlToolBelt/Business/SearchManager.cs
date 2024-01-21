using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.DataObjects.TableType;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the search logic
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="SearchManager"/>
/// </remarks>
/// <param name="settingsManager">The instance of the settings manager</param>
/// <param name="tableManager">The instance for the interaction with the tables</param>
/// <param name="dataSource">The name / path of the SQL server</param>
/// <param name="database">The name of the database</param>
internal class SearchManager(SettingsManager settingsManager, TableManager tableManager, string dataSource, string database) : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Occurs when progress was made
    /// </summary>
    public event EventHandler<string>? ProgressEvent; 

    /// <summary>
    /// The instance for the interaction with the database
    /// </summary>
    private readonly SearchRepo _repo = new(dataSource, database);

    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private readonly TableManager _tableManager = tableManager;

    /// <summary>
    /// The instance for the interaction with the table types
    /// </summary>
    private readonly TableTypeManager _tableTypeManager = new(dataSource, database);

    /// <summary>
    /// The instance for the interaction with the definition export
    /// </summary>
    private readonly DefinitionExportManager _definitionManager = new(settingsManager, dataSource, database);

    /// <summary>
    /// Gets the list with the result types
    /// </summary>
    public List<string> ResultTypes { get; } = [];

    /// <summary>
    /// Gets the list with the found entries
    /// </summary>
    public List<SearchResult> SearchResults { get; private set; } = [];

    /// <summary>
    /// Gets or sets the selected result
    /// </summary>
    public SearchResult? SelectedResult { get; set; }

    /// <summary>
    /// Gets the value which indicates if there was an error while search jobs
    /// </summary>
    public bool HasJobSearchError { get; private set; }

    /// <summary>
    /// Executes the search and stores the result into <see cref="SearchResults"/>
    /// </summary>
    /// <param name="search">The search term</param>
    /// <param name="options">The search options</param>
    /// <param name="ignoreList">The list with the ignore filters</param>
    /// <returns>The awaitable task</returns>
    public async Task SearchAsync(string search, SearchOptions options, List<FilterEntry> ignoreList)
    {
        if (search.Contains('*'))
            search = search.Replace("*", "%");

        var cleanSearchValue = search.Replace("%", "");

        var result = new List<SearchResult>();

        // Load the tables
        if (options.Tables)
        {
            ProgressEvent?.Invoke(this, $"Scanning tables for '{cleanSearchValue}'...");
            var tables = await _tableManager.LoadTablesAsync(search);

            // Add the tables
            result.AddRange(ignoreList.Count > 0
                ? tables.Where(w => w.Name.IsValid(ignoreList)).Select(s => (SearchResult)s)
                : tables.Select(s => (SearchResult)s));
        }

        // Load the table types
        if (options.TableTypes)
        {
            ProgressEvent?.Invoke(this, $"Scanning table types for '{cleanSearchValue}'...");
            var tableTypes = await _tableTypeManager.LoadTableTypesAsync(search);

            // Add the table types
            result.AddRange(ignoreList.Count > 0
                ? tableTypes.Where(w => w.Name.IsValid(ignoreList)).Select(s => (SearchResult)s)
                : tableTypes.Select(s => (SearchResult)s));
        }

        // Load the other objects (procedures, etc.)
        if (options.Objects)
        {
            ProgressEvent?.Invoke(this, $"Scanning objects (procedures, views, etc.) for '{cleanSearchValue}'...");
            var objectResult = await _repo.SearchObjectsAsync(search);
            AddToResultList(result, objectResult);
        }

        // Load the jobs (this maybe will fail if you don't have access to the MSDB database)
        if (options.Jobs)
        {
            ProgressEvent?.Invoke(this, $"Scanning jobs for '{cleanSearchValue}'...");
            var (jobResult, hasError) = await _repo.SearchJobsAsync(search);
            AddToResultList(result, jobResult);
            HasJobSearchError = hasError;
        }

        SearchResults = result;

        // Add the types
        ResultTypes.Clear();
        ResultTypes.Add("All");
        ResultTypes.AddRange(SearchResults.Select(s => s.Type).Distinct().ToList());

        return;

        void AddToResultList(List<SearchResult> resultList, List<SearchResult> searchResult)
        {
            resultList.AddRange(ignoreList.Count > 0 ? searchResult.Where(w => w.Name.IsValid(ignoreList)) : searchResult);
        }
    }

    /// <summary>
    /// Enriches the data of the selected item
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task EnrichDataAsync()
    {
        await EnrichTableAsync();
        await EnrichTableTypeAsync();
        await EnrichJobAsync();
    }

    /// <summary>
    /// Enriches the selected table with additional information
    /// </summary>
    /// <returns>The awaitable task</returns>
    private Task EnrichTableAsync()
    {
        if (SelectedResult is not {BoundItem: TableEntry table})
            return Task.CompletedTask;

        // Check if the columns are already loaded
        return table.Columns.Count == 0 ? _tableManager.EnrichTableAsync(table) : Task.CompletedTask;
    }

    /// <summary>
    /// Enriches the selected table type with additional information
    /// </summary>
    /// <returns>The awaitable task</returns>
    private Task EnrichTableTypeAsync()
    {
        if (SelectedResult is not {BoundItem: TableTypeEntry tableType})
            return Task.CompletedTask;

        // Check if the columns are already loaded
        return tableType.Columns.Count == 0 ? _tableTypeManager.EnrichTableTypeAsync(tableType) : Task.CompletedTask;
    }

    /// <summary>
    /// Loads the table / table type definition
    /// </summary>
    /// <returns>The awaitable task</returns>
    public Task LoadTableDefinitionAsync()
    {
        return SelectedResult switch
        {
            { BoundItem: TableEntry table } when string.IsNullOrWhiteSpace(table.Definition) => _definitionManager
                .LoadTableDefinitionAsync(table),
            { BoundItem: TableTypeEntry tableType } when string.IsNullOrWhiteSpace(tableType.Definition) =>
                _definitionManager.LoadTableTypeDefinitionAsync(tableType),
            _ => Task.CompletedTask
        };
    }

    /// <summary>
    /// Enriches the selected job with additional information
    /// </summary>
    /// <returns>The awaitable task</returns>
    private Task EnrichJobAsync()
    {
        if (SelectedResult is not {BoundItem: JobEntry job})
            return Task.CompletedTask;

        // Check if the steps are already loaded
        if (job.JobSteps.Count > 0)
            return Task.CompletedTask;

        return _repo.EnrichJobAsync(job);
    }

    /// <summary>
    /// Gets the table definition
    /// </summary>
    /// <returns>The table definition</returns>
    public string GetTableDefinition()
    {
        return SelectedResult switch
        {
            { BoundItem: TableEntry table } => table.Definition,
            { BoundItem: TableTypeEntry tableType } => tableType.Definition,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Check if the selected entry contains "data"
    /// </summary>
    /// <returns><see langword="true"/> when the entry has data, otherwise <see langword="false"/></returns>
    public bool EntryHasData()
    {
        return SelectedResult switch
        {
            { BoundItem: TableEntry { Columns.Count: > 0 } } => true,
            { BoundItem: TableTypeEntry { Columns.Count: > 0 } } => true,
            { BoundItem: ObjectEntry objectEntry } when !string.IsNullOrWhiteSpace(objectEntry.Definition) => true,
            { BoundItem: JobEntry { JobSteps.Count: > 0 } } => true,
            _ => false
        };
    }

    /// <summary>
    /// Releases all resources used by the <see cref="SearchManager"/>
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _repo.Dispose();
        _tableManager.Dispose();

        _disposed = true;
    }
}