namespace MsSqlToolBelt.DataObjects.Table;

/// <summary>
/// Represents the definition of a table
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="TableDefinition"/>
/// </remarks>
/// <param name="name">The name of the table</param>
/// <param name="definition">The definition</param>
internal class TableDefinition(string name, string definition)
{
    /// <summary>
    /// Gets the name of the table
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the definition of the table
    /// </summary>
    public string Definition { get; } = definition;
}