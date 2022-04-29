using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MsSqlToolBelt.DataObjects.Internal;

/// <summary>
/// Represents a filter entry
/// </summary>
internal class FilterEntry
{
    /// <summary>
    /// Gets or sets the id of the entry
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the id of the filter type (see <see cref="Common.Enums.FilterType"/>)
    /// </summary>
    [Required]
    public int FilterTypeId { get; set; }

    /// <summary>
    /// Gets or sets the search value
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;
}