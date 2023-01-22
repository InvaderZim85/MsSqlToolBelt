using System.Collections.Generic;
using System.Threading.Tasks;
using MsSqlToolBelt.DataObjects.Common;

namespace MsSqlToolBelt.Data;

internal class DefinitionExportRepo : BaseRepo
{
    /// <summary>
    /// Creates a new instance of the <see cref="DefinitionExportRepo"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    public DefinitionExportRepo(string dataSource, string database) : base(dataSource, database) { }

    /// <summary>
    /// Loads all relevant objects (procedures, views, ...)
    /// </summary>
    /// <returns></returns>
    public async Task<List<ObjectEntry>> LoadObjectsAsync()
    {
        const string query =
            @"SELECT DISTINCT
                OBJECT_NAME(m.object_id) AS [Name],
                m.definition AS [Definition],
                o.[type],
                o.create_date AS CreationDateTime,
                o.modify_date AS ModifiedDateTime
            FROM
                sys.sql_modules AS m

                INNER JOIN sys.objects AS o
                ON o.object_id = m.object_id
            WHERE
                o.[type] <> 'U' -- Ignore the tables
                AND o.is_ms_shipped = 0; -- Only user stuff";

        return await QueryAsListAsync<ObjectEntry>(query);
    }
}