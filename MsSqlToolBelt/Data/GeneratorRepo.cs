using System.Collections.Generic;
using System.Linq;
using Dapper;
using MsSqlToolBelt.Data.Queries;
using MsSqlToolBelt.DataObjects;
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
        public List<Table> LoadTables()
        {
            var result = _connector.Connection.QueryMultiple(QueryManager.LoadTables);

            var tables = result.Read<Table>().ToList();
            var columns = result.Read<TableColumn>().ToList();

            foreach (var table in tables)
            {
                table.Columns = columns.Where(w => w.Table.Equals(table.Name)).ToList();
            }

            return tables;
        }
    }
}
