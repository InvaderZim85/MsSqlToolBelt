namespace MsSqlToolBelt.DataObjects.Search
{
    /// <summary>
    /// Represents a table index
    /// </summary>
    public sealed class TableIndex
    {
        /// <summary>
        /// Gets or sets the name of the index
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the table
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Gets or sets the column of the index
        /// </summary>
        public string Column { get; set; }
    }
}
