using System.IO;

namespace MsSqlToolBelt.Common;

/// <summary>
/// Provides several default entries
/// </summary>
internal static class DefaultEntries
{
    /// <summary>
    /// The default color scheme
    /// </summary>
    public const string ColorScheme = "Emerald";

    /// <summary>
    /// The default value for the auto wildcard
    /// </summary>
    public const bool AutoWildcard = true;

    /// <summary>
    /// The default "copy to clipboard" format (CSV)
    /// </summary>
    public const int CopyToClipboardFormat = 1;

    /// <summary>
    /// The default amount of search history entries
    /// </summary>
    public const int SearchHistoryCount = 50;

    /// <summary>
    /// Gets the filepath of the custom color file
    /// </summary>
    public static string CustomColorFile => Path.Combine(AppContext.BaseDirectory, "CustomColors.json");
}