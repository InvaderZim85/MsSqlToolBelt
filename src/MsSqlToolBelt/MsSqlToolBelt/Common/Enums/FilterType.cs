using System.ComponentModel;

namespace MsSqlToolBelt.Common.Enums;

/// <summary>
/// Provides the different 
/// </summary>
internal enum FilterType
{
    /// <summary>
    /// Entry has to contains the given value
    /// </summary>
    [Description("Contains")]
    Contains,

    /// <summary>
    /// Entry has to be equal with the given value
    /// </summary>
    [Description("Equals")]
    Equals,

    /// <summary>
    /// Entry has to starts with the given value
    /// </summary>
    [Description("Starts with")]
    StartsWith,

    /// <summary>
    /// Entry has to ends with the given value
    /// </summary>
    [Description("Ends with")]
    EndsWith
}