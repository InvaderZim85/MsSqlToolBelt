using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.Common;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides several basic functions
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="BaseManager"/>
/// </remarks>
/// <param name="dataSource">The name / path of the ms sql server</param>
public sealed class BaseManager(string dataSource) : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The instance for the basic interaction with the server
    /// </summary>
    private readonly BaseRepo _repo = new(dataSource);

    /// <summary>
    /// Gets the list with the server information
    /// </summary>
    public List<ServerInfoEntry> ServerInfo => _repo.ServerInfo;

    /// <summary>
    /// Loads all available databases of the selected server
    /// </summary>
    /// <returns>The list with the databases</returns>
    public Task<List<string>> LoadDatabasesAsync()
    {
        return _repo.LoadDatabasesAsync();
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
    public Task LoadServerInformationAsync()
    {
        return Task.Run(_repo.LoadServerInfo);
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