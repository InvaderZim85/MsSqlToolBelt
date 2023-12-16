using Newtonsoft.Json;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a server info entry
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ServerInfoEntry"/>
/// </remarks>
/// <param name="order">The order</param>
/// <param name="group">The name of the group</param>
/// <param name="key">The key</param>
/// <param name="value">The value</param>
public class ServerInfoEntry(int order, string group, string key, string value)
{
    /// <summary>
    /// Gets the order
    /// </summary>
    public int Order { get; } = order;

    /// <summary>
    /// Gets the group name
    /// </summary>
    public string Group { get; } = group;

    /// <summary>
    /// Gets the key
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// Gets the value
    /// </summary>
    public string Value { get; } = value;

    /// <summary>
    /// Gets or sets the list with the child values
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public List<ServerInfoEntry> ChildValues { get; set; } = [];
}