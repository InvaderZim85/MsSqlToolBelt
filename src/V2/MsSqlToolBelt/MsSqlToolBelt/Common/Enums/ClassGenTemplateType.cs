namespace MsSqlToolBelt.Common.Enums;

/// <summary>
/// Provides the different template types
/// </summary>
internal enum ClassGenTemplateType
{
    /// <summary>
    /// Default class
    /// </summary>
    ClassDefault,

    /// <summary>
    /// Default property with backing field and comments
    /// </summary>
    PropertyBackingFieldComment,

    /// <summary>
    /// Default property with backing field
    /// </summary>
    PropertyBackingFieldDefault,
    
    /// <summary>
    /// Default property without comments
    /// </summary>
    PropertyDefault,

    /// <summary>
    /// Default property with comments
    /// </summary>
    PropertyDefaultComment
}