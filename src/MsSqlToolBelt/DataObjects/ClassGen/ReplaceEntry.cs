namespace MsSqlToolBelt.DataObjects.ClassGen;

/// <summary>
/// Represents a replace value
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ReplaceEntry"/>
/// </remarks>
/// <param name="oldValue">The old value which should be replaced with the <paramref name="newValue"/></param>
/// <param name="newValue">The new value which replaces the <paramref name="oldValue"/></param>
internal class ReplaceEntry(string oldValue, string newValue)
{
    /// <summary>
    /// Gets the old value which should be replaced with the <see cref="NewValue"/>
    /// </summary>
    public string OldValue { get; } = oldValue;

    /// <summary>
    /// Gets the new value which replaces the <see cref="OldValue"/>
    /// </summary>
    public string NewValue { get; } = newValue;
}