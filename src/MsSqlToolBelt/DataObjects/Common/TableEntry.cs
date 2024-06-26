﻿using MsSqlToolBelt.DataObjects.Search;
using Newtonsoft.Json;
using System.Diagnostics;
using MsSqlToolBelt.DataObjects.Table;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a table
/// </summary>
[DebuggerDisplay("{Name}")]
public class TableEntry : IdNameBase
{
    /// <summary>
    /// Gets or sets the name of the according schema like dbo
    /// </summary>
    public string Schema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicates if the table is replicated
    /// </summary>
    public bool IsReplicated { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if the table has a replication filter
    /// </summary>
    public bool HasReplicationFilter { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if the table is watch by CDC (Change Data Capture)
    /// </summary>
    public bool Cdc { get; set; }

    /// <summary>
    /// Gets the name of the available publications
    /// </summary>
    [Appearance(Ignore = true)]
    public string Publications => string.Join(", ", ReplicationInformation.Select(s => s.Publication).Distinct().OrderBy(o => o));

    /// <summary>
    /// Gets or sets the creation date / time
    /// </summary>
    public DateTime CreationDateTime { get; set; }

    /// <summary>
    /// Gets or sets the modification date / time
    /// </summary>
    public DateTime ModifiedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the list with the columns
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public List<ColumnEntry> Columns { get; set; } = [];

    /// <summary>
    /// Gets or sets the list with the indizes
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public List<IndexEntry> Indexes { get; set; } = [];

    /// <summary>
    /// Gets or sets the value which indicates if there are columns with the same name.
    /// <para />
    /// Only needed for the class generator which generates a class from a query
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public bool ColumnUniqueError { get; set; }

    /// <summary>
    /// Gets or sets the definition of the table
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public string Definition { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list with the replication information
    /// </summary>
    [Appearance(Ignore = true)]
    public List<ReplicationArticle> ReplicationInformation { get; set; } = [];

    /// <summary>
    /// Gets an info message of the table
    /// </summary>
    /// <returns>The information of the table</returns>
    public string GetInfo()
    {
        var replicated = $"Replicated: {GetYesNo(IsReplicated)}";
        var replicationFilter = $"Replication filter: {GetYesNo(HasReplicationFilter)}";
        var cdc = $"CDC: {GetYesNo(Cdc)}";

        return $"{replicated}, {replicationFilter}, {cdc}";

        static string GetYesNo(bool value)
        {
            return value ? "Yes" : "No";
        }
    }
}