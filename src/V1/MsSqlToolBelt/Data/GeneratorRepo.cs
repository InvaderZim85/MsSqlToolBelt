using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
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
        public async Task<List<Table>> LoadTablesAsync()
        {
            var tables = await _connector.Connection.ExtractMultiSearchTableResultAsync(QueryManager.LoadTables);

            var result = tables.Select(s => (Table) s).ToList();
            return QueryHelper.ClearTableList(result);
        }

        /// <summary>
        /// Executes the query to retrieve the metadata
        /// </summary>
        /// <param name="query">The query which should be executed</param>
        /// <returns>The class data</returns>
        public async Task<List<SchemaColumn>> ExecuteQueryAsync(string query)
        {
            using var reader = await _connector.Connection.ExecuteReaderAsync(query);
            var schemaTable = reader.GetSchemaTable();

            if (schemaTable == null || schemaTable.Rows.Count == 0)
                return new List<SchemaColumn>(0);

            return (from DataRow row in schemaTable.Rows
                select new SchemaColumn
                {
                    ColumnName = row["ColumnName"].ToString(),
                    DateType = row["DataType"].ToString(),
                    ColumnOrdinal = (int)row["ColumnOrdinal"],
                    IsNullable = (bool)row["AllowDBNull"]
                }).ToList();
        }
    }
}
