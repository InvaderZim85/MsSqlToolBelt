using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.ClassGen;
using Newtonsoft.Json;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the functions for the generation of a class
/// </summary>
internal class ClassGenManager : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private readonly TableManager _tableManager;

    /// <summary>
    /// The list with the conversion types
    /// </summary>
    private List<ClassGenTypeEntry> _conversionTypes = new();

    /// <summary>
    /// Contains the list with the templates
    /// </summary>
    private readonly SortedList<ClassGenTemplateType, string> _templates = new();

    /// <summary>
    /// Gets or sets the list with the tables
    /// </summary>
    public List<TableDto> Tables { get; set; } = new();

    /// <summary>
    /// Gets or sets the selected table
    /// </summary>
    public TableDto? SelectedTable { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="ClassGenManager"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    public ClassGenManager(string dataSource, string database)
    {
        _tableManager = new TableManager(dataSource, database);
    }

    #region Loading
    /// <summary>
    /// Loads all available tables and stores them into <see cref="Tables"/>
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task LoadTablesAsync()
    {
        var tables = await _tableManager.LoadTablesAsync();
        Tables = tables.Select(s => (TableDto) s).ToList();
    }

    /// <summary>
    /// Enriches the selected table
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task EnrichTableAsync()
    {
        if (SelectedTable == null)
            return;

        await _tableManager.EnrichTableAsync(SelectedTable.Table);

        SelectedTable.SetColumns();

        // Clean the names of the column
        foreach (var column in from column in SelectedTable.Columns
                 let cleanName = CleanName(column.Name)
                 where !column.Name.EqualsIgnoreCase(cleanName)
                 select column)
        {
            column.Alias = CleanName(column.Name);
        }
    }

    /// <summary>
    /// Cleans the name and removes "illegal" chars like a underscore in the name
    /// </summary>
    /// <param name="name">The original name</param>
    /// <returns>The cleaned name</returns>
    private static string CleanName(string name)
    {
        var replaceValues = GetReplaceList();

        foreach (var entry in replaceValues.Where(w => name.Contains(w.OldValue)))
        {
            name = name.Replace(entry.OldValue, entry.NewValue);
        }

        // If this happens something is pretty broken
        if (name.Length == 0)
            return "Column";

        // Check if the name starts with a digit, if so, add "Column" to prevent errors
        // because a property / variable hast to start with a "char" and not a number...
        if (name[0].IsNumeric())
            name = $"Column{name}";

        return name;
    }
    #endregion

    #region Class generator
    /// <summary>
    /// Generates a class based on the options and columns
    /// </summary>
    /// <param name="options">The desired options</param>
    /// <returns>The content of the class</returns>
    public string GenerateClassAsync(ClassGenOptions options)
    {
        if (SelectedTable == null)
            return string.Empty;

        // Step 1: Load the conversion types
        LoadTypeConversion();

        // Step 2: Load the templates
        LoadTemplates();

        // Get the class template
        var classTemplate = _templates[ClassGenTemplateType.ClassDefault];

        // Generate the properties
        //var properties = SelectedTable.Columns.Where(w => w.Use).Select(column => GenerateColumn(options, column)).ToList();
        var properties = new List<string>();

        foreach (var column in SelectedTable.Columns.Where(w => w.Use))
        {
            properties.Add(GenerateColumn(options, column));
            properties.Add(""); // Add an empty line for the break
        }

        // Remove the last empty line (not very nice, but hey, if it works, it works :D
        properties.RemoveAt(properties.Count - 1);

        classTemplate = classTemplate.Replace("$PROPERTIES$", string.Join(Environment.NewLine, properties));
        classTemplate = classTemplate.Replace("$MODIFIER$", options.Modifier);
        classTemplate = classTemplate.Replace("$SEALED$", options.SealedClass ? " sealed" : "");
        classTemplate = classTemplate.Replace("$NAME$", options.ClassName);

        return !options.DbModel
            ? classTemplate
            : $"[Table(\"{SelectedTable.Name}\", Schema = \"{SelectedTable.Table.Schema}\")]{Environment.NewLine}{classTemplate}";
    }

    /// <summary>
    /// Generates the content for a single column
    /// </summary>
    /// <param name="options">The options</param>
    /// <param name="column">The column</param>
    /// <returns>The code for the column</returns>
    private string GenerateColumn(ClassGenOptions options, ColumnDto column)
    {
        var spacer = "".PadLeft(4, ' ');
        var template = GetTemplate(options);

        // DataType
        var dataType = GetCSharpType(column.DataType);
        template = template.Replace("$TYPE$", dataType.CSharpType);
        if (column.IsNullable && (!dataType.IsNullable || options.Nullable))
            template = template.Replace("$NULLABLE$", "?");
        else
            template = template.Replace("$NULLABLE$", "");

        // Name
        template = template.Replace("$NAME$", string.IsNullOrEmpty(column.Alias) ? column.Name : column.Alias);

        // Backing field name
        template = template.Replace("$NAME2$",
            CreateBackingFieldName(string.IsNullOrEmpty(column.Alias) ? column.Name : column.Alias));

        var content = template.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList();

        if (!options.DbModel || string.IsNullOrEmpty(column.Alias) || column.Name.Equals(column.Alias, StringComparison.OrdinalIgnoreCase))
            return string.Join(Environment.NewLine, content.Select(s => $"{spacer}{s}"));

        // Add the EF attribute for the column
        var attribute =
            $"[Column(\"{column.Name}\"{(column.DataType.EqualsIgnoreCase("date") ? ", DataType(DataType.Date)" : "")})]";

        var index = content.IndexOf(content.FirstOrDefault(f => f.ContainsIgnoreCase("public")) ?? "");
        if (index != -1)
            content.Insert(index, attribute);

        return string.Join(Environment.NewLine, content.Select(s => $"{spacer}{s}"));

    }

    /// <summary>
    /// Creates the name of the backing field
    /// </summary>
    /// <param name="name">The name of the backing field</param>
    /// <returns>The backing field name</returns>
    private static string CreateBackingFieldName(string name)
    {
        return $"_{name.FirstCharToLower()}";
    }

    /// <summary>
    /// Gets the CSharp type according to the specified sql type
    /// </summary>
    /// <param name="sqlType">The sql type</param>
    /// <returns>The CSharp type</returns>
    private ClassGenTypeEntry GetCSharpType(string sqlType)
    {
        return _conversionTypes.FirstOrDefault(f => f.SqlType.EqualsIgnoreCase(sqlType)) ?? new ClassGenTypeEntry();
    }
    #endregion

    #region Template
    /// <summary>
    /// Loads the desired template
    /// </summary>
    /// <param name="options">The options</param>
    /// <returns>The template</returns>
    private string GetTemplate(ClassGenOptions options)
    {
        return options.AddSummary switch
        {
            true when options.WithBackingField => _templates[ClassGenTemplateType.PropertyBackingFieldComment],
            true when !options.WithBackingField => _templates[ClassGenTemplateType.PropertyDefaultComment],
            false when options.WithBackingField => _templates[ClassGenTemplateType.PropertyBackingFieldDefault],
            false when !options.WithBackingField => _templates[ClassGenTemplateType.PropertyDefault],
            _ => string.Empty
        };
    }

    /// <summary>
    /// Loads all templates and stores them into <see cref="_templates"/>
    /// </summary>
    private void LoadTemplates()
    {
        if (_templates.Any())
            return;

        foreach (var template in Enum.GetValues<ClassGenTemplateType>())
        {
            _templates.Add(template, LoadTemplate(template));
        }
    }

    /// <summary>
    /// Loads the template for the desired type
    /// </summary>
    /// <param name="type">The template type</param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException">Will be thrown when the template directory is missing</exception>
    /// <exception cref="FileNotFoundException">Will be thrown when the template file is missing</exception>
    private static string LoadTemplate(ClassGenTemplateType type)
    {
        var dir = new DirectoryInfo(Path.Combine(Core.GetBaseDirPath(), "Templates"));
        if (!dir.Exists)
            throw new DirectoryNotFoundException("The template directory is missing.");

        var templates = dir.GetFiles("*.cgt");

        var file = templates.FirstOrDefault(f => f.Name.ContainsIgnoreCase(type.ToString()));
        if (file is not {Exists: true})
            throw new FileNotFoundException($"The template file for '{type}' is missing.");

        return File.ReadAllText(file.FullName);
    }
    #endregion

    #region Various
    /// <summary>
    /// Gets the list with the replace values
    /// </summary>
    /// <returns>The list with the replace values</returns>
    private static IEnumerable<ReplaceEntry> GetReplaceList()
    {
        return new List<ReplaceEntry>
        {
            new("_", ""),
            new("-", ""),
            new(" ", ""), // this should never happen...
            new("ä", "ae"),
            new("ö", "oe"),
            new("ü", "ue"),
            new("ß", "ss"),
            new("Ä", "Ae"),
            new("Ö", "Oe"),
            new("Ü", "Ue")
        };
    }

    /// <summary>
    /// Loads the conversion types
    /// </summary>
    /// <returns>The list with the conversion types</returns>
    private void LoadTypeConversion()
    {
        if (_conversionTypes.Any())
            return;

        var path = Path.Combine(Core.GetBaseDirPath(), "ClassGenTypeConversion.json");

        if (!File.Exists(path))
            _conversionTypes = new List<ClassGenTypeEntry>();

        var content = File.ReadAllText(path);

        _conversionTypes = JsonConvert.DeserializeObject<List<ClassGenTypeEntry>>(content) ?? new List<ClassGenTypeEntry>();
    }
    #endregion

    /// <summary>
    /// Releases all resources used by the <see cref="ClassGenManager"/>
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _tableManager.Dispose();

        _disposed = true;
    }
}