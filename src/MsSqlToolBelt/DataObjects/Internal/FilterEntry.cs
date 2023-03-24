using MsSqlToolBelt.Common.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.DataObjects.Internal;

/// <summary>
/// Represents a filter entry
/// </summary>
public class FilterEntry
{
    /// <summary>
    /// Gets or sets the id of the entry
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the id of the filter type (see <see cref="MsSqlToolBelt.Common.Enums.FilterType"/>)
    /// </summary>
    [Required]
    public int FilterTypeId { get; set; }

    /// <summary>
    /// Gets the name of the filter type
    /// </summary>
    [NotMapped]
    public string FilterType
    {
        get
        {
            var type = (FilterType)FilterTypeId;
            return type.GetAttribute<DescriptionAttribute>()?.Description ?? type.ToString();
        }
    }

    /// <summary>
    /// Gets or sets the search value
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets the filter info
    /// </summary>
    /// <returns>The info</returns>
    public string GetInfo()
    {
        return $"{FilterType} '{Value}'";
    }

    /// <summary>
    /// Creates a new empty instance of the <see cref="FilterEntry"/>
    /// </summary>
    public FilterEntry() { }

    /// <summary>
    /// Creates a new instance of the <see cref="FilterEntry"/>
    /// </summary>
    /// <param name="entry">The "old" entry</param>
    public FilterEntry(FilterEntry entry)
    {
        FilterTypeId = entry.FilterTypeId;
        Value = entry.Value;
    }
}