using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MsSqlToolBelt.DataObjects.Internal;

/// <summary>
/// Represents a server entry
/// </summary>
[Table("Server")]
internal class ServerEntry
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
    /// Creates a new empty entry
    /// </summary>
    public ServerEntry(){}

    /// <summary>
    /// Creates a new server wit the specified name
    /// </summary>
    /// <param name="name">The name / path of the server</param>
    /// <param name="defaultDatabase">The name of the default database</param>
    public ServerEntry(string name, string defaultDatabase)
    {
        Name = name;
        DefaultDatabase = defaultDatabase;
    }
}