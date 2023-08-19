using Dapper;
using Microsoft.SqlServer.Management.Smo;
using MsSqlToolBelt.DataObjects.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
    /// Contains the fail info
    /// </summary>
    private const string FailInfo = "Can't load information";

    /// <summary>
    /// The name / path of the ms sql server
    /// </summary>
    private readonly string _dataSource;

    /// <summary>
    /// The name of the database
    /// </summary>
    private string _database;

    /// <summary>
    /// Gets the list with the server information
    /// </summary>
    public List<ServerInfoEntry> ServerInfo { get; private set; } = new();

    /// <summary>
    /// The instance for the interaction with the database
    /// </summary>
    protected readonly Connector Connector;

    /// <summary>
    /// Gets the sql connection
    /// </summary>
    protected SqlConnection Connection => Connector.Connection;

    /// <summary>
    /// Creates a new instance of the <see cref="BaseRepo"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    public BaseRepo(string dataSource)
    {
        _dataSource = dataSource;
        _database = string.Empty;
        Connector = new Connector(dataSource);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BaseRepo"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    protected BaseRepo(string dataSource, string database)
    {
        _dataSource = dataSource;
        _database = database;
        Connector = new Connector(dataSource, database);
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

        var result = await Connector.Connection.QueryAsync<T>(query, parameters);

        return result.ToList();
    }

    /// <summary>
    /// Loads all available databases of the selected server
    /// </summary>
    /// <returns>The list with the databases</returns>
    public async Task<List<string>> LoadDatabasesAsync()
    {
        return await QueryAsListAsync<string>("SELECT [name] FROM sys.databases ORDER BY [name]");
    }

    /// <summary>
    /// Switches the database
    /// </summary>
    /// <param name="database">The desired database</param>
    public void SwitchDatabase(string database)
    {
        if (string.IsNullOrWhiteSpace(database))
            return;

        _database = database;

        Connector.SwitchDatabase(database);
    }

    /// <summary>
    /// Loads the server information
    /// </summary>
    /// <return>The server information</return>
    public void LoadServerInfo()
    {
        var server = new Server(_dataSource);

        // Add the main values
        AddValues(server, new SortedList<string, string>
        {
            {"Backup directory", nameof(Server.BackupDirectory)},
            {"CLR Version", nameof(Server.BuildClrVersion)},
            {"Collation", nameof(Server.Collation)},
            {"Build number", nameof(Server.BuildNumber)},
            {"Database engine edition", nameof(Server.DatabaseEngineEdition)},
            {"Database engine type", nameof(Server.DatabaseEngineType)},
            {"Edition", nameof(Server.Edition)},
            {"Host platform", nameof(Server.HostPlatform)},
            {"Language", nameof(Server.Language)},
            {"Version", nameof(Server.Version)}
        }, 1, "Server");

        // Add the server configuration
        AddConfigPropertyValue(server.Configuration, 2, "Server configuration");

        if (string.IsNullOrEmpty(_database))
            return;

        Database database = server.Databases[_database];

        // Add the database values
        AddValues(database, new SortedList<string, string>
        {
            {"Collation", nameof(Database.Collation)},
            {"Creation date", nameof(Database.CreateDate)},
            {"Default schema", nameof(Database.DefaultSchema)},
            {"Default language", nameof(Database.DefaultLanguage)},
            {"Name", nameof(Database.Name)},
            {"Owner", nameof(Database.Owner)},
            {"Read only", nameof(Database.ReadOnly)},
            {"Recovery model", nameof(Database.RecoveryModel)},
            {"Size", nameof(Database.Size)},
            {"Status", nameof(Database.Status)},
            {"Available space", nameof(Database.SpaceAvailable)}
        }, 3, "Database");

        // Add the database options
        AddDatabaseOptions(database.DatabaseOptions, 4, "Database options");
    }

    /// <summary>
    /// Adds the values
    /// </summary>
    /// <param name="obj">The object</param>
    /// <param name="values">The list with the values, which should be added to the <see cref="ServerInfo"/></param>
    /// <param name="order">The desired order</param>
    /// <param name="groupName">The name of the group</param>
    private void AddValues(object obj, SortedList<string, string> values, int order, string groupName)
    {
        var properties = obj.GetType().GetProperties();

        foreach (var value in values)
        {
            AddValue(obj, properties, value.Value, value.Key, order, groupName);
        }
    }

    /// <summary>
    /// Adds the config properties
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="order">The order</param>
    /// <param name="groupName">The group name</param>
    private void AddConfigPropertyValue(Configuration configuration, int order, string groupName)
    {

        void AddEntry(ServerInfoEntry rootEntry, ConfigProperty rootProperty, string rootGroupName)
        {
            var properties = rootProperty.GetType().GetProperties();

            foreach (var property in properties)
            {
                try
                {
                    rootEntry.ChildValues.Add(new ServerInfoEntry(order, rootGroupName, property.Name,
                        property.GetValue(rootProperty)?.ToString() ?? FailInfo));
                }
                catch
                {
                    rootEntry.ChildValues.Add(new ServerInfoEntry(order, rootGroupName, property.Name, FailInfo));
                }
            }
        }

        var properties = configuration.GetType().GetProperties();

        foreach (var property in properties)
        {
            try
            {
                var tmpValue = property.GetValue(configuration);
                if (tmpValue == null)
                    continue;

                var tmpProperty = new ServerInfoEntry(order, groupName, property.Name, "See details for more information");
                var tmpGroupName = $"{groupName} - {property.Name}";

                if (tmpValue is not ConfigProperty configProperty)
                    continue;

                AddEntry(tmpProperty, configProperty, tmpGroupName);

                ServerInfo.Add(tmpProperty);
            }
            catch
            {
                ServerInfo.Add(new ServerInfoEntry(order, groupName, property.Name, FailInfo));
            }
        }
    }

    /// <summary>
    /// Adds the value of the desired property
    /// </summary>
    /// <param name="obj">The object</param>
    /// <param name="properties">The list with the properties</param>
    /// <param name="propertyName">The name of the property</param>
    /// <param name="keyName">The name of the key (this is displayed to the user)</param>
    /// <param name="order">The desired order</param>
    /// <param name="groupName">The name of the group</param>
    private void AddValue(object obj, IEnumerable<PropertyInfo> properties, string propertyName, string keyName, int order, string groupName)
    {
        try
        {
            var property = properties.FirstOrDefault(f => f.Name == propertyName);
            ServerInfo.Add(property == null
                ? new ServerInfoEntry(order, groupName, keyName, "/")
                : new ServerInfoEntry(order, groupName, keyName, property.GetValue(obj)?.ToString() ?? FailInfo));
        }
        catch
        {
            ServerInfo.Add(new ServerInfoEntry(order, groupName, keyName, FailInfo));
        }
    }

    /// <summary>
    /// Adds the database options
    /// </summary>
    /// <param name="options">The options</param>
    /// <param name="order">The order</param>
    /// <param name="groupName">The group name</param>
    private void AddDatabaseOptions(DatabaseOptions options, int order, string groupName)
    {
        var properties = options.GetType().GetProperties();

        foreach (var property in properties)
        {
            try
            {
                ServerInfo.Add(new ServerInfoEntry(order, groupName, property.Name,
                    property.GetValue(options)?.ToString() ?? FailInfo));
            }
            catch
            {
                ServerInfo.Add(new ServerInfoEntry(order, groupName, property.Name, FailInfo));
            }
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="BaseRepo"/>
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        Connector.Dispose();

        _disposed = true;
    }
}