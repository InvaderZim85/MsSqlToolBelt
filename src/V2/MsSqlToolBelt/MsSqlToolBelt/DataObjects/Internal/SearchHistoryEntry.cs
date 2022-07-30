using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MsSqlToolBelt.DataObjects.Internal;

/// <summary>
/// Represents a search history
/// </summary>
public class SearchHistoryEntry
{
    /// <summary>
    /// Gets or sets the id of the entry
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the search entry
    /// </summary>
    public string SearchEntry { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date / time of the search entry
    /// </summary>
    public DateTime DateTime { get; set; } = new();

    /// <summary>
    /// Creates a new empty instance of the <see cref="SearchHistoryEntry"/>
    /// </summary>
    public SearchHistoryEntry() { }

    /// <summary>
    /// Creates a new instance of the <see cref="SearchHistoryEntry"/> with the specified <paramref name="searchEntry"/> and the current date / time
    /// </summary>
    /// <param name="searchEntry">The search entry</param>
    public SearchHistoryEntry(string searchEntry)
    {
        SearchEntry = searchEntry;
        DateTime = DateTime.Now;
    }
}