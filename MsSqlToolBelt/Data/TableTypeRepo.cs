using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MsSqlToolBelt.Data.Queries;
using MsSqlToolBelt.DataObjects.TableType;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.Data
{
    internal sealed class TableTypeRepo
    {
        /// <summary>
        /// The instance which creates the connection to the database
        /// </summary>
        private readonly Connector _connector;

        /// <summary>
        /// Creates a new instance of the <see cref="TableTypeRepo"/>
        /// </summary>
        /// <param name="connector">The instance of the connector</param>
        public TableTypeRepo(Connector connector)
        {
            _connector = connector;
        }

        /// <summary>
        /// Loads all available custom table types
        /// </summary>
        /// <returns>The list with the table types</returns>
        public async Task<List<TableType>> LoadTableTypesAsync()
        {
            var reader = await _connector.Connection.QueryMultipleAsync(QueryManager.LoadTableTypes);

            if (reader == null)
                return null;

            var tables = await reader.ReadResultAsync<TableType>();
            var columns = await reader.ReadResultAsync<TableTypeColumn>();
            var keyColumns = await reader.ReadResultAsync<KeyValuePair<int, int>>();

            foreach (var table in tables)
            {
                table.Columns = columns.Where(w => w.TableTypeId == table.Id).ToList();

                foreach (var column in table.Columns)
                {
                    column.IsPrimaryKey = keyColumns.Any(a => a.Key == table.Id && a.Value == column.ColumnId);
                }
            }

            return tables;
        }
    }
}
