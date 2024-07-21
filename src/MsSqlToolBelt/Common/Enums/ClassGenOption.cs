namespace MsSqlToolBelt.Common.Enums;

/// <summary>
/// Provides the different class gen options
/// </summary>
/// <remarks>
/// <b>NOTE</b>: The names of the individual entries <b>must</b> match the names of the properties in class <see cref="DataObjects.ClassGen.ClassGenOptions"/>, otherwise problems may occur when loading / saving the values.
/// </remarks>
public enum ClassGenOption
{
    /// <summary>
    /// Option "Sealed class"
    /// </summary>
    SealedClass,

    /// <summary>
    /// Option "DB Model (EF)"
    /// </summary>
    DbModel,

    /// <summary>
    /// Option "Column attribute (EF)"
    /// </summary>
    AddColumnAttribute,

    /// <summary>
    /// Option "Backing field"
    /// </summary>
    WithBackingField,

    /// <summary>
    /// Option "Summary"
    /// </summary>
    AddSummary,

    /// <summary>
    /// Option "Nullable enabled"
    /// </summary>
    Nullable,

    /// <summary>
    /// Option "Use 'SetProperty' method
    /// </summary>
    AddSetField,

    /// <summary>
    /// Option "Add table name in summary
    /// </summary>
    AddTableNameInSummary
}