using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a column
/// </summary>
public class ColumnEntry
{
    /// <summary>
    /// Gets or sets the name of the column
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order of the column
    /// <para/>
    /// The "order" is also the "column id" for a table type column
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the name of the data type
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the max length (if the value is -1 and the <see cref="DataType"/> is "nvarchar", the value is "MAX")
    /// </summary>
    [Appearance(Ignore = true)]
    public int MaxLength { get; set; }

    /// <summary>
    /// Gets the max length value (only needed for the table grid)
    /// </summary>
    [Appearance(Name = "MaxLength")]
    public string MaxLengthView => MaxLength == -1 ? "MAX" : MaxLength.ToString();

    /// <summary>
    /// Gets or sets the precision
    /// </summary>
    [Appearance(Ignore = true)]
    public int Precision { get; set; }

    /// <summary>
    /// Gets or sets the scale (this value defines the decimal places or the milliseconds)
    /// </summary>
    [Appearance(Ignore = true)]
    public int Scale { get; set; }

    /// <summary>
    /// Gets the precision value (<see cref="Precision"/>, <see cref="Scale"/> - only needed for the table grid)
    /// </summary>
    [Appearance(Name = "Precision")]
    public string PrecisionView => $"{Precision}, {Scale}";

    /// <summary>
    /// Gets or sets the value which indicates if the column can be null
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if the column is replicated
    /// </summary>
    public bool IsReplicated { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if the column is an identity (auto increment) column
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if the value of the column is automatically computed
    /// </summary>
    public bool IsComputed { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if the column is a part of the primary key
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if the column is used in an index
    /// </summary>
    public bool InIndex { get; set; }

    /// <summary>
    /// Gets or sets the default value
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;
}