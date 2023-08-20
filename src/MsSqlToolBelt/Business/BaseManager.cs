using MsSqlToolBelt.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MsSqlToolBelt.DataObjects.Common;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides several basic functions
/// </summary>
public sealed class BaseManager : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The instance for the basic interaction with the server
    /// </summary>
    private readonly BaseRepo _repo;

    /// <summary>
    /// Gets the list with the server information
    /// </summary>
    public List<ServerInfoEntry> ServerInfo => _repo.ServerInfo;

    /// <summary>
    /// Creates a new instance of the <see cref="BaseManager"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the ms sql server</param>
    public BaseManager(string dataSource)
    {
        _repo = new BaseRepo(dataSource);
    }

    /// <summary>
    /// Loads all available databases of the selected server
    /// </summary>
    /// <returns>The list with the databases</returns>
    public async Task<List<string>> LoadDatabasesAsync()
    {
        return await _repo.LoadDatabasesAsync();
    }

    /// <summary>
    /// Switches the database
    /// </summary>
    /// <param name="database">The desired database</param>
    public void SwitchDatabase(string database)
    {
        _repo.SwitchDatabase(database);
    }

    /// <summary>
    /// Loads the server information
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task LoadServerInformationAsync()
    {
        await Task.Run(_repo.LoadServerInfo);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _repo.Dispose();

        _disposed = true;
    }
}