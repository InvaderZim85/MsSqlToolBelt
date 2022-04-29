using System.Collections.Generic;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.TableType
{
    /// <summary>
    /// Represents a table type
    /// </summary>
    public sealed class TableType
    {
        /// <summary>
        /// Gets or sets the table object id
        /// </summary>
        [Appearance(Ignore = true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the table type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list with the columns
        /// </summary>
        [Appearance(Ignore = true)]
        public List<TableTypeColumn> Columns { get; set; }

        /// <summary>
        /// Gets the column count
        /// </summary>
        [Appearance(Name = "Columns")]
        public int ColumnCount => Columns?.Count ?? 0;
    }
}
