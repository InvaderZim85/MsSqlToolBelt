using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.TableType;
using MsSqlToolBelt.Templates;
using Newtonsoft.Json;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the functions for the generation of a class
/// </summary>
public sealed class ClassGenManager : IDisposable
{
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Contains the tab indent
    /// </summary>
    private static readonly string Tab = new(' ', 4);

    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private readonly TableManager _tableManager;

    /// <summary>
    /// The instance for the interaction with the table types
    /// </summary>
    private readonly TableTypeManager _tableTypeManager;

    /// <summary>
    /// The instance for the interaction with the database
    /// </summary>
    private readonly ClassGenRepo _repo;

    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private readonly SettingsManager _settingsManager;

    /// <summary>
    /// The instance for the interaction with the templates
    /// </summary>
    private readonly TemplateManager _templateManager = new();

    /// <summary>
    /// The list with the conversion types
    /// </summary>
    private List<ClassGenTypeEntry> _conversionTypes = new();

    /// <summary>
    /// Gets the list with the tables
    /// </summary>
    public List<TableDto> Tables { get; private set; } = new();

    /// <summary>
    /// Gets or sets the selected table
    /// </summary>
    public TableDto? SelectedTable { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="ClassGenManager"/>
    /// </summary>
    /// <param name="settingsManager">The instance of the settings manager</param>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    public ClassGenManager(SettingsManager settingsManager, string dataSource, string database)
    {
        _tableManager = new TableManager(dataSource, database);
        _tableTypeManager = new TableTypeManager(dataSource, database);
        _repo = new ClassGenRepo(dataSource, database);
        _settingsManager = settingsManager;

        Mediator.AddAction("ReloadTemplates", ReloadTemplates);
    }

    #region Loading
    /// <summary>
    /// Loads all available tables and stores them into <see cref="Tables"/>
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task LoadTablesAsync()
    {
        // Load the table and the table types
        var tables = await _tableManager.LoadTablesAsync();
        await _tableTypeManager.LoadTableTypesAsync();
        
        // Combine the lists
        var tmpTables = tables.Select(s => (TableDto) s).ToList();
        tmpTables.AddRange(_tableTypeManager.TableTypes.Select(s => (TableDto)s));

        // Load the filter and check the entry
        await _settingsManager.LoadFilterAsync();
        Tables = _settingsManager.FilterList.Any()
            ? tmpTables.Where(w => w.Name.IsValid(_settingsManager.FilterList)).ToList()
            : tmpTables;
    }

    /// <summary>
    /// Enriches the selected table
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task EnrichTableAsync()
    {
        if (SelectedTable == null)
            return;

        switch (SelectedTable.Table)
        {
            case TableEntry table:
                await _tableManager.EnrichTableAsync(table);
                break;
            case TableTypeEntry tableType:
                await _tableTypeManager.EnrichTableTypeAsync(tableType);
                break;
        }

        SelectedTable.SetColumns();

        // Ignore the alias if we work with a table type but set the use flag
        if (SelectedTable.Type == TableDtoType.TableType)
        {
            foreach (var column in SelectedTable.Columns)
            {
                column.Use = true;
            }
        }
        else
        {
            // Clean the names of the column
            foreach (var column in from column in SelectedTable.Columns
                     let cleanName = CleanName(column.Name)
                     where !column.Name.EqualsIgnoreCase(cleanName)
                     select column)
            {
                column.Alias = CleanName(column.Name);
            }
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
    public ClassGenResult GenerateCode(ClassGenOptions options)
    {
        return SelectedTable == null ? new ClassGenResult() : GenerateCode(options, SelectedTable);
    }

    /// <summary>
    /// Generates a class based on the options and columns
    /// </summary>
    /// <param name="options">The desired options</param>
    /// <param name="table">The desired table</param>
    /// <param name="infoText">An info which will be added before the class (optional)</param>
    /// <returns>The content of the class</returns>
    private ClassGenResult GenerateCode(ClassGenOptions options, TableDto table, string infoText = "")
    {
        // Step 1: Load the conversion types
        LoadTypeConversion();

        var result = new ClassGenResult
        {
            // Class code
            ClassCode = GenerateClass(options, table, infoText)
        };

        // Check if the table is a table type, if so, 
        if (table.Type == TableDtoType.TableType)
            return result;

        // Ef key code
        if (options.DbModel)
        {
            var (completeCode, shortCode) = GenerateEfKeyCode(options, table);
            result.EfCoreKeyCode = completeCode;
            result.EfCoreKeyCodeShort = shortCode;
        }

        // SQL Query
        result.SqlQuery = !string.IsNullOrEmpty(options.SqlQuery) ? options.SqlQuery : GenerateSqlQuery();

        return result;
    }

    /// <summary>
    /// Generates a class from a sql query
    /// </summary>
    /// <param name="options">The options</param>
    /// <returns>The content of the class</returns>
    public async Task<ClassGenResult> GenerateFromQueryAsync(ClassGenOptions options)
    {
        // Step 1: Load the conversion types
        LoadTypeConversion();

        var table = await LoadMetadataAsync(options);
        var tmpTable = (TableDto) table;

        foreach (var column in tmpTable.Columns)
        {
            // Why this? The class generator works with the SQL Type! So that we can use
            // the existing logic, we convert the system type to the sql type
            var sqlType = GetSqlTypeBySystemType(column.DataType);
            if (!string.IsNullOrEmpty(sqlType))
                column.DataType = sqlType;
            column.Use = true;
        }

        var sb = new StringBuilder();

        if (table.ColumnUniqueError)
        {
            sb.AppendLine("/*")
                .AppendLine(" NOTE - Unique property names")
                .AppendLine(" ----------------------------")
                .AppendLine(" In your SQL query are several columns with the same name.")
                .AppendLine(" Since this is not possible in C#, please adjust the column names")
                .AppendLine(" and run the process again.")
                .AppendLine("*/")
                .AppendLine();
        }

        return GenerateCode(options, tmpTable, sb.ToString());
    }

    #region CSharp class
    /// <summary>
    /// Loads the templates
    /// </summary>
    private void ReloadTemplates()
    {
        _templateManager.LoadTemplates();
    }

    /// <summary>
    /// Generates the class code
    /// </summary>
    /// <param name="options">The desired options</param>
    /// <param name="table">The desired table</param>
    /// <param name="infoText">An info which will be added before the class</param>
    /// <returns>The code of the class</returns>
    private string GenerateClass(ClassGenOptions options, TableDto table, string infoText)
    {
        // Load the templates
        _templateManager.LoadTemplates(false);

        // Get the class template
        var classTemplate = _templateManager.GetTemplateContent(ClassGenTemplateType.ClassDefault);

        // Generate the properties
        //var properties = SelectedTable.Columns.Where(w => w.Use).Select(column => GenerateColumn(options, column)).ToList();
        var properties = new List<string>();

        foreach (var column in table.Columns.Where(w => w.Use).OrderBy(o => o.Order))
        {
            properties.Add(GenerateColumn(options, column));
            properties.Add(""); // Add an empty line for the break
        }

        // Remove the last empty line (not very nice, but hey, if it works, it works :D
        if (properties.Any())
            properties.RemoveAt(properties.Count - 1);

        // Set the values
        classTemplate = classTemplate.Replace("$PROPERTIES$", string.Join(Environment.NewLine, properties));
        classTemplate = classTemplate.Replace("$MODIFIER$", options.Modifier);
        classTemplate = classTemplate.Replace("$SEALED$", options.SealedClass ? " sealed" : string.Empty);
        classTemplate = classTemplate.Replace("$NAME$", options.ClassName);
        
        // Set the "inherits" value
        var inherits = options.AddSetField ? ": ObservableObject" : string.Empty;
        classTemplate = classTemplate.Replace("$INHERITS$", inherits);

        if (options.DbModel && !table.Name.Equals(options.ClassName))
            classTemplate = string.IsNullOrEmpty(table.Schema)
                ? $"[Table(\"{table.Name}\")]{Environment.NewLine}{classTemplate}"
                : $"[Table(\"{table.Name}\", Schema = \"{table.Schema}\")]{Environment.NewLine}{classTemplate}";

        return string.IsNullOrEmpty(infoText)
            ? classTemplate
            : $"{infoText}{classTemplate}";
    }

    /// <summary>
    /// Generates the content for a single column
    /// </summary>
    /// <param name="options">The options</param>
    /// <param name="column">The column</param>
    /// <returns>The code for the column</returns>
    private string GenerateColumn(ClassGenOptions options, ColumnDto column)
    {
        var template = GetClassTemplate(options);

        // DataType
        var dataType = GetCSharpType(column.DataType);
        template = template.Replace("$TYPE$",
            string.IsNullOrEmpty(dataType.CSharpType) ? column.DataType : dataType.CSharpType);
        if (column.IsNullable && (!dataType.IsNullable || options.Nullable))
            template = template.Replace("$NULLABLE$", "?");
        else
            template = template.Replace("$NULLABLE$", "");

        // Name
        template = template.Replace("$NAME$", column.PropertyName);

        // Backing field name
        template = template.Replace("$NAME2$", CreateBackingFieldName(column.PropertyName));

        var content = template.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList();

        int index;
        if (options.DbModel && column.IsPrimaryKey)
        {
            // Add the key attribute for the column
            index = content.IndexOf(content.FirstOrDefault(f => f.ContainsIgnoreCase("public")) ?? "");
            if (index != -1)
                content.Insert(index, "[Key]");
        }

        if (!options.DbModel || string.IsNullOrEmpty(column.Alias) || column.Name.Equals(column.Alias, StringComparison.OrdinalIgnoreCase))
            return string.Join(Environment.NewLine, content.Select(s => $"{Tab}{s}"));
        
        // Add the EF attribute for the column
        var columnAttribute =
            $"[Column(\"{column.Name}\"{(column.DataType.EqualsIgnoreCase("date") ? ", DataType(DataType.Date)" : "")})]";

        index = content.IndexOf(content.FirstOrDefault(f => f.ContainsIgnoreCase("public")) ?? "");
        if (index != -1)
            content.Insert(index, columnAttribute);

        return string.Join(Environment.NewLine, content.Select(s => $"{Tab}{s}"));
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
    /// <returns>The type entry</returns>
    private ClassGenTypeEntry GetCSharpType(string sqlType)
    {
        return _conversionTypes.FirstOrDefault(f => f.SqlType.EqualsIgnoreCase(sqlType)) ?? new ClassGenTypeEntry();
    }

    /// <summary>
    /// Gets the CSharp type according to the specified system type
    /// </summary>
    /// <param name="systemType">The system type like "System.Int32"</param>
    /// <returns>The type entry</returns>
    private string GetSqlTypeBySystemType(string systemType)
    {
        return _conversionTypes.FirstOrDefault(f => f.CSharpSystemType.EqualsIgnoreCase(systemType))?.SqlType ??
               systemType;
    }
    #endregion

    #region EFCore Key
    /// <summary>
    /// Generates the code for the ef key
    /// </summary>
    /// <param name="options">The options</param>
    /// <param name="table">The desired table</param>
    /// <returns>The code for the keys</returns>
    private (string completeCode, string shortCode) GenerateEfKeyCode(ClassGenOptions options, TableDto table)
    {
        var template = _templateManager.GetTemplateContent(ClassGenTemplateType.EfCreatingBuilder);

        var keyColumns = table.Columns.Where(w => w.IsPrimaryKey).ToList();
        if (!keyColumns.Any())
            return (string.Empty, string.Empty);

        var shortCode = $"modelBuilder.Entity<{options.ClassName}>().HasKey(k => {{ {string.Join(", ", keyColumns.Select(s => $"k.{s.PropertyName}"))} }});";
        var completeCode = template.Replace("$ENTRIES$", shortCode);

        return (completeCode, shortCode);

    }
    #endregion

    #region Sql

    /// <summary>
    /// Generates the SQL query
    /// </summary>
    /// <returns>The sql query</returns>
    private string GenerateSqlQuery()
    {
        var sb = new StringBuilder();
        sb.AppendLine("SELECT");

        var count = 1;
        var columnCount = SelectedTable!.Columns.Count(c => c.Use);
        foreach (var column in SelectedTable!.Columns.Where(w => w.Use).OrderBy(o => o.Order))
        {
            var comma = count++ == columnCount ? "" : ",";

            sb.AppendLine(string.IsNullOrEmpty(column.Alias)
                ? $"{Tab}[{column.Name}]{comma}"
                : $"{Tab}[{column.Name}] AS [{column.Alias}]{comma}");
        }

        sb.AppendLine("FROM").AppendLine($"{Tab}[{SelectedTable!.Schema}].[{SelectedTable.Name}]");

        return sb.ToString();
    }

    /// <summary>
    /// Loads the metadata of the query
    /// </summary>
    /// <param name="options">The options</param>
    /// <returns>The loaded metadata as table</returns>
    private async Task<TableEntry> LoadMetadataAsync(ClassGenOptions options)
    {
        var columns = await _repo.LoadMetadataAsync(options.SqlQuery);

        var uniqueError = columns.GroupBy(g => g.Name).Select(s => new
        {
            Count = s.Count()
        }).Any(a => a.Count > 1);

        return new TableEntry
        {
            Name = options.ClassName,
            Columns = columns.OrderBy(o => o.Order).ToList(),
            ColumnUniqueError = uniqueError
        };
    }

    #endregion
    #endregion

    #region Template
    /// <summary>
    /// Loads the desired template
    /// </summary>
    /// <param name="options">The options</param>
    /// <returns>The template</returns>
    private string GetClassTemplate(ClassGenOptions options)
    {
        return options.AddSummary switch
        {
            true when options.WithBackingField && !options.AddSetField => _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyBackingFieldComment),
            true when options.WithBackingField && options.AddSetField => _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyBackingFieldCommentSetField),
            true when !options.WithBackingField => _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyDefaultComment),
            false when options.WithBackingField &&! options.AddSetField => _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyBackingFieldDefault),
            false when options.WithBackingField && options.AddSetField => _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyBackingFieldDefaultSetField),
            false when !options.WithBackingField => _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyDefault),
                
            _ => string.Empty
        };
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
    private void LoadTypeConversion()
    {
        if (_conversionTypes.Any())
            return;

        _conversionTypes = LoadDataTypes();
    }

    /// <summary>
    /// Loads the conversion types
    /// </summary>
    /// <returns>The list with the conversion types</returns>
    public static List<ClassGenTypeEntry> LoadDataTypes()
    {
        var path = Path.Combine(Core.GetBaseDirPath(), "ClassGenTypeConversion.json");

        if (!File.Exists(path))
            return new List<ClassGenTypeEntry>();

        var content = File.ReadAllText(path);

        return JsonConvert.DeserializeObject<List<ClassGenTypeEntry>>(content) ?? new List<ClassGenTypeEntry>();
    }

    /// <summary>
    /// Checks if the desired class name is valid
    /// </summary>
    /// <param name="className">The name of the class</param>
    /// <returns><see langword="true"/> when the name is valid, otherwise <see langword="false"/></returns>
    public bool ClassNameValid(string className)
    {
        // Check if the class name starts with a number
        bool ClassNameStartsWithNumber()
        {
            if (string.IsNullOrWhiteSpace(className))
                return false;

            var firstChar = className[0].ToString();
            return int.TryParse(firstChar, out _);
        }

        return !string.IsNullOrWhiteSpace(className) && !ClassNameStartsWithNumber();
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

        Mediator.RemoveAction("LoadTemplates");

        _disposed = true;
    }
}