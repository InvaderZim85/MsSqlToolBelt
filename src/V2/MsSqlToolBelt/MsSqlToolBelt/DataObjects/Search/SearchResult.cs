using System;
using MsSqlToolBelt.DataObjects.Common;

namespace MsSqlToolBelt.DataObjects.Search;

/// <summary>
/// Represents the search result
/// </summary>
internal class SearchResult
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the info
    /// </summary>
    public string Info { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the entry
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date / time
    /// </summary>
    public DateTime CreationDateTime { get; init; }

    /// <summary>
    /// Gets or sets the modification date / time
    /// </summary>
    public DateTime ModifiedDateTime { get; init; }

    /// <summary>
    /// Gets or sets the bounded item
    /// </summary>
    public object? BoundItem { get; private init; }

    /// <summary>
    /// Converts the <see cref="TableEntry"/> into a <see cref="SearchResult"/>
    /// </summary>
    /// <param name="table">The table</param>
    public static explicit operator SearchResult(TableEntry table)
    {
        return new SearchResult
        {
            Name = table.Name,
            Info = table.GetInfo(),
            Type = "Table",
            CreationDateTime = table.CreationDateTime,
            ModifiedDateTime = table.ModifiedDateTime,
            BoundItem = table
        };
    }

    /// <summary>
    /// Converts the <see cref="ObjectEntry"/> into a <see cref="SearchResult"/>
    /// </summary>
    /// <param name="entry">The object</param>
    public static explicit operator SearchResult(ObjectEntry entry)
    {
        return new SearchResult
        {
            Name = entry.Name,
            Type = entry.TypeName,
            CreationDateTime = entry.CreationDateTime,
            ModifiedDateTime = entry.ModifiedDateTime,
            BoundItem = entry
        };
    }

    /// <summary>
    /// Converts the <see cref="JobEntry"/> into a <see cref="SearchResult"/>
    /// </summary>
    /// <param name="job">The job</param>
    public static explicit operator SearchResult(JobEntry job)
    {
        var enableInfo = job.Enabled ? "Job is enabled" : "Job is disabled";
        var jobVersion = $"Version: {job.Version:N0}";

        return new SearchResult
        {
            Name = job.Name,
            Info = $"{enableInfo}, {jobVersion}",
            Type = "Job",
            CreationDateTime = job.CreationDateTime,
            ModifiedDateTime = job.ModifiedDateTime,
            BoundItem = job
        };
    }
}