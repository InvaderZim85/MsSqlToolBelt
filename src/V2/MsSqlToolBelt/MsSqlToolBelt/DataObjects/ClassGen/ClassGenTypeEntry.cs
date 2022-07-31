﻿using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.ClassGen;

/// <summary>
/// Represents a class gen type
/// </summary>
public class ClassGenTypeEntry
{
    /// <summary>
    /// Gets or sets the sql type
    /// </summary>
    public string SqlType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the csharp type
    /// </summary>
    [Appearance(Name = "C# Type")]
    public string CSharpType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the csharp system type (like <see langword="System.Int32"/> for an <see langword="int"/>)
    /// </summary>
    [Appearance(Name = "C# System Type")]
    public string CSharpSystemType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the is nullable value
    /// </summary>
    public bool IsNullable { get; set; }
}