using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsSqlToolBelt.DataObjects.TableType;

internal class TableTypeColumnEntry
{
    /// <summary>
    /// Gets or sets the name of the column
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type
    /// </summary>
    public string DataType { get; set; } = string.Empty;
}