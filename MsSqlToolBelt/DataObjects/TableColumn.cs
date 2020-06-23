namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Represents a table column
    /// </summary>
    internal sealed class TableColumn
    {
        /// <summary>
        /// Gets or sets the name of the table
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Gets or sets the name of the column
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// Gets or sets the data type of the column
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if the column should used for the generation
        /// </summary>
        public bool Use { get; set; } = true;

        /// <summary>
        /// Gets or sets the alias of the column
        /// </summary>
        public string Alias { get; set; }
    }
}
