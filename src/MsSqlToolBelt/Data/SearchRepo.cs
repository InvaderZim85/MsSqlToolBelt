using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MsSqlToolBelt.Data;

/// <summary>
/// Provides the functions for the search
/// </summary>
internal sealed class SearchRepo : BaseRepo
{
    /// <summary>
    /// Contains the name of the selected database
    /// </summary>
    private readonly string _selectedDatabase;

    /// <summary>
    /// Creates a new instance of the <see cref="SearchRepo"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    public SearchRepo(string dataSource, string database) : base(dataSource, database)
    {
        _selectedDatabase = database;
    }

    /// <summary>
    /// Switches the database 
    /// </summary>
    /// <param name="switchToMsdb"><see langword="true"/> to switch to the MSDB database, <see langword="false"/> to switch back to the selected database</param>
    private void SwitchDatabase(bool switchToMsdb)
    {
        SwitchDatabase(switchToMsdb ? "MSDB" : _selectedDatabase);
    }

    #region Search
    /// <summary>
    /// Executes the search
    /// </summary>
    /// <param name="search">The search string</param>
    /// <returns>The list with the results</returns>
    public async Task<List<SearchResult>> SearchAsync(string search)
    {
        // NOTE: The tables will be added in the manager...
        var result = new List<SearchResult>();
        
        // Step 1: Objects (procedures, views, etc.)
        var objects = await LoadObjectsAsync(search);
        result.AddRange(objects.Select(s => (SearchResult) s));

        // Step 2: Jobs
        var jobs = await LoadJobsAsync(search);
        result.AddRange(jobs.Select(s => (SearchResult) s));

        return result;
    }

    /// <summary>
    /// Loads all objects which matches the search string
    /// </summary>
    /// <param name="search">The search string</param>
    /// <returns>The list with the objects</returns>
    private async Task<List<ObjectEntry>> LoadObjectsAsync(string search)
    {
        const string query =
            @"SELECT DISTINCT
                m.object_id AS Id,
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
                (
                    m.definition LIKE @search
                    OR OBJECT_NAME(m.object_id) LIKE @search
                )
                AND o.[type] <> 'U' -- Ignore the tables
                AND o.is_ms_shipped = 0; -- Only user stuff";

        return await QueryAsListAsync<ObjectEntry>(query, new
        {
            search
        });
    }

    /// <summary>
    /// Load all jobs which matches the search string (Search is done in the name, description, step name, step command)
    /// </summary>
    /// <param name="search">The search string</param>
    /// <returns>The list with the jobs</returns>
    private async Task<List<JobEntry>> LoadJobsAsync(string search)
    {
        // Change the connection to the msdb database
        SwitchDatabase(true);

        const string query =
            @"SELECT DISTINCT
                j.job_id AS Id,
                j.[name],
                j.[description] AS [Description],
                j.enabled AS [Enabled],
                j.start_step_id AS StartStepId,
                j.version_number AS Version,
                j.owner_sid,
                j.date_created AS CreationDateTime,
                j.date_modified AS ModifiedDateTime
            FROM
                dbo.sysjobs AS j

                INNER JOIN dbo.sysjobsteps AS js
                ON js.job_id = j.job_id
            WHERE
                j.name LIKE @search
                OR j.[description] LIKE @search
                OR js.step_name LIKE @search
                OR js.command LIKE @search;";

        var result = await QueryAsListAsync<JobEntry>(query, new
        {
            search
        });

        // Switch back
        SwitchDatabase(false);

        return result;
    }
    #endregion

    #region Job information
    /// <summary>
    /// Enriches the job with the job steps
    /// </summary>
    /// <param name="job">The job which should be enriched</param>
    /// <returns>The awaitable task</returns>
    public async Task EnrichJobAsync(JobEntry job)
    {
        // Change the connection to the msdb database
        SwitchDatabase(true);

        const string query =
            @"SELECT DISTINCT
                js.step_id AS Id,
                js.step_name AS [Name],
                js.step_id AS Step,
                js.command,
                js.on_success_action AS SuccessActionId,
                js.on_success_step_id AS SuccessNextStepId,
                js.on_fail_action AS FailActionId,
                js.on_fail_step_id AS FailNextStepId,
                js.database_name AS RunsOn,
                js.retry_attempts AS RetryAttempts,
                js.last_run_outcome AS LastRunId,
                js.last_run_duration AS LastRunDuration,
                js.last_run_retries AS LastRunRetries,
                js.last_run_date AS LastRunDate,
                js.last_run_time AS LastRunTime
            FROM
                dbo.sysjobs AS j

                INNER JOIN dbo.sysjobsteps AS js
                ON js.job_id = j.job_id
            WHERE
                j.job_id = @id;";

        var result = await QueryAsListAsync<JobStepEntry>(query, job);

        job.JobSteps = result;

        // Change the connection back
        SwitchDatabase(false);
    }
    #endregion
}