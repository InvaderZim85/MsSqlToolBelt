using Dapper;
using MsSqlToolBelt.DataObjects.Common;
using System.Data;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Data;

/// <summary>
/// Provides the functions for the interaction with the metadata of a sql query
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ClassGenRepo"/>
/// </remarks>
/// <param name="dataSource">The name / path of the MSSQL database</param>
/// <param name="database">The name of the database</param>
internal sealed class ClassGenRepo(string dataSource, string database) : BaseRepo(dataSource, database)
{
    /// <summary>
    /// Loads the metadata according to the specified query
    /// </summary>
    /// <param name="query">The query which should be executed</param>
    /// <returns>THe list with the columns</returns>
    public async Task<List<ColumnEntry>> LoadMetadataAsync(string query)
    {
        // Sanitize the query
        query = query.TrimEnd();

        // Check if the query ends with a semicolon. If so remove it.
        if (query.EndsWith(';'))
            query = query.Replace(";", "");

        if (!query.ContainsIgnoreCase("WHERE"))
            query += " WHERE 0 = 1"; // Add this to force an "empty" result

        await using var reader = await Connection.ExecuteReaderAsync(query);
        var schemaTable = await reader.GetSchemaTableAsync();

        if (schemaTable == null || schemaTable.Rows.Count == 0)
            return [];

        return (from DataRow row in schemaTable.Rows
            select new ColumnEntry
            {
                Name = row["ColumnName"].ToString() ?? "",
                DataType = row["DataType"].ToString() ?? "",
                Order = (int) row["ColumnOrdinal"],
                IsNullable = (bool) row["AllowDBNull"]
            }).ToList();
    }
}