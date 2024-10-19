using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using System.IO;
using System.Text;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Templates;

/// <summary>
/// Provides the functions for the interaction with the templates
/// </summary>
internal static class TemplateManager
{
    /// <summary>
    /// Contains the templates
    /// </summary>
    private static readonly List<TemplateEntry> TemplateList = [];

    /// <summary>
    /// Gets the list with the templates
    /// </summary>
    public static List<TemplateEntry> Templates
    {
        get
        {
            if (TemplateList.Count == 0)
                LoadTemplates();

            return TemplateList;
        }
    }

    /// <summary>
    /// Loads the desired template
    /// </summary>
    /// <param name="type">The type of the template</param>
    /// <returns>The content of the template</returns>
    public static string GetTemplateContent(ClassGenTemplateType type)
    {
        return Templates.FirstOrDefault(f => f.Type == type)?.Content ??
               string.Empty;
    }

    /// <summary>
    /// Loads all available templates and stores them into <see cref="TemplateList"/>
    /// </summary>
    /// <param name="reload">true to reload all templates, otherwise false</param>
    public static void LoadTemplates(bool reload = true)
    {
        if (TemplateList.Count > 0 && !reload)
            return;

        // Remove all loaded templates
        TemplateList.Clear();

        foreach (var type in Enum.GetValues<ClassGenTemplateType>())
        {
            // Load and store the template
            TemplateList.Add(LoadTemplate(type));
        }
    }

    /// <summary>
    /// Loads the desired template
    /// </summary>
    /// <param name="type">The template type</param>
    /// <returns>The template</returns>
    /// <exception cref="DirectoryNotFoundException">Will be thrown when the template directory doesn't exist</exception>
    /// <exception cref="FileNotFoundException">Will be thrown when the specified template file doesn't exist</exception>
    private static TemplateEntry LoadTemplate(ClassGenTemplateType type)
    {
        var templateName = type.ToString();
        var dir = new DirectoryInfo(Path.Combine(Core.GetBaseDirPath(), "Templates"));
        if (!dir.Exists)
            throw new DirectoryNotFoundException("The template directory is missing");

        var templates = dir.GetFiles("*.cgt");

        var file = templates.FirstOrDefault(f => f.Name.Contains(templateName, StringComparison.OrdinalIgnoreCase));
        if (file is not {Exists: true})
            throw new FileNotFoundException($"The template file for '{type}' is missing.");

        var content = File.ReadAllText(file.FullName, Encoding.UTF8);

        return new TemplateEntry(type, file, content);
    }

    /// <summary>
    /// Updates an existing template
    /// </summary>
    /// <param name="template">The template with the new content</param>
    public static void UpdateTemplate(TemplateEntry template)
    {
        // Save the file
        File.WriteAllText(template.FilePath, template.Content, Encoding.UTF8);

        // Execute the reload
        Mediator.ExecuteAction(MediatorKey.ReloadTemplates);
    }

    /// <summary>
    /// Creates a backup of the desired template
    /// </summary>
    /// <param name="template">The template</param>
    /// <param name="backupPath">The path of the backup</param>
    public static void CreateBackup(TemplateEntry template, string backupPath)
    {
        File.WriteAllText(backupPath, template.Content, Encoding.UTF8);
    }

    /// <summary>
    /// Loads the content of a backup and stores it into the template
    /// </summary>
    /// <param name="template">The template</param>
    /// <param name="backupPath">The path of the backup</param>
    public static void LoadBackup(TemplateEntry template, string backupPath)
    {
        if (!File.Exists(backupPath))
            return;

        var content = File.ReadAllText(backupPath);

        template.Content = content;
    }
}