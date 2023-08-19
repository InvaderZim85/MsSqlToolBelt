using System.Collections.Generic;

namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a server info entry
/// </summary>
public class ServerInfoEntry
{
    /// <summary>
    /// Gets the order
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Gets the group name
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// Gets the key
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the value
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets or sets the list with the child values
    /// </summary>
    public List<ServerInfoEntry> ChildValues { get; set; } = new();

    /// <summary>
    /// Creates a new instance of the <see cref="ServerInfoEntry"/>
    /// </summary>
    /// <param name="order">The order</param>
    /// <param name="group">The name of the group</param>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    public ServerInfoEntry(int order, string group, string key, string value)
    {
        Order = order;
        Group = group;
        Key = key;
        Value = value;
    }
}