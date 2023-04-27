using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the functions for the interaction with the tables
/// </summary>
internal class TableQueryManager : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private readonly TableQueryRepo _repo;

    /// <summary>
    /// Gets the result table
    /// </summary>
    public DataTable ResultTable { get; private set; } = new();

    /// <summary>
    /// Gets the list with the limit values
    /// </summary>
    public static List<IdTextEntry> LimitList => new()
    {
        new IdTextEntry(0, "Don't limit"),
        new IdTextEntry(10, "Limit to 10 rows"),
        new IdTextEntry(50, "Limit to 50 rows"),
        new IdTextEntry(100, "Limit to 100 rows"),
        new IdTextEntry(200, "Limit to 200 rows"),
        new IdTextEntry(300, "Limit to 300 rows"),
        new IdTextEntry(400, "Limit to 400 rows"),
        new IdTextEntry(500, "Limit to 500 rows"),
        new IdTextEntry(1000, "Limit to 1.000 rows"),
        new IdTextEntry(2000, "Limit to 2.000 rows"),
        new IdTextEntry(5000, "Limit to 5.000 rows"),
        new IdTextEntry(10000, "Limit to 10.000 rows"),
        new IdTextEntry(50000, "Limit to 50.000 rows")
    };

    /// <summary>
    /// Creates a new instance of the <see cref="TableQueryManager"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    public TableQueryManager(string dataSource, string database)
    {
        _repo = new TableQueryRepo(dataSource, database);
    }

    /// <summary>
    /// Creates the query for the table
    /// </summary>
    /// <param name="table">The table</param>
    /// <param name="limit">The limit value</param>
    /// <param name="addRowNumber"><see langword="true" /> to add a row number, otherwise <see langword="false"/></param>
    /// <returns>The query</returns>
    public static string CreateQuery(TableEntry table, IdTextEntry limit, bool addRowNumber = false)
    {
        var spacer = new string(' ', 4);
        // Generate the query
        var query = new StringBuilder();

        // Add the select with the limit aka TOP
        query.AppendLine(limit.Id == 0 ? "SELECT" : $"SELECT TOP ({limit.Id})");

        if (addRowNumber)
        {
            var pkColumn = table.Columns.FirstOrDefault(f => f.IsPrimaryKey) ?? table.Columns.FirstOrDefault();
            query.Append($"{spacer}ROW_NUMBER() OVER(ORDER BY [{pkColumn!.Name}]) AS Row,");
        }

        // Add the columns
        var count = 1;
        foreach (var column in table.Columns.OrderBy(o => o.Order))
        {
            query.AppendLine($"{spacer}t.[{column.Name}]{(count == table.Columns.Count ? "" : ",")}");

            count++;
        }

        // Add the from statement
        query
            .AppendLine("FROM")
            .AppendLine($"{spacer}[{table.Schema}].[{table.Name}] AS t");

        return query.ToString();
    }

    /// <summary>
    /// Loads the table data and stores the result into <see cref="ResultTable"/>
    /// </summary>
    /// <param name="table">The desired table</param>
    /// <param name="limit">The limit</param>
    /// <param name="addRowNumber"><see langword="true" /> to add a row number, otherwise <see langword="false"/></param>
    /// <returns>Some additional information</returns>
    public async Task<(TimeSpan Duration, int Rows)> LoadTableDataAsync(TableEntry table, IdTextEntry limit, bool addRowNumber)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var result = await _repo.LoadTableDataAsync(CreateQuery(table, limit, addRowNumber), table.Name);

        stopwatch.Stop();

        ResultTable = result;

        return (stopwatch.Elapsed, result.Rows.Count);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="TableQueryManager"/>
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _repo.Dispose();

        _disposed = true;
    }
}