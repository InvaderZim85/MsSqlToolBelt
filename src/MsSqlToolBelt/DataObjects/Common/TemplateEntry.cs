using MsSqlToolBelt.Common.Enums;
using System.IO;

namespace MsSqlToolBelt.DataObjects.Common;

/// <summary>
/// Represents a template file
/// </summary>
internal class TemplateEntry
{
    /// <summary>
    /// Gets the type of the template
    /// </summary>
    public ClassGenTemplateType Type { get; }

    /// <summary>
    /// Gets the name of the template file
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the path of the template file
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets or sets the content of the template file
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="TemplateEntry"/>
    /// </summary>
    /// <param name="type">The type of the template</param>
    /// <param name="file">The template</param>
    /// <param name="content">The content of the template</param>
    public TemplateEntry(ClassGenTemplateType type, FileSystemInfo file, string content)
    {
        Type = type;
        FileName = file.Name;
        FilePath = file.FullName;
        Content = content;
    }

    /// <summary>
    /// Returns the name of the file
    /// </summary>
    /// <returns>The file name</returns>
    public override string ToString()
    {
        return FileName;
    }
}