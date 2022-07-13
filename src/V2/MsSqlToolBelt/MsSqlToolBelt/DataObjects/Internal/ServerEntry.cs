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
    /// Creates a new empty instance of the <see cref="ServerEntry"/> 
    /// </summary>
    public ServerEntry(){}

    /// <summary>
    /// Creates a new instance of the <see cref="ServerEntry"/> with the name / path of the server
    /// </summary>
    /// <param name="name">The name / path of the server</param>
    public ServerEntry(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ServerEntry"/> with the name / path of the server and the default database
    /// </summary>
    /// <param name="name">The name / path of the server</param>
    /// <param name="defaultDatabase">The name of the default database</param>
    public ServerEntry(string name, string defaultDatabase)
    {
        Name = name;
        DefaultDatabase = defaultDatabase;
    }
}