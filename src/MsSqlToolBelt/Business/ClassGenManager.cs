using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.TableType;
using MsSqlToolBelt.Templates;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
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
    /// Occurs when the export makes progress
    /// </summary>
    public event EventHandler<string>? Progress;

    /// <summary>
    /// The name of the class gen type conversion file
    /// </summary>
    private const string ClassGenTypeConversionFileName = "ClassGenTypeConversion.json";

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
    private List<ClassGenTypeEntry> _conversionTypes = [];

    /// <summary>
    /// Gets the list with the tables
    /// </summary>
    public List<TableDto> Tables { get; private set; } = [];

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

        Mediator.AddAction(nameof(ReloadTemplates), ReloadTemplates);
        Mediator.AddAction(nameof(ResetConversionTypes), ResetConversionTypes);
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
        Tables = _settingsManager.FilterList.Count > 0
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
                     let cleanName = CleanColumnName(column.Name)
                     where !column.Name.EqualsIgnoreCase(cleanName)
                     select column)
            {
                column.Alias = CleanColumnName(column.Name);
            }
        }
    }

    /// <summary>
    /// Cleans the name and removes "illegal" chars like a underscore in the name
    /// </summary>
    /// <param name="name">The original name</param>
    /// <returns>The cleaned name</returns>
    private static string CleanColumnName(string name)
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

    /// <summary>
    /// Cleans the name of the namespace and removes spaces
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The cleaned namespace name</returns>
    private static string CleanNamespace(string name)
    {
        const char dot = '.';
        if (!name.Contains(dot)) 
            return name.FirstChatToUpper().Replace(" ", "");

        var content = name.Split(dot, StringSplitOptions.RemoveEmptyEntries).ToList();
        name = string.Join(dot, content.Select(s => s.FirstChatToUpper()));

        return name.FirstChatToUpper().Replace(" ", "");
    }

    /// <summary>
    /// Generates a "valid" class name
    /// </summary>
    /// <param name="tableName">The original name of the table</param>
    /// <returns>The generated class name</returns>
    private static string GenerateClassName(string tableName)
    {
        IEnumerable<ReplaceEntry> replaceList;

        // Check if the class name contains a underscore
        if (tableName.Contains('_'))
        {
            replaceList = GetReplaceList(false);

            // Split entry at underscore
            var content = tableName.Split('_', StringSplitOptions.RemoveEmptyEntries);

            // Create a new "class" name
            tableName = content.Aggregate(string.Empty, (current, entry) => current + entry.FirstChatToUpper());
        }
        else
        {
            replaceList = GetReplaceList();
            tableName = tableName.FirstChatToUpper();
        }

        // Remove all "invalid" chars
        foreach (var entry in replaceList.Where(w => tableName.Contains(w.OldValue)))
        {
            tableName = tableName.Replace(entry.OldValue, entry.NewValue);
        }

        return tableName;
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
    /// <param name="createSqlQuery"><see langword="true"/> to generate a sql query, otherwise <see langword="false"/></param>
    /// <param name="infoText">An info which will be added before the class (optional)</param>
    /// <returns>The content of the class</returns>
    private ClassGenResult GenerateCode(ClassGenOptions options, TableDto table, bool createSqlQuery = true, string infoText = "")
    {
        // Step 1: Load the conversion types
        LoadTypeConversion();

        var result = new ClassGenResult
        {
            // Class code
            ClassCode = GenerateClass(options, table, infoText)
        };

        // Check if the table is a table type, if so, return the result
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
        if (createSqlQuery)
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

        return GenerateCode(options, tmpTable, infoText: sb.ToString());
    }

    /// <summary>
    /// Generates the classes for the given tables
    /// </summary>
    /// <param name="options">The desired options</param>
    /// <param name="tables">The list with the tables</param>
    /// <param name="ct">The token to cancel the process</param>
    /// <returns>The awaitable task</returns>
    public async Task GenerateClassesAsync(ClassGenOptions options, List<TableDto> tables, CancellationToken ct)
    {
        // Generates the file name for the class
        string GenerateFilePath(string tableName, int index)
        {
            while (true)
            {
                var path = Path.Combine(options.OutputDirectory, index == 0 ? $"{tableName}.cs" : $"{tableName}_{index}.cs");

                // Check if the file already exists
                if (!File.Exists(path)) 
                    return path;

                index += 1;
            }
        }

        // Check if the directory should be cleared
        if (options.EmptyDirectoryBeforeExport)
        {
            Directory.Delete(options.OutputDirectory, true); // Delete the dir
            Directory.CreateDirectory(options.OutputDirectory); // Create it new
        }

        var count = 1;
        var total = tables.Count(c => c.Use);
        foreach (var table in tables.Where(w => w.Use))
        {
            options.ClassName = string.IsNullOrEmpty(table.Alias) ? GenerateClassName(table.Name) : table.Alias;
            var path = GenerateFilePath(options.ClassName, 0);
            Progress?.Invoke(this, $"{count++} of {total} > Generate class for table '{table.Name}'. File name: '{Path.GetFileName(path)}'");

            if (ct.IsCancellationRequested)
            {
                Progress?.Invoke(this, "Cancellation requested by user.");
                return;
            }

            if (table.Table is not TableEntry tmpTable)
                continue;

            // Step 1: Load the columns of the table (only if they are empty)
            if (tmpTable.Columns.Count == 0)
            {
                await _tableManager.EnrichTableAsync(tmpTable);
                table.SetColumns();
            }

            // Step 2: Create the class
            var result = GenerateCode(options, table, false);

            // Step 3: Export the class
            await File.WriteAllTextAsync(path, result.ClassCode, Encoding.UTF8, ct);
        }

        Progress?.Invoke(this, "Done.");
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
        var classTemplate = _templateManager.GetTemplateContent(options.WithNamespace
            ? ClassGenTemplateType.ClassDefaultWithNs
            : ClassGenTemplateType.ClassDefault);

        // Generate the properties
        var properties = new List<string>();

        foreach (var column in table.Columns.Where(w => w.Use).OrderBy(o => o.Order))
        {
            properties.Add(GenerateColumn(options, column));
            properties.Add(""); // Add an empty line for the break
        }

        // Remove the last empty line (not very nice, but hey, if it works, it works :D
        if (properties.Count > 0)
            properties.RemoveAt(properties.Count - 1);

        // Set the values
        classTemplate = classTemplate.Replace("$NAMESPACE$", CleanNamespace(options.Namespace));
        classTemplate = classTemplate.Replace("$PROPERTIES$", string.Join(Environment.NewLine, properties));
        classTemplate = classTemplate.Replace("$MODIFIER$", options.Modifier);
        classTemplate = classTemplate.Replace("$SEALED$", options.SealedClass ? " sealed" : string.Empty);
        classTemplate = classTemplate.Replace("$NAME$", options.ClassName);
        
        // Set the "inherits" value
        var inherits = options.AddSetField ? ": ObservableObject" : string.Empty;
        classTemplate = classTemplate.Replace("$INHERITS$", inherits);

        // A normal class should be created, so we don't need the table attribute
        if (!options.DbModel || table.Name.Equals(options.ClassName))
            return string.IsNullOrEmpty(infoText)
                ? classTemplate
                : $"{infoText}{classTemplate}";

        // Create the table attribute
        var spacer = options.WithNamespace ? new string(' ', 4) : string.Empty;
        var classHeader = string.IsNullOrEmpty(table.Schema)
            ? $"{spacer}[Table(\"{table.Name}\")]"
            : $"{spacer}[Table(\"{table.Name}\", Schema = \"{table.Schema}\")]";

        // Split the template into it's lines to add the class header
        var content = classTemplate.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList();

        // Get the index of the class name
        var classIndex = content.FindIndex(f => f.ContainsIgnoreCase("class"));
        if (classIndex != -1) // Nothing found
        {
            // Add the table attribute before the class name
            content.Insert(classIndex, classHeader);
        }

        classTemplate = string.Join(Environment.NewLine, content);

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
        var spacer = options.WithNamespace ? $"{Tab}{Tab}" : Tab;
        // DataType
        var dataType = GetCSharpType(column.DataType);

        // Template
        var template = GetClassTemplate(options, dataType);
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

        if (!options.DbModel)
            return string.Join(Environment.NewLine, content.Select(s => $"{spacer}{s}"));
        
        // Add the EF attribute for the column
        var columnAttributes = new List<string>();
        if ((!string.IsNullOrEmpty(column.Alias) && !column.Name.EqualsIgnoreCase(column.Alias)) || options.AddColumnAttribute)
        {
            columnAttributes.Add($"[Column(\"{column.Name}\")]");
        }

        if (column.DataType.EqualsIgnoreCase("date"))
        {
            columnAttributes.Add("[DataType(DataType.Date)]");
        }

        if ((column.DataType.EqualsIgnoreCase("varchar") || column.DataType.EqualsIgnoreCase("nvarchar")) && column.MaxLength != -1) // -1 indicates a NVARCHAR(MAX)
        {
            columnAttributes.Add($"[MaxLength({column.MaxLength})]");
        }

        if (columnAttributes.Count == 0)
            return string.Join(Environment.NewLine, content.Select(s => $"{spacer}{s}"));

        var columnAttribute = string.Join(Environment.NewLine, columnAttributes);

        index = content.IndexOf(content.FirstOrDefault(f => f.ContainsIgnoreCase("public")) ?? "");
        if (index != -1)
            content.Insert(index, columnAttribute);

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
        if (keyColumns.Count == 0)
            return (string.Empty, string.Empty);

        var shortCode = $"modelBuilder.Entity<{options.ClassName}>().HasKey(k => new {{ {string.Join(", ", keyColumns.Select(s => $"k.{s.PropertyName}"))} }});";
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
            Columns = [.. columns.OrderBy(o => o.Order)],
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
    /// <param name="dataType">Gets the data type</param>
    /// <returns>The template</returns>
    private string GetClassTemplate(ClassGenOptions options, ClassGenTypeEntry dataType)
    {
        var template = options.AddSummary switch
        {
            true when options is {WithBackingField: true, AddSetField: false} =>
                _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyBackingFieldComment),
            true when options is {WithBackingField: true, AddSetField: true} =>
                _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyBackingFieldCommentSetField),
            true when !options.WithBackingField =>
                _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyDefaultComment),
            false when options is {WithBackingField: true, AddSetField: false} =>
                _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyBackingFieldDefault),
            false when options is {WithBackingField: true, AddSetField: true} =>
                _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyBackingFieldDefaultSetField),
            false when !options.WithBackingField =>
                _templateManager.GetTemplateContent(ClassGenTemplateType.PropertyDefault),
            _ => string.Empty
        };

        if (!options.Nullable) 
            return template;

        if (!dataType.CSharpType.EqualsIgnoreCase("string"))
            return template;

        var lines = GetTemplateLines(template);
        if (lines.Count == 0) // If there is no line, something is wrong...
            return template;

        if (template.Contains("$NAME2$"))
        {
            var index = GetLineIndex(lines, "$NAME2$");
            if (index == -1) // If this happened, something is wrong...
                return template;

            lines[index] = lines[index].Replace(";", " = string.Empty;");
        }
        else if (template.Contains("$NAME$"))
        {
            var index = GetLineIndex(lines, "$NAME$");
            if (index == -1) // If this happened, something is wrong...
                return template;

            lines[index] += " = string.Empty;";
        }

        template = string.Join(Environment.NewLine, lines);

        return template;
    }

    /// <summary>
    /// Splits the template into each line
    /// </summary>
    /// <param name="template">The template</param>
    /// <returns>The lines of the template</returns>
    private static List<string> GetTemplateLines(string template)
    {
        return [.. template.Split(new[] { Environment.NewLine }, StringSplitOptions.None)];
    }

    /// <summary>
    /// Gets the index of the line, which contains the desired value
    /// </summary>
    /// <param name="lines">The list with the lines</param>
    /// <param name="value">The value to search for</param>
    /// <returns>The index of the line, if nothing was found, -1 will be returned</returns>
    private static int GetLineIndex(IReadOnlyList<string> lines, string value)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains(value))
                return i;
        }

        return -1;
    }
    #endregion

    #region Various
    /// <summary>
    /// Gets the list with the "replace" values
    /// </summary>
    /// <param name="includeUnderscore"><see langword="true"/> to include the underscore in the list, otherwise <see langword="false"/> (optional)</param>
    /// <returns>The list with the "replace" values</returns>
    private static List<ReplaceEntry> GetReplaceList(bool includeUnderscore = true)
    {
        var tmpList = new List<ReplaceEntry>
        {
            new("_", ""),
            new(" ", ""), // this should never happen...
            new("ä", "ae"),
            new("ö", "oe"),
            new("ü", "ue"),
            new("ß", "ss"),
            new("Ä", "Ae"),
            new("Ö", "Oe"),
            new("Ü", "Ue")
        };

        if (includeUnderscore)
            tmpList.Add(new ReplaceEntry("_", ""));

        return tmpList;
    }

    /// <summary>
    /// Gets the list with the modifier
    /// </summary>
    /// <returns>The list with the modifier</returns>
    public static ObservableCollection<string> GetModifierList()
    {
        return
        [
            "public",
            "internal",
            "protected",
            "protected internal"
        ];
    }

    /// <summary>
    /// Loads the conversion types
    /// </summary>
    private void LoadTypeConversion()
    {
        if (_conversionTypes.Count > 0)
            return;

        _conversionTypes = LoadDataTypes();
    }

    /// <summary>
    /// Reload the types
    /// </summary>
    private void ResetConversionTypes()
    {
        // Clear the list, so it has to be reloaded the next time
        _conversionTypes.Clear();
    }

    /// <summary>
    /// Loads the conversion types
    /// </summary>
    /// <returns>The list with the conversion types</returns>
    public static List<ClassGenTypeEntry> LoadDataTypes()
    {
        var path = Path.Combine(Core.GetBaseDirPath(), ClassGenTypeConversionFileName);

        if (!File.Exists(path))
            return [];

        var content = File.ReadAllText(path);

        var data = JsonConvert.DeserializeObject<List<ClassGenTypeEntry>>(content) ?? [];

        foreach (var entry in data)
        {
            entry.Id = Helper.GenerateHashCode(entry.CSharpType, entry.CSharpSystemType, entry.SqlType);
        }

        return data;
    }

    /// <summary>
    /// Saves the data types
    /// </summary>
    /// <param name="types">The list with the data types</param>
    public static void SaveDataTypes(IEnumerable<ClassGenTypeEntry> types)
    {
        var path = Path.Combine(Core.GetBaseDirPath(), ClassGenTypeConversionFileName);

        var content = JsonConvert.SerializeObject(types, Formatting.Indented);

        File.WriteAllText(path, content, Encoding.UTF8);

        Mediator.ExecuteAction(nameof(ResetConversionTypes));
    }

    /// <summary>
    /// Checks if the desired class name is valid
    /// </summary>
    /// <param name="className">The name of the class</param>
    /// <returns><see langword="true"/> when the name is valid, otherwise <see langword="false"/></returns>
    public static bool ClassNameValid(string className)
    {
        return !string.IsNullOrWhiteSpace(className) && !ClassNameStartsWithNumber();

        // Check if the class name starts with a number
        bool ClassNameStartsWithNumber()
        {
            if (string.IsNullOrWhiteSpace(className))
                return false;

            var firstChar = className[0].ToString();
            return int.TryParse(firstChar, out _);
        }
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