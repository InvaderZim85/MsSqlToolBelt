namespace MsSqlToolBelt.DataObjects.ClassGen;

/// <summary>
/// Provides the class generator options
/// </summary>
public class ClassGenOptions
{
    /// <summary>
    /// Gets or sets the modifier
    /// </summary>
    public string Modifier { get; set; } = "public";

    /// <summary>
    /// Gets or sets the name of the class (only for the generation of a single class)
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
    /// Gets or sets the value which indicates if the column attribute should be added
    /// <para />
    /// Only for EF properties. This property will be ignores when <see cref="DbModel"/> is <see langword="false"/>
    /// </summary>
    public bool AddColumnAttribute { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if a backing field should be created
    /// </summary>
    public bool WithBackingField { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if the "SetField" method should be used
    /// <para />
    /// If this value is enabled, the <see cref="WithBackingField"/> will be overwritten and set to true
    /// </summary>
    public bool AddSetField { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if a summary should be added
    /// </summary>
    public bool AddSummary { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if a class for a nullable project should be created (.NET 6)
    /// </summary>
    public bool Nullable { get; set; }

    /// <summary>
    /// Gets or sets the sql query (only needed for the function which generates a class from a sql query)
    /// </summary>
    public string SqlQuery { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path of the output directory (only needed for the generation of multiple classes)
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicates if the export directory should be cleared before the export starts (only needed for the generation of multiple classes)
    /// </summary>
    public bool EmptyDirectoryBeforeExport { get; set; }

    /// <summary>
    /// Gets or sets the name of the namespace
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets the value which indicates if the template with namespace should be used
    /// </summary>
    public bool WithNamespace => !string.IsNullOrWhiteSpace(Namespace);
}