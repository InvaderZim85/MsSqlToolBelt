namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a single message entry
/// </summary>
internal class MessageEntry
{
    /// <summary>
    /// Gets the header / caption (can be used in a message dialog)
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// Gets the message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="MessageEntry"/>
    /// </summary>
    /// <param name="header">The header / caption</param>
    /// <param name="message">The message</param>
    public MessageEntry(string header, string message)
    {
        Header = header;
        Message = message;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MessageEntry"/>
    /// </summary>
    /// <param name="header">The header / caption</param>
    /// <param name="messageParts">The message parts</param>
    public MessageEntry(string header, params string[] messageParts)
    {
        Header = header;
        Message = string.Join(Environment.NewLine, messageParts);
    }
}