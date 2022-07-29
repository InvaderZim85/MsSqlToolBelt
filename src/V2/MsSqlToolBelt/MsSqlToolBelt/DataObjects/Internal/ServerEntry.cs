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
    /// Gets the name of the server
    /// </summary>
    /// <returns>The name / path of the server</returns>
    public override string ToString()
    {
        return Name;
    }
}