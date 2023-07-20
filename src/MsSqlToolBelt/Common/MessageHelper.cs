using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;

namespace MsSqlToolBelt.Common;

/// <summary>
/// Contains the different error messages
/// </summary>
internal static class MessageHelper
{
    #region Search
    /// <summary>
    /// Gets the message which indicates that there was an error while accessing the msdb database during the search process.
    /// <para />
    /// The message ends with the option "Should the message continue to be displayed?"
    /// </summary>
    public static MessageEntry SearchMsdbAccessViolation => new("Error",
        "An error occurred while the jobs were being scanned. This may be because you do not have the necessary permissions to access the MSDB database where the job information is stored.",
        "",
        "If this problem persists, contact the database administrator get access to the MSDB.",
        "",
        "Should the message continue to be displayed (settings will be reset after restarting the application)?");

    #endregion

    #region Class generator
    /// <summary>
    /// Gets the message that the class name is not valid
    /// </summary>
    public static MessageEntry ClassGenValidName => new("Class generator",
        "Please enter a valid class name.",
        "",
        "Hint: Must not start with a number and must not be empty.");
    #endregion

    #region Validation
    /// <summary>
    /// Gets the message that the query is valid
    /// </summary>
    public static MessageEntry ValidationValidNote => new("Validation",
        "Inserted SQL query / statement is valid.",
        "",
        "Note: Even if the SQL query / statement has been validated successfully, it may not be possible to execute the query, for example, a column or table does not exist or is misspelled.");
    #endregion

    #region Error messages
    /// <summary>
    /// Gets the error message according to the specified type
    /// </summary>
    /// <param name="type">The error type</param>
    /// <param name="additionalInfo">Additional information. Only supported when <paramref name="type"/> is <see cref="ErrorMessageType.Complex"/></param>
    /// <returns>The error message</returns>
    public static MessageEntry GetErrorMessage(ErrorMessageType type, string additionalInfo = "")
    {
        var message = type switch
        {
            ErrorMessageType.Load => "An error occurred while loading the data.",
            ErrorMessageType.Save => "An error occurred while saving / exporting the data.",
            ErrorMessageType.Generate => "An error occurred while generating the data.",
            ErrorMessageType.Import => "An error occurred while importing the data.",
            ErrorMessageType.Connection => "An error occurred while establishing the connection.",
            ErrorMessageType.Complex => string.IsNullOrWhiteSpace(additionalInfo)
                ? "An error has occurred."
                : $"An error has occurred: {additionalInfo}",
            _ => "An error has occurred."
        };

        return new MessageEntry("Error", message, "", "For more information please check the log.");
    }
    #endregion
}