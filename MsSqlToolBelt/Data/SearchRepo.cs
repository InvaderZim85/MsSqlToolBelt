using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.Search;
using ZimLabs.CoreLib.Extensions;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.Data
{
    /// <summary>
    /// Provides the functions to search in the database for a given string
    /// </summary>
    internal sealed class SearchRepo
    {
        /// <summary>
        /// Contains the instance of the <see cref="Connector"/>
        /// </summary>
        private readonly Connector _connector;

        /// <summary>
        /// Contains the name of the selected database
        /// </summary>
        private string _selectedDatabase;

        /// <summary>
        /// Creates a new instance of the <see cref="SearchRepo"/>
        /// </summary>
        /// <param name="connector">The instance of the connector</param>
        public SearchRepo(Connector connector)
        {
            _connector = connector;
        }

        /// <summary>
        /// Switches the database
        /// </summary>
        /// <param name="switchToMsdb">true to switch to the msdb, otherwise false</param>
        private void SwitchDatabaseForJobSearch(bool switchToMsdb)
        {
            if (switchToMsdb)
            {
                _selectedDatabase = _connector.InitialCatalog;
                _connector.SwitchDatabase("msdb");
            }
            else
            {
                _connector.SwitchDatabase(_selectedDatabase);
            }
        }

        /// <summary>
        /// Performs a search with the given value
        /// </summary>
        /// <param name="value">The value to search for</param>
        /// <returns>The result</returns>
        public List<SearchResult> Search(string value)
        {
            value = $"%{value}%";

            const string query =
                @"DECLARE @result TABLE
                (
                    [Id] INT NOT NULL,
                    [Name] NVARCHAR(200) NOT NULL,
                    [Definition] NVARCHAR(MAX) NULL,
                    [Type] NVARCHAR(10) NOT NULL
                );

                INSERT INTO @result
                SELECT
                    m.object_id,
                    OBJECT_NAME(m.object_id) AS [Name],
                    m.[definition] AS [Definition],
                    'Procedure' AS [Type]
                FROM
                    sys.sql_modules m
                WHERE
                    m.[definition] LIKE @search

                UNION

                SELECT DISTINCT
                    t.object_id,
                    t.[name],
                    '',
                    'Table'
                FROM
                    sys.tables t

                    INNER JOIN sys.columns c
                    ON c.object_id = t.object_id
                WHERE
                    t.[Name] LIKE @search
                    OR c.[name] LIKE @search;

                SELECT
                    r.Id,
                    r.[Name],
                    r.[Definition],
                    r.[Type]
                FROM
                    @result r
                WHERE
                    r.[Name] NOT LIKE 'syncobj%'
                    AND r.[Name] NOT LIKE '%_CT'
                    AND r.[Name] NOT LIKE 'SYNC%'
                ORDER BY
                    r.[Type];";

            var results = _connector.Connection.Query<SearchResult>(query, new
            {
                search = value
            }).ToList();

            var columns = LoadColumns(results.Where(w => w.Type == "Table").Select(s => s.Id).ToList());

            foreach (var entry in results.Where(w => w.Type == "Table"))
            {
                var tmpColumns = columns.Where(w => w.Table.EqualsIgnoreCase(entry.Name)).ToList();

                entry.Definition = CreateTableDefinition(tmpColumns);
            }

            results.Add(SearchJobs(value));
            results.AddRange(SearchJobSteps(value));

            return results.Where(w => w != null).ToList();
        }

        /// <summary>
        /// Loads the columns of the given tables
        /// </summary>
        /// <param name="tables">The list with the tables</param>
        /// <returns>The list with the columns</returns>
        private List<TableColumn> LoadColumns(List<int> tables)
        {
            const string query =
                @"SELECT 
                    t.[name] AS [Table],
                    c.[name] AS [Column],
                    ty.[name] AS [DataType]
                FROM
                    sys.columns c

                    INNER JOIN sys.tables t
                    ON t.object_id = c.object_id

                    INNER JOIN sys.types ty
                    ON ty.system_type_id = c.system_type_id
                WHERE 
                    c.object_id IN @tables
                    AND ty.[name] <> 'sysname'";

            return _connector.Connection.Query<TableColumn>(query, new
            {
                tables
            }).ToList();
        }

        /// <summary>
        /// Search in the jobs for the given string
        /// </summary>
        /// <param name="value">The search value</param>
        /// <returns>The result</returns>
        private SearchResult SearchJobs(string value)
        {
            SwitchDatabaseForJobSearch(true);

            const string query =
                @"SELECT
                    0 AS Id,
                    j.[name],
                    j.[description] AS [Definition],
                    'Job' AS [Type],
                    js.step_id AS StepId,
                    js.step_name AS StepName,
                    js.on_success_action AS SuccessAction,
                    js.on_success_step_id AS SuccessStepId,
                    js.on_fail_action AS FailAction,
                    js.on_fail_step_id AS FailStepId
                FROM
                    dbo.sysjobs AS j

                    INNER JOIN dbo.sysjobsteps AS js
                    ON js.job_id = j.job_id
                WHERE
                    j.[name] LIKE @value";

            var tmpResult = _connector.Connection.Query<SearchResult>(query, new { value }).ToList();

            return CreateJobDescription(tmpResult);
        }

        /// <summary>
        /// Search in the job list for the given string 
        /// </summary>
        /// <param name="value">The search value</param>
        /// <returns>The result</returns>
        private List<SearchResult> SearchJobSteps(string value)
        {
            // Step 1: Switch to the MSDB database
            _connector.SwitchDatabase("msdb");

            // Step 2: Search
            const string query =
                @"SELECT
                    0 AS Id,
                    j.[name],
                    j.[description] AS [Definition],
                    'Job' AS [Type],
                    js.step_id AS StepId,
                    js.step_name AS StepName,
                    js.on_success_action AS SuccessAction,
                    js.on_success_step_id AS SuccessStepId,
                    js.on_fail_action AS FailAction,
                    js.on_fail_step_id AS FailStepId
                FROM
                    dbo.sysjobs AS j

                    INNER JOIN dbo.sysjobsteps AS js
                    ON js.job_id = j.job_id
                WHERE
                    js.command LIKE @value";

            var tmpResult = _connector.Connection.Query<SearchResult>(query, new { value }).ToList();

            foreach (var entry in tmpResult)
            {
                entry.Definition = CreateJobDescriptionForStep(entry);
                entry.Name = $"{entry.Name} | {entry.StepName}";
            }

            // Step 3: Switch back to the original database
            SwitchDatabaseForJobSearch(false);

            return tmpResult;
        }

        /// <summary>
        /// Beautifies the description of the jop and its steps
        /// </summary>
        /// <param name="jobData">The job data</param>
        /// <returns>The result data</returns>
        private SearchResult CreateJobDescription(List<SearchResult> jobData)
        {
            // Get the first entry
            var job = jobData?.FirstOrDefault();
            if (job == null)
                return null;

            // Get the values for the "table"
            var longestStepName = jobData.Max(m => m.StepName.Length);
            if (longestStepName < 4)
                longestStepName = 4;

            var stepLength = jobData.Max(m => m.StepId.ToString().Length);
            if (stepLength < 4)
                stepLength = 4;

            // Create the definition for the first entry
            var result = CreateJobDefinition(job.Definition);

            // Add the steps
            var sb = new StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine($" * Job: {job.Name}");
            sb.AppendLine(" * ---------------");
            sb.AppendLine(" * Description:");
            foreach (var entry in result)
            {
                sb.AppendLine($" *  {entry}");
            }
            sb.AppendLine(" * ---------------");
            sb.AppendLine(" * Steps:");
            PrintHorizontalLine();
            sb.AppendLine($" * | {"Step".PadRight(stepLength)} | {"Name".PadRight(longestStepName)} | Success action    | Fail action       | ");
            PrintHorizontalLine();

            foreach (var jobStep in jobData.OrderBy(o => o.StepId))
            {
                sb.AppendLine($" * | {jobStep.StepId.ToString().PadRight(stepLength)} | {jobStep.StepName.PadRight(longestStepName)} | " +
                              $"{GetAction(jobStep.SuccessAction, jobStep.SuccessStepId, 17)} | {GetAction(jobStep.FailAction, jobStep.FailStepId, 17)} |");
            }

            PrintHorizontalLine();
            sb.AppendLine("*/");

            void PrintHorizontalLine()
            {
                sb.AppendLine($" * +-{"-".PadRight(stepLength, '-')}-+-{"-".PadRight(longestStepName, '-')}-+-------------------+-------------------+");
            }

            job.Definition = sb.ToString();

            return job;
        }

        /// <summary>
        /// Creates the column information for a table
        /// </summary>
        /// <param name="columns">The list with the columns</param>
        /// <returns>The table "definition"</returns>
        private string CreateTableDefinition(List<TableColumn> columns)
        {
            var tableName = columns.FirstOrDefault()?.Table;

            if (string.IsNullOrEmpty(tableName))
                return "-- Columns missing";

            var sb = new StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine($" * Columns of '{tableName}'");

            var longestColumnName = columns.Max(m => m.Column.Length);
            if (longestColumnName < 6)
                longestColumnName = 6;

            var longestType = columns.Max(m => m.DataType.Length);
            if (longestType < 4)
                longestType = 4;

            PrintHorizontalLine();
            sb.AppendLine($" * | Nr | {"Column".PadRight(longestColumnName)} | {"Type".PadRight(longestType)} |");
            PrintHorizontalLine();

            var count = 1;
            foreach (var column in columns)
            {
                //sb.AppendLine($" * {count++} - {column.Column} ({column.DataType})");
                sb.AppendLine(
                    $" * | {count++,2} | {column.Column.PadRight(longestColumnName)} | {column.DataType.PadRight(longestType)} |");
            }
            PrintHorizontalLine();
            sb.AppendLine(" *");
            sb.AppendLine(" * For more information of the table execute the following command:");
            sb.AppendLine("*/");
            sb.AppendLine($"EXEC sys.sp_help @objname = N'{tableName}'");

            void PrintHorizontalLine()
            {
                sb.AppendLine($" * +----+-{"-".PadRight(longestColumnName, '-')}-+-{"-".PadRight(longestType, '-')}-+");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Beautifies the description of the job
        /// </summary>
        /// <param name="jobData">The job data</param>
        /// <returns>The beautified description</returns>
        private string CreateJobDescriptionForStep(SearchResult jobData)
        {
            if (string.IsNullOrEmpty(jobData.Definition))
                return "";

            var result = CreateJobDefinition(jobData.Definition);

            // Create the output
            var sb = new StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine($" * Job......: {jobData.Name}");
            sb.AppendLine($" * Step.....: {jobData.StepId}");
            sb.AppendLine($" * Step name: {jobData.StepName}");
            sb.AppendLine(" * Action:");
            sb.AppendLine($" *  - Success: {GetAction(jobData.SuccessAction, jobData.SuccessStepId)}");
            sb.AppendLine($" *  - Fail...: {GetAction(jobData.FailAction, jobData.FailStepId)}");
            sb.AppendLine(" * ---------------");
            sb.AppendLine(" * Description:");

            foreach (var entry in result)
            {
                sb.AppendLine($" *  {entry}");
            }

            sb.AppendLine(" */");

            return sb.ToString();
        }

        /// <summary>
        /// Creates the job definition as a list of strings
        /// </summary>
        /// <param name="definition">The job definition</param>
        /// <returns>The list with the data</returns>
        private List<string> CreateJobDefinition(string definition)
        {
            const int maxLength = 60;
            definition = definition.Replace("\r\n", " "); // Remove the line breaks

            var content = definition.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var result = new List<string>();
            var tmpEntry = "";

            foreach (var entry in content)
            {
                // Check if the max length was reached
                if (tmpEntry.Length + entry.Length > maxLength)
                {
                    result.Add($"{tmpEntry}{entry}");
                    tmpEntry = "";
                }
                else
                {
                    tmpEntry += $"{entry} ";
                }
            }
            // Add the last entry
            result.Add(tmpEntry);

            return result;
        }

        /// <summary>
        /// Gets the description of a job step action id
        /// </summary>
        /// <param name="id">The action id</param>
        /// <param name="nextStepId">The id of the next step</param>
        /// <param name="maxLength">The max length of the result</param>
        /// <returns>The description of the action</returns>
        private string GetAction(int id, int nextStepId, int maxLength = 0)
        {
            string result;
            switch (id)
            {
                case 1:
                    result = "Quit with success";
                    break;
                case 2:
                    result = "Quit with failure";
                    break;
                case 3:
                    result = "Go to next step";
                    break;
                case 4:
                    result = $"Go to step {nextStepId}";
                    break;
                default:
                    result = "/";
                    break;
            }

            return maxLength == 0 ? result : result.PadRight(maxLength);
        }
    }
}
