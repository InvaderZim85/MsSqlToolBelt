using System.Collections.Generic;
using System.Threading.Tasks;
using MsSqlToolBelt.DataObjects.DefinitionExport;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.Data
{
    /// <summary>
    /// Provides the functions for the interaction with the object definitions
    /// </summary>
    internal sealed class DefinitionRepo
    {
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
        public async Task<List<string>> LoadDefinitionAsync(string objectName)
        {
            const string query = "EXEC sp_helptext @objname = @procName";
            return await _connector.Connection.QueryListAsync<string>(query, new
            {
                procName = objectName
            });
        }

        /// <summary>
        /// Loads all available procedures
        /// </summary>
        /// <returns>The list with the procedures</returns>
        public async Task<List<DefinitionEntry>> LoadProceduresAsync()
        {
            const string query =
                @"SELECT 
                    SCHEMA_NAME(schema_id) AS [Schema],
                    [name]
                FROM 
                    sys.objects
                WHERE 
                    type = 'P';";

            return await _connector.Connection.QueryListAsync<DefinitionEntry>(query);
        }
    }
}
