using CommunityToolkit.Mvvm.ComponentModel;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.TableType;
using Newtonsoft.Json;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.DefinitionExport;

/// <summary>
/// Represents an <see cref="ObjectEntry"/> for the definition export grid
/// </summary>
internal class DefinitionExportObject : ObservableObject
{
    /// <summary>
    /// Backing field for <see cref="Export"/>
    /// </summary>
    private bool _export;

    /// <summary>
    /// Gets or sets the value which indicates if the object should be exported
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public bool Export
    {
        get => _export;
        set => SetProperty(ref _export, value);
    }

    /// <summary>
    /// Gets the name of the object
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the type of the object
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the entry type
    /// </summary>
    public EntryType EntryType { get; init; }

    /// <summary>
    /// Gets the original object
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public ObjectEntry OriginalObject { get; init; } = new();

    /// <summary>
    /// Gets or sets the original table
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public TableEntry OriginalTable { get; init; } = new();

    /// <summary>
    /// Gets or sets the original table type
    /// </summary>
    [Appearance(Ignore = true)]
    [JsonIgnore]
    public TableTypeEntry OriginalTableType { get; init; } = new();

    /// <summary>
    /// Converts a <see cref="ObjectEntry"/> into an <see cref="DefinitionExportObject"/>
    /// </summary>
    /// <param name="obj">The object which should be converted</param>
    public static explicit operator DefinitionExportObject(ObjectEntry obj)
    {
        return new DefinitionExportObject
        {
            Name = obj.Name,
            Type = obj.TypeName,
            EntryType = EntryType.Object,
            OriginalObject = obj
        };
    }

    /// <summary>
    /// Converts a <see cref="TableEntry"/> into an <see cref="DefinitionExportObject"/>
    /// </summary>
    /// <param name="entry">The table which should be converted</param>

    public static explicit operator DefinitionExportObject(TableEntry entry)
    {
        return new DefinitionExportObject
        {
            Name = entry.Name,
            EntryType = EntryType.Table,
            OriginalTable = entry
        };
    }

    /// <summary>
    /// Converts a <see cref="TableTypeEntry"/> into an <see cref="DefinitionExportObject"/>
    /// </summary>
    /// <param name="entry">The table type which should be converted</param>

    public static explicit operator DefinitionExportObject(TableTypeEntry entry)
    {
        return new DefinitionExportObject
        {
            Name = entry.Name,
            EntryType = EntryType.TableType,
            OriginalTableType = entry
        };
    }
}