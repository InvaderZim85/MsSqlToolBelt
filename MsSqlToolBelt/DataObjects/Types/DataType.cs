namespace MsSqlToolBelt.DataObjects.Types
{
    /// <summary>
    /// Represents the supported data types
    /// </summary>
    internal sealed class DataType
    {
        /// <summary>
        /// Gets or sets the sql type
        /// </summary>
        public string SqlType { get; set; }

        /// <summary>
        /// Gets or sets the C# type
        /// </summary>
        public string CSharpType { get; set; }
    }
}
