using MsSqlToolBelt.Common.Enums;
using System.IO;

namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a template file
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="TemplateEntry"/>
/// </remarks>
/// <param name="type">The type of the template</param>
/// <param name="file">The template</param>
/// <param name="content">The content of the template</param>
internal class TemplateEntry(ClassGenTemplateType type, FileSystemInfo file, string content)
{
    /// <summary>
    /// Gets the type of the template
    /// </summary>
    public ClassGenTemplateType Type { get; } = type;

    /// <summary>
    /// Gets the name of the template file
    /// </summary>
    public string FileName { get; } = file.Name;

    /// <summary>
    /// Gets the path of the template file
    /// </summary>
    public string FilePath { get; } = file.FullName;

    /// <summary>
    /// Gets or sets the content of the template file
    /// </summary>
    public string Content { get; set; } = content;

    /// <summary>
    /// Returns the name of the file
    /// </summary>
    /// <returns>The file name</returns>
    public override string ToString()
    {
        return FileName;
    }
}