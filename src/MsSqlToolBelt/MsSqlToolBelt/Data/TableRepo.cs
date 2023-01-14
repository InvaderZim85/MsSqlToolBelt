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
    /// Loads the foreign keys of the desired table
    /// </summary>
    /// <param name="table">The table</param>
    /// <returns>The list with the foreign keys</returns>
    public async Task<List<ForeignKeyDto>> LoadForeignKeyInfoAsync(TableEntry table)
    {
        const string query =
            @"SELECT 
                fk.name AS [Name],
                c_parent.name AS ColumnName,
                t_child.name AS ReferencedTableName,
                c_child.name AS ReferencedColumnName
            FROM 
                sys.foreign_keys AS fk 

                INNER JOIN sys.foreign_key_columns AS fkc
                ON fkc.constraint_object_id = fk.object_id

                INNER JOIN sys.tables AS t_parent
                ON t_parent.object_id = fk.parent_object_id

                INNER JOIN sys.columns AS c_parent
                ON fkc.parent_column_id = c_parent.column_id  
                AND c_parent.object_id = t_parent.object_id 

                INNER JOIN sys.tables AS t_child
                ON t_child.object_id = fk.referenced_object_id

                INNER JOIN sys.columns AS c_child
                ON c_child.object_id = t_child.object_id
                AND fkc.referenced_column_id = c_child.column_id
            WHERE
                t_parent.object_id = @id
            ORDER BY 
                t_parent.name,
                c_parent.name;";

        var result = await QueryAsListAsync<ForeignKeyDto>(query, table);

        return result;
    }
}