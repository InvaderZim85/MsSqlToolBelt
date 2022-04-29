using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.ClassGenerator;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.Business
{
    /// <summary>
    /// Provides the functions to create a class of a table
    /// </summary>
    internal sealed class ClassGenerator
    {
        /// <summary>
        /// Contains the tab indent
        /// </summary>
        private static readonly string Tab = new(' ', 4);

        /// <summary>
        /// The instance for the interaction with the database
        /// </summary>
        private readonly GeneratorRepo _repo;

        /// <summary>
        /// Creates a new instance of the <see cref="ClassGenerator"/>
        /// </summary>
        /// <param name="connector"></param>
        public ClassGenerator(Connector connector)
        {
            _repo = new GeneratorRepo(connector);
        }

        /// <summary>
        /// Loads all tables with its columns
        /// </summary>
        /// <returns>The list with the tables</returns>
        public async Task<List<Table>> LoadTablesAsync()
        {
            return await _repo.LoadTablesAsync();
        }

        /// <summary>
        /// Generates the code
        /// </summary>
        /// <param name="settings">The settings for the class generator</param>
        /// <returns>The CSharp code and the sql statement</returns>
        public ClassGenResult Generate(ClassGenSettingsDto settings)
        {
            if (settings?.Table == null)
                throw new ArgumentNullException(nameof(settings));

            var classCode = GenerateClass(settings);

            var sql = GenerateSqlQuery(settings.Table);

            var (efKeyCode, efKeyCodeOption) = settings.EfClass ? CreateEfKeyCode(settings) : ("", "");

            return new ClassGenResult(classCode, sql, efKeyCode, efKeyCodeOption);
        }

        /// <summary>
        /// Generates the code from a sql query
        /// </summary>
        /// <param name="settings">The settings for the class generator</param>
        /// <returns>The CSharp code and the sql statement</returns>
        public async Task<ClassGenResult> GenerateFromQueryAsync(ClassGenSettingsDto settings)
        {
            await GenerateClassFromQueryAsync(settings);
            var classCode = GenerateClass(settings);

            var (efKeyCode, efKeyCodeOption) = settings.EfClass ? CreateEfKeyCode(settings) : ("", "");

            return new ClassGenResult(classCode, settings.SqlQuery, efKeyCode, efKeyCodeOption);
        }

        #region Default class generator
        /// <summary>
        /// Gets the name of the property which should be created
        /// </summary>
        /// <param name="column">The column</param>
        /// <param name="colPrefix">A value which will be added with a dot as prefix</param>
        /// <returns>The property name</returns>
        private string GetPropertyName(TableColumn column, string colPrefix = "")
        {
            if (column == null)
                return "";

            var propName = string.IsNullOrEmpty(column.Alias) ? column.Column.Replace(" ", "") : column.Alias;
            // If the name is numeric, add "Column" to prevent errors, because a variable must not start with a number
            if (propName.IsNumeric())
                propName = $"Column{propName}";

            return string.IsNullOrEmpty(colPrefix) ? propName : $"{colPrefix}.{propName}";
        }

        /// <summary>
        /// Generates the code of the CSharp class
        /// </summary>
        /// <param name="settings">The settings for the class generator</param>
        /// <returns>The CSharp code</returns>
        private string GenerateClass(ClassGenSettingsDto settings)
        {
            var sb = new StringBuilder();

            if (settings.Table.UniqueError)
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

            if (!string.IsNullOrWhiteSpace(settings.SqlQuery) && settings.EfClass)
            {
                sb.AppendLine("/*")
                    .AppendLine(" NOTE - DB Context class (EF)")
                    .AppendLine(" ----------------------------")
                    .AppendLine(" The class was generated based on metadata. ")
                    .AppendLine(" Therefore, the value of the column attribute may not be correct.")
                    .AppendLine("*/")
                    .AppendLine();
            }

            void AddSummary(string message, bool withTab = true)
            {
                var spacer = withTab ? Tab : "";
                sb.AppendLine($"{spacer}/// <summary>")
                    .AppendLine($"{spacer}/// {message}")
                    .AppendLine($"{spacer}/// </summary>");
            }

            void AddPropertyAttributes(TableColumn column, string propName)
            {
                if (column.IsPrimaryKey)
                    sb.AppendLine($"{Tab}[Key]");

                // The column name and the property name equals! So ignore the rest
                if (column.Column.Equals(propName))
                    return;

                sb.AppendLine($"{Tab}[Column(\"{column.Column}\"){AddTypeInfo(column)}]");
            }

            // Adds a type change if any is needed (for example the db type is Date)
            string AddTypeInfo(TableColumn column)
            {
                return column == null
                    ? ""
                    : column.DataType.EqualsIgnoreCase("date")
                        ? ", DataType(DataType.Date)"
                        : "";
            }

            var className = string.IsNullOrEmpty(settings.ClassName) ? settings.Table.Name : settings.ClassName;

            // Add summary / ef attributes
            if (settings.AddSummary)
                AddSummary("TODO", false);
            if (settings.EfClass)
                sb.AppendLine($"[Table(\"{settings.Table.Name}\", Schema = \"{settings.Table.Schema}\")]");

            sb.AppendLine($"{settings.Modifier} {(settings.MarkAsSealed ? "sealed " : "")}class {className.FirstCharToUpper()}")
                .AppendLine("{");

            var count = 1;
            var columnCount = settings.Table.Columns.Count(c => c.Use);
            foreach (var column in settings.Table.Columns.OrderBy(o => o.ColumnPosition).Where(w => w.Use))
            {
                // Get the name for the property
                var propName = GetPropertyName(column);
                if (string.IsNullOrEmpty(propName))
                    continue; // Skip the rest if the property name is empty

                // Get the type of the column
                var dataType = GetDataType(column);

                // Check if a backing field should be created
                if (settings.BackingField)
                {
                    var field = $"_{propName.FirstCharToLower()}";

                    if (settings.AddSummary)
                        AddSummary($"Backing field for <see cref=\"{propName}\"/>");

                    sb.AppendLine($"{Tab}private {dataType} {field};").AppendLine();

                    // Add summary / ef attributes
                    if (settings.AddSummary)
                        AddSummary("Gets or sets TODO");
                    if (settings.EfClass)
                        AddPropertyAttributes(column, propName);

                    sb.AppendLine($"{Tab}public {dataType} {propName}")
                        .AppendLine($"{Tab}{{")
                        .AppendLine($"{Tab}{Tab}get => {field};")
                        .AppendLine($"{Tab}{Tab}set => {field} = value;")
                        .AppendLine($"{Tab}}}");
                }
                else
                {
                    // Add summary / ef attributes
                    if (settings.AddSummary)
                        AddSummary("TODO");
                    if (settings.EfClass)
                        AddPropertyAttributes(column, propName);

                    sb.AppendLine($"{Tab}public {dataType} {propName} {{ get; set; }}");
                }

                // Add only a space when there are columns left
                if (count != columnCount)
                    sb.AppendLine();

                count++;
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the sql query for the selected columns
        /// </summary>
        /// <param name="table">The table name</param>
        /// <returns></returns>
        private string GenerateSqlQuery(Table table)
        {
            var sb = new StringBuilder();

            sb.AppendLine("SELECT");

            var count = 1;
            var columnCount = table.Columns.Count(w => w.Use);
            foreach (var column in table.Columns.OrderBy(o => o.ColumnPosition).Where(w => w.Use))
            {
                var comma = count++ == columnCount ? "" : ",";

                sb.AppendLine(string.IsNullOrEmpty(column.Alias)
                    ? $"{Tab}[{column.Column}]{comma}"
                    : $"{Tab}[{column.Column}] AS [{column.Alias}]{comma}");
            }

            sb.AppendLine("FROM").AppendLine($"{Tab}[{table.Schema}].[{table.Name}]");

            return sb.ToString();
        }

        /// <summary>
        /// Gets the corresponding c# data type of the sql type
        /// </summary>
        /// <param name="column">The column</param>
        /// <returns>The c# data type</returns>
        private string GetDataType(TableColumn column)
        {
            var type = Helper.DataTypes.FirstOrDefault(f => f.SqlType.EqualsIgnoreCase(column.DataType));
            if (type != null)
            {
                // If the column is nullable but the type not, add a question mark
                if (column.NullableView && !type.IsNullable)
                    return $"{type.CSharpType}?";
                return type.CSharpType;
            }

            // Fall back
            return column.DataType.ToLower() switch
            {
                "bigint" => "double",
                "smallmoney" => "decimal",
                "money" => "decimal",
                "decimal" => "decimal",
                "numeric" => "decimal",
                "int" => "int",
                "tinyint" => "byte",
                "smallint" => "short",
                "bit" => "bool",
                "nvarchar" => "string",
                "char" => "string",
                "text" => "string",
                "varchar" => "string",
                "nchar" => "string",
                "ntext" => "string",
                "date" => "DateTime",
                "datetime" => "DateTime",
                "datetime2" => "DateTime",
                "smalldatetime" => "DateTime",
                "uniqueidentifier" => "GUID",
                _ => column.DataType
            };
        }

        /// <summary>
        /// Creates the code which is needed to create multiple key columns
        /// </summary>
        /// <param name="settings">The settings</param>
        /// <returns>The c# code</returns>
        private (string code, string optionalCode) CreateEfKeyCode(ClassGenSettingsDto settings)
        {
            if (settings?.Table == null)
                return ("", "");

            var keyColumns = settings.Table.Columns.Where(w => w.IsPrimaryKey).OrderBy(o => o.ColumnPosition).ToList();
            if (!keyColumns.Any())
                return ("", "");

            var columns = keyColumns.Select(s => GetPropertyName(s, "c")).ToList();

            var sb = new StringBuilder()
                .AppendLine("protected override void OnModelCreating(ModelBuilder modelBuilder)")
                .AppendLine("{")
                .AppendLine($"{Tab}modelBuilder.Entity<{settings.ClassName}>()")
                .AppendLine($"{Tab}{Tab}.HasKey(c => new {{ {string.Join(", ", columns)} }});")
                .AppendLine("}");

            var sbOption = new StringBuilder()
                .AppendLine($"modelBuilder.Entity<{settings.ClassName}>()")
                .AppendLine($"{Tab}.HasKey(c => new {{ {string.Join(", ", columns)} }});");

            return (sb.ToString(), sbOption.ToString());
        }
        #endregion

        #region Query class generator

        /// <summary>
        /// Generates the class code from the specified query
        /// </summary>
        /// <param name="settings">The settings with the query</param>
        /// <returns>The class code</returns>
        private async Task GenerateClassFromQueryAsync(ClassGenSettingsDto settings)
        {
            var result = await _repo.ExecuteQueryAsync(settings.SqlQuery);

            // Check if there are columns with the same name
            var uniqueError = result.GroupBy(g => g.ColumnName).Select(s => new
            {
                Count = s.Count()
            }).Any(a => a.Count > 1);

            settings.Table = new Table
            {
                Name = settings.ClassName,
                Columns = result.OrderBy(o => o.ColumnOrdinal).Select(s => new TableColumn
                {
                    ColumnPosition = s.ColumnOrdinal,
                    Column = s.ColumnName,
                    DataType = Helper.GetTypeAlias(s.DateType),
                    Nullable = s.IsNullable ? "YES" : "NO"
                }).ToList(),
                UniqueError = uniqueError
            };
        }

        #endregion
    }
}
