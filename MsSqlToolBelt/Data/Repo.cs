using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.Data
{
    /// <summary>
    /// Provides the functions to load the databases
    /// </summary>
    internal sealed class Repo : IDisposable
    {
        /// <summary>
        /// Contains the value which indicates if the class was already disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Gets the connection
        /// </summary>
        public Connector Connector { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="SearchRepo"/>
        /// </summary>
        /// <param name="server">The desired server</param>
        public Repo(string server)
        {
            Connector = new Connector(server);
        }

        /// <summary>
        /// Switches the database
        /// </summary>
        /// <param name="database">The desired database</param>
        public void SwitchDatabase(string database)
        {
            Connector.SwitchDatabase(database);
        }

        /// <summary>
        /// Loads the available databases
        /// </summary>
        /// <returns>The list with the databases</returns>
        public List<string> LoadDatabases()
        {
            return Connector.Connection.Query<string>("SELECT [name] FROM sys.databases").ToList();
        }

        /// <summary>
        /// Releases all resources used by the <see cref="Repo"/>
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            Connector?.Dispose();

            _disposed = true;
        }
    }
}
