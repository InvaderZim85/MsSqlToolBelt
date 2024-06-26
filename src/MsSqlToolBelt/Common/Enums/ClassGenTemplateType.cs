﻿namespace MsSqlToolBelt.Common.Enums;

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
    /// Default class with a summary
    /// </summary>
    ClassDefaultComment,
    
    /// <summary>
    /// Default class with namespace
    /// </summary>
    ClassDefaultWithNs,

    /// <summary>
    /// Default class with namespace and summary
    /// </summary>
    ClassDefaultWithNsComment,

    /// <summary>
    /// Default property with backing field and comments
    /// </summary>
    PropertyBackingFieldComment,

    /// <summary>
    /// Default property with backing field which uses the SetField method and comments
    /// </summary>
    PropertyBackingFieldCommentSetField,

    /// <summary>
    /// Default property with backing field
    /// </summary>
    PropertyBackingFieldDefault,

    /// <summary>
    /// Default property with backing field which uses the SetField method
    /// </summary>
    PropertyBackingFieldDefaultSetField,

    /// <summary>
    /// Default property without comments
    /// </summary>
    PropertyDefault,

    /// <summary>
    /// Default property with comments
    /// </summary>
    PropertyDefaultComment,

    /// <summary>
    /// Class for the EF model creator
    /// </summary>
    EfCreatingBuilder
}