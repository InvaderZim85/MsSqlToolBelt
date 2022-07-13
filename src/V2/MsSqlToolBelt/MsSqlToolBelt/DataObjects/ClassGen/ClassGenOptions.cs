namespace MsSqlToolBelt.DataObjects.ClassGen;

/// <summary>
/// Provides the class generator options
/// </summary>
internal class ClassGenOptions
{
    /// <summary>
    /// Gets or sets the modifier
    /// </summary>
    public string Modifier { get; set; } = "public";

    /// <summary>
    /// Gets or sets the name of the class
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicates if a sealed class should be generated
    /// </summary>
    public bool SealedClass { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if a entity framework db model should be created
    /// </summary>
    public bool DbModel { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if a backing field should be created
    /// </summary>
    public bool WithBackingField { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if a summary should be added
    /// </summary>
    public bool AddSummary { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if a class for a nullable project should be created (.NET 6)
    /// </summary>
    public bool Nullable { get; set; }
}