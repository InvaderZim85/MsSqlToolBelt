using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.Search;

/// <summary>
/// Represents an T-SQL job
/// </summary>
[DebuggerDisplay("{Name}")]
internal class JobEntry
{
    /// <summary>
    /// Gets or sets the id of the job
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the job
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the job
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicates if the job is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the id of the start step
    /// </summary>
    public int StartStepId { get; set; }

    /// <summary>
    /// Gets or sets the version number (The number is automatically incremented by one with each change)
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the creation date / time
    /// </summary>
    public DateTime CreationDateTime { get; set; }

    /// <summary>
    /// Gets or sets the modification date / time
    /// </summary>
    public DateTime ModifiedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the list with the job steps
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public List<JobStepEntry> JobSteps { get; set; } = new();
}