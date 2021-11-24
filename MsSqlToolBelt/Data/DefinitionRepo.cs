using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.Data
{
    /// <summary>
    /// Provides the functions for the interaction with the object definitions
    /// </summary>
    internal sealed class DefinitionRepo : IDisposable
    {
        /// <summary>
        /// Contains the value which indicates if the class was already disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The instance for the connection with the database
        /// </summary>
        private readonly Connector _connector;

        /// <summary>
        /// Creates a new instance of the <see cref="DefinitionRepo"/>
        /// </summary>
        /// <param name="connector">The instance of the connector</param>
        public DefinitionRepo(Connector connector)
        {
            _connector = connector;
        }

        /// <summary>
        /// Loads the definition of the specified object
        /// </summary>
        /// <param name="objectName">The name of the object</param>
        /// <returns></returns>
        public async Task<List<string>> LoadDefinition(string objectName)
        {
            const string query = "EXEC sp_helptext @objname = @procName";
            return await _connector.Connection.QueryListAsync<string>(query, new
            {
                procName = objectName
            });
        }

        /// <summary>
        /// Releases all resources used by the <see cref="DefinitionRepo"/>
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
        }
    }
}
