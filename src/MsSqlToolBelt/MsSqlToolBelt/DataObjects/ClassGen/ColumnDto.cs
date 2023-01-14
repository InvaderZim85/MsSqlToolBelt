using CommunityToolkit.Mvvm.ComponentModel;
using MsSqlToolBelt.DataObjects.Common;

namespace MsSqlToolBelt.DataObjects.ClassGen;

/// <summary>
/// Represents a column (only needed for the class generator). For the normal column see <see cref="ColumnEntry"/>
/// </summary>
public class ColumnDto : ObservableObject
{
    /// <summary>
    /// Backing field for <see cref="Use"/>
    /// </summary>
    private bool _use = true;

    /// <summary>
    /// Gets or sets the value which indicates if the column should be used
    /// </summary>
    public bool Use
    {
        get => _use;
        set => SetProperty(ref _use, value);
    }

    /// <summary>
    /// Gets or sets the name of the column
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Backing field for <see cref="Alias"/>
    /// </summary>
    private string _alias = string.Empty;

    /// <summary>
    /// Gets or sets the alias of the column
    /// </summary>
    public string Alias
    {
        get => _alias;
        set
        {
            SetProperty(ref _alias, value);
            Use = !string.IsNullOrEmpty(value);
        }
    }

    /// <summary>
    /// Gets the name for the property.
    /// <para />
    /// If the <see cref="Alias"/> is not empty, it will be used, otherwise the <see cref="Name"/> will be used
    /// </summary>
    public string PropertyName => string.IsNullOrEmpty(Alias) ? Name : Alias;

    /// <summary>
    /// Gets or sets the data type of the column
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicates if the column is a primary key
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if the column is nullable
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets the order of the column
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Converts the <see cref="ColumnEntry"/> into a <see cref="ColumnDto"/>
    /// </summary>
    /// <param name="column">The column</param>
    public static explicit operator ColumnDto(ColumnEntry column)
    {
        return new ColumnDto
        {
            Name = column.Name,
            DataType = column.DataType,
            IsPrimaryKey = column.IsPrimaryKey,
            IsNullable = column.IsNullable,
            Order = column.Order
        };
    }
}