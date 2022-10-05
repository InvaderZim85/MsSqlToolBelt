using System.Collections.Generic;
using System.Threading.Tasks;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;

namespace MsSqlToolBelt.Data;

/// <summary>
/// Provides functions for the interaction with the tables
/// </summary>
internal class TableRepo : BaseRepo
{
    /// <summary>
    /// Creates a new instance of the <see cref="TableRepo"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL database</param>
    /// <param name="database">The name of the database</param>
    public TableRepo(string dataSource, string database) : base(dataSource, database) { }

    /// <summary>
    /// Loads all table which matches the search string
    /// </summary>
    /// <param name="search">The search string. If empty, all user defined tables will be loaded</param>
    /// <returns>The list with the tables</returns>
    public async Task<List<TableEntry>> LoadTablesAsync(string search)
    {
        var query =
            @"SELECT DISTINCT
                t.object_id AS Id,
                t.[name] AS [Name],
                s.[name] AS [Schema],
                t.is_replicated AS [IsReplicated],
                t.has_replication_filter AS [HasReplicationFilter],
                t.is_tracked_by_cdc AS [Cdc],
                t.create_date AS CreationDateTime,
                t.modify_date AS ModifiedDateTime
            FROM
                sys.tables AS t

                INNER JOIN sys.columns AS c
                ON c.object_id = t.object_id

                INNER JOIN sys.schemas AS s
                ON s.schema_id = t.schema_id
            WHERE
                t.is_ms_shipped = 0 -- only user tables";

        if (!string.IsNullOrEmpty(search))
            query +=
                @"
                AND 
                (
                    t.[name] LIKE @search
                    OR c.[name] LIKE @search
                );";

        return await QueryAsListAsync<TableEntry>(query, new
        {
            search
        });
    }

    /// <summary>
    /// Loads the columns of a table
    /// </summary>
    /// <param name="table">The table</param>
    /// <returns>The awaitable task</returns>
    public async Task LoadColumnsAsync(TableEntry table)
    {
        const string query =
            @"SELECT DISTINCT
                c.[name],
                c.column_id AS [Order],
                t.[name] AS DataType,
                c.max_length AS [MaxLength],
                c.[precision],
                c.scale,
                c.is_nullable AS IsNullable,
                c.is_replicated AS IsReplicated,
                c.is_identity AS IsIdentity,
                c.is_computed AS IsComputed,
                ISNULL(OBJECT_DEFINITION(c.default_object_id), 'NULL') AS DefaultValue
            FROM
                sys.columns AS c

                INNER JOIN sys.types AS t
                ON t.user_type_id = c.user_type_id
            WHERE
                object_id = @id;";

        var result = await QueryAsListAsync<ColumnEntry>(query, table);

        table.Columns = result;
    }

    /// <summary>
    /// Loads the indexes of the specified tables
    /// </summary>
    /// <param name="table">The table</param>
    /// <returns>The awaitable task</returns>
    public async Task<List<IndexDto>> LoadTableIndexAsync(TableEntry table)
    {
        const string query =
            @"SELECT DISTINCT
                i.[name] AS [Name],
                COL_NAME(ic.object_id, ic.column_id) AS [Column]
            FROM
                sys.indexes AS i

                INNER JOIN sys.index_columns AS ic
                ON ic.object_id = i.object_id
                AND ic.index_id = i.index_id

                INNER JOIN sys.columns AS c
                ON c.object_id = ic.object_id
            WHERE
                c.object_id = @id";

        var result = await QueryAsListAsync<IndexDto>(query, table);

        return result;
    }

    /// <summary>
    /// Loads the primary key information of the table
    /// </summary>
    /// <param name="table">The table</param>
    /// <returns>The awaitable task</returns>
    public async Task LoadPrimaryKeyInfoAsync(TableEntry table)
    {
        const string query =
            @"DECLARE @pkValues TABLE
            (
                [Database] SYSNAME NOT NULL,
                [Schema] SYSNAME NOT NULL,
                [Table] SYSNAME NOT NULL,
                [Column] SYSNAME NOT NULL,
                [KeySeq] SMALLINT NOT NULL,
                [PkName] SYSNAME NOT NULL
            );

            INSERT INTO @pkValues
            EXEC sp_pkeys @table_name = @name, @table_owner = @schema;

            SELECT
                [Column]
            FROM
                @pkValues;";

        var columns = await QueryAsListAsync<string>(query, table);

        foreach (var column in table.Columns)
        {
            column.IsPrimaryKey = columns.Contains(column.Name);
        }
    }
}