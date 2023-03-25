using MsSqlToolBelt.DataObjects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using MsSqlToolBelt.DataObjects.DefinitionExport;
using MsSqlToolBelt.DataObjects.Table;

namespace MsSqlToolBelt.Data;

/// <summary>
/// Provides the functions for the definition export
/// </summary>
internal class DefinitionExportRepo : BaseRepo
{
    /// <summary>
    /// Occurs when the definition load makes progress
    /// </summary>
    public event EventHandler<string>? Progress;

    /// <summary>
    /// The name / path of the MSSQL server
    /// </summary>
    private readonly string _dataSource;

    /// <summary>
    /// The name of the database
    /// </summary>
    private readonly string _database;

    /// <summary>
    /// Creates a new instance of the <see cref="DefinitionExportRepo"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    public DefinitionExportRepo(string dataSource, string database) : base(dataSource, database)
    {
        _dataSource = dataSource;
        _database = database;
    }

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

    /// <summary>
    /// Loads the definition of the desired tables
    /// </summary>
    /// <param name="tables">The list with the tables</param>
    /// <returns>The list with the table definitions</returns>
    public async Task<List<TableDefinition>> LoadTableDefinitionAsync(List<DefinitionExportObject> tables)
    {
        var server = new Server(_dataSource);
        var database = server.Databases[_database];

        // Define the scripter
        var scripter = new Scripter(server)
        {
            Options = new ScriptingOptions
            {
                ClusteredIndexes = true, // Include the clustered indices
                Default = true,
                FullTextIndexes = true, // Include full text indexes
                Indexes = true, // Include the indices
                NonClusteredIndexes = true,
                SchemaQualify = true, // Qualify objects with schema names
                ScriptData = false, // We don't want the data...
                ScriptDrops = false, // Script drop statements
                ScriptSchema = true,
                Statistics = true,
                Triggers = true, // To get the triggers
                WithDependencies = true, // Include the dependencies. If a table needs another table, the table will be included into the script
                DriAll = true,
                IncludeIfNotExists = true, // Includes an if not exists check
                NoCollation = true, // Use default collation
                ExtendedProperties = true, // Script extended properties
                IncludeDatabaseContext = true,
                EnforceScriptingOptions = true,
            }
        };

        var count = 1;
        var totalCount = tables.Count;

        var result = new List<TableDefinition>();

        foreach (Table table in database.Tables)
        {
            // We don't want to check system tables
            if (table.IsSystemObject)
                continue;

            // Check if the table should be exported
            if (!tables.Select(s => s.Name).Contains(table.Name, StringComparer.OrdinalIgnoreCase))
                continue;

            Progress?.Invoke(this, $"{count++} of {totalCount} > Load '{table.Name}' definition...");

            var script = await Task.Run(() => scripter.Script(new[] { table.Urn }));
            var content = new StringBuilder();
            
            AddNote(content);

            foreach (var contentLine in script)
            {
                content.AppendLine(contentLine);
            }

            result.Add(new TableDefinition(table.Name, content.ToString()));
        }

        return result;
    }

    /// <summary>
    /// Adds a note to the builder
    /// </summary>
    /// <param name="builder">The string builder</param>
    private void AddNote(StringBuilder builder)
    {
        builder
            .AppendLine("/*")
            .AppendLine(" * NOTE / ATTENTION")
            .AppendLine(" * ----------------")
            .AppendLine(
                " * The following script was generated automatically. Please check the content before you execute the script!")
            .AppendLine(" */")
            .AppendLine();
    }
}