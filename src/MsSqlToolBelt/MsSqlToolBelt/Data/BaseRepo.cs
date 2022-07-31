using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.Data;

/// <summary>
/// Provides several "default" functions
/// </summary>
internal class BaseRepo : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The instance for the interaction with the database
    /// </summary>
    protected readonly Connector _connector;

    /// <summary>
    /// Gets the sql connection
    /// </summary>
    public SqlConnection Connection => _connector.Connection;

    /// <summary>
    /// Creates a new instance of the <see cref="BaseRepo"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    public BaseRepo(string dataSource)
    {
        _connector = new Connector(dataSource);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BaseRepo"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    protected BaseRepo(string dataSource, string database)
    {
        _connector = new Connector(dataSource, database);
    }

    /// <summary>
    /// Executes a query and returns the result as list
    /// </summary>
    /// <typeparam name="T">The type of the list</typeparam>
    /// <param name="query">The query</param>
    /// <param name="parameters">The parameters (optional)</param>
    /// <returns>The result</returns>
    protected async Task<List<T>> QueryAsListAsync<T>(string query, object? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<T>();

        var result = await _connector.Connection.QueryAsync<T>(query, parameters);

        return result.ToList();
    }

    /// <summary>
    /// Loads all available databases of the selected server
    /// </summary>
    /// <returns>The list with the databases</returns>
    public async Task<List<string>> LoadDatabasesAsync()
    {
        return await QueryAsListAsync<string>("SELECT [name] FROM sys.databases");
    }

    /// <summary>
    /// Switches the database
    /// </summary>
    /// <param name="database">The desired database</param>
    public void SwitchDatabase(string database)
    {
        if (string.IsNullOrWhiteSpace(database))
            return;

        _connector.SwitchDatabase(database);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="BaseRepo"/>
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _connector.Dispose();

        _disposed = true;
    }
}