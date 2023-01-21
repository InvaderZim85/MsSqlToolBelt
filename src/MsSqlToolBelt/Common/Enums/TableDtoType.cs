namespace MsSqlToolBelt.Common.Enums;

/// <summary>
/// The type of the <see cref="DataObjects.ClassGen.TableDto"/>
/// </summary>
public enum TableDtoType
{
    /// <summary>
    /// A table (<see cref="DataObjects.Common.TableEntry"/>)
    /// </summary>
    Table = 1,

    /// <summary>
    /// A table type (<see cref="DataObjects.TableType.TableTypeEntry"/>)
    /// </summary>
    TableType = 2
}