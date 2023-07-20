namespace MsSqlToolBelt.Common.Enums;

/// <summary>
/// Provides the different error message types
/// </summary>
internal enum ErrorMessageType
{
    /// <summary>
    /// Default error
    /// </summary>
    Default,

    /// <summary>
    /// Error while loading
    /// </summary>
    Load,

    /// <summary>
    /// Error while saving / exporting
    /// </summary>
    Save,

    /// <summary>
    /// Error while generating
    /// </summary>
    Generate,

    /// <summary>
    /// Error while import
    /// </summary>
    Import,

    /// <summary>
    /// Error while connecting
    /// </summary>
    Connection,

    /// <summary>
    /// Show the exception message with an additional information
    /// </summary>
    Complex

}