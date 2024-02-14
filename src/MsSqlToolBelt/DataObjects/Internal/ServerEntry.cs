using MsSqlToolBelt.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MsSqlToolBelt.DataObjects.Internal;

/// <summary>
/// Represents a server entry
/// </summary>
[Table("Server")]
public class ServerEntry
{
    /// <summary>
    /// Gets or sets the id of the server
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name / path of the sever
    /// </summary>
    [Required]
    [ClipboardProperty]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the default database
    /// </summary>
    public string DefaultDatabase { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order of the entry
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether to connect to the default database automatically (only used when a default database is set)
    /// </summary>
    public bool AutoConnect { get; set; }

    /// <summary>
    /// Gets or sets the description of the server (optional)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new empty instance of the <see cref="ServerEntry"/>
    /// </summary>
    public ServerEntry() { }

    /// <summary>
    /// Creates a new instance of the <see cref="ServerEntry"/>
    /// </summary>
    /// <param name="entry">The "old" entry</param>
    public ServerEntry(ServerEntry entry)
    {
        Name = entry.Name;
        DefaultDatabase = entry.DefaultDatabase;
        Order = entry.Order;
        AutoConnect = entry.AutoConnect;
        Description = entry.Description;
    }

    /// <summary>
    /// Gets the name of the server
    /// </summary>
    /// <returns>The name / path of the server</returns>
    public override string ToString()
    {
        return string.IsNullOrEmpty(Description) ? Name : $"{Name} ({Description})";
    }
}