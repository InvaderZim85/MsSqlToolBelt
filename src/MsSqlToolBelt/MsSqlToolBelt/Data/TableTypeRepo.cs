using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.TableType;

namespace MsSqlToolBelt.Data;

/// <summary>
/// Provides the functions for the interaction with the table types
/// </summary>
internal sealed class TableTypeRepo : BaseRepo
{
    /// <summary>
    /// Creates a new instance of the <see cref="TableTypeRepo"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    public TableTypeRepo(string dataSource, string database) : base(dataSource, database) { }

    /// <summary>
    /// Loads all available user defined table types
    /// </summary>
    /// <returns>The list with the table types</returns>
    public async Task<List<TableTypeEntry>> LoadTableTypesAsync()
    {
        const string query =
            @"SELECT 
                tt.type_table_object_id AS Id,
                tt.[name]
            FROM
                sys.table_types AS tt
            WHERE
                tt.is_user_defined = 1;";

        return await QueryAsListAsync<TableTypeEntry>(query);
    }

    /// <summary>
    /// Enriches the table type with its columns
    /// </summary>
    /// <param name="tableType">The table type</param>
    /// <returns>The awaitable task</returns>
    public async Task EnrichTableTypeAsync(TableTypeEntry tableType)
    {
        // Step 1: Columns
        await LoadColumnsAsync(tableType);

        // Step 2: Primary key information
        await LoadPrimaryKeyInfoAsync(tableType);
    }

    /// <summary>
    /// Loads the columns of a table type
    /// </summary>
    /// <param name="tableType">The table type</param>
    /// <returns>The awaitable task</returns>
    private async Task LoadColumnsAsync(TableTypeEntry tableType)
    {
        const string query =
            @"SELECT
                c.[name] AS [Name],
                c.column_id AS [Order],
                st.[name] AS Datatype,
                c.max_length AS [MaxLength],
                c.precision,
                c.scale,
                c.is_nullable AS IsNullable,
                c.is_identity AS IsIdentity,
                c.is_computed AS IsComputed,
                ISNULL(OBJECT_DEFINITION(c.default_object_id), 'NULL') AS DefaultValue
            FROM
                sys.table_types AS tt

                INNER JOIN sys.columns AS c
                ON c.object_id = tt.type_table_object_id

                INNER JOIN sys.systypes AS st
                ON st.xusertype = c.system_type_id 
                AND st.uid = 4
            WHERE
                tt.type_table_object_id = @id
            ORDER BY
                tt.[name],
                c.column_id;";

        var result = await QueryAsListAsync<ColumnEntry>(query, tableType);

        tableType.Columns = result;
    }

    /// <summary>
    /// Loads the primary key information of the table type
    /// </summary>
    /// <param name="tableType">The table type</param>
    /// <returns>The awaitable task</returns>
    private async Task LoadPrimaryKeyInfoAsync(TableTypeEntry tableType)
    {
        const string query =
            @"SELECT 
                ic.column_id AS ColumnId
            FROM 
                sys.table_types AS tt
                
                INNER JOIN sys.key_constraints AS kc
                ON kc.parent_object_id = tt.type_table_object_id
                
                INNER JOIN sys.indexes AS i
                ON i.object_id = kc.parent_object_id
                    AND i.index_id = kc.unique_index_id
                
                INNER JOIN sys.index_columns AS ic
                ON ic.object_id = kc.parent_object_id
                
                INNER JOIN sys.columns AS c
                ON c.object_id = ic.object_id
                AND c.column_id = ic.column_id
            WHERE   
                tt.type_table_object_id = @id";

        var keyColumns = await QueryAsListAsync<int>(query, tableType);
        if (!keyColumns.Any())
            return;

        foreach (var column in tableType.Columns)
        {
            // Check over the "column id" which is in this case the "order"
            column.IsPrimaryKey = keyColumns.Contains(column.Order);
        }
    }
}