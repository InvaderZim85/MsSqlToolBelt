using MsSqlToolBelt.Common;

namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a package entry
/// </summary>
internal class PackageInfo
{
    /// <summary>
    /// Gets or sets the name of the package
    /// </summary>
    [ClipboardProperty]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the package
    /// </summary>
    public string Version { get; set; } = string.Empty;
}