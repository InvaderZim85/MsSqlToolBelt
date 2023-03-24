using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.DataObjects.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    /// The instance for the interaction with the database
    /// </summary>
    private readonly SearchRepo _repo;

    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private readonly TableManager _tableManager;

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
    /// Creates a new instance of the <see cref="SearchManager"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the SQL server</param>
    /// <param name="database">The name of the database</param>
    public SearchManager(string dataSource, string database)
    {
        _repo = new SearchRepo(dataSource, database);
        _tableManager = new TableManager(dataSource, database);
    }

    /// <summary>
    /// Executes the search and stores the result into <see cref="SearchResults"/>
    /// </summary>
    /// <param name="search">The search term</param>
    /// <param name="ignoreList">The list with the ignore filters</param>
    /// <returns>The awaitable task</returns>
    public async Task SearchAsync(string search, List<FilterEntry> ignoreList)
    {
        if (search.Contains('*'))
            search = search.Replace("*", "%");

        var result = new List<SearchResult>();
        
        // Load the tables
        var tables = await _tableManager.LoadTablesAsync(search);

        // Add the tables
        result.AddRange(ignoreList.Any()
            ? tables.Where(w => w.Name.IsValid(ignoreList)).Select(s => (SearchResult) s)
            : tables.Select(s => (SearchResult) s));

        // Load the other objects (procedures, jobs, etc.)
        var searchResult = await _repo.SearchAsync(search);
        result.AddRange(ignoreList.Any()
            ? searchResult.Where(w => w.Name.IsValid(ignoreList))
            : searchResult);

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
        if (table.Columns.Any())
            return;

        await _tableManager.EnrichTableAsync(table);
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