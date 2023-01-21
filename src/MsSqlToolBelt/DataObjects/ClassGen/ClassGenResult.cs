namespace MsSqlToolBelt.DataObjects.ClassGen;

/// <summary>
/// Represents the result of the class generator
/// </summary>
public class ClassGenResult
{
    /// <summary>
    /// Gets or sets the class code
    /// </summary>
    public string ClassCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity framework key code which is needed when the specified class / table has more than one key column
    /// </summary>
    public string EfCoreKeyCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity framework key code which is needed when the specified class / table has more than one key column
    /// <para />
    /// This version doesn't contain the function body
    /// </summary>
    public string EfCoreKeyCodeShort { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sql query
    /// </summary>
    public string SqlQuery { get; set; } = string.Empty;
}