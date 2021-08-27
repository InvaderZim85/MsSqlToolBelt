using System;
using System.Linq;
using System.Text;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.ClassGenerator;

namespace MsSqlToolBelt.Business
{
    /// <summary>
    /// Provides the functions to create a class of a table
    /// </summary>
    internal static class ClassGenerator
    {
        /// <summary>
        /// Contains the tab indent
        /// </summary>
        private static readonly string Tab = new(' ', 4);

        /// <summary>
        /// Generates the code
        /// </summary>
        /// <param name="table">The selected table</param>
        /// <param name="modifier">The modifier of the class</param>
        /// <param name="markAsSealed">true to mark as sealed, otherwise false</param>
        /// <param name="className">The name of the desired class</param>
        /// <param name="backingField">true to create a backing field, otherwise false</param>
        /// <param name="efClass">true to add the Entity Framework attributes, otherwise false</param>
        /// <param name="addSummary">true to add an empty summary to the class, otherwise false</param>
        /// <returns>The CSharp code and the sql statement</returns>
        public static (string ClassCode, string SqlStatement) Generate(Table table, string modifier, bool markAsSealed, string className, bool backingField, bool efClass, bool addSummary)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var classCode = GenerateClass(table, modifier, markAsSealed, className, backingField, efClass, addSummary);

            var sql = GenerateSqlQuery(table);

            return (classCode, sql);
        }

        /// <summary>
        /// Generates the code of the CSharp class
        /// </summary>
        /// <param name="table">The table with the needed information</param>
        /// <param name="modifier">The modifier of the class</param>
        /// <param name="markAsSealed">true to mark as sealed, otherwise false</param>
        /// <param name="className">The name of the class</param>
        /// <param name="backingField">true to create a backing field, otherwise false</param>
        /// <param name="efClass">true to add the Entity Framework attributes, otherwise false</param>
        /// <param name="addSummary">true to add an empty summary to the class, otherwise false</param>
        /// <returns>The CSharp code</returns>
        private static string GenerateClass(Table table, string modifier, bool markAsSealed, string className, bool backingField, bool efClass, bool addSummary)
        {
            var sb = new StringBuilder();

            void AddSummary(string message, bool withTab = true)
            {
                var spacer = withTab ? Tab : "";
                sb.AppendLine($"{spacer}/// <summary>");
                sb.AppendLine($"{spacer}/// {message}");
                sb.AppendLine($"{spacer}/// </summary>");
            }

            void AddPropertyAttributes(bool isPrimaryKey, string columnName)
            {
                if (isPrimaryKey)
                    sb.AppendLine($"{Tab}[Key]");

                if (!string.IsNullOrEmpty(columnName))
                    sb.AppendLine($"{Tab}[Column(\"{columnName}\")]");
            }

            if (string.IsNullOrEmpty(className))
                className = table.Name;

            // Add summary / ef attributes
            if (addSummary)
                AddSummary("TODO", false);
            if (efClass)
                sb.AppendLine($"[Table(\"{table.Name}\")]");

            sb.AppendLine($"{modifier} {(markAsSealed ? "sealed " : "")}class {className.FirstCharToUpper()}");
            sb.AppendLine("{");

            var count = 1;
            var columnCount = table.Columns.Count(c => c.Use);
            foreach (var column in table.Columns.OrderBy(o => o.ColumnPosition).Where(w => w.Use))
            {
                var fieldName = string.IsNullOrEmpty(column.Alias) ? column.Column.Replace(" ", "") : column.Alias;
                if (fieldName.IsNumeric())
                    fieldName = $"Column{fieldName}";

                var dataType = GetDataType(column);
                if (backingField)
                {
                    var field = $"_{fieldName.FirstCharToLower()}";

                    if (addSummary)
                        AddSummary($"Backing field for <see cref=\"{fieldName}\"/>");

                    sb.AppendLine($"{Tab}private {dataType} {field};");

                    // Add summary / ef attributes
                    if (addSummary)
                        AddSummary("TODO");
                    if (efClass)
                        AddPropertyAttributes(column.IsPrimaryKey, column.Column);

                    sb.AppendLine($"{Tab}public {dataType} {fieldName}");
                    sb.AppendLine($"{Tab}{{");
                    sb.AppendLine($"{Tab}{Tab}get => {field};");
                    sb.AppendLine($"{Tab}{Tab}set => {field} = value;");
                    sb.AppendLine($"{Tab}}}");
                    sb.AppendLine(); // Line break
                }
                else
                {
                    // Add summary / ef attributes
                    if (addSummary)
                        AddSummary("TODO");
                    if (efClass)
                        AddPropertyAttributes(column.IsPrimaryKey, column.Column);

                    sb.AppendLine($"{Tab}public {dataType} {fieldName} {{ get; set; }}");
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
        private static string GenerateSqlQuery(Table table)
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

            sb.AppendLine("FROM");
            sb.AppendLine($"{Tab}[{table.Schema}].[{table.Name}]");

            return sb.ToString();
        }

        /// <summary>
        /// Gets the corresponding c# data type of the sql type
        /// </summary>
        /// <param name="column">The column</param>
        /// <returns>The c# data type</returns>
        private static string GetDataType(TableColumn column)
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
    }
}
