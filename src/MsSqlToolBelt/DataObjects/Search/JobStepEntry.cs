using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data;
using Newtonsoft.Json;
using System;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.Search;

/// <summary>
/// Represents a job step
/// </summary>
internal class JobStepEntry
{
    /// <summary>
    /// Gets or sets the id of the step
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the id of the step
    /// </summary>
    public int Step { get; set; }

    /// <summary>
    /// Gets or sets the name of the step
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command of the step
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the success action
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public int SuccessActionId { get; set; }

    /// <summary>
    /// Gets the description of the success action
    /// </summary>
    public string SuccessAction => SuccessActionId.GetJobInfo(JobStepInfoType.SuccessAction);

    /// <summary>
    /// Gets or sets the id of the next step when the step was successful
    /// </summary>
    public int SuccessNextStepId { get; set; }

    /// <summary>
    /// Gets or sets the id of the fail action
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public int FailActionId { get; set; }

    /// <summary>
    /// Gets the description of the fail action
    /// </summary>
    public string FailAction => FailActionId.GetJobInfo(JobStepInfoType.FailAction);

    /// <summary>
    /// Gets or sets the id of the next step when the step failed
    /// </summary>
    public int FailNextStepId { get; set; }

    /// <summary>
    /// Gets or sets the name of the database on which the job will be executed
    /// </summary>
    [Appearance(Name = "Database")]
    [JsonProperty("Database")]
    public string RunsOn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the amount of retry attempts
    /// </summary>
    public int RetryAttempts { get; set; }

    /// <summary>
    /// Gets or sets the id of the last run
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public int LastRunId { get; set; }

    /// <summary>
    /// Gets the information of the last run
    /// </summary>
    public string LastRun => LastRunId.GetJobInfo(JobStepInfoType.LastRun);

    /// <summary>
    /// Gets or sets the duration of the last run (format is hhmmss)
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public int LastRunDuration { get; set; }

    /// <summary>
    /// Gets the last run duration as TimeSpan
    /// </summary>
    public string LastDuration => DataHelper.ConvertToTimeSpan(LastRunDuration).ToFormattedString();

    /// <summary>
    /// Gets or sets the retries of the last run
    /// </summary>
    public int LastRunRetries { get; set; }

    /// <summary>
    /// Gets or sets the last run date (format is yyyymmdd)
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public int LastRunDate { get; set; }

    /// <summary>
    /// Gets or sets the last run time (format is hhmmss)
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public int LastRunTime { get; set; }

    /// <summary>
    /// Gets the date / time of the last run
    /// </summary>
    public DateTime LastRunDateTime => DataHelper.ConvertToDateTime(LastRunDate, LastRunTime);
}