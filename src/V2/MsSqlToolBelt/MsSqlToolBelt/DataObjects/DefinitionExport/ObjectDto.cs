using MsSqlToolBelt.DataObjects.Common;
using ZimLabs.TableCreator;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.DataObjects.DefinitionExport;

/// <summary>
/// Represents an <see cref="ObjectEntry"/> for the definition export grid
/// </summary>
internal class ObjectDto : ObservableObject
{
    /// <summary>
    /// Backing field for <see cref="Export"/>
    /// </summary>
    private bool _export;

    /// <summary>
    /// Gets or sets the value which indicates if the object should be exported
    /// </summary>
    [Appearance(Ignore = true)]
    public bool Export
    {
        get => _export;
        set => SetField(ref _export, value);
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
    /// Gets the original object
    /// </summary>
    [Appearance(Ignore = true)]
    public ObjectEntry OriginalObject { get; init; } = new();

    /// <summary>
    /// Converts a <see cref="ObjectEntry"/> into an <see cref="ObjectDto"/>
    /// </summary>
    /// <param name="obj">The object which should be converted</param>
    public static explicit operator ObjectDto(ObjectEntry obj)
    {
        return new ObjectDto
        {
            Name = obj.Name,
            Type = obj.TypeName,
            OriginalObject = obj
        };
    }
}