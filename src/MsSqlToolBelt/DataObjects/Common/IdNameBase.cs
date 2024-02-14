using MsSqlToolBelt.Common;

namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a simple id / name entry
/// </summary>
public class IdNameBase
{
    /// <summary>
    /// Gets or sets the id of the table (this is the "object_id" in the database)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the table
    /// </summary>
    [ClipboardProperty]
    public string Name { get; set; } = string.Empty;
}