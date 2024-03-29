﻿using MsSqlToolBelt.Common;

namespace MsSqlToolBelt.DataObjects.Search;

/// <summary>
/// Represents an table index
/// </summary>
public class IndexEntry
{
    /// <summary>
    /// Gets or sets the name of the index
    /// </summary>
    [ClipboardProperty]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of the columns
    /// </summary>
    public string Columns { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the table fragmentation
    /// </summary>
    public string Fragmentation { get; set; } = "No yet loaded.";
}