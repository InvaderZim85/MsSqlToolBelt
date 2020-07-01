using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MsSqlToolBelt.Data.Queries;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.ClassGenerator;
using ZimLabs.CoreLib.Extensions;
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

            ClearTableList(tables);
            return tables;
        }

        /// <summary>
        /// Removes all tables which should be ignored
        /// </summary>
        /// <param name="tableList">The list with the tables</param>
        private void ClearTableList(List<Table> tableList)
        {
            var settings = Helper.LoadSettings();
            if (!settings.TableIgnoreList.Any())
                return;

            var removeList = new List<Table>();

            foreach (var ignoreEntry in settings.TableIgnoreList)
            {
                switch (ignoreEntry.FilterType)
                {
                    case CustomEnums.FilterType.Equals:
                        removeList.AddRange(tableList.Where(w => w.Name.EqualsIgnoreCase(ignoreEntry.Value)));
                        break;
                    case CustomEnums.FilterType.Contains:
                        removeList.AddRange(tableList.Where(w => w.Name.ContainsIgnoreCase(ignoreEntry.Value)));
                        break;
                    case CustomEnums.FilterType.StartsWith:
                        removeList.AddRange(tableList.Where(w => w.Name.StartsWith(ignoreEntry.Value, StringComparison.OrdinalIgnoreCase)));
                        break;
                    case CustomEnums.FilterType.EndsWith:
                        removeList.AddRange(tableList.Where(w => w.Name.EndsWith(ignoreEntry.Value, StringComparison.OrdinalIgnoreCase)));
                        break;
                }
            }

            foreach (var entry in removeList)
            {
                tableList.Remove(entry);
            }
        }
    }
}
