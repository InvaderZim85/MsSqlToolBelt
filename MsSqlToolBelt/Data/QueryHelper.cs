using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.ClassGenerator;
using MsSqlToolBelt.DataObjects.Search;

namespace MsSqlToolBelt.Data
{
    /// <summary>
    /// Provides some helper functions
    /// </summary>
    internal static class QueryHelper
    {
        /// <summary>
        /// Contains the name of the type for a table (SearchResult)
        /// </summary>
        public const string TableTypeName = "Table";

        /// <summary>
        /// Extracts the result of the main search and the class generator query (both queries has the same result)
        /// </summary>
        /// <param name="connection">The current connection</param>
        /// <param name="query">The query</param>
        /// <param name="parameter">The parameters</param>
        /// <returns>The result</returns>
        public static async Task<List<SearchResult>> ExtractMultiSearchTableResult(this SqlConnection connection, string query, object parameter = null)
        {
            var multiResult = await connection.QueryMultipleAsync(query, parameter);

            var mainResult = await multiResult.ReadResultAsync<SearchResult>();
            var columns = await multiResult.ReadResultAsync<TableColumn>();
            var keyColumns = await multiResult.ReadResultAsync<KeyColumn>();
            var tmpIndices = await multiResult.ReadResultAsync<TableIndex>();
            var indices = tmpIndices.Where(w => !string.IsNullOrWhiteSpace(w.Name)).ToList();

            if (columns.Any() && keyColumns.Any())
            {
                foreach (var column in columns)
                {
                    column.IsPrimaryKey =
                        keyColumns.Any(a => a.Table.Equals(column.Table) && a.Column.Equals(column.Column));
                }
            }

            foreach (var entry in mainResult.Where(w => w.Type.Equals(TableTypeName)))
            {
                entry.Definition = "";
                entry.Columns = columns.Where(w => w.Table.Equals(entry.Name)).ToList();

                // Combine the indices for the table and add the result to the search result
                var tableIndices = indices.Where(w => w.Table.EqualsIgnoreCase(entry.Name)).ToList();

                if (!tableIndices.Any())
                    continue;

                var indexNames = tableIndices.Select(s => s.Name).Where(w => !string.IsNullOrWhiteSpace(w)).Distinct();

                foreach (var index in indexNames.OrderBy(o => o))
                {
                    var indexColumns = tableIndices.Where(w => w.Name.Equals(index)).Select(s => s.Column).ToList();
                    entry.Indices.Add(new TableIndex
                    {
                        Name = index,
                        Table = entry.Name,
                        Column = string.Join(", ", indexColumns.OrderBy(o => o))
                    });

                    foreach (var entryColumn in indexColumns
                        .Select(indexColumn =>
                            entry.Columns.FirstOrDefault(f => f.Column.EqualsIgnoreCase(indexColumn)))
                        .Where(entryColumn => entryColumn != null))
                    {
                        entryColumn.UsedInIndex = true;
                    }
                }
            }

            return mainResult;
        }

        /// <summary>
        /// Returns the result of the grid reader as a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static async Task<List<T>> ReadResultAsync<T>(this SqlMapper.GridReader reader)
        {
            var result = await reader.ReadAsync<T>();

            return result.ToList();
        }



        /// <summary>
        /// Removes all tables which should be ignored
        /// </summary>
        /// <param name="tableList">The list with the tables</param>
        public static List<Table> ClearTableList(List<Table> tableList)
        {
            var settings = Helper.LoadSettings();
            if (!settings.TableIgnoreList.Any())
                return tableList;

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

            return tableList;
        }

        /// <summary>
        /// Removes all tables which should be ignored
        /// </summary>
        /// <param name="searchResult">The list with the tables</param>
        public static void ClearTableList(List<SearchResult> searchResult)
        {
            var settings = Helper.LoadSettings();
            if (!settings.TableIgnoreList.Any())
                return;

            var removeList = new List<SearchResult>();

            foreach (var ignoreEntry in settings.TableIgnoreList)
            {
                switch (ignoreEntry.FilterType)
                {
                    case CustomEnums.FilterType.Equals:
                        removeList.AddRange(searchResult.Where(w => w.Type.Equals(TableTypeName) && w.Name.EqualsIgnoreCase(ignoreEntry.Value)));
                        break;
                    case CustomEnums.FilterType.Contains:
                        removeList.AddRange(searchResult.Where(w => w.Type.Equals(TableTypeName) && w.Name.ContainsIgnoreCase(ignoreEntry.Value)));
                        break;
                    case CustomEnums.FilterType.StartsWith:
                        removeList.AddRange(searchResult.Where(w => w.Type.Equals(TableTypeName) && w.Name.StartsWith(ignoreEntry.Value, StringComparison.OrdinalIgnoreCase)));
                        break;
                    case CustomEnums.FilterType.EndsWith:
                        removeList.AddRange(searchResult.Where(w => w.Type.Equals(TableTypeName) && w.Name.EndsWith(ignoreEntry.Value, StringComparison.OrdinalIgnoreCase)));
                        break;
                }
            }

            foreach (var entry in removeList)
            {
                searchResult.Remove(entry);
            }
        }

        /// <summary>
        /// Validates the specified sql query
        /// </summary>
        /// <param name="sqlQuery">The sql query</param>
        /// <returns>The validation result</returns>
        public static async Task<(bool valid, string message)> ValidateSql(string sqlQuery)
        {
            if (string.IsNullOrEmpty(sqlQuery))
                return (false, "");

            var result = await Task.Run(() => Parser.Parse(sqlQuery));
            
            var message = result.Errors.Any()
                ? string.Join(Environment.NewLine,
                    result.Errors.Select(s =>
                        $"{s.Message} (Line {s.Start.LineNumber}, Column {s.Start.ColumnNumber})"))
                : "";

            return (!result.Errors.Any(), message);
        }
    }
}
