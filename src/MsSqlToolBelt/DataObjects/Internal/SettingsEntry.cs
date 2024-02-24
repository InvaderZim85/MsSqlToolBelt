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
    [MaxLength(200)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new empty instance of the <see cref="SettingsEntry"/>
    /// </summary>
    public SettingsEntry() { }

    /// <summary>
    /// Creates a new instance of the <see cref="SettingsEntry"/>
    /// </summary>
    /// <param name="entry">The "old" entry</param>
    public SettingsEntry(SettingsEntry entry)
    {
        KeyId = entry.KeyId;
        Value = entry.Value;
    }
}