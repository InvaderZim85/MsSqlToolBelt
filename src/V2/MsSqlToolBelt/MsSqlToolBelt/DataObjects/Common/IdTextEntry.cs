using System;
using System.Collections.Generic;
using System.Linq;

namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a id / text entry
/// </summary>
internal class IdTextEntry
{
    /// <summary>
    /// Gets or sets the id of the entry
    /// </summary>
    public  int Id { get; set; }

    /// <summary>
    /// Gets or sets the text of the item
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Returns the <see cref="Text"/> of the entry
    /// </summary>
    /// <returns>The text</returns>
    public override string ToString()
    {
        return Text;
    }

    /// <summary>
    /// Creates a list of an enum
    /// </summary>
    /// <param name="value">The enum value</param>
    /// <returns>The list with the entries</returns>
    public static List<IdTextEntry> CreateList(Type value)
    {
        return (from object? entry in Enum.GetValues(value)
            select new IdTextEntry
            {
                Id = (int) entry, 
                Text = entry.ToString() ?? string.Empty
            }).ToList();
    }
}