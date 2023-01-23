using System.Collections.Generic;

namespace MsSqlToolBelt.DataObjects.Internal;

/// <summary>
/// Represents the settings
/// </summary>
internal class SettingsDto
{
    /// <summary>
    /// Gets or sets the list with the settings
    /// </summary>
    public List<SettingsEntry> Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets the list with the settings
    /// </summary>
    public List<ServerEntry> Servers { get; set; } = new();

    /// <summary>
    /// Gets or sets the list with the settings
    /// </summary>
    public List<FilterEntry> Filters { get; set; } = new();
}