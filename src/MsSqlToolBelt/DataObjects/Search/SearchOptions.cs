using MsSqlToolBelt.Common;

namespace MsSqlToolBelt.DataObjects.Search;

/// <summary>
/// Provides the different search options
/// </summary>
public sealed class SearchOptions
{
    /// <summary>
    /// Gets or sets the value which indicates if tables should be searched
    /// </summary>
    public bool Tables { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if table types should be searched
    /// </summary>
    public bool TableTypes { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if objects should be searched
    /// </summary>
    public bool Objects { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if jobs should be searched
    /// </summary>
    public bool Jobs { get; set; }

    /// <summary>
    /// Gets the value which indicates if the options are valid. At least one property has to be <see langword="true"/>
    /// </summary>
    public bool OptionsValid => Tables || TableTypes || Objects || Jobs;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Tables.BoolToString()};{TableTypes.BoolToString()};{Objects.BoolToString()};{Jobs.BoolToString()}";
    }
}