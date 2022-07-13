namespace MsSqlToolBelt.Common.Enums;

/// <summary>
/// Provides the different export types
/// </summary>
public enum ExportType
{
    /// <summary>
    /// Markdown
    /// </summary>
    [ExportTypeDescription("Markdown", true, true, "md")]
    Markdown,

    /// <summary>
    /// CSV file
    /// </summary>
    [ExportTypeDescription("CSV", true, true, "csv")]
    Csv,

    /// <summary>
    /// ASCII-Table
    /// </summary>
    [ExportTypeDescription("ASCII Table", true, true, "txt")]
    Ascii,

    /// <summary>
    /// List with hyphens
    /// </summary>
    [ExportTypeDescription("List - Hyphens", false, true, "txt")]
    ListHyphens,

    /// <summary>
    /// Numbered list
    /// </summary>
    [ExportTypeDescription("List - Numbered", false, true, "txt")]
    ListNumbered,

    /// <summary>
    /// JSON formatted
    /// </summary>
    [ExportTypeDescription("JSON", true, true, "json")]
    Json
}