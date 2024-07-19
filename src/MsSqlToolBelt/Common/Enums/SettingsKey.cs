namespace MsSqlToolBelt.Common.Enums;

/// <summary>
/// Provides the different settings keys
/// </summary>
public enum SettingsKey
{
    /// <summary>
    /// The color theme of the application
    /// </summary>
    ColorScheme = 0,

    /// <summary>
    /// The auto wildcard value
    /// </summary>
    AutoWildcard = 1,

    /// <summary>
    /// The format of the clipboard
    /// </summary>
    CopyToClipboardFormat = 2,

    /// <summary>
    /// The count of history entries
    /// </summary>
    SearchHistoryEntryCount = 3,

    /// <summary>
    /// The up time of the application
    /// </summary>
    UpTime = 4,

    /// <summary>
    /// The search options
    /// </summary>
    SearchOptions = 5,

    /// <summary>
    /// Specifies whether only the name should be copied if only one row is selected in the grid.
    /// </summary>
    CopyGridSingleLineOnlyValue = 6,

    /// <summary>
    /// The tab settings
    /// <para />
    /// <b>Note</b>: The values are stored as comma separated string!
    /// </summary>
    TabSettings = 7,

    /// <summary>
    /// The class generator options
    /// </summary>
    ClassGenOptions = 8,

    /// <summary>
    /// Specifies whether the class generator options should be saved or not
    /// </summary>
    SaveClassGenOptions = 9,

    /// <summary>
    /// The name of the default modifier
    /// </summary>
    ClassGenDefaultModifier = 10
}