namespace MsSqlToolBelt.DataObjects.TableType
{
    /// <summary>
    /// Represents a column of a <see cref="TableType"/>
    /// </summary>
    public sealed class TableTypeColumn
    {
        /// <summary>
        /// Gets or sets the id of the table type
        /// </summary>
        public int TableTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the data type of the column
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the size of the data type
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if the column is null able
        /// </summary>
        public bool Nullable { get; set; }

        /// <summary>
        /// Gets or sets the id of the column (can be used for sorting!)
        /// </summary>
        public int ColumnId { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if the column is uses as a key
        /// </summary>
        public bool IsPrimaryKey { get; set; }
    }
}
