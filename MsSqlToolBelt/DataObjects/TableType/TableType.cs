﻿using System.Collections.Generic;

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
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the table type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list with the columns
        /// </summary>
        public List<TableTypeColumn> Columns { get; set; }
    }
}
