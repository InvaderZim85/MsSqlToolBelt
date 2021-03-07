namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Provides the table and the column name of a key column
    /// </summary>
    internal sealed class KeyColumn
    {
        /// <summary>
        /// Gets or sets the name of the table
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Gets or sets the name of the key column
        /// </summary>
        public string Column { get; set; }
    }
}
