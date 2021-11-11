namespace MsSqlToolBelt.DataObjects.ClassGenerator
{
    /// <summary>
    /// Represents a schema column
    /// </summary>
    internal sealed class SchemaColumn
    {
        /// <summary>
        /// Gets or sets the name of the column
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the type of the column
        /// </summary>
        public string DateType { get; set; }

        /// <summary>
        /// Gets or sets the column ordinal aka position of the column
        /// </summary>
        public int ColumnOrdinal { get; set; }
    }
}
