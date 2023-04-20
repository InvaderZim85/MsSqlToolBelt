﻿using MsSqlToolBelt.Data;
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
    /// Gets the list with the limit values
    /// </summary>
    public List<IdTextEntry> LimitList => new List<IdTextEntry>
    {
        new(0, "Don't limit"),
        new(10, "Limit to 10 rows"),
        new(50, "Limit to 50 rows"),
        new(100, "Limit to 100 rows"),
        new(200, "Limit to 200 rows"),
        new(300, "Limit to 300 rows"),
        new(400, "Limit to 400 rows"),
        new(500, "Limit to 500 rows"),
        new(1000, "Limit to 1.000 rows"),
        new(2000, "Limit to 2.000 rows"),
        new(5000, "Limit to 5.000 rows"),
        new(10000, "Limit to 10.000 rows"),
        new(50000, "Limit to 50.000 rows")
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
    /// <returns>The query</returns>
    public string CreateQuery(TableEntry table, IdTextEntry limit)
    {
        var spacer = new string(' ', 4);
        // Generate the query
        var query = new StringBuilder();

        // Add the select with the limit aka TOP
        query.AppendLine(limit.Id == 0 ? "SELECT" : $"SELECT TOP ({limit.Id})");

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
    /// Loads the table data
    /// </summary>
    /// <param name="table">The desired table</param>
    /// <param name="limit">The limit</param>
    /// <returns>The data table</returns>
    public async Task<(DataTable Table, TimeSpan Duration, int Rows)> LoadTableDataAsync(TableEntry table, IdTextEntry limit)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var result = await _repo.LoadTableDataAsync(CreateQuery(table, limit), table.Name);

        stopwatch.Stop();

        return (result, stopwatch.Elapsed, result.Rows.Count);
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