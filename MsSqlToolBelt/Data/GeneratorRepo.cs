using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MsSqlToolBelt.Data.Queries;
using MsSqlToolBelt.DataObjects.ClassGenerator;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.Data
{
    /// <summary>
    /// Provides the functions for the interaction with the database tables
    /// </summary>
    internal sealed class GeneratorRepo
    {
        /// <summary>
        /// Contains the instance of the <see cref="Connector"/>
        /// </summary>
        private readonly Connector _connector;

        /// <summary>
        /// Creates a new instance of the <see cref="GeneratorRepo"/>
        /// </summary>
        /// <param name="connector">The instance of the connector</param>
        public GeneratorRepo(Connector connector)
        {
            _connector = connector;
        }

        /// <summary>
        /// Loads all tables with its columns
        /// </summary>
        /// <returns>The list with the tables</returns>
        public async Task<List<Table>> LoadTables()
        {
            var tables = await _connector.Connection.ExtractMultiSearchTableResult(QueryManager.LoadTables);

            var result = tables.Select(s => (Table) s).ToList();
            return QueryHelper.ClearTableList(result);
        }
    }
}
