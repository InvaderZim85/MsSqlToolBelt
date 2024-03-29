﻿using MsSqlToolBelt.Data;
using System.Diagnostics;

namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a database object like a procedure, view
/// </summary>
[DebuggerDisplay("{Name}")]
public sealed class ObjectEntry : IdNameBase
{
    /// <summary>
    /// Gets or sets the definition of the object (aka Create-Statement)
    /// </summary>
    public string Definition { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the object
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets the type name
    /// </summary>
    public string TypeName => DataHelper.GetTypeName(Type);

    /// <summary>
    /// Gets or sets the creation date / time
    /// </summary>
    public DateTime CreationDateTime { get; set; }

    /// <summary>
    /// Gets or sets the modification date / time
    /// </summary>
    public DateTime ModifiedDateTime { get; set; }
}