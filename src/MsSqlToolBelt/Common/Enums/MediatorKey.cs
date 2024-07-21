namespace MsSqlToolBelt.Common.Enums;

/// <summary>
/// Provides the different mediator keys
/// </summary>
internal enum MediatorKey
{
    /// <summary>
    /// Resets / Reloads the conversion types for the class generator
    /// </summary>
    ResetConversionTypes,

    /// <summary>
    /// Sets the visibility of the tabs in the main window
    /// </summary>
    SetTabVisibility,

    /// <summary>
    /// Reloads the class generator templates when they were changed
    /// </summary>
    ReloadTemplates,

    /// <summary>
    /// Sets the visibility of the SQL Query window in the class generator
    /// </summary>
    SetClassGenQueryVisibility
}