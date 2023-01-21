namespace MsSqlToolBelt.DataObjects.ClassGen;

/// <summary>
/// Represents a replace value
/// </summary>
internal class ReplaceEntry
{
    /// <summary>
    /// Gets the old value which should be replaced with the <see cref="NewValue"/>
    /// </summary>
    public string OldValue { get; }

    /// <summary>
    /// Gets the new value which replaces the <see cref="OldValue"/>
    /// </summary>
    public string NewValue { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="ReplaceEntry"/>
    /// </summary>
    /// <param name="oldValue">The old value which should be replaced with the <paramref name="newValue"/></param>
    /// <param name="newValue">The new value which replaces the <paramref name="oldValue"/></param>
    public ReplaceEntry(string oldValue, string newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}