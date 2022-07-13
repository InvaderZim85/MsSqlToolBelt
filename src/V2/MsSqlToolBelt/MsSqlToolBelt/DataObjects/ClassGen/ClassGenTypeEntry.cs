﻿namespace MsSqlToolBelt.DataObjects.ClassGen;

/// <summary>
/// Represents a class gen type
/// </summary>
internal class ClassGenTypeEntry
{
    /// <summary>
    /// Gets or sets the sql type
    /// </summary>
    public string SqlType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the csharp type
    /// </summary>
    public string CSharpType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the is nullable value
    /// </summary>
    public bool IsNullable { get; set; }
}