using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.DataObjects.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MsSqlToolBelt.DataObjects.TableType;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the search logic
/// </summary>
internal class SearchManager : IDisposable
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
    private readonly SearchRepo _repo;

    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private readonly TableManager _tableManager;

    /// <summary>
    /// The instance for the interaction with the table types
    /// </summary>
    private readonly TableTypeManager _tableTypeManager;

    /// <summary>
    /// The instance for the interaction with the definition export
    /// </summary>
    private readonly DefinitionExportManager _definitionManager;

    /// <summary>
    /// Gets the list with the result types
    /// </summary>
    public List<string> ResultTypes { get; private set; } = new();

    /// <summary>
    /// Gets the list with the found entries
    /// </summary>
    public List<SearchResult> SearchResults { get; private set; } = new();

    /// <summary>
    /// Gets or sets the selected result
    /// </summary>
    public SearchResult? SelectedResult { get; set; }

    /// <summary>
    /// Gets the value which indicates if there was an error while search jobs
    /// </summary>
    public bool HasJobSearchError { get; private set; }

    /// <summary>
    /// Creates a new instance of the <see cref="SearchManager"/>
    /// </summary>
    /// <param name="settingsManager">The instance of the settings manager</param>
    /// <param name="dataSource">The name / path of the SQL server</param>
    /// <param name="database">The name of the database</param>
    public SearchManager(SettingsManager settingsManager, string dataSource, string database)
    {
        _repo = new SearchRepo(dataSource, database);
        _tableManager = new TableManager(dataSource, database);
        _tableTypeManager = new TableTypeManager(dataSource, database);
        _definitionManager = new DefinitionExportManager(settingsManager, dataSource, database);
    }

    /// <summary>
    /// Executes the search and stores the result into <see cref="SearchResults"/>
    /// </summary>
    /// <param name="search">The search term</param>
    /// <param name="ignoreList">The list with the ignore filters</param>
    /// <returns>The awaitable task</returns>
    public async Task SearchAsync(string search, List<FilterEntry> ignoreList)
    {
        void AddToResultList(List<SearchResult> resultList, List<SearchResult> searchResult)
        {
            resultList.AddRange(ignoreList.Any() ? searchResult.Where(w => w.Name.IsValid(ignoreList)) : searchResult);
        }

        if (search.Contains('*'))
            search = search.Replace("*", "%");

        var cleanSearchValue = search.Replace("%", "");

        var result = new List<SearchResult>();

        // Load the tables
        ProgressEvent?.Invoke(this, $"Scanning tables for '{cleanSearchValue}'...");
        var tables = await _tableManager.LoadTablesAsync(search);

        // Add the tables
        result.AddRange(ignoreList.Any()
            ? tables.Where(w => w.Name.IsValid(ignoreList)).Select(s => (SearchResult) s)
            : tables.Select(s => (SearchResult) s));

        // Load the table types
        ProgressEvent?.Invoke(this, $"Scanning table types for '{cleanSearchValue}'...");
        var tableTypes = await _tableTypeManager.LoadTableTypesAsync(search);

        // Add the table types
        result.AddRange(ignoreList.Any()
            ? tableTypes.Where(w => w.Name.IsValid(ignoreList)).Select(s => (SearchResult) s)
            : tableTypes.Select(s => (SearchResult) s));

        // Load the other objects (procedures, etc.)
        ProgressEvent?.Invoke(this, $"Scanning objects (procedures, views, etc.) for '{cleanSearchValue}'...");
        var objectResult = await _repo.SearchObjectsAsync(search);
        AddToResultList(result, objectResult);

        // Load the jobs (this maybe will fail if you don't have access to the MSDB database)
        ProgressEvent?.Invoke(this, $"Scanning jobs for '{cleanSearchValue}'...");
        var (jobResult, hasError) = await _repo.SearchJobsAsync(search);
        AddToResultList(result, jobResult);
        HasJobSearchError = hasError;

        SearchResults = result;

        // Add the types
        ResultTypes.Clear();
        ResultTypes.Add("All");
        ResultTypes.AddRange(SearchResults.Select(s => s.Type).Distinct().ToList());
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
    private async Task EnrichTableAsync()
    {
        if (SelectedResult is not {BoundItem: TableEntry table})
            return;

        // Check if the columns are already loaded
        if (table.Columns.Count == 0)
            await _tableManager.EnrichTableAsync(table);

        await _definitionManager.LoadTableDefinitionAsync(table);
    }

    /// <summary>
    /// Enriches the selected table type with additional information
    /// </summary>
    /// <returns>The awaitable task</returns>
    private async Task EnrichTableTypeAsync()
    {
        if (SelectedResult is not {BoundItem: TableTypeEntry tableType})
            return;

        // Check if the columns are already loaded
        if (tableType.Columns.Count == 0)
            await _tableTypeManager.EnrichTableTypeAsync(tableType);

        await _definitionManager.LoadTableTypeDefinitionAsync(tableType);
    }

    /// <summary>
    /// Enriches the selected job with additional information
    /// </summary>
    /// <returns>The awaitable task</returns>
    private async Task EnrichJobAsync()
    {
        if (SelectedResult is not {BoundItem: JobEntry job})
            return;

        // Check if the steps are already loaded
        if (job.JobSteps.Any())
            return;

        await _repo.EnrichJobAsync(job);
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