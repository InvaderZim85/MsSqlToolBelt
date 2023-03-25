namespace MsSqlToolBelt.DataObjects.Table;

/// <summary>
/// Represents the definition of a table
/// </summary>
internal class TableDefinition
{
    /// <summary>
    /// Gets the name of the table
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the definition of the table
    /// </summary>
    public string Definition { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="TableDefinition"/>
    /// </summary>
    /// <param name="name">The name of the table</param>
    /// <param name="definition">The definition</param>
    public TableDefinition(string name, string definition)
    {
        Name = name;
        Definition = definition;
    }
}