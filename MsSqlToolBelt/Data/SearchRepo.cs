using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MsSqlToolBelt.Data.Queries;
using MsSqlToolBelt.DataObjects.Search;
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
        /// <param name="matchWholeWord">true to match only the whole word, otherwise false</param>
        /// <returns>The result</returns>
        public async Task<List<SearchResult>> SearchAsync(string value, bool matchWholeWord)
        {
            var searchString = $"%{value}%";

            var searchResults = await _connector.Connection.ExtractMultiSearchTableResultAsync(QueryManager.Search, new
            {
                search = searchString
            });

            // Clean the search result and remove all not needed tables
            QueryHelper.ClearTableList(searchResults);

            searchResults.Add(await SearchJobsAsync(searchString));
            searchResults.AddRange(await SearchJobStepsAsync(searchString));

            if (!matchWholeWord) 
                return searchResults.Where(w => w != null).ToList();

            var regex = new Regex($@"(^|\s|\.){value}(\s|$)");

            return searchResults.Where(w =>
                w != null &&
                (regex.IsMatch(w.Name) || regex.IsMatch(w.Definition))).ToList();
        }

        /// <summary>
        /// Search in the jobs for the given string
        /// </summary>
        /// <param name="value">The search value</param>
        /// <returns>The result</returns>
        private async Task<SearchResult> SearchJobsAsync(string value)
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

            var tmpResult = await _connector.Connection.QueryListAsync<SearchResult>(query);

            return CreateJobDescription(tmpResult);
        }

        /// <summary>
        /// Search in the job list for the given string 
        /// </summary>
        /// <param name="value">The search value</param>
        /// <returns>The result</returns>
        private async Task<List<SearchResult>> SearchJobStepsAsync(string value)
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

            var tmpResult = await _connector.Connection.QueryListAsync<SearchResult>(query, new { value });

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
        private static IEnumerable<string> CreateJobDefinition(string definition)
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
        private static string GetAction(int id, int nextStepId, int maxLength = 0)
        {
            var result = id switch
            {
                1 => "Quit with success",
                2 => "Quit with failure",
                3 => "Go to next step",
                4 => $"Go to step {nextStepId}",
                _ => "/"
            };

            return maxLength == 0 ? result : result.PadRight(maxLength);
        }
    }
}
