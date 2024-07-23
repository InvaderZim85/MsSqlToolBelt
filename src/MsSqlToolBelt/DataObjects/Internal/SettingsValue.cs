using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;

namespace MsSqlToolBelt.DataObjects.Internal;

/// <summary>
/// Represents a single settings value (original value and changed value with the target type)
/// </summary>
/// <param name="key">The key of the value</param>
public sealed class SettingsValue(SettingsKey key)
{
    /// <summary>
    /// Gets the key of the settings value
    /// </summary>
    public SettingsKey Key { get; } = key;

    /// <summary>
    /// Gets or sets the original value
    /// </summary>
    public string OriginalValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets the value
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <returns>The value</returns>
    public T GetValue<T>(T fallback)
    {
        return OriginalValue.ChangeType(fallback);
    }
}