using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MsSqlToolBelt.DataObjects.Internal;

/// <summary>
/// Represents a settings entry
/// </summary>
internal class SettingsEntry
{
    /// <summary>
    /// Gets or sets the id
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the id of the key (see <see cref="MsSqlToolBelt.Common.Enums.SettingsKey"/>)
    /// </summary>
    [Required]
    public int KeyId { get; set; }

    /// <summary>
    /// Gets or sets the value of the key
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;
}