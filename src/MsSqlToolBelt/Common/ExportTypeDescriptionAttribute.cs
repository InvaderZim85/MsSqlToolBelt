using System;

namespace MsSqlToolBelt.Common;

/// <summary>
/// Represents an export type description
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal class ExportTypeDescriptionAttribute : Attribute
{
    /// <summary>
    /// Gets the description
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the value which indicates if the type is for a list of objects
    /// </summary>
    public bool List { get; }

    /// <summary>
    /// Gets the value which indicates if the type is for a single object
    /// </summary>
    public bool Single { get; }

    /// <summary>
    /// Gets the extension of the export type
    /// </summary>
    public string Extension { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="ExportTypeDescriptionAttribute"/>
    /// </summary>
    /// <param name="description">The description</param>
    /// <param name="list">The value which indicates if the type is for a list of objects</param>
    /// <param name="single">The value which indicates if the type is for a single object</param>
    /// <param name="extension">The extension of the export type</param>
    public ExportTypeDescriptionAttribute(string description, bool list, bool single, string extension)
    {
        Description = description;
        List = list;
        Single = single;
        Extension = extension;
    }
}