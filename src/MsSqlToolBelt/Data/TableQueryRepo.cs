using Microsoft.Data.SqlClient;
using System.Data;

namespace MsSqlToolBelt.Data;

/// <summary>
/// Provides the functions for the interaction with the table data
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="TableQueryRepo"/>
/// </remarks>
/// <param name="dataSource">The name / path of the MSSQL server</param>
/// <param name="database">The name of the database</param>
internal class TableQueryRepo(string dataSource, string database) : BaseRepo(dataSource, database)
{
    /// <summary>
    /// Executes a query to load the top X rows of the table
    /// </summary>
    /// <param name="query">The query which should be executed</param>
    /// <param name="tableName">The name of the table</param>
    /// <returns>The data table with the content of the table</returns>
    public async Task<DataTable> LoadTableDataAsync(string query, string tableName)
    {
        await using var cmd = new SqlCommand(query, Connection);
        using var adapter = new SqlDataAdapter(cmd);
        var resultTable = new DataTable(tableName);
        adapter.Fill(resultTable);

        return resultTable;
    }
}