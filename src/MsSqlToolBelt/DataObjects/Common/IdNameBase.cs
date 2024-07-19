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

    /// <summary>
    /// Creates a new, empty instance of the <see cref="IdNameBase"/>
    /// </summary>
    public IdNameBase() { }

    /// <summary>
    /// Creates a new instance of the <see cref="IdNameBase"/>
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="name">The name</param>
    public IdNameBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }
}