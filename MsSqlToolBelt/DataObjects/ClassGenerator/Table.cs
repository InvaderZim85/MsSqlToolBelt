using System.Collections.Generic;

namespace MsSqlToolBelt.DataObjects.ClassGenerator
{
    /// <summary>
    /// Represents a table
    /// </summary>
    internal sealed class Table
    {
        /// <summary>
        /// Gets or sets the name of the table
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the according database schema
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets or sets the list with the columns
        /// </summary>
        public List<TableColumn> Columns { get; set; }
    }
}
