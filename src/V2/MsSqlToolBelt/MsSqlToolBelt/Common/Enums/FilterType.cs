namespace MsSqlToolBelt.Common.Enums;

/// <summary>
/// Provides the different 
/// </summary>
internal enum FilterType
{
    /// <summary>
    /// Entry has to contains the given value
    /// </summary>
    Contains,

    /// <summary>
    /// Entry has to be equal with the given value
    /// </summary>
    Equals,

    /// <summary>
    /// Entry has to starts with the given value
    /// </summary>
    StartsWith,

    /// <summary>
    /// Entry has to ends with the given value
    /// </summary>
    EndsWith
}